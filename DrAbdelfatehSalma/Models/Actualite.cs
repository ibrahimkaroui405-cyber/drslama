using System.ComponentModel.DataAnnotations;

namespace DrAbdelfatehSalma.Models;

public class Actualite
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(250)]
    public string Title { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty; // Short description

    public string Content { get; set; } = string.Empty;  // Full content
    public string ImageUrl { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty; // Comma-separated tags e.g. "#ISAPS2026,#Rhinoplastie"

    public string DateLabel { get; set; } = string.Empty; // e.g. "AOÛT 2026 • SOUSSE / PARIS"

    public bool IsFeatured { get; set; } = false; // Show as top banner card

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
