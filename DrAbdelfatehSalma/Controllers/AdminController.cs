using System.Security.Claims;
using DrAbdelfatehSalma.Data;
using DrAbdelfatehSalma.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrAbdelfatehSalma.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ============================================
    // 1. AUTHENTICATION (LOGIN / LOGOUT)
    // ============================================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl)
    {
        // Simple secure admin credentials check
        if ((username == "admin" || username == "drslama") && (password == "slama2026" || password == "admin123"))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Dr. Abdelfateh Slama"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        ViewData["ErrorMessage"] = "Identifiant ou mot de passe incorrect.";
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ============================================
    // 2. DASHBOARD
    // ============================================
    [Authorize]
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveNav"] = "Dashboard";
        ViewBag.TotalChirurgies = await _context.Chirurgies.CountAsync();
        ViewBag.TotalEsthetique = await _context.Esthetiques.CountAsync();
        ViewBag.TotalResultats = await _context.Resultats.CountAsync();
        ViewBag.TotalTemoignages = await _context.Temoignages.CountAsync();
        ViewBag.TotalActualites = await _context.Actualites.CountAsync();
        ViewBag.PendingRendezVous = await _context.DemandesContact.CountAsync(d => !d.IsProcessed);

        var recentRdv = await _context.DemandesContact
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(recentRdv);
    }

    // ============================================
    // 3. CRUD: CHIRURGIES
    // ============================================
    [Authorize]
    public async Task<IActionResult> Chirurgies()
    {
        ViewData["ActiveNav"] = "Chirurgies";
        var list = await _context.Chirurgies.ToListAsync();
        return View(list);
    }

    private async Task<List<string>> GetChirurgieCategoriesAsync()
    {
        var defaults = new List<string> { "VISAGE & COU", "POITRINE", "SILHOUETTE" };
        var dbCategories = await _context.Chirurgies
            .Select(c => c.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToListAsync();

        foreach (var cat in dbCategories)
        {
            if (!defaults.Any(d => d.Equals(cat, StringComparison.OrdinalIgnoreCase)))
            {
                defaults.Add(cat);
            }
        }
        return defaults;
    }

    private async Task<List<string>> GetReparatriceCategoriesAsync()
    {
        var defaults = new List<string> { "MALFORMATIONS", "SÉQUELLES DE TRAUMATISMES", "TUMEURS" };
        var dbCategories = await _context.Reparatrices
            .Select(c => c.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToListAsync();

        foreach (var cat in dbCategories)
        {
            if (!defaults.Any(d => d.Equals(cat, StringComparison.OrdinalIgnoreCase)))
            {
                defaults.Add(cat);
            }
        }
        return defaults;
    }

    [Authorize]
    public async Task<IActionResult> CreateChirurgie()
    {
        ViewData["ActiveNav"] = "Chirurgies";
        ViewBag.ExistingCategories = await GetChirurgieCategoriesAsync();
        return View(new Chirurgie());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChirurgie(Chirurgie model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Chirurgies";

        if (imageFile != null && imageFile.Length > 0)
        {
            model.ImageUrl = await UploadFileAsync(imageFile);
        }

        model.Title ??= string.Empty;
        model.Slug = string.IsNullOrEmpty(model.Slug) ? model.Title.ToLower().Replace(" ", "-") : model.Slug;
        model.Category ??= string.Empty;
        model.Subtitle ??= string.Empty;
        model.ImageUrl ??= string.Empty;
        model.Duree ??= string.Empty;
        model.Anesthesie ??= string.Empty;
        model.Eviction ??= string.Empty;
        model.Hospitalisation ??= string.Empty;
        model.Overview ??= string.Empty;
        model.Indications ??= string.Empty;
        model.Steps ??= string.Empty;
        model.Faqs ??= string.Empty;

        _context.Chirurgies.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Procédure de chirurgie ajoutée avec succès !";
        return RedirectToAction("Chirurgies");
    }

    [Authorize]
    public async Task<IActionResult> EditChirurgie(int id)
    {
        ViewData["ActiveNav"] = "Chirurgies";
        var item = await _context.Chirurgies.FindAsync(id);
        if (item == null) return NotFound();
        ViewBag.ExistingCategories = await GetChirurgieCategoriesAsync();
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditChirurgie(Chirurgie model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Chirurgies";
        var existing = await _context.Chirurgies.FindAsync(model.Id);
        if (existing == null) return NotFound();

        if (imageFile != null && imageFile.Length > 0)
        {
            existing.ImageUrl = await UploadFileAsync(imageFile);
        }
        else if (!string.IsNullOrEmpty(model.ImageUrl))
        {
            existing.ImageUrl = model.ImageUrl;
        }

        existing.Title = model.Title;
        existing.Slug = model.Slug;
        existing.Category = model.Category;
        existing.Subtitle = model.Subtitle;
        existing.Duree = model.Duree;
        existing.Anesthesie = model.Anesthesie;
        existing.Eviction = model.Eviction;
        existing.Hospitalisation = model.Hospitalisation;
        existing.Overview = model.Overview;
        existing.Indications = model.Indications;
        existing.Steps = model.Steps;
        existing.Faqs = model.Faqs;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Chirurgie mise à jour avec succès !";
        return RedirectToAction("Chirurgies");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChirurgie(int id)
    {
        var item = await _context.Chirurgies.FindAsync(id);
        if (item != null)
        {
            _context.Chirurgies.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Procédure de chirurgie supprimée.";
        }
        return RedirectToAction("Chirurgies");
    }

    // ============================================
    // 3.5. CRUD: REPARATRICE
    // ============================================
    [Authorize]
    public async Task<IActionResult> Reparatrice()
    {
        ViewData["ActiveNav"] = "Reparatrice";
        var list = await _context.Reparatrices.ToListAsync();
        return View(list);
    }

    [Authorize]
    public async Task<IActionResult> CreateReparatrice()
    {
        ViewData["ActiveNav"] = "Reparatrice";
        ViewBag.ExistingCategories = await GetReparatriceCategoriesAsync();
        return View(new Reparatrice());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReparatrice(Reparatrice model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Reparatrice";

        if (imageFile != null && imageFile.Length > 0)
        {
            model.ImageUrl = await UploadFileAsync(imageFile);
        }

        model.Title ??= string.Empty;
        model.Slug = string.IsNullOrEmpty(model.Slug) ? model.Title.ToLower().Replace(" ", "-") : model.Slug;
        model.Category ??= string.Empty;
        model.Subtitle ??= string.Empty;
        model.ImageUrl ??= string.Empty;
        model.Duree ??= string.Empty;
        model.Anesthesie ??= string.Empty;
        model.Eviction ??= string.Empty;
        model.Hospitalisation ??= string.Empty;
        model.Overview ??= string.Empty;
        model.Indications ??= string.Empty;
        model.Steps ??= string.Empty;
        model.Faqs ??= string.Empty;

        _context.Reparatrices.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Procédure de chirurgie réparatrice ajoutée avec succès !";
        return RedirectToAction("Reparatrice");
    }

    [Authorize]
    public async Task<IActionResult> EditReparatrice(int id)
    {
        ViewData["ActiveNav"] = "Reparatrice";
        var item = await _context.Reparatrices.FindAsync(id);
        if (item == null) return NotFound();
        ViewBag.ExistingCategories = await GetReparatriceCategoriesAsync();
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReparatrice(Reparatrice model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Reparatrice";
        var existing = await _context.Reparatrices.FindAsync(model.Id);
        if (existing == null) return NotFound();

        if (imageFile != null && imageFile.Length > 0)
        {
            existing.ImageUrl = await UploadFileAsync(imageFile);
        }
        else if (!string.IsNullOrEmpty(model.ImageUrl))
        {
            existing.ImageUrl = model.ImageUrl;
        }

        existing.Title = model.Title;
        existing.Slug = model.Slug;
        existing.Category = model.Category;
        existing.Subtitle = model.Subtitle;
        existing.Duree = model.Duree;
        existing.Anesthesie = model.Anesthesie;
        existing.Eviction = model.Eviction;
        existing.Hospitalisation = model.Hospitalisation;
        existing.Overview = model.Overview;
        existing.Indications = model.Indications;
        existing.Steps = model.Steps;
        existing.Faqs = model.Faqs;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Chirurgie réparatrice mise à jour avec succès !";
        return RedirectToAction("Reparatrice");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReparatrice(int id)
    {
        var item = await _context.Reparatrices.FindAsync(id);
        if (item != null)
        {
            _context.Reparatrices.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Procédure de chirurgie réparatrice supprimée.";
        }
        return RedirectToAction("Reparatrice");
    }

    // ============================================
    // 4. CRUD: ESTHÉTIQUE
    // ============================================
    [Authorize]
    public async Task<IActionResult> Esthetique()
    {
        ViewData["ActiveNav"] = "Esthetique";
        var list = await _context.Esthetiques.ToListAsync();
        return View(list);
    }

    [Authorize]
    public async Task<IActionResult> CreateEsthetique()
    {
        ViewData["ActiveNav"] = "Esthetique";
        ViewBag.ExistingCategories = await _context.Esthetiques
            .Select(e => e.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToListAsync();
        return View(new Esthetique());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEsthetique(Esthetique model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Esthetique";

        if (imageFile != null && imageFile.Length > 0)
        {
            model.ImageUrl = await UploadFileAsync(imageFile);
        }

        model.Title ??= string.Empty;
        model.Slug = string.IsNullOrEmpty(model.Slug) ? model.Title.ToLower().Replace(" ", "-") : model.Slug;
        model.Category ??= string.Empty;
        model.Subtitle ??= string.Empty;
        model.ImageUrl ??= string.Empty;
        model.Duree ??= string.Empty;
        model.Anesthesie ??= string.Empty;
        model.Eviction ??= string.Empty;
        model.Hospitalisation ??= string.Empty;
        model.Overview ??= string.Empty;
        model.Indications ??= string.Empty;
        model.Steps ??= string.Empty;
        model.Faqs ??= string.Empty;

        _context.Esthetiques.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Soin esthétique ajouté avec succès !";
        return RedirectToAction("Esthetique");
    }

    [Authorize]
    public async Task<IActionResult> EditEsthetique(int id)
    {
        ViewData["ActiveNav"] = "Esthetique";
        var item = await _context.Esthetiques.FindAsync(id);
        if (item == null) return NotFound();
        ViewBag.ExistingCategories = await _context.Esthetiques
            .Select(e => e.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToListAsync();
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEsthetique(Esthetique model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Esthetique";
        var existing = await _context.Esthetiques.FindAsync(model.Id);
        if (existing == null) return NotFound();

        if (imageFile != null && imageFile.Length > 0)
        {
            existing.ImageUrl = await UploadFileAsync(imageFile);
        }
        else if (!string.IsNullOrEmpty(model.ImageUrl))
        {
            existing.ImageUrl = model.ImageUrl;
        }

        existing.Title = model.Title;
        existing.Slug = model.Slug;
        existing.Category = model.Category;
        existing.Subtitle = model.Subtitle;
        existing.Duree = model.Duree;
        existing.Anesthesie = model.Anesthesie;
        existing.Eviction = model.Eviction;
        existing.Hospitalisation = model.Hospitalisation;
        existing.Overview = model.Overview;
        existing.Indications = model.Indications;
        existing.Steps = model.Steps;
        existing.Faqs = model.Faqs;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Soin esthétique mis à jour !";
        return RedirectToAction("Esthetique");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEsthetique(int id)
    {
        var item = await _context.Esthetiques.FindAsync(id);
        if (item != null)
        {
            _context.Esthetiques.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Soin esthétique supprimé.";
        }
        return RedirectToAction("Esthetique");
    }

    // ============================================
    // 5. CRUD: RÉSULTATS (AVANT / APRÈS)
    // ============================================
    [Authorize]
    public async Task<IActionResult> Resultats()
    {
        ViewData["ActiveNav"] = "Resultats";
        var list = await _context.Resultats.ToListAsync();
        return View(list);
    }

    private Task<List<CategoryItem>> GetResultatCategoriesAsync()
    {
        var list = new List<CategoryItem>
        {
            new CategoryItem { Key = "visage", Title = "CHIRURGIE DU VISAGE" },
            new CategoryItem { Key = "poitrine", Title = "CHIRURGIE DU SEIN (POITRINE)" },
            new CategoryItem { Key = "silhouette", Title = "CHIRURGIE DE LA SILHOUETTE" }
        };

        return Task.FromResult(list);
    }

    [Authorize]
    public async Task<IActionResult> CreateResultat()
    {
        ViewData["ActiveNav"] = "Resultats";
        ViewBag.ExistingCategories = await GetResultatCategoriesAsync();
        return View(new Resultat());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResultat(Resultat model, IFormFile? beforeFile, IFormFile? afterFile)
    {
        ViewData["ActiveNav"] = "Resultats";

        if (beforeFile != null && beforeFile.Length > 0)
        {
            model.BeforeImageUrl = await UploadFileAsync(beforeFile);
        }

        if (afterFile != null && afterFile.Length > 0)
        {
            model.AfterImageUrl = await UploadFileAsync(afterFile);
        }

        model.Title ??= string.Empty;
        model.CategoryTitle ??= string.Empty;
        if (string.IsNullOrWhiteSpace(model.CategoryKey) && !string.IsNullOrWhiteSpace(model.CategoryTitle))
        {
            model.CategoryKey = model.CategoryTitle.ToLower().Replace(" ", "-");
        }
        model.CategoryKey ??= string.Empty;
        model.Description ??= string.Empty;
        model.BeforeImageUrl ??= string.Empty;
        model.AfterImageUrl ??= string.Empty;
        model.PatientInfo ??= string.Empty;
        model.Technique ??= string.Empty;
        model.Anesthesia ??= string.Empty;
        model.RecoveryTime ??= string.Empty;

        _context.Resultats.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cas de résultat Avant/Après ajouté !";
        return RedirectToAction("Resultats");
    }

    [Authorize]
    public async Task<IActionResult> EditResultat(int id)
    {
        ViewData["ActiveNav"] = "Resultats";
        var item = await _context.Resultats.FindAsync(id);
        if (item == null) return NotFound();
        ViewBag.ExistingCategories = await GetResultatCategoriesAsync();
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResultat(Resultat model, IFormFile? beforeFile, IFormFile? afterFile)
    {
        ViewData["ActiveNav"] = "Resultats";
        var existing = await _context.Resultats.FindAsync(model.Id);
        if (existing == null) return NotFound();

        if (beforeFile != null && beforeFile.Length > 0)
        {
            existing.BeforeImageUrl = await UploadFileAsync(beforeFile);
        }
        else if (!string.IsNullOrEmpty(model.BeforeImageUrl))
        {
            existing.BeforeImageUrl = model.BeforeImageUrl;
        }

        if (afterFile != null && afterFile.Length > 0)
        {
            existing.AfterImageUrl = await UploadFileAsync(afterFile);
        }
        else if (!string.IsNullOrEmpty(model.AfterImageUrl))
        {
            existing.AfterImageUrl = model.AfterImageUrl;
        }

        existing.Title = model.Title;
        existing.CategoryKey = model.CategoryKey;
        existing.CategoryTitle = model.CategoryTitle;
        existing.Description = model.Description;
        existing.PatientInfo = model.PatientInfo;
        existing.Technique = model.Technique;
        existing.Anesthesia = model.Anesthesia;
        existing.RecoveryTime = model.RecoveryTime;
        existing.IsFeatured = model.IsFeatured;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cas de résultat mis à jour !";
        return RedirectToAction("Resultats");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResultat(int id)
    {
        var item = await _context.Resultats.FindAsync(id);
        if (item != null)
        {
            _context.Resultats.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cas de résultat supprimé.";
        }
        return RedirectToAction("Resultats");
    }

    // ============================================
    // 6. CRUD: TÉMOIGNAGES
    // ============================================
    [Authorize]
    public async Task<IActionResult> Temoignages()
    {
        ViewData["ActiveNav"] = "Temoignages";
        var list = await _context.Temoignages.ToListAsync();
        return View(list);
    }

    [Authorize]
    public IActionResult CreateTemoignage()
    {
        ViewData["ActiveNav"] = "Temoignages";
        return View(new Temoignage());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTemoignage(Temoignage model, IFormFile? imageFile, IFormFile? videoFile)
    {
        ViewData["ActiveNav"] = "Temoignages";

        if (imageFile != null && imageFile.Length > 0)
        {
            model.ImageUrl = await UploadFileAsync(imageFile);
        }

        if (videoFile != null && videoFile.Length > 0)
        {
            model.VideoUrl = await UploadFileAsync(videoFile);
        }

        model.VideoUrl ??= string.Empty;
        model.ImageUrl ??= string.Empty;
        model.PatientInfo ??= string.Empty;
        model.Quote ??= string.Empty;
        model.FollowUpTime ??= string.Empty;
        model.Type ??= "text";

        _context.Temoignages.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Témoignage patient ajouté avec succès !";
        return RedirectToAction("Temoignages");
    }

    [Authorize]
    public async Task<IActionResult> EditTemoignage(int id)
    {
        ViewData["ActiveNav"] = "Temoignages";
        var item = await _context.Temoignages.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTemoignage(Temoignage model, IFormFile? imageFile, IFormFile? videoFile)
    {
        ViewData["ActiveNav"] = "Temoignages";
        var existing = await _context.Temoignages.FindAsync(model.Id);
        if (existing == null) return NotFound();

        if (imageFile != null && imageFile.Length > 0)
        {
            existing.ImageUrl = await UploadFileAsync(imageFile);
        }
        else if (!string.IsNullOrEmpty(model.ImageUrl))
        {
            existing.ImageUrl = model.ImageUrl;
        }

        if (videoFile != null && videoFile.Length > 0)
        {
            existing.VideoUrl = await UploadFileAsync(videoFile);
        }
        else if (!string.IsNullOrEmpty(model.VideoUrl))
        {
            existing.VideoUrl = model.VideoUrl;
        }

        existing.PatientName = model.PatientName;
        existing.PatientInfo = model.PatientInfo ?? string.Empty;
        existing.Type = model.Type ?? "text";
        existing.Quote = model.Quote ?? string.Empty;
        existing.Rating = model.Rating;
        existing.FollowUpTime = model.FollowUpTime ?? string.Empty;
        existing.IsFeatured = model.IsFeatured;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Témoignage mis à jour !";
        return RedirectToAction("Temoignages");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTemoignage(int id)
    {
        var item = await _context.Temoignages.FindAsync(id);
        if (item != null)
        {
            _context.Temoignages.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Témoignage supprimé.";
        }
        return RedirectToAction("Temoignages");
    }

    // ============================================
    // 7. DEMANDES DE RENDEZ-VOUS
    // ============================================
    [Authorize]
    public async Task<IActionResult> RendezVous()
    {
        ViewData["ActiveNav"] = "RendezVous";
        var list = await _context.DemandesContact.OrderByDescending(d => d.CreatedAt).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProcessRendezVous(int id)
    {
        var rdv = await _context.DemandesContact.FindAsync(id);
        if (rdv != null)
        {
            rdv.IsProcessed = !rdv.IsProcessed;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = rdv.IsProcessed ? "Demande marquée comme traitée." : "Demande marquée comme non traitée.";
        }
        return RedirectToAction("RendezVous");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRendezVous(int id)
    {
        var rdv = await _context.DemandesContact.FindAsync(id);
        if (rdv != null)
        {
            _context.DemandesContact.Remove(rdv);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Demande de rendez-vous supprimée.";
        }
        return RedirectToAction("RendezVous");
    }

    // ============================================
    // FILE UPLOAD HELPER
    // ============================================
    private async Task<string> UploadFileAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return "/uploads/" + uniqueFileName;
    }

    // ============================================
    // 8. CRUD: ACTUALITÉS
    // ============================================
    [Authorize]
    public async Task<IActionResult> Actualites()
    {
        ViewData["ActiveNav"] = "Actualites";
        var list = await _context.Actualites.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return View(list);
    }

    [Authorize]
    public IActionResult CreateActualite()
    {
        ViewData["ActiveNav"] = "Actualites";
        return View(new Actualite());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateActualite(Actualite model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Actualites";

        if (imageFile != null && imageFile.Length > 0)
        {
            model.ImageUrl = await UploadFileAsync(imageFile);
        }

        model.Title ??= string.Empty;
        model.Excerpt ??= string.Empty;
        model.Content ??= string.Empty;
        model.ImageUrl ??= string.Empty;
        model.Tags ??= string.Empty;
        model.DateLabel ??= string.Empty;
        model.CreatedAt = DateTime.UtcNow;

        _context.Actualites.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Actualité ajoutée avec succès !";
        return RedirectToAction("Actualites");
    }

    [Authorize]
    public async Task<IActionResult> EditActualite(int id)
    {
        ViewData["ActiveNav"] = "Actualites";
        var item = await _context.Actualites.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditActualite(Actualite model, IFormFile? imageFile)
    {
        ViewData["ActiveNav"] = "Actualites";
        var existing = await _context.Actualites.FindAsync(model.Id);
        if (existing == null) return NotFound();

        if (imageFile != null && imageFile.Length > 0)
        {
            existing.ImageUrl = await UploadFileAsync(imageFile);
        }
        else if (!string.IsNullOrEmpty(model.ImageUrl))
        {
            existing.ImageUrl = model.ImageUrl;
        }

        existing.Title = model.Title ?? string.Empty;
        existing.Excerpt = model.Excerpt ?? string.Empty;
        existing.Content = model.Content ?? string.Empty;
        existing.Tags = model.Tags ?? string.Empty;
        existing.DateLabel = model.DateLabel ?? string.Empty;
        existing.IsFeatured = model.IsFeatured;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Actualité mise à jour avec succès !";
        return RedirectToAction("Actualites");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteActualite(int id)
    {
        var item = await _context.Actualites.FindAsync(id);
        if (item != null)
        {
            _context.Actualites.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Actualité supprimée.";
        }
        return RedirectToAction("Actualites");
    }
}

public class CategoryItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

