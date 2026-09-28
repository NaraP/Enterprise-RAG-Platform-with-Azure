using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RagPlatform.Web.Mvc.Models.ViewModels;

public class UploadDocumentViewModel
{
    [Required]
    [Display(Name = "Document")]
    public IFormFile? File { get; set; }

    [Display(Name = "Tags (comma-separated)")]
    public string? Tags { get; set; }

    [Display(Name = "Description")]
    [StringLength(1000)]
    public string? Description { get; set; }
}
