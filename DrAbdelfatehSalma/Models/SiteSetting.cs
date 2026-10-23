using System.ComponentModel.DataAnnotations;

namespace DrAbdelfatehSalma.Models;

/// <summary>
/// Generic key/value store for site settings editable from the admin panel
/// (e.g. the access code of the Before/After results gallery).
/// </summary>
public class SiteSetting
{
    public const string GalleryAccessCodeKey = "GalleryAccessCode";
    public const string DefaultGalleryAccessCode = "slama-vip-2026!";

    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
