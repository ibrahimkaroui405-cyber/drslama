using DrAbdelfatehSalma.Models;
using Microsoft.EntityFrameworkCore;

namespace DrAbdelfatehSalma.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // Ensure Reparatrices table exists in SQLite DB
        try
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Reparatrices"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""Slug"" TEXT NOT NULL,
                    ""Title"" TEXT NOT NULL,
                    ""Category"" TEXT NOT NULL,
                    ""Subtitle"" TEXT NULL,
                    ""ImageUrl"" TEXT NULL,
                    ""Duree"" TEXT NULL,
                    ""Anesthesie"" TEXT NULL,
                    ""Eviction"" TEXT NULL,
                    ""Hospitalisation"" TEXT NULL,
                    ""Overview"" TEXT NULL,
                    ""Indications"" TEXT NULL,
                    ""Steps"" TEXT NULL,
                    ""Faqs"" TEXT NULL
                );
            ");
        }
        catch { }

        try
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Actualites"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""Title"" TEXT NOT NULL,
                    ""Excerpt"" TEXT NULL,
                    ""Content"" TEXT NULL,
                    ""Category"" TEXT NOT NULL DEFAULT 'congres',
                    ""ImageUrl"" TEXT NULL,
                    ""Tags"" TEXT NULL,
                    ""DateLabel"" TEXT NULL,
                    ""IsFeatured"" INTEGER NOT NULL DEFAULT 0,
                    ""CreatedAt"" TEXT NOT NULL DEFAULT (datetime('now'))
                );
            ");
        }
        catch { }

        // Ensure new columns in DemandesContact exist in SQLite safely
        try
        {
            var conn = context.Database.GetDbConnection();
            bool wasClosed = conn.State != System.Data.ConnectionState.Open;
            if (wasClosed) conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(\"DemandesContact\");";
                var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var colName = reader["name"]?.ToString();
                        if (!string.IsNullOrEmpty(colName)) columns.Add(colName);
                    }
                }

                void AddColIfNotExists(string name, string typeDef)
                {
                    if (!columns.Contains(name))
                    {
                        using var alterCmd = conn.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE \"DemandesContact\" ADD COLUMN \"{name}\" {typeDef};";
                        alterCmd.ExecuteNonQuery();
                    }
                }

                AddColIfNotExists("Nom", "TEXT NULL");
                AddColIfNotExists("Prenom", "TEXT NULL");
                AddColIfNotExists("Indicatif", "TEXT NULL");
                AddColIfNotExists("Objet", "TEXT NULL");
                AddColIfNotExists("PhotoPath", "TEXT NULL");
                AddColIfNotExists("ConsentAccepted", "INTEGER NOT NULL DEFAULT 1");
            }
            if (wasClosed) conn.Close();
        }
        catch { }

        // Automatic Migration / Cleanup of existing category names & video testimonial URLs
        var existingChirurgies = context.Chirurgies.ToList();
        foreach (var c in existingChirurgies)
        {
            if (c.Category == "CHIRURGIE DU VISAGE & DU COU") c.Category = "VISAGE & COU";
            if (c.Category == "CHIRURGIE DE LA POITRINE") c.Category = "POITRINE";
            if (c.Category == "RECONSTRUCTRICE")
            {
                context.Chirurgies.Remove(c);
            }
        }
        // Ensure strictly ONLY the 3 cards exist in the database (no 4th card, no auto-generated extras)
        if (!context.Reparatrices.Any())
        {
            context.Reparatrices.AddRange(
                new Reparatrice
                {
                    Slug = "malformations",
                    Title = "Malformations",
                    Category = "MALFORMATIONS",
                    Subtitle = "Surtout des lèvres, du nez et des oreilles.",
                    Overview = "Correction spécialisée des malformations congénitales de la face. Prise en charge des fentes labio-palatines, dysmorphies nasales et otoplasties réparatrices avec rétablissement de la fonction et de la symétrie.",
                    Indications = "Lèvres & Nez;Oreilles;Symétrie",
                    ImageUrl = "/images/rep1.png"
                },
                new Reparatrice
                {
                    Slug = "sequelles-de-traumatismes",
                    Title = "Séquelles de Traumatismes",
                    Category = "SÉQUELLES DE TRAUMATISMES",
                    Subtitle = "Cicatrices et fractures.",
                    Overview = "Traitement réparateur des cicatrices complexes (hypertrophiques, chéloïdes, rétractions cutanées post-brûlures ou post-opératoires) et correction des séquelles de fractures du massif facial et du cou.",
                    Indications = "Reprise de Cicatrice;Fractures;Plasties Z",
                    ImageUrl = "/images/rep2.png"
                },
                new Reparatrice
                {
                    Slug = "tumeurs-du-visage-et-du-cou",
                    Title = "Tumeurs du Visage & Cou",
                    Category = "TUMEURS",
                    Subtitle = "Exérèse et reconstruction.",
                    Overview = "Prise en charge chirurgicale des tumeurs cutanées ou sous-cutanées de la face et du cou, avec reconstruction visant à préserver à la fois la fonction et l'esthétique de la région.",
                    Indications = "Exérèse Cutanée;Lambeaux;Greffes",
                    ImageUrl = "/images/rep3.png"
                }
            );
        }
        else
        {
            // Remove any 4th / extra items from earlier runs
            var allRep = context.Reparatrices.ToList();
            if (allRep.Count > 3)
            {
                var canonical = new[] { "malformations", "sequelles-de-traumatismes", "tumeurs-du-visage-et-du-cou" };
                var extras = allRep.Where(r => !canonical.Any(c => r.Slug.Equals(c, StringComparison.OrdinalIgnoreCase) || r.Title.Contains(c, StringComparison.OrdinalIgnoreCase))).ToList();
                if (extras.Any())
                {
                    context.Reparatrices.RemoveRange(extras);
                }
            }

            var tumeurItem = allRep.FirstOrDefault(r => r.Slug.Contains("tumeur"));
            if (tumeurItem != null)
            {
                tumeurItem.Subtitle = "Exérèse et reconstruction.";
                tumeurItem.Overview = "Prise en charge chirurgicale des tumeurs cutanées ou sous-cutanées de la face et du cou, avec reconstruction visant à préserver à la fois la fonction et l'esthétique de la région.";
            }
        }
        var existingEsthetiques = context.Esthetiques.ToList();
        foreach (var e in existingEsthetiques)
        {
            if (e.Category == "MÉDECINE ESTHÉTIQUE") e.Category = "INJECTIONS";
        }
        var dummyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Caroline B.", "Sophie R.", "Marc B.", "Élodie P.", "Nadia M." };
        var dummyTemoignages = context.Temoignages.Where(t => dummyNames.Contains(t.PatientName)).ToList();
        if (dummyTemoignages.Any())
        {
            context.Temoignages.RemoveRange(dummyTemoignages);
            context.SaveChanges();
        }

        // 1. Seed / Upsert Chirurgies
        var defaultChirurgies = new List<Chirurgie>
        {
            // --- a) CHIRURGIE ESTHÉTIQUE DU VISAGE ---
            new Chirurgie
            {
                Slug = "rhinoplastie",
                Title = "Rhinoplastie (Esthétique & Fonctionnelle)",
                Category = "VISAGE & COU",
                Subtitle = "Harmonisation du nez, correction de bosse, affinement de la pointe et libération respiratoire.",
                ImageUrl = "/images/dr_slama_details.png",
                Duree = "2h00 – 2h30",
                Anesthesie = "Générale",
                Eviction = "7 à 10 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• Le nez est le reflet de la personnalité et il est souvent une source de frustration et de manque de confiance en soi.\n• La rhinoplastie esthétique (enlever une bosse, affiner la pointe et les ailes narinaires) permet d’harmoniser et d’équilibrer le nez.\n• La rhinoplastie fonctionnelle, souvent associée à une septoplastie, permet de libérer la respiration nasale et corrige également tous les défauts d’origine traumatique.",
                Indications = "Correction de bosse ou pointe large;Asymétrie et déviation du septum nasal;Gêne respiratoire fonctionnelle (Septoplastie);Séquelles traumatiques nasales",
                Steps = "Consultation initiale & Simulation morphométrique;Chirurgie sous anesthésie générale en bloc stérile;Pose d'attelle légère de protection;Suivi post-opératoire rigoureux à Sousse",
                Faqs = "Quelle est la durée d'hospitalisation ? 24 heures en clinique privée agréée.;Quelle est la durée de convalescence ? 7 à 10 jours."
            },
            new Chirurgie
            {
                Slug = "lifting-cervico-facial",
                Title = "Lifting Cervico-Facial",
                Category = "VISAGE & COU",
                Subtitle = "Rajeunissement global de la face et du cou, redonnant fermeté et vitalité naturelle.",
                ImageUrl = "/images/dr_slama_portrait.png",
                Duree = "3h00 – 4h00",
                Anesthesie = "Générale ou Locale assistée",
                Eviction = "10 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• Le lifting de la face s’adresse aux personnes qui présentent des signes d’un vieillissement excessif ou prématuré.\n• Il permet de redonner au visage une apparence jeune et pleine de vitalité.\n• On distingue le lifting frontal, temporal, facial et cervical. Il peut être associé à une blépharoplastie, une Bichectomie ou à une lipoaspiration du cou et du menton.",
                Indications = "Relâchement des ovales et bajoues;Rides et fanons du cou;Perte de vitalité faciale;Recherche d'un rajeunissement naturel",
                Steps = "Bilan anatomique personnalisé;Repositionnement des structures musculaires et cutanées;Sutures fines et discrètes;Suivi post-opératoire à Sousse",
                Faqs = "Quelles interventions peuvent être associées ? Blépharoplastie, bichectomie ou lipoaspiration du cou.;Durée d'hospitalisation ? 24h d'hospitalisation."
            },
            new Chirurgie
            {
                Slug = "blepharoplastie",
                Title = "Blépharoplastie (Lifting des Paupières)",
                Category = "VISAGE & COU",
                Subtitle = "Correction du regard fatigué, élimination des poches et résection de la peau tombante.",
                ImageUrl = "/images/dr_slama_consultation.png",
                Duree = "1h00 – 1h30",
                Anesthesie = "Locale",
                Eviction = "7 à 8 jours",
                Hospitalisation = "Hôpital de jour (Ambulatoire)",
                Overview = "• La blépharoplastie ou lifting des paupières s’adresse aux personnes qui ont un regard fatigué ou des poches sous les yeux.\n• La blépharoplastie peut concerner les paupières supérieures et/ou inférieures et peut être associée à un lifting des sourcils.\n• Après blépharoplastie, le regard sera rajeuni et embelli.",
                Indications = "Paupières supérieures lourdes ou tombantes;Poches graisseuses sous les yeux;Regard fatigué ou vieilli",
                Faqs = "Quelle est l'anesthésie utilisée ? Anesthésie locale à l'hôpital de jour.;Combien de temps dure la convalescence ? 7 à 8 jours."
            },
            new Chirurgie
            {
                Slug = "genioplastie",
                Title = "Génioplastie (Chirurgie du Menton)",
                Category = "VISAGE & COU",
                Subtitle = "Harmonisation du profil et correction du menton fuyant, saillant ou dévié.",
                ImageUrl = "/images/dr_slama_rigueur_elegance.png",
                Duree = "1h30",
                Anesthesie = "Générale",
                Eviction = "7 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• Le menton doit être en équilibre et en harmonie avec les lèvres, le nez et les joues.\n• La génioplastie corrige un menton fuyant, saillant ou dévié pour rétablir une parfaite profiloplastie.",
                Indications = "Menton fuyant (rétrognathie);Menton saillant (prognathie);Asymétrie ou déviation du menton",
                Faqs = "Durée d'hospitalisation ? 24 heures d'hospitalisation.;Convalescence ? Environ 7 jours."
            },
            new Chirurgie
            {
                Slug = "otoplastie",
                Title = "Otoplastie (Oreilles Décollées)",
                Category = "VISAGE & COU",
                Subtitle = "Correction de l'aspect décollé des oreilles pour un résultat symétrique et naturel.",
                ImageUrl = "/images/hero_results_doctor_face.png",
                Duree = "1h00",
                Anesthesie = "Générale ou Locale (selon l'âge)",
                Eviction = "5 à 7 jours",
                Hospitalisation = "12 à 24h d'hospitalisation",
                Overview = "• Cette chirurgie intéresse surtout les enfants et adultes qui présentent un aspect décollé des pavillons des oreilles.\n• L’otoplastie permet d’obtenir des oreilles symétriques, de taille et d’aspect naturel.",
                Indications = "Oreilles décollées chez l'enfant ou l'adulte;Asymétrie des pavillons auriculaires",
                Faqs = "À quel âge peut-on réaliser une otoplastie ? Dès l'âge de 7 ans (lorsque le pavillon a atteint sa croissance quasi-définitive)."
            },

            // --- b) CHIRURGIE ESTHÉTIQUE DES SEINS ---
            new Chirurgie
            {
                Slug = "augmentation-mammaire",
                Title = "Augmentation Mammaire",
                Category = "POITRINE",
                Subtitle = "Pose de prothèses ou lipofilling pour redonner volume, galbe et féminité.",
                ImageUrl = "/images/medical_mammaire.png",
                Duree = "1h30",
                Anesthesie = "Générale",
                Eviction = "7 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• L’augmentation mammaire permet de redonner du volume à la poitrine symbole de la féminité.\n• Cette chirurgie s’adresse à des seins petits, hypotrophiques ou qui ont diminué de taille après les grossesses.\n• L’augmentation mammaire permet à la femme de se refaire les seins grâce à la pose de prothèses mammaires ou l’injection de graisses (lipofilling des seins).",
                Indications = "Hypotrophie mammaire (seins trop petits);Perte de volume après grossesse ou amaigrissement;Asymétrie mammaire",
                Faqs = "Prothèses ou Lipofilling ? Le choix est personnalisé lors de la consultation selon votre morphologie.;Hospitalisation ? 24 heures d'hospitalisation."
            },
            new Chirurgie
            {
                Slug = "lifting-mammaire",
                Title = "Lifting des Seins (Mastopexie)",
                Category = "POITRINE",
                Subtitle = "Correction de la ptose mammaire pour retrouver une poitrine ferme, galbée et bien positionnée.",
                ImageUrl = "/images/dr_slama_operating_suite.png",
                Duree = "2h00 – 2h30",
                Anesthesie = "Générale",
                Eviction = "7 à 10 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• Les seins qui tombent et les mamelons qui sont bas situés par rapport à la normale présentent un aspect disgracieux.\n• Un lifting mammaire permet de leur redonner un aspect galbé et ferme.\n• Le lifting des seins peut se faire, selon les cas, avec ou sans prothèses mammaires.",
                Indications = "Ptose mammaire (seins tombants);Mamelons abaissés;Relâchement après grossesse ou perte de poids",
                Faqs = "Peut-on associer des prothèses ? Oui, si un manque de volume supérieur est présent.;Convalescence ? 7 à 10 jours."
            },
            new Chirurgie
            {
                Slug = "reduction-mammaire",
                Title = "Réduction Mammaire",
                Category = "POITRINE",
                Subtitle = "Allègement du volume de la poitrine, soulagement des douleurs dorsales et remodelage.",
                ImageUrl = "/images/ch1.webp",
                Duree = "2h30 – 3h00",
                Anesthesie = "Générale",
                Eviction = "10 à 12 jours",
                Hospitalisation = "48h d'hospitalisation",
                Overview = "• La chirurgie de réduction du volume mammaire est pratiquée chez les femmes aux poitrines fortes ou lourdes qui éprouvent des inconforts esthétiques et physiques, y compris des douleurs au niveau du cou et du dos.\n• Cette hypertrophie mammaire est souvent associée à une ptose des seins.\n• Au cours de cette chirurgie, on réduit le volume et on traite la ptose mammaire. Le résultat est immédiat et naturel.",
                Indications = "Hypertrophie mammaire gênante;Douleurs dorsales, cervicales ou aux épaules;Gêne dans le sport et le vestimentaire",
                Faqs = "Quel est le temps d'hospitalisation ? 48 heures d'hospitalisation.;Durée de convalescence ? 10 à 12 jours."
            },
            new Chirurgie
            {
                Slug = "gynecomastie",
                Title = "Gynécomastie (Chirurgie du Torse Homme)",
                Category = "POITRINE",
                Subtitle = "Correction du développement mammaire chez l'homme pour un thorax plat et masculin.",
                ImageUrl = "/images/hero_results_surgical.png",
                Duree = "1h30",
                Anesthesie = "Générale",
                Eviction = "7 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• Chez l’homme, l’hypertrophie des seins est à l’origine d’inconforts et de complexes.\n• Cette hypertrophie peut être glandulaire et/ou graisseuse et la chirurgie s’adresse à la gynécomastie d’origine idiopathique.\n• Après traitement chirurgical de la gynécomastie, le patient retrouve un thorax plat et harmonieux.",
                Indications = "Excès glandulaire ou graisseux au niveau du torse masculin;Complexe vestimentaire chez l'homme",
                Faqs = "L'opération laisse-t-elle des cicatrices visibles ? Les incisions sont minimes autour de l'aréole et s'estompent rapidement."
            },

            // --- c) CHIRURGIE ESTHÉTIQUE DE LA SILHOUETTE ---
            new Chirurgie
            {
                Slug = "liposuccion",
                Title = "Liposuccion & Liposculpture",
                Category = "SILHOUETTE",
                Subtitle = "Aspiration ciblée des surcharges graisseuses (ventre, hanches, cuisses, bras, menton).",
                ImageUrl = "/images/medical_corps.png",
                Duree = "1h30 – 2h30",
                Anesthesie = "Générale ou Locale",
                Eviction = "5 à 7 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• La liposuccion ou lipoaspiration ou liposculpture intéresse l’excès de graisse jugé disgracieux surtout au niveau du ventre, hanches, cuisses, genoux, bras, dos.\n• Le résultat est immédiat et on peut s’habiller en toute liberté et avec une nouvelle assurance.",
                Indications = "Graisse localisée résistante au régime (ventre, culotte de cheval, hanches, cuisses, bras);Sculpture des contours corporels",
                Faqs = "Le résultat est-il immédiat ? Oui, le galbe se dessine dès la résorption de l'œdème.;Hospitalisation ? 24h d'hospitalisation."
            },
            new Chirurgie
            {
                Slug = "abdominoplastie",
                Title = "Abdominoplastie (Lifting du Ventre)",
                Category = "SILHOUETTE",
                Subtitle = "Remodelage complet de la paroi abdominale, retension cutanée et musculaire.",
                ImageUrl = "/images/ch4.jpg",
                Duree = "2h30 – 3h00",
                Anesthesie = "Générale",
                Eviction = "10 à 12 jours",
                Hospitalisation = "48h d'hospitalisation",
                Overview = "• C’est un lifting du ventre qui permet de retendre la paroi abdominale proéminente voire tombante.\n• Elle vise à corriger les effets combinés de l’âge, les grossesses et les séquelles d’amaigrissement.\n• Elle est souvent associée à une liposuccion du ventre. Après abdominoplastie, on peut s’afficher en maillot de bain avec un ventre plat et lisse.",
                Indications = "Relâchement cutané abdominal;Tablier abdominal après grossesses ou perte de poids importante;Diastasis (écartement des muscles abdominaux)",
                Faqs = "Hospitalisation nécessaire ? 48h d'hospitalisation en clinique partenaire à Sousse.;Temps de repos ? 10 à 12 jours."
            },
            new Chirurgie
            {
                Slug = "lifting-bras-cuisses",
                Title = "Lifting des Bras et des Cuisses",
                Category = "SILHOUETTE",
                Subtitle = "Raffermissement cutané et élimination des excès de peau aux bras (Brachioplastie) et cuisses.",
                ImageUrl = "/images/silhouette_corps.jpg",
                Duree = "2h00 – 2h30",
                Anesthesie = "Générale",
                Eviction = "8 à 10 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• La peau des bras et des cuisses se distend avec l’âge, le surpoids et après un régime.\n• En pratiquant un lifting, associé souvent à une liposuccion, les bras et les cuisses retrouvent un aspect fin et harmonieux.",
                Indications = "Peau fripée ou tombante à la face interne des bras ou des cuisses;Relâchement post-régime",
                Faqs = "Est-ce associé à une liposuccion ? Oui, très fréquemment pour affiner les volumes."
            },
            new Chirurgie
            {
                Slug = "augmentation-fesses",
                Title = "Augmentation des Fesses (BBL & Prothèses)",
                Category = "SILHOUETTE",
                Subtitle = "Galbe et volume des fesses par injection de graisse (Lipofilling / BBL) ou prothèses.",
                ImageUrl = "/images/dr_slama_lounge.png",
                Duree = "2h00 – 2h30",
                Anesthesie = "Générale",
                Eviction = "7 à 10 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "Cette chirurgie s’adresse à des fesses plates ou trop minces. Le lipofilling des fesses (injection de graisse, connu sous le nom de BBL) ou la pose de prothèses permettent de redonner de la forme et retrouver une silhouette galbée.",
                Indications = "Fesses plates ou hypoplasiques;Perte de galbe fessier;Recherche d'une silhouette sculptée et féminine",
                Faqs = "Qu'est-ce que le BBL ? Le Brazilian Butt Lift utilise votre propre graisse purifiée extraite par liposuccion."
            }
        };

        foreach (var item in defaultChirurgies)
        {
            if (!context.Chirurgies.Any(c => c.Slug == item.Slug))
            {
                context.Chirurgies.Add(item);
            }
        }
        context.SaveChanges();

        // 2. Seed / Upsert Reparatrices
        var defaultReparatrices = new List<Reparatrice>
        {
            new Reparatrice
            {
                Slug = "malformations-levres-nez-oreilles",
                Title = "Malformations : surtout des lèvres, du nez et des oreilles",
                Category = "MALFORMATIONS",
                Subtitle = "Prise en charge réparatrice des malformations congénitales surtout des lèvres, du nez et des oreilles.",
                ImageUrl = "/images/chergievisage.jpg",
                Duree = "2h00 – 3h00",
                Anesthesie = "Générale",
                Eviction = "10 à 14 jours",
                Hospitalisation = "24h à 48h d'hospitalisation",
                Overview = "• Prise en charge réparatrice des malformations congénitales (surtout des lèvres, du nez et des oreilles).\n• Utilisation de techniques de chirurgie réparatrice (lambeaux et greffes) permettant de rétablir la fonction et l'esthétique.",
                Indications = "Malformations des lèvres (fente labiale);Malformations du nez et des oreilles",
                Faqs = "À quel âge intervenir ? La prise en charge est évaluée lors de la consultation selon le type de malformation."
            },
            new Reparatrice
            {
                Slug = "sequelles-de-traumatismes-cicatrices-fractures",
                Title = "Séquelles de traumatismes : cicatrices et fractures",
                Category = "SÉQUELLES DE TRAUMATISMES",
                Subtitle = "Traitement réparateur des séquelles de traumatismes : cicatrices complexes et fractures.",
                ImageUrl = "/images/drwork.png",
                Duree = "1h00 – 2h00",
                Anesthesie = "Locale ou Générale",
                Eviction = "5 à 7 jours",
                Hospitalisation = "Ambulatoire (Jour Même)",
                Overview = "• Correction et reprise plastique des séquelles de traumatismes (cicatrices complexes, hypertrophiques et fractures).\n• Technique de chirurgie réparatrice pour rétablir à la fois la fonction défaillante et l'esthétique des régions réparées.",
                Indications = "Cicatrices complexes ou rétractiles;Séquelles de traumatismes faciaux et fractures",
                Faqs = "Est-ce que la cicatrice s'estompe ? La chirurgie plastique permet de rendre la cicatrice très fine, souple et discrète."
            },
            new Reparatrice
            {
                Slug = "tumeurs-du-visage-et-du-cou",
                Title = "Tumeurs du visage et du cou",
                Category = "TUMEURS",
                Subtitle = "Exérèse chirurgicale et reconstruction des tumeurs du visage et du cou par lambeaux et greffes.",
                ImageUrl = "/images/dr_slama_details.png",
                Duree = "1h30 – 2h30",
                Anesthesie = "Générale ou Locale",
                Eviction = "7 à 10 jours",
                Hospitalisation = "24h d'hospitalisation",
                Overview = "• Traitement et exérèse des tumeurs du visage et du cou.\n• Reconstruction immédiate par des techniques de chirurgie réparatrice (des lambeaux et des greffes) pour rétablir la fonction et l'esthétique.",
                Indications = "Exérèse de tumeurs cutanées avec reconstruction;Reconstruction plastique par lambeaux et greffes",
                Faqs = "Comment est assurée la reconstruction ? Par des lambeaux locaux ou greffes pour préserver à la fois la fonction et l'esthétique."
            }
        };

        foreach (var item in defaultReparatrices)
        {
            if (!context.Reparatrices.Any(r => r.Slug == item.Slug))
            {
                context.Reparatrices.Add(item);
            }
        }
        context.SaveChanges();

        // 2. Seed / Upsert Esthetique
        var defaultEsthetiques = new List<Esthetique>
        {
            new Esthetique
            {
                Slug = "lipofilling-du-visage",
                Title = "Lipofilling du Visage (MiliFat, MicroFat, NanoFat)",
                Category = "LIPOFILLING",
                Subtitle = "Technique de comblement par réinjection de graisse autologue préparée.",
                ImageUrl = "/images/medcineesththique.jpg",
                Duree = "45 min – 1h15",
                Anesthesie = "Anesthésie locale",
                Eviction = "1 à 3 jours de convalescence",
                Hospitalisation = "Cabinet / Ambulatoire",
                Overview = "• Le lipofilling est une technique de comblement et consiste à injecter de la graisse (après préparation) prélevée sur votre corps.\n• Le lipofilling du visage permet de corriger les pommettes plates, les lèvres trop minces et aussi les cernes de la paupière inférieure.\n• Il peut être associé à un lifting ou à une blépharoplastie.\n• Il existe plusieurs techniques de lipofilling du visage : MiliFat, MicroFat, NanoFat.\n• Le lipofilling du visage est pratiqué sous anesthésie locale et nécessite une convalescence de 1 à 3 jours.",
                Indications = "Pommettes plates;Lèvres trop minces;Cernes de la paupière inférieure;Association possible à un lifting ou une blépharoplastie",
                Steps = "Prélèvement minutieux de la graisse autologue;Préparation et purification (MiliFat, MicroFat, NanoFat);Réinjection de précision au niveau des volumes du visage",
                Faqs = "Quelle est la durée de convalescence ? De 1 à 3 jours.;Quel type d'anesthésie ? Réalisé sous anesthésie locale."
            },
            new Esthetique
            {
                Slug = "fils-tenseurs-visage",
                Title = "Fils Tenseurs (Lifting Sans Cicatrice)",
                Category = "LES FILS TENSEURS",
                Subtitle = "Rajeunissement du visage et lifting sans cicatrice des régions temporale, jugale et cervicale.",
                ImageUrl = "/images/hero_aesthetic_doctor_face.png",
                Duree = "30 – 45 min",
                Anesthesie = "Anesthésie locale",
                Eviction = "1 à 2 jours de convalescence",
                Hospitalisation = "Cabinet Médical",
                Overview = "• C’est une technique de rajeunissement du visage qui utilise des fils tenseurs résorbables ou non.\n• Les fils tenseurs permettent de réaliser un lifting, sans cicatrice, des régions temporale, jugale et cervicale.\n• Le résultat est naturel et le visage retrouve un aspect jeune et sain.\n• Cette technique est pratiquée sous anesthésie locale et nécessite une convalescence de 1 à 2 jours.",
                Indications = "Relâchement des régions temporale, jugale et cervicale;Lifting sans cicatrice du visage et du cou;Recherche d'un aspect jeune, naturel et sain",
                Steps = "Repérage des vecteurs de tension;Insertion indolore des fils sous anesthésie locale;Mise en tension et fixation discrète",
                Faqs = "Combien de jours de convalescence ? Nécessite une convalescence de 1 à 2 jours.;Est-ce naturel ? Le résultat est naturel et le visage retrouve un aspect jeune et sain."
            },
            new Esthetique
            {
                Slug = "botox-comblement-prp",
                Title = "Injection de Botox, Comblement & PRP",
                Category = "INJECTION DE BOTOX, COMBLEMENT ET PRP",
                Subtitle = "Techniques mini-invasives d'injections au cabinet (Botox, Acide Hyaluronique, PRP).",
                ImageUrl = "/images/hero_aesthetic_medical_noface.png",
                Duree = "20 – 30 min",
                Anesthesie = "Anesthésie locale en cabinet",
                Eviction = "Immédiate (Aucune éviction)",
                Hospitalisation = "Cabinet Médical",
                Overview = "• Ces techniques mini-invasives utilisent des produits résorbables et sont réalisées au cabinet sous anesthésie locale :\n- Le Botox (toxine botulique) : traite les rides du visage surtout du front.\n- L’acide hyaluronique : est un produit de comblement pour traiter les cernes, les imperfections et augmenter le volume (Lèvres, pommettes).\n- PRP (plasmas riches en plaquettes) : présente un effet régénérateur de la peau du visage et des cheveux.",
                Indications = "Botox : Rides du visage surtout du front;Acide Hyaluronique : Comblement des cernes, imperfections et augmentation de volume (Lèvres, pommettes);PRP : Effet régénérateur de la peau du visage et des cheveux",
                Steps = "Bilan et choix du produit résorbable adapté;Injections de haute précision sous anesthésie locale au cabinet;Contrôle immédiat du rendu naturel",
                Faqs = "Où est réalisée l'intervention ? Directement au cabinet sous anesthésie locale.;Quels produits sont utilisés ? Des produits résorbables agréés de première qualité."
            }
        };

        foreach (var item in defaultEsthetiques)
        {
            if (!context.Esthetiques.Any(e => e.Slug == item.Slug))
            {
                context.Esthetiques.Add(item);
            }
            else
            {
                var existing = context.Esthetiques.First(e => e.Slug == item.Slug);
                // Preserve custom ImageUrl set via Admin panel
                if (string.IsNullOrEmpty(existing.ImageUrl))
                {
                    existing.ImageUrl = item.ImageUrl;
                }
            }
        }

        // Also clean up old default categories in existing rows if needed
        var allEsthetiques = context.Esthetiques.ToList();
        foreach (var est in allEsthetiques)
        {
            if (est.Category == "INJECTIONS" || est.Category == "TOXINE BOTULIQUE")
            {
                est.Category = "INJECTION DE BOTOX, COMBLEMENT ET PRP";
            }
            else if (est.Category == "BIOSTIMULATEURS" || est.Category == "SKINBOOSTERS & ÉCLAT")
            {
                est.Category = "LIPOFILLING";
            }
        }
        context.SaveChanges();

        // 3. Seed Resultats
        if (!context.Resultats.Any())
        {
            context.Resultats.AddRange(
                new Resultat
                {
                    CategoryKey = "visage",
                    CategoryTitle = "CHIRURGIE DU VISAGE",
                    Title = "Rhinoplastie Primaire Ultrasonique",
                    Description = "Correction d'une déviation septale avec lissage du profil et projection contrôlée de la pointe. Sculpture piézo-électrique haute précision.",
                    BeforeImageUrl = "/images/rhino_before.png",
                    AfterImageUrl = "/images/rhino_after.png",
                    PatientInfo = "Femme, 29 ans (Sousse)",
                    Technique = "Piezo-Rhinoplastie Ouverte",
                    Anesthesia = "Générale (Ambulatoire)",
                    RecoveryTime = "Résultat final à 8 mois",
                    IsFeatured = true
                },
                new Resultat
                {
                    CategoryKey = "visage",
                    CategoryTitle = "CHIRURGIE DU VISAGE",
                    Title = "Deep Plane Facelift & Cou",
                    Description = "Repositionnement profond des structures musculaires (SMAS) pour un rajeunissement naturel du visage et du cou sans tension cutanée.",
                    BeforeImageUrl = "/images/facelift_before.png",
                    AfterImageUrl = "/images/facelift_after.png",
                    PatientInfo = "Patient, 54 ans (Sousse)",
                    Technique = "Repositionnement SMAS",
                    Anesthesia = "Générale",
                    RecoveryTime = "10 à 14 jours",
                    IsFeatured = false
                },
                new Resultat
                {
                    CategoryKey = "poitrine",
                    CategoryTitle = "CHIRURGIE DU SEIN (POITRINE)",
                    Title = "Augmentation Mammaire Sur-Mesure",
                    Description = "Prothèses ergonomiques haute résilience insérées sous le plan pectoral pour un décolleté harmonieux et naturel.",
                    BeforeImageUrl = "/images/rhino_before.png",
                    AfterImageUrl = "/images/rhino_after.png",
                    PatientInfo = "Femme, 28 ans (Sousse)",
                    Technique = "Prothèses Ergonomiques",
                    Anesthesia = "Générale",
                    RecoveryTime = "7 à 10 jours",
                    IsFeatured = false
                },
                new Resultat
                {
                    CategoryKey = "silhouette",
                    CategoryTitle = "CHIRURGIE DE LA SILHOUETTE",
                    Title = "Liposculpture HD & Remodelage",
                    Description = "Remodelage harmonieux des contours de la silhouette avec affinement précis des lignes anatomiques.",
                    BeforeImageUrl = "/images/injections_before.png",
                    AfterImageUrl = "/images/injections_after.png",
                    PatientInfo = "Femme, 34 ans (Sousse)",
                    Technique = "Liposculpture Haute Définition",
                    Anesthesia = "Générale ou Locale assistée",
                    RecoveryTime = "5 à 7 jours",
                    IsFeatured = false
                }
            );
        }

        var allResultats = context.Resultats.ToList();
        foreach (var res in allResultats)
        {
            if (res.CategoryKey == "rhinoplastie" || res.CategoryKey == "lifting" || res.CategoryTitle.Contains("RHINOPLASTIE") || res.CategoryTitle.Contains("LIFTING") || res.CategoryTitle.Contains("VISAGE"))
            {
                res.CategoryKey = "visage";
                res.CategoryTitle = "CHIRURGIE DU VISAGE";
            }
            else if (res.CategoryKey == "mammaire" || res.CategoryTitle.Contains("MAMMAIRE") || res.CategoryTitle.Contains("POITRINE") || res.CategoryTitle.Contains("SEIN"))
            {
                res.CategoryKey = "poitrine";
                res.CategoryTitle = "CHIRURGIE DU SEIN (POITRINE)";
            }
            else if (res.CategoryKey == "injections" || res.CategoryTitle.Contains("SILHOUETTE") || res.CategoryTitle.Contains("ESTHÉTIQUE"))
            {
                res.CategoryKey = "silhouette";
                res.CategoryTitle = "CHIRURGIE DE LA SILHOUETTE";
            }
        }
        context.SaveChanges();

        // 4. Testimonials are not seeded (ready for real patient reviews in production)

        // 5. Seed Esthetiques
        if (!context.Esthetiques.Any())
        {
            context.Esthetiques.AddRange(
                new Esthetique
                {
                    Slug = "morpheus8",
                    Title = "Morpheus8 & Radiofréquence Fractionnée",
                    Category = "REMODELAGE & DERM-TIGHTENING",
                    Subtitle = "Micro-aiguilles plaquées or et radiofréquence fractionnée pénétrant jusqu'à 8mm.",
                    ImageUrl = "/images/3.png",
                    Duree = "45 min – 1h",
                    Anesthesie = "Crème Anesthésiante",
                    Eviction = "24 à 48h (Rougissements mineurs)",
                    Hospitalisation = "Ambulatoire (Sans hospitalisation)",
                    Overview = "Le Morpheus8 associe le microneedling d'exception et la radiofréquence fractionnée pour retendre la peau relâchée du visage, du cou et du corps, relancer la production de collagène et traiter le double menton.",
                    Indications = "Relâchement cutané du bas du visage et du cou;Rides fines autour des yeux et des lèvres;Cicatrices d'acné et pores dilatés;Double menton et surcharge graisseuse localisée",
                    Steps = "Application d'une crème anesthésiante hautement dosée pendant 45 min;Passage uniforme de la pièce à main Morpheus8 sur les zones cibles;Application d'un sérum apaisant à l'acide hyaluronique;Protocole de soin post-acte à domicile",
                    Faqs = "Est-ce douloureux ? L'application de la crème anesthésiante rend la séance très confortable.;Combien de séances faut-il ? En général 2 à 3 séances espacées de 4 à 6 semaines sont recommandées.;Quand voit-on les résultats ? Un coup d'éclat est visible dès 10 jours, et le remodelage collagénique se poursuit pendant 3 mois."
                },
                new Esthetique
                {
                    Slug = "injections-acide-hyaluronique",
                    Title = "Injections d'Acide Hyaluronique & Profiloplastie",
                    Category = "INJECTIONS & VOLUMÉTRIE",
                    Subtitle = "Comblement des rides, restauration des volumes et harmonisation des lèvres et des pommettes.",
                    ImageUrl = "/images/1.png",
                    Duree = "30 min",
                    Anesthesie = "Locale / Cannule Douce",
                    Eviction = "Immédiate (Reprise d'activité directe)",
                    Hospitalisation = "Sans hospitalisation",
                    Overview = "Les injections d'acide hyaluronique de haute qualité permettent de combler les sillons, redéfinir les pommettes, pulper les lèvres ou réharmoniser le profil (Rhino-médicale) sans chirurgie.",
                    Indications = "Sillons nasogéniens et plis d'amertume;Lèvres fines ou déshydratées;Perte de volume des pommettes et des tempes;Corriger une petite bosse nasale (Rhinoplastie médicale)",
                    Steps = "Analyse morphologique dynamique du visage;Injections ciblées à la micro-cannule souple;Massage sculptant doux de lissage;Contrôle et retouche offerte à 15 jours",
                    Faqs = "Combien de temps dure l'effet ? L'effet dure entre 9 et 18 mois selon la densité du gel.;Le rendu est-il naturel ? Oui, le Dr. Slama favorise une approche 'French Touch' sans surcorrection."
                },
                new Esthetique
                {
                    Slug = "toxine-botulique-botox",
                    Title = "Toxine Botulique (Botox Haute Précision)",
                    Category = "RELAXATION MUSCULAIRE",
                    Subtitle = "Lissage préventif et curatif des rides d'expression du front, de la ride du lion et des pattes d'oie.",
                    ImageUrl = "/images/2.png",
                    Duree = "20 min",
                    Anesthesie = "Aucune (Indolore)",
                    Eviction = "Immédiate",
                    Hospitalisation = "Sans hospitalisation",
                    Overview = "La toxine botulique met au repos ciblé les muscles peauciers responsables des rides dynamiques du haut du visage, offrant un regard défatigué et un front lisse.",
                    Indications = "Rides du front et ride du lion (Inter-sourcilière);Rides de la patte d'oie (Coin des yeux);Sourire gingival (Gummy Smile);Transpiration excessive (Hyperhidrose)",
                    Steps = "Désinfection et repérage des points d'injection musculaires;Micro-injections indolores par aiguille ultra-fine;Recommandations post-acte simples (ne pas s'allonger dans les 4h);Contrôle de perfectionnement à 14 jours",
                    Faqs = "Quand le Botox commence-t-il à agir ? Les premiers effets apparaissent dès 3 jours et le résultat optimal s'installe à 10-14 jours.;Durée de l'effet ? 4 à 6 mois."
                }
            );
        }

        // 6. Reset & Seed ONLY the 2 Client Actualites
        var currentActualites = context.Actualites.ToList();
        foreach (var act in currentActualites)
        {
            if (act.Title.Contains("rajeunissement", StringComparison.OrdinalIgnoreCase))
            {
                act.Title = "L’approche globale du rajeunissement du visage :";
            }
            if (act.Title.Contains("RHINOPLASTIE", StringComparison.OrdinalIgnoreCase))
            {
                act.Title = "RHINOPLASTIE ULTRASONIQUE : Remodelage du nez en douceur";
            }
        }
        context.SaveChanges();

        var hasExactTwo = currentActualites.Count == 2 
            && currentActualites.Any(a => a.Title.Contains("rajeunissement"))
            && currentActualites.Any(a => a.Title.Contains("RHINOPLASTIE"));

        if (!hasExactTwo)
        {
            context.Actualites.RemoveRange(currentActualites);
            context.SaveChanges();

            context.Actualites.AddRange(
                new Actualite
                {
                    Title = "L’approche globale du rajeunissement du visage :",
                    Excerpt = "Harmoniser le visage sans le transformer : Avec le temps, le visage vieillit par l’apparition des rides et la diminution des volumes. En effet, les tissus s’affaissent, la qualité de la peau évolue et certains muscles deviennent plus actifs. Ces phénomènes modifient progressivement l’équilibre du visage. Notre philosophie est d’analyser l’ensemble de ces changements et proposer un plan de traitement personnalisé qui permet de préserver l’harmonie du visage tout en respectant son identité.",
                    Content = @"Harmoniser le visage sans le transformer :
Avec le temps, le visage vieillit par l’apparition des rides et la diminution des volumes. En effet, les tissus s’affaissent, la qualité de la peau évolue et certains muscles deviennent plus actifs. Ces phénomènes modifient progressivement l’équilibre du visage.
Notre philosophie est d’analyser l’ensemble de ces changements et proposer un plan de traitement personnalisé qui permet de préserver l’harmonie du visage tout en respectant son identité. 
 L’objectif du traitement n’est plus de modifier les traits ou de créer des volumes excessifs, mais de restaurer l’harmonie globale du visage sans corriger une seule partie de manière isolée.
Il s’agit d’une stratégie globale, personnalisée, qui prend en compte les proportions du visage, les effets du vieillissement et les attentes du patient afin d’obtenir un résultat naturel et équilibré.
Chaque visage est unique. C’est pourquoi aucun traitement ne ressemble à un autre.

Pourquoi traiter le visage dans son ensemble ?
Une ride ou un creux peut être la conséquence d’un déséquilibre situé ailleurs sur le visage.
Par exemple, vouloir uniquement traiter les sillons nasogéniens sans restaurer les volumes des pommettes conduit souvent à un résultat moins naturel. De la même manière, traiter seulement les lèvres sans tenir compte du menton et la projection du nez peut rompre l’équilibre des proportions.
En travaillant sur l’ensemble du visage, on obtient un résultat plus harmonieux, plus discret et surtout plus durable.

Quels traitements peut-on réaliser ?
Selon les besoins de chaque patient, plusieurs techniques peuvent être réalisées et parfois combinées.

Les procédés de médecine esthétique :
- L’injection d’acide hyaluronique : permet de restaurer les volumes, redessiner les contours du visage et améliorer l’hydratation de la peau tout en conservant des expressions naturelles.
- L’injection de toxine botulique : agit sur les muscles responsables des rides d’expression, notamment au niveau du front, de la glabelle et de la patte d’oie. Elle permet d’adoucir le regard tout en préservant la mobilité du visage.
- L’injection de plasma riche en plaquettes (PRP) : améliore la qualité de la peau en stimulant la régénération cellulaire et en libérant des facteurs de croissance.
- Le Skin booster : améliore la qualité de la peau, apporte hydratation et souplesse, sans modifier les volumes.

Les procédés de chirurgie esthétique :
- Le lipofilling : consiste à injecter sa propre graisse (après prélèvement et préparation). Le résultat est double, effet volumateur (restaurer les volumes) et effet inducteur régénérateur (améliore la qualité de la peau). 
- Le lifting chirurgical du visage : peut être partiel (minilift) ou total (lifting complet) intéressant un ou plusieurs étages du visage (temporal, jugal, cervical). Le concept actuel est de réaliser un lifting profond (Deep plane face lift) qui intéresse la peau et les muscles d’expressions avec un résultat naturel et durable.

En résumé :
L’approche globale du rajeunissement du visage représente aujourd’hui l’approche la plus moderne de la médecine et de la chirurgie esthétique. Plutôt que de traiter une zone de façon isolée, il s’appuie sur une analyse globale du visage afin de restaurer l’harmonie et sublimer les traits de manière naturelle.
Pour nous, chaque plan de traitement est personnalisé. L’objectif est simple : révéler une version harmonieuse et naturelle de votre visage, sans jamais le transformer.",
                    ImageUrl = "/images/dr_slama_portrait.png",
                    Tags = "#Rajeunissement,#Harmonie,#MédecineEsthétique,#ChirurgieEsthétique",
                    DateLabel = "AOÛT 2026",
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Actualite
                {
                    Title = "RHINOPLASTIE ULTRASONIQUE : Remodelage du nez en douceur",
                    Excerpt = "La rhinoplastie ultrasonique permet un remodelage complet du nez en douceur avec des gestes opératoires très précis et des suites opératoires plus simples. La rhinoplastie ultrasonique ou piézoélectrique est une technique de remodelage du nez par ultrasons. Avec cette technologie moderne, il devient possible de corriger une bosse sur le nez sans recourir à des instruments invasifs de la rhinoplastie classique.",
                    Content = @"Remodelage du nez en douceur 
La rhinoplastie ultrasonique permet un remodelage complet du nez en douceur avec des gestes opératoires très précis et des suites opératoires plus simples.
La rhinoplastie ultrasonique ou piézoélectrique est une technique de remodelage du nez par ultrasons. Avec cette technologie moderne, il devient possible de corriger une bosse sur le nez sans recourir à des instruments invasifs de la rhinoplastie classique. La précision offerte par cette technique apporte un grand bénéfice.

Qu'est-ce qu'une rhinoplastie ultrasonique ?
La rhinoplastie ultrasonique est une technique de rhinoplastie réalisée au piézotome. Elle est moins traumatisante et plus précise que la rhinoplastie classique avec des suites opératoires plus simples. 
Le piézotome est un instrument chirurgical qui génère des vibrations ultrasoniques à haute fréquence, permettant de remodeler, découper, râper et affiner les os du nez avec une précision millimétrique, et ceci sans endommager les cartilages et les parties molles : peau, vaisseaux, muscles, muqueuses.
La rhinoplastie ultrasonique permet donc de réaliser une rhinoplastie plus précise avec des suites opératoires simplifiées (réduction des ecchymoses et des œdème postopératoires).

À qui s’adresse la rhinoplastie ultrasonique ?
La rhinoplastie ultrasonique convient à la majorité des patients souhaitant corriger une bosse, une déviation, un nez trop large ou trop long. 
Principales indications :
•	Bosse proéminente sur le dos du nez, souvent d’origine génétique ou post-traumatique.
•	Remodelage d’un nez trop large, dévié ou disproportionné.
•	Sculpture précise des os du nez pour corriger des asymétries et les irrégularités.

Comment se déroule la rhinoplastie ultrasonique ?
Avant toute rhinoplastie ultrasonique, le patient est reçu pour une 1ère consultation préopératoire. Celle-ci permet de poser l’indication et d’analyser le visage en 3 dimensions, en particulier les structures osseuse et cartilagineuse du nez, l’épaisseur de la peau, la statique de la cloison et l’équilibre du visage dans son ensemble.
La rhinoplastie ultrasonique se déroule sous anesthésie générale et dure deux heures en moyenne.

Lors de l’intervention, le piézotome agit comme un bistouri, dont la lame soumise à des oscillations passe aisément à travers l’os, sans forcer. En effet, les lignes de fracture des os du nez sont très précises et sans risques de traits de fractures incontrôlés. En plus, l’os est coupé sans endommager les tissus mous adjacents (la peau, les muqueuses, les vaisseaux et les cartilages) ce qui diminue le saignement, l’ecchymose, le gonflement et la douleur post-opératoires.
Cette technologie piézoélectrique innovante permet donc un geste osseux plus précis et moins invasif.  La bosse sur le nez est ainsi éliminée, le nez est affiné et retrouve une belle harmonie avec un résultat plus naturel.",
                    ImageUrl = "/images/dr_slama_details.png",
                    Tags = "#Rhinoplastie,#PiezoUltrasons,#PrécisionChirurgicale,#Sousse",
                    DateLabel = "JUILLET 2026",
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            );
            context.SaveChanges();
        }

        context.SaveChanges();
    }
}
