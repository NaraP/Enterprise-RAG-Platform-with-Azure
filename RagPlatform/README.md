# RAG Platform

An enterprise-grade Retrieval-Augmented Generation (RAG) platform built on .NET 9 with
Clean Architecture, implementing secure document ingestion, event-driven processing via
Azure Service Bus, and hybrid retrieval via Azure AI Search.

## Architecture

Clean Architecture / Onion Architecture, dependencies point inward only:

```
RagPlatform.Web.Mvc  ─┐
RagPlatform.Worker    ├──▶ RagPlatform.Infrastructure ──▶ RagPlatform.Application ──▶ RagPlatform.Domain
                       ┘
```

| Project | Responsibility |
|---|---|
| **RagPlatform.Domain** | Entities (`Document`, `DocumentChunk`, `Tenant`, `ApplicationUser`, `DocumentPermission`), enums, domain logic, repository interfaces. Zero external dependencies. |
| **RagPlatform.Application** | Use cases as MediatR commands/queries (CQRS), DTOs, validation (FluentValidation), pipeline behaviors (logging, validation), and the *interfaces* Infrastructure implements (`IBlobStorageService`, `IServiceBusPublisher`, `ISearchIndexService`, `IEmbeddingService`, `IDocumentTextExtractor`). Depends only on Domain. |
| **RagPlatform.Infrastructure** | Azure-backed implementations: Blob Storage, Service Bus, Azure AI Search (hybrid vector+keyword+semantic), Azure OpenAI embeddings, document text extraction (PDF/Word/Excel/PowerPoint), EF Core persistence, Entra ID current-user/RBAC. |
| **RagPlatform.Worker** | Background service (`BackgroundService`) with a session-aware Service Bus processor. Executes steps 03-05: extract → chunk → embed → index. Scales horizontally; sessions keep per-document messages ordered while different documents process in parallel. |
| **RagPlatform.Web.Mvc** | ASP.NET Core **MVC** front end: document upload, dashboard/status, and search UI. Auth via Microsoft Entra ID (Microsoft.Identity.Web). |

## Requirements-to-code mapping

| Requirement | Where |
|---|---|
| Web app with upload + auth | `RagPlatform.Web.Mvc` (`DocumentsController`, Entra ID via `Program.cs`) |
| Document storage, structured folders + metadata | `AzureBlobStorageService`, path built in `UploadDocumentCommandHandler` (`tenant/user/yyyy/MM/...`) |
| Service Bus queues/topics for the full lifecycle | `QueueNames` (ingestion, preprocessing, indexing, status, errors, retry) |
| Publish message on upload with doc id/path/user/metadata | `UploadDocumentCommandHandler` → `DocumentIngestionMessageContract` |
| Reliable delivery, dead-lettering, retry | `DocumentProcessingService` (Worker) — session processor, `AbandonMessageAsync`/`DeadLetterMessageAsync`, `Document.ScheduleRetry()/MarkDeadLettered()` |
| Background Service Worker(s), async & parallel | `RagPlatform.Worker` — `MaxConcurrentSessions = 8` |
| Extraction (PDF/Word/Excel/PPT/Text) | `TextExtraction/*Extractor.cs` + `TextExtractorFactory` |
| Chunking with overlap, structure preserved | `TextChunkingService` |
| Embedding generation | `AzureOpenAiEmbeddingService` |
| Index create/update, tenant-specific, incremental | `AzureAiSearchService.EnsureIndexExistsAsync` / `IndexChunksAsync` (index name = `tenant-{tenantId}`) |
| Hybrid search (vector + keyword + semantic) | `AzureAiSearchService.HybridSearchAsync`, exposed via `SearchDocumentsQuery` / `SearchController` |
| Monitoring/logging/metrics | `LoggingBehavior<,>` (MediatR pipeline) + Application Insights wired in both Worker and Web `Program.cs` |
| RBAC + document-level security | `RoleNames` / `AuthorizationPolicies` (Entra ID app roles), `DocumentPermission`, `Document.CanBeAccessedBy`, and the `allowedRoles`/`ownerUserId` filter in `HybridSearchAsync` |
| Audit trail | `AuditLogEntry`, `IAuditLogRepository`, written on upload (extend to search/view events as needed) |

## Prerequisites

- .NET 9 SDK
- Azure subscription with: Storage Account, Service Bus namespace, Azure AI Search, Azure OpenAI (embedding deployment), Azure SQL (or SQL Server), Entra ID app registration, Application Insights
- All Azure clients authenticate via `DefaultAzureCredential` — no keys/secrets in config. Grant the app's managed identity (or your local `az login` principal for dev) the relevant data-plane RBAC roles: **Storage Blob Data Contributor**, **Azure Service Bus Data Owner**, **Search Index Data Contributor** + **Search Service Contributor**, **Cognitive Services OpenAI User**.

## Configuration

Fill in the placeholders in each project's `appsettings.json` (`Azure:*`, `AzureAd:*`,
`ConnectionStrings:ApplicationDb`) or override them with **user secrets** locally:

```bash
cd src/Web/RagPlatform.Web.Mvc
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<client-id>"
```

Provision the Service Bus queues named in `QueueNames` (document-ingestion as a **session-enabled**
queue with dead-lettering on max delivery count) and the SQL database (`dotnet ef database update`
from `RagPlatform.Infrastructure` once you add a migration).

## Running locally

```bash
dotnet restore
dotnet build

# Terminal 1 — Web app (upload / dashboard / search UI)
dotnet run --project src/Web/RagPlatform.Web.Mvc

# Terminal 2 — Worker (consumes Service Bus, runs the extraction/chunk/embed/index pipeline)
dotnet run --project src/Workers/RagPlatform.Worker
```

## Tests

```bash
dotnet test
```

## Notable design decisions

- **One Azure AI Search index per tenant** (`tenant-{tenantId}`) keeps data isolation simple
  and lets each tenant scale independently, at the cost of index-count overhead versus a
  single shared index with a tenant filter — revisit if you expect thousands of tenants.
- **Service Bus sessions** (`sessionId = documentId`) guarantee ordered processing per
  document while still allowing many *different* documents to process concurrently.
- **Content-hash de-duplication**: re-uploading identical bytes returns the existing
  `Document` instead of reprocessing.
- **`Result<T>`** in Domain is available for handlers that prefer explicit success/failure
  over exceptions; the scaffolded handlers use exceptions + the `NotFoundException` /
  `ForbiddenAccessException` / `ValidationException` types for simplicity — adopt `Result<T>`
  more broadly if you'd rather avoid exception-driven control flow.
- **EF Core owns metadata/RBAC/audit only** — chunk text + vectors live in Azure AI Search;
  `DocumentChunk` rows in SQL are metadata/audit copies, not the retrieval source of truth.

## What's scaffolded vs. what you still need to build out

This is a working architectural skeleton with real Azure SDK calls, not a toy — but for
production you'll still want to: add EF Core migrations, provision infra (Bicep/Terraform),
add integration tests against Azurite/a Service Bus emulator, add the operational dashboard
(query `ProcessingHistory`/`AuditLog`) mentioned in requirement #9, and tune HNSW/semantic
config + chunk size/overlap for your document mix.
