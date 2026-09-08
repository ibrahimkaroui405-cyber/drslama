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
        foreach (var item in sqliteChirurgies)
        {
            if (!pgContext.Chirurgies.Any(x => x.Id == item.Id || x.Slug == item.Slug))
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
                    Faqs = item.Faqs
                });
            }
        }
        pgContext.SaveChanges();

        // 2. Reparatrices
        var sqliteReparatrices = sqliteContext.Reparatrices.AsNoTracking().ToList();
        foreach (var item in sqliteReparatrices)
        {
            if (!pgContext.Reparatrices.Any(x => x.Id == item.Id || x.Slug == item.Slug))
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
        }
        pgContext.SaveChanges();

        // 3. Esthetiques
        var sqliteEsthetiques = sqliteContext.Esthetiques.AsNoTracking().ToList();
        foreach (var item in sqliteEsthetiques)
        {
            if (!pgContext.Esthetiques.Any(x => x.Id == item.Id || x.Slug == item.Slug))
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
        }
        pgContext.SaveChanges();

        // 4. Resultats
        var sqliteResultats = sqliteContext.Resultats.AsNoTracking().ToList();
        foreach (var item in sqliteResultats)
        {
            if (!pgContext.Resultats.Any(x => x.Id == item.Id || x.Title == item.Title))
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
        foreach (var item in sqliteActualites)
        {
            if (!pgContext.Actualites.Any(x => x.Id == item.Id || x.Title == item.Title))
            {
                var dt = item.CreatedAt.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc) 
                    : item.CreatedAt.ToUniversalTime();

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
