using System.Diagnostics;
using DrAbdelfatehSalma.Data;
using DrAbdelfatehSalma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrAbdelfatehSalma.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public HomeController(ILogger<HomeController> logger, AppDbContext context, IWebHostEnvironment env)
    {
        _logger = logger;
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        DbInitializer.Initialize(_context);
        ViewBag.Resultats = await _context.Resultats.ToListAsync();
        ViewBag.Temoignages = await _context.Temoignages.ToListAsync();
        ViewBag.Chirurgies = await _context.Chirurgies.ToListAsync();
        ViewBag.Esthetiques = await _context.Esthetiques.ToListAsync();
        ViewBag.Actualites = await _context.Actualites.OrderByDescending(a => a.IsFeatured).ThenByDescending(a => a.CreatedAt).ToListAsync();
        return View();
    }


    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> Chirurgies()
    {
        ViewBag.Resultats = await _context.Resultats.ToListAsync();
        var list = await _context.Chirurgies.ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Esthetique()
    {
        var list = await _context.Esthetiques.ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Reparatrice()
    {
        var list = await _context.Reparatrices.ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Resultats()
    {
        var list = await _context.Resultats.ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Temoignages()
    {
        DbInitializer.Initialize(_context);
        var list = await _context.Temoignages.OrderByDescending(t => t.Id).ToListAsync();
        ViewBag.Actualites = await _context.Actualites.OrderByDescending(a => a.IsFeatured).ThenByDescending(a => a.CreatedAt).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> ActualiteDetail(int id)
    {
        var item = await _context.Actualites.FirstOrDefaultAsync(a => a.Id == id);
        if (item == null)
        {
            return RedirectToAction("Temoignages");
        }

        ViewBag.OtherActualites = await _context.Actualites
            .Where(a => a.Id != id)
            .OrderByDescending(a => a.CreatedAt)
            .Take(3)
            .ToListAsync();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTemoignage(Temoignage model)
    {
        if (!string.IsNullOrWhiteSpace(model.PatientName) && !string.IsNullOrWhiteSpace(model.Quote))
        {
            model.Type = "text";
            model.PatientInfo = string.IsNullOrWhiteSpace(model.PatientInfo) ? "PATIENT DU CABINET • SOUSSE" : model.PatientInfo.Trim();
            model.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=600" : model.ImageUrl;
            model.VideoUrl = string.Empty;
            model.Rating = model.Rating < 1 || model.Rating > 5 ? 5 : model.Rating;
            model.FollowUpTime = string.IsNullOrWhiteSpace(model.FollowUpTime) ? "Avis Patient Vérifié" : model.FollowUpTime.Trim();
            model.IsFeatured = false;

            _context.Temoignages.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Merci infiniment ! Votre témoignage a été publié avec succès.";
        }
        else
        {
            TempData["ErrorMessage"] = "Veuillez remplir votre nom et votre témoignage.";
        }

        return RedirectToAction("Temoignages");
    }

    [HttpGet]
    public async Task<IActionResult> Contact(string? procedure = null, string? type = null)
    {
        ViewBag.SelectedProcedure = procedure ?? type ?? "";
        ViewBag.Chirurgies = await _context.Chirurgies.OrderBy(c => c.Category).ThenBy(c => c.Title).ToListAsync();
        ViewBag.Reparatrices = await _context.Reparatrices.OrderBy(r => r.Title).ToListAsync();
        ViewBag.Esthetiques = await _context.Esthetiques.OrderBy(e => e.Title).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(DemandeContact model, IFormFile? photoFile)
    {
        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            model.FullName = $"{model.Nom} {model.Prenom}".Trim();
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                model.FullName = "Patient";
            }
        }

        if (!string.IsNullOrWhiteSpace(model.Indicatif) && !string.IsNullOrWhiteSpace(model.Phone) && !model.Phone.StartsWith("+"))
        {
            model.Phone = $"{model.Indicatif} {model.Phone}".Trim();
        }

        if (photoFile != null && photoFile.Length > 0)
        {
            try
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "demandes");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(photoFile.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }
                model.PhotoPath = $"/uploads/demandes/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload de photo pour la demande de contact.");
            }
        }

        if (!string.IsNullOrWhiteSpace(model.Phone) && !string.IsNullOrWhiteSpace(model.Email))
        {
            // Ensure SQLite columns exist dynamically
            try { _context.Database.ExecuteSqlRaw(@"ALTER TABLE ""DemandesContact"" ADD COLUMN ""Nom"" TEXT NULL;"); } catch { }
            try { _context.Database.ExecuteSqlRaw(@"ALTER TABLE ""DemandesContact"" ADD COLUMN ""Prenom"" TEXT NULL;"); } catch { }
            try { _context.Database.ExecuteSqlRaw(@"ALTER TABLE ""DemandesContact"" ADD COLUMN ""Indicatif"" TEXT NULL;"); } catch { }
            try { _context.Database.ExecuteSqlRaw(@"ALTER TABLE ""DemandesContact"" ADD COLUMN ""Objet"" TEXT NULL;"); } catch { }
            try { _context.Database.ExecuteSqlRaw(@"ALTER TABLE ""DemandesContact"" ADD COLUMN ""PhotoPath"" TEXT NULL;"); } catch { }
            try { _context.Database.ExecuteSqlRaw(@"ALTER TABLE ""DemandesContact"" ADD COLUMN ""ConsentAccepted"" INTEGER NOT NULL DEFAULT 1;"); } catch { }

            if (string.IsNullOrWhiteSpace(model.Objet))
            {
                model.Objet = !string.IsNullOrWhiteSpace(model.ConsultationType) ? model.ConsultationType : "Demande de contact";
            }
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                model.FullName = $"{model.Prenom} {model.Nom}".Trim();
            }

            model.CreatedAt = DateTime.Now;
            model.IsProcessed = false;
            _context.DemandesContact.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Votre demande a bien été envoyée au cabinet du Pr. Abdelfateh Slama. Nous vous répondrons dans les plus brefs délais.";
            return RedirectToAction("Contact");
        }

        ViewBag.Chirurgies = await _context.Chirurgies.OrderBy(c => c.Category).ThenBy(c => c.Title).ToListAsync();
        ViewBag.Reparatrices = await _context.Reparatrices.OrderBy(r => r.Title).ToListAsync();
        ViewBag.Esthetiques = await _context.Esthetiques.OrderBy(e => e.Title).ToListAsync();
        TempData["ErrorMessage"] = "Veuillez remplir correctement tous les champs obligatoires (*).";
        return View(model);
    }

    public async Task<IActionResult> ProcedureDetail(string id)
    {
        var slug = id ?? "rhinoplastie";
        ViewBag.ProcedureId = slug;

        // Try to find in Chirurgies first
        var chirurgie = await _context.Chirurgies.FirstOrDefaultAsync(c => c.Slug == slug);
        if (chirurgie != null)
        {
            ViewBag.ProcedureType = "chirurgie";
            return View(chirurgie);
        }

        // Try in Reparatrice
        var reparatrice = await _context.Reparatrices.FirstOrDefaultAsync(r => r.Slug == slug);
        if (reparatrice != null)
        {
            ViewBag.ProcedureType = "chirurgie";
            return View(new Chirurgie
            {
                Id = reparatrice.Id,
                Slug = reparatrice.Slug,
                Title = reparatrice.Title,
                Category = reparatrice.Category,
                Subtitle = reparatrice.Subtitle,
                ImageUrl = reparatrice.ImageUrl,
                Duree = reparatrice.Duree,
                Anesthesie = reparatrice.Anesthesie,
                Eviction = reparatrice.Eviction,
                Hospitalisation = reparatrice.Hospitalisation,
                Overview = reparatrice.Overview,
                Indications = reparatrice.Indications,
                Steps = reparatrice.Steps,
                Faqs = reparatrice.Faqs
            });
        }

        // Try in Esthetique
        var esthetique = await _context.Esthetiques.FirstOrDefaultAsync(e => e.Slug == slug);
        if (esthetique != null)
        {
            ViewBag.ProcedureType = "esthetique";
            return View(esthetique);
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
