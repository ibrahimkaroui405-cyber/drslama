using System;
using System.IO;
using System.Linq;
using DrAbdelfatehSalma.Models;
using Microsoft.EntityFrameworkCore;

namespace DrAbdelfatehSalma.Data;

public static class DataMigrator
{
    public static void MigrateFromSqliteToPostgres(string sqliteDbPath, AppDbContext pgContext)
    {
        if (!File.Exists(sqliteDbPath))
        {
            Console.WriteLine($"[Migrator] SQLite database not found at {sqliteDbPath}. Skipping migration.");
            return;
        }

        Console.WriteLine($"[Migrator] Ensuring PostgreSQL database schema is created...");
        pgContext.Database.EnsureCreated();

        var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={sqliteDbPath}")
            .Options;

        using var sqliteContext = new AppDbContext(sqliteOptions);

        // 1. Chirurgies
        var sqliteChirurgies = sqliteContext.Chirurgies.AsNoTracking().ToList();
        var existingPgChirurgies = pgContext.Chirurgies.ToList();
        var sqliteChirSlugs = sqliteChirurgies.Select(s => s.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toDeleteChir = existingPgChirurgies.Where(p => !sqliteChirSlugs.Contains(p.Slug)).ToList();
        if (toDeleteChir.Any())
        {
            pgContext.Chirurgies.RemoveRange(toDeleteChir);
        }

        foreach (var item in sqliteChirurgies)
        {
            var pgItem = existingPgChirurgies.FirstOrDefault(x => x.Slug == item.Slug);
            if (pgItem == null)
            {
                pgContext.Chirurgies.Add(new Chirurgie
                {
                    Slug = item.Slug,
                    Title = item.Title,
                    Category = item.Category,
                    Subtitle = item.Subtitle,
                    ImageUrl = item.ImageUrl,
                    Duree = item.Duree,
                    Anesthesie = item.Anesthesie,
                    Eviction = item.Eviction,
                    Hospitalisation = item.Hospitalisation,
                    Overview = item.Overview,
                    Indications = item.Indications,
                    Steps = item.Steps,
                    Faqs = item.Faqs,
                    Order = item.Order
                });
            }
            else
            {
                pgItem.Title = item.Title;
                pgItem.Category = item.Category;
                pgItem.Subtitle = item.Subtitle;
                pgItem.ImageUrl = item.ImageUrl;
                pgItem.Duree = item.Duree;
                pgItem.Anesthesie = item.Anesthesie;
                pgItem.Eviction = item.Eviction;
                pgItem.Hospitalisation = item.Hospitalisation;
                pgItem.Overview = item.Overview;
                pgItem.Indications = item.Indications;
                pgItem.Steps = item.Steps;
                pgItem.Faqs = item.Faqs;
                pgItem.Order = item.Order;
            }
        }
        pgContext.SaveChanges();

        // 2. Reparatrices
        var sqliteReparatrices = sqliteContext.Reparatrices.AsNoTracking().ToList();
        var existingPgRep = pgContext.Reparatrices.ToList();
        var sqliteRepSlugs = sqliteReparatrices.Select(s => s.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toDeleteRep = existingPgRep.Where(p => !sqliteRepSlugs.Contains(p.Slug)).ToList();
        if (toDeleteRep.Any())
        {
            pgContext.Reparatrices.RemoveRange(toDeleteRep);
        }

        foreach (var item in sqliteReparatrices)
        {
            var pgItem = existingPgRep.FirstOrDefault(x => x.Slug == item.Slug);
            if (pgItem == null)
            {
                pgContext.Reparatrices.Add(new Reparatrice
                {
                    Slug = item.Slug,
                    Title = item.Title,
                    Category = item.Category,
                    Subtitle = item.Subtitle,
                    ImageUrl = item.ImageUrl,
                    Duree = item.Duree,
                    Anesthesie = item.Anesthesie,
                    Eviction = item.Eviction,
                    Hospitalisation = item.Hospitalisation,
                    Overview = item.Overview,
                    Indications = item.Indications,
                    Steps = item.Steps,
                    Faqs = item.Faqs
                });
            }
            else
            {
                pgItem.Title = item.Title;
                pgItem.Category = item.Category;
                pgItem.Subtitle = item.Subtitle;
                pgItem.ImageUrl = item.ImageUrl;
                pgItem.Duree = item.Duree;
                pgItem.Anesthesie = item.Anesthesie;
                pgItem.Eviction = item.Eviction;
                pgItem.Hospitalisation = item.Hospitalisation;
                pgItem.Overview = item.Overview;
                pgItem.Indications = item.Indications;
                pgItem.Steps = item.Steps;
                pgItem.Faqs = item.Faqs;
            }
        }
        pgContext.SaveChanges();

        // 3. Esthetiques
        var sqliteEsthetiques = sqliteContext.Esthetiques.AsNoTracking().ToList();
        var existingPgEst = pgContext.Esthetiques.ToList();
        var sqliteEstSlugs = sqliteEsthetiques.Select(s => s.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toDeleteEst = existingPgEst.Where(p => !sqliteEstSlugs.Contains(p.Slug)).ToList();
        if (toDeleteEst.Any())
        {
            pgContext.Esthetiques.RemoveRange(toDeleteEst);
        }

        foreach (var item in sqliteEsthetiques)
        {
            var pgItem = existingPgEst.FirstOrDefault(x => x.Slug == item.Slug);
            if (pgItem == null)
            {
                pgContext.Esthetiques.Add(new Esthetique
                {
                    Slug = item.Slug,
                    Title = item.Title,
                    Category = item.Category,
                    Subtitle = item.Subtitle,
                    ImageUrl = item.ImageUrl,
                    Duree = item.Duree,
                    Anesthesie = item.Anesthesie,
                    Eviction = item.Eviction,
                    Hospitalisation = item.Hospitalisation,
                    Overview = item.Overview,
                    Indications = item.Indications,
                    Steps = item.Steps,
                    Faqs = item.Faqs
                });
            }
            else
            {
                pgItem.Title = item.Title;
                pgItem.Category = item.Category;
                pgItem.Subtitle = item.Subtitle;
                pgItem.ImageUrl = item.ImageUrl;
                pgItem.Duree = item.Duree;
                pgItem.Anesthesie = item.Anesthesie;
                pgItem.Eviction = item.Eviction;
                pgItem.Hospitalisation = item.Hospitalisation;
                pgItem.Overview = item.Overview;
                pgItem.Indications = item.Indications;
                pgItem.Steps = item.Steps;
                pgItem.Faqs = item.Faqs;
            }
        }
        pgContext.SaveChanges();

        // 4. Resultats
        var sqliteResultats = sqliteContext.Resultats.AsNoTracking().ToList();
        var existingPgRes = pgContext.Resultats.ToList();
        foreach (var item in sqliteResultats)
        {
            var pgItem = existingPgRes.FirstOrDefault(x => x.Title == item.Title || x.Id == item.Id);
            if (pgItem == null)
            {
                pgContext.Resultats.Add(new Resultat
                {
                    CategoryKey = item.CategoryKey,
                    CategoryTitle = item.CategoryTitle,
                    Title = item.Title,
                    Description = item.Description,
                    BeforeImageUrl = item.BeforeImageUrl,
                    AfterImageUrl = item.AfterImageUrl,
                    PatientInfo = item.PatientInfo,
                    Technique = item.Technique,
                    Anesthesia = item.Anesthesia,
                    RecoveryTime = item.RecoveryTime,
                    IsFeatured = item.IsFeatured
                });
            }
            else
            {
                pgItem.CategoryKey = item.CategoryKey;
                pgItem.CategoryTitle = item.CategoryTitle;
                pgItem.Title = item.Title;
                pgItem.Description = item.Description;
                pgItem.BeforeImageUrl = item.BeforeImageUrl;
                pgItem.AfterImageUrl = item.AfterImageUrl;
                pgItem.PatientInfo = item.PatientInfo;
                pgItem.Technique = item.Technique;
                pgItem.Anesthesia = item.Anesthesia;
                pgItem.RecoveryTime = item.RecoveryTime;
                pgItem.IsFeatured = item.IsFeatured;
            }
        }
        pgContext.SaveChanges();

        // 5. Temoignages
        var sqliteTemoignages = sqliteContext.Temoignages.AsNoTracking().ToList();
        foreach (var item in sqliteTemoignages)
        {
            if (!pgContext.Temoignages.Any(x => x.Id == item.Id || (x.PatientName == item.PatientName && x.Quote == item.Quote)))
            {
                pgContext.Temoignages.Add(new Temoignage
                {
                    PatientName = item.PatientName,
                    PatientInfo = item.PatientInfo,
                    Type = item.Type,
                    Quote = item.Quote,
                    VideoUrl = item.VideoUrl,
                    ImageUrl = item.ImageUrl,
                    Rating = item.Rating,
                    FollowUpTime = item.FollowUpTime,
                    IsFeatured = item.IsFeatured
                });
            }
        }
        pgContext.SaveChanges();

        // 6. Actualites
        var sqliteActualites = sqliteContext.Actualites.AsNoTracking().ToList();
        var existingPgAct = pgContext.Actualites.ToList();
        var sqliteActTitles = sqliteActualites.Select(s => s.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toDeleteAct = existingPgAct.Where(p => !sqliteActTitles.Contains(p.Title)).ToList();
        if (toDeleteAct.Any())
        {
            pgContext.Actualites.RemoveRange(toDeleteAct);
        }

        foreach (var item in sqliteActualites)
        {
            var dt = item.CreatedAt.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc) 
                : item.CreatedAt.ToUniversalTime();

            var pgItem = existingPgAct.FirstOrDefault(x => x.Title == item.Title);
            if (pgItem == null)
            {
                pgContext.Actualites.Add(new Actualite
                {
                    Title = item.Title,
                    Excerpt = item.Excerpt,
                    Content = item.Content,
                    ImageUrl = item.ImageUrl,
                    Tags = item.Tags,
                    DateLabel = item.DateLabel,
                    IsFeatured = item.IsFeatured,
                    CreatedAt = dt
                });
            }
            else
            {
                pgItem.Title = item.Title;
                pgItem.Excerpt = item.Excerpt;
                pgItem.Content = item.Content;
                pgItem.ImageUrl = item.ImageUrl;
                pgItem.Tags = item.Tags;
                pgItem.DateLabel = item.DateLabel;
                pgItem.IsFeatured = item.IsFeatured;
                pgItem.CreatedAt = dt;
            }
        }
        pgContext.SaveChanges();

        // 7. DemandesContact
        var sqliteDemandes = sqliteContext.DemandesContact.AsNoTracking().ToList();
        foreach (var item in sqliteDemandes)
        {
            if (!pgContext.DemandesContact.Any(x => x.Id == item.Id || (x.Email == item.Email && x.CreatedAt == item.CreatedAt)))
            {
                var dt = item.CreatedAt.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc) 
                    : item.CreatedAt.ToUniversalTime();

                pgContext.DemandesContact.Add(new DemandeContact
                {
                    FullName = item.FullName,
                    Email = item.Email,
                    Phone = item.Phone,
                    PreferredSlot = item.PreferredSlot,
                    ConsultationType = item.ConsultationType,
                    Message = item.Message,
                    CreatedAt = dt,
                    IsProcessed = item.IsProcessed,
                    Nom = item.Nom,
                    Prenom = item.Prenom,
                    Indicatif = item.Indicatif,
                    Objet = item.Objet,
                    PhotoPath = item.PhotoPath,
                    ConsentAccepted = item.ConsentAccepted
                });
            }
        }
        pgContext.SaveChanges();

        // Reset PostgreSQL identity sequences so subsequent manual inserts work without ID conflicts
        try
        {
            string[] tables = { "Chirurgies", "Reparatrices", "Esthetiques", "Resultats", "Temoignages", "Actualites", "DemandesContact" };
            foreach (var table in tables)
            {
                pgContext.Database.ExecuteSqlRaw($@"
                    SELECT setval(
                        pg_get_serial_sequence('""{table}""', 'Id'),
                        COALESCE((SELECT MAX(""Id"") FROM ""{table}""), 1)
                    );
                ");
            }
        }
        catch { }

        Console.WriteLine("\n========================================================");
        Console.WriteLine("    MIGRATION VALIDATION: SQLITE vs NEON POSTGRESQL     ");
        Console.WriteLine("========================================================");
        Console.WriteLine($"  - Chirurgies     : Neon={pgContext.Chirurgies.Count()} | SQLite={sqliteChirurgies.Count}");
        Console.WriteLine($"  - Reparatrices   : Neon={pgContext.Reparatrices.Count()} | SQLite={sqliteReparatrices.Count}");
        Console.WriteLine($"  - Esthetiques    : Neon={pgContext.Esthetiques.Count()} | SQLite={sqliteEsthetiques.Count}");
        Console.WriteLine($"  - Resultats      : Neon={pgContext.Resultats.Count()} | SQLite={sqliteResultats.Count}");
        Console.WriteLine($"  - Temoignages    : Neon={pgContext.Temoignages.Count()} | SQLite={sqliteTemoignages.Count}");
        Console.WriteLine($"  - Actualites     : Neon={pgContext.Actualites.Count()} | SQLite={sqliteActualites.Count}");
        Console.WriteLine($"  - DemandesContact: Neon={pgContext.DemandesContact.Count()} | SQLite={sqliteDemandes.Count}");
        Console.WriteLine("========================================================\n");
    }
}
