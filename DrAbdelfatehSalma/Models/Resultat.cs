using System.ComponentModel.DataAnnotations;

namespace DrAbdelfatehSalma.Models;

public class Resultat
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string CategoryKey { get; set; } = "rhinoplastie"; // rhinoplastie, lifting, mammaire, injections

    [Required]
    [StringLength(150)]
    public string CategoryTitle { get; set; } = "RHINOPLASTIE ULTRASONIQUE";

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string BeforeImageUrl { get; set; } = string.Empty;

    public string AfterImageUrl { get; set; } = string.Empty;

    public string PatientInfo { get; set; } = string.Empty; // e.g. "Femme, 29 ans (Sousse)"

    public string Technique { get; set; } = string.Empty;

    public string Anesthesia { get; set; } = string.Empty;

    public string RecoveryTime { get; set; } = string.Empty;

    public bool IsFeatured { get; set; } = false;
}
