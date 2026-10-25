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

    [StringLength(100)]
    public string? SubCategoryKey { get; set; } = string.Empty;

    [StringLength(150)]
    public string? SubCategoryTitle { get; set; } = string.Empty;

    public bool IsFeatured { get; set; } = false;
}

public static class ResultatTaxonomy
{
    public class SubCategoryItem
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class CategoryItem
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<SubCategoryItem> SubCategories { get; set; } = new();
    }

    public static readonly List<CategoryItem> Categories = new()
    {
        new CategoryItem
        {
            Key = "visage",
            Title = "CHIRURGIE DU VISAGE",
            SubCategories = new()
            {
                new SubCategoryItem { Key = "rhinoplastie", Title = "Rhinoplastie" },
                new SubCategoryItem { Key = "lifting-visage", Title = "Lifting visage" },
                new SubCategoryItem { Key = "blepharoplastie", Title = "Blépharoplastie" },
                new SubCategoryItem { Key = "genioplastie", Title = "Génioplastie" },
                new SubCategoryItem { Key = "otoplastie", Title = "Otoplastie" }
            }
        },
        new CategoryItem
        {
            Key = "poitrine",
            Title = "CHIRURGIE DU SEIN (POITRINE)",
            SubCategories = new()
            {
                new SubCategoryItem { Key = "augmentation-mammaire", Title = "Augmentation Mammaire" },
                new SubCategoryItem { Key = "reduction-mammaire", Title = "Réduction Mammaire" },
                new SubCategoryItem { Key = "lifting-seins", Title = "Lifting des seins" }
            }
        },
        new CategoryItem
        {
            Key = "silhouette",
            Title = "CHIRURGIE DE LA SILHOUETTE",
            SubCategories = new()
            {
                new SubCategoryItem { Key = "liposuccion", Title = "Liposuccion" },
                new SubCategoryItem { Key = "abdominoplastie", Title = "Abdominoplastie" },
                new SubCategoryItem { Key = "lifting-bras", Title = "Lifting des bras" }
            }
        }
    };

    public static string GetDefaultSubCategoryKey(string? categoryKey)
    {
        var cat = Categories.FirstOrDefault(c => c.Key.Equals(categoryKey, StringComparison.OrdinalIgnoreCase));
        return cat?.SubCategories.FirstOrDefault()?.Key ?? "rhinoplastie";
    }

    public static string GetSubCategoryTitle(string? categoryKey, string? subCategoryKey)
    {
        if (string.IsNullOrWhiteSpace(subCategoryKey)) return string.Empty;
        var cat = Categories.FirstOrDefault(c => c.Key.Equals(categoryKey, StringComparison.OrdinalIgnoreCase));
        if (cat != null)
        {
            var sub = cat.SubCategories.FirstOrDefault(s => s.Key.Equals(subCategoryKey, StringComparison.OrdinalIgnoreCase));
            if (sub != null) return sub.Title;
        }

        // Fallback search across all categories
        foreach (var c in Categories)
        {
            var sub = c.SubCategories.FirstOrDefault(s => s.Key.Equals(subCategoryKey, StringComparison.OrdinalIgnoreCase));
            if (sub != null) return sub.Title;
        }

        return subCategoryKey;
    }
}
