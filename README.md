# Enterprise RAG Platform with Azure

An **enterprise-grade Retrieval-Augmented Generation (RAG) platform** built with **Angular and .NET 10**, designed for secure document ingestion, asynchronous processing, intelligent indexing, and hybrid AI-powered search using Azure services.

### 🚀 Architecture

**Angular → .NET 10 API → Azure Storage → Azure Service Bus → Azure Functions/Container Apps → Azure AI Search → RAG Retrieval**

### 🔑 Key Features

* 📄 Secure upload and storage of **PDF, Word, Excel, PowerPoint, and text documents**
* 🔐 **Entra ID authentication, RBAC, document-level security, and audit tracking**
* ⚡ Event-driven document processing using **Azure Service Bus Queues/Topics**
* 🔄 Background processing with **Azure Functions / Azure Container Apps**
* ✂️ Intelligent text extraction, chunking, and metadata preservation
* 🧠 Vector embedding generation for document chunks
* 🔎 **Azure AI Search** with **vector, keyword, semantic, and hybrid search**
* 🏢 Tenant/user-aware indexing and secure filtering
* 📊 Processing status, logging, monitoring, retries, and dead-letter handling
* 📈 Scalable and decoupled architecture supporting parallel document processing

### 🔄 Processing Flow

1. User uploads a document through the Angular application.
2. .NET 10 API validates the request and stores the file in Azure Storage.
3. An ingestion message is published to Azure Service Bus.
4. Background workers extract and preprocess the document.
5. Content is chunked and converted into vector embeddings.
6. Chunks, metadata, security information, and embeddings are indexed in Azure AI Search.
7. User queries are processed through hybrid/vector search with security filtering.
8. Azure Monitor and Application Insights provide observability across the pipeline.

### 🛠️ Technology Stack

**Frontend:** Angular, TypeScript
**Backend:** C#, .NET 10, ASP.NET Core Web API
**Azure:** Azure Storage, Service Bus, Functions, Container Apps, AI Search, Entra ID, Azure Monitor, Application Insights
**AI/RAG:** Embeddings, Vector Search, Semantic Search, Hybrid Search
**Architecture:** Clean Architecture, Microservices, Event-Driven Architecture, Asynchronous Processing

### 🎯 Objective

Build a **secure, scalable, cloud-native RAG platform** that transforms enterprise documents into searchable knowledge using Azure AI services while maintaining strong security, reliability, and operational visibility.
