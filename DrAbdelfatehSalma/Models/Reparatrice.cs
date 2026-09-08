using System.ComponentModel.DataAnnotations;

namespace DrAbdelfatehSalma.Models;

public class Reparatrice
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string Duree { get; set; } = string.Empty;

    public string Anesthesie { get; set; } = string.Empty;

    public string Eviction { get; set; } = string.Empty;

    public string Hospitalisation { get; set; } = string.Empty;

    public string Overview { get; set; } = string.Empty;

    public string Indications { get; set; } = string.Empty;

    public string Steps { get; set; } = string.Empty;

    public string Faqs { get; set; } = string.Empty;
}
