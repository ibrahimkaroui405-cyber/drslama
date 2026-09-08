using System.ComponentModel.DataAnnotations;

namespace DrAbdelfatehSalma.Models;

public class Temoignage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string PatientName { get; set; } = string.Empty;

    public string PatientInfo { get; set; } = string.Empty; // e.g. "RHINOPLASTIE — SOUSSE"

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "text"; // "video" or "text"

    public string Quote { get; set; } = string.Empty;

    public string VideoUrl { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int Rating { get; set; } = 5;

    public string FollowUpTime { get; set; } = "6 mois post-op";

    public bool IsFeatured { get; set; } = false;
}
