using System.ComponentModel.DataAnnotations;

namespace DrAbdelfatehSalma.Models;

public class DemandeContact
{
    [Key]
    public int Id { get; set; }

    public string? Nom { get; set; } = string.Empty;

    public string? Prenom { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Indicatif { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Objet { get; set; } = string.Empty;

    public string ConsultationType { get; set; } = string.Empty;

    public string? PhotoPath { get; set; } = string.Empty;

    public bool ConsentAccepted { get; set; } = true;

    public string PreferredSlot { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsProcessed { get; set; } = false;
}

