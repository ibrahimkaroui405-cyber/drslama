$baseDir = $PSScriptRoot
$viewsDir = Join-Path $baseDir "DrAbdelfatehSalma\Views"
$layoutPath = Join-Path $viewsDir "Shared\_Layout.cshtml"
$outDir = Join-Path $baseDir "site_html_export"

if (!(Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
}

$layout = Get-Content $layoutPath -Raw -Encoding UTF8

$views = @(
    @{ Name = "Index"; Output = "index.html"; Title = "Accueil" },
    @{ Name = "About"; Output = "about.html"; Title = "CV & Formation" },
    @{ Name = "Chirurgies"; Output = "chirurgies.html"; Title = "Chirurgie Esthétique" },
    @{ Name = "Reparatrice"; Output = "reparatrice.html"; Title = "Chirurgie Réparatrice" },
    @{ Name = "Esthetique"; Output = "esthetique.html"; Title = "Médecine Esthétique" },
    @{ Name = "Resultats"; Output = "resultats.html"; Title = "Avant / Après & Résultats" },
    @{ Name = "Temoignages"; Output = "temoignages.html"; Title = "Témoignages & Actualités" },
    @{ Name = "Contact"; Output = "contact.html"; Title = "Contact & Rendez-vous" }
)

foreach ($v in $views) {
    $vPath = Join-Path $viewsDir ("Home\" + $v.Name + ".cshtml")
    if (Test-Path $vPath) {
        $content = Get-Content $vPath -Raw -Encoding UTF8
        
        # Clean razor blocks
        $contentClean = [System.Text.RegularExpressions.Regex]::Replace($content, '@\{[\s\S]*?\}', '')
        $contentClean = [System.Text.RegularExpressions.Regex]::Replace($contentClean, 'asp-controller="Home"\s+asp-action="(\w+)"', 'href="$1.html"')
        $contentClean = [System.Text.RegularExpressions.Regex]::Replace($contentClean, 'asp-controller="\w+"\s+asp-action="(\w+)"', 'href="$1.html"')
        $contentClean = $contentClean.Replace('~/', './')
        $contentClean = $contentClean.Replace('href="Index.html"', 'href="index.html"')
        $contentClean = $contentClean.Replace('href="About.html"', 'href="about.html"')
        $contentClean = $contentClean.Replace('href="Chirurgies.html"', 'href="chirurgies.html"')
        $contentClean = $contentClean.Replace('href="Reparatrice.html"', 'href="reparatrice.html"')
        $contentClean = $contentClean.Replace('href="Esthetique.html"', 'href="esthetique.html"')
        $contentClean = $contentClean.Replace('href="Temoignages.html"', 'href="temoignages.html"')
        $contentClean = $contentClean.Replace('href="Contact.html"', 'href="contact.html"')
        $contentClean = $contentClean.Replace('href="Resultats.html"', 'href="resultats.html"')
        $contentClean = $contentClean.Replace('@@', '@')

        # Clean layout
        $layoutClean = [System.Text.RegularExpressions.Regex]::Replace($layout, '@\(ViewData\["Title"\] != null \? ViewData\["Title"\] \+ " - " : ""\)', ($v.Title + " - "))
        $layoutClean = [System.Text.RegularExpressions.Regex]::Replace($layoutClean, 'asp-controller="Home"\s+asp-action="(\w+)"', 'href="$1.html"')
        $layoutClean = [System.Text.RegularExpressions.Regex]::Replace($layoutClean, 'asp-controller="\w+"\s+asp-action="(\w+)"', 'href="$1.html"')
        $layoutClean = $layoutClean.Replace('~/', './')
        $layoutClean = $layoutClean.Replace('href="Index.html"', 'href="index.html"')
        $layoutClean = $layoutClean.Replace('href="About.html"', 'href="about.html"')
        $layoutClean = $layoutClean.Replace('href="Chirurgies.html"', 'href="chirurgies.html"')
        $layoutClean = $layoutClean.Replace('href="Reparatrice.html"', 'href="reparatrice.html"')
        $layoutClean = $layoutClean.Replace('href="Esthetique.html"', 'href="esthetique.html"')
        $layoutClean = $layoutClean.Replace('href="Temoignages.html"', 'href="temoignages.html"')
        $layoutClean = $layoutClean.Replace('href="Contact.html"', 'href="contact.html"')
        $layoutClean = $layoutClean.Replace('href="Resultats.html"', 'href="resultats.html"')
        $layoutClean = $layoutClean.Replace('@@', '@')
        $layoutClean = [System.Text.RegularExpressions.Regex]::Replace($layoutClean, '@await RenderSectionAsync\([^\)]*\)', '')

        # Inject into main
        $finalHtml = [System.Text.RegularExpressions.Regex]::Replace($layoutClean, '<main>[\s\S]*?@RenderBody\(\)[\s\S]*?</main>', ("<main>`n" + $contentClean + "`n</main>"))
        
        $destFile = Join-Path $outDir $v.Output
        [System.IO.File]::WriteAllText($destFile, $finalHtml, [System.Text.Encoding]::UTF8)
        Write-Host "Generated: $destFile"
    }
}

# Copy Assets
$imgSource = Join-Path $baseDir "DrAbdelfatehSalma\wwwroot\images"
$imgDest = Join-Path $outDir "images"
if (Test-Path $imgSource) {
    Copy-Item -Recurse -Force $imgSource $imgDest
    Write-Host "Copied images"
}

$vidSource = Join-Path $baseDir "DrAbdelfatehSalma\wwwroot\videos"
$vidDest = Join-Path $outDir "videos"
if (Test-Path $vidSource) {
    Copy-Item -Recurse -Force $vidSource $vidDest
    Write-Host "Copied videos"
}

$favSource = Join-Path $baseDir "DrAbdelfatehSalma\wwwroot\favicon.ico"
if (Test-Path $favSource) {
    Copy-Item -Force $favSource (Join-Path $outDir "favicon.ico")
}

Write-Host "EXPORT COMPLETED SUCCESSFULLY!"
