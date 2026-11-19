namespace Tooded_2
{
    /// <summary>
    /// Algandmed: kategooriad ja tooted, mis peavad poes kohe olemas olema.
    /// Käivitub programmi alguses:
    ///  - kui andmebaasis pole veel ühtegi algtoodet, lisatakse kõik (ja puuduvad kategooriad);
    ///  - muidu ei lisata midagi, et admini kustutatud tooted tagasi ei tuleks,
    ///    ainult pildita algtoodetele pannakse pildi link (AlgPildid).
    /// </summary>
    public static class Seeder
    {
        private record AlgToode(string Nimi, string Kategooria, int Kogus, float Hind);

        private static readonly AlgToode[] Tooted =
        {
            // Piimatooted
            new("Piim 2,5% 1 l", "Piimatooted", 40, 1.29f),
            new("Juust Eesti 400 g", "Piimatooted", 25, 4.49f),
            new("Või 82% 200 g", "Piimatooted", 30, 2.99f),
            new("Maasikajogurt 380 g", "Piimatooted", 20, 1.19f),

            // Pagaritooted
            new("Rukkileib 500 g", "Pagaritooted", 35, 1.89f),
            new("Sai 400 g", "Pagaritooted", 30, 1.29f),
            new("Croissant", "Pagaritooted", 50, 0.89f),

            // Puu- ja köögiviljad
            new("Banaanid 1 kg", "Puu- ja köögiviljad", 60, 1.49f),
            new("Õunad 1 kg", "Puu- ja köögiviljad", 50, 1.99f),
            new("Tomatid 500 g", "Puu- ja köögiviljad", 40, 2.49f),
            new("Kurk", "Puu- ja köögiviljad", 45, 0.99f),

            // Joogid
            new("Kohvioad 500 g", "Joogid", 15, 8.99f),
            new("Apelsinimahl 1 l", "Joogid", 25, 2.29f),
            new("Mineraalvesi 1,5 l", "Joogid", 80, 0.79f),

            // Maiustused
            new("Piimašokolaad 100 g", "Maiustused", 60, 1.59f),
            new("Küpsised 200 g", "Maiustused", 40, 1.39f),

            // Elektroonika
            new("Juhtmevabad kõrvaklapid", "Elektroonika", 10, 49.99f),
            new("Nutikell", "Elektroonika", 8, 89.99f),
            new("USB-C laadija 20 W", "Elektroonika", 20, 14.99f),

            // Kodu
            new("Kohvimasin", "Kodu", 5, 119.00f),
            new("LED lauavalgusti", "Kodu", 12, 24.99f),
        };

        public static void Kaivita()
        {
            using var db = new AppDbContext();

            var olemas = db.Toodetabel.ToList();
            bool onAlgandmed = olemas.Any(o => Tooted.Any(t =>
                string.Equals(o.Toodenimetus, t.Nimi, StringComparison.OrdinalIgnoreCase)));

            if (onAlgandmed)
            {
                LisaPuuduvadPildid(db, olemas);
                return;
            }

            // 1) Kategooriad (nime järgi, tõstutundetult)
            var kategooriad = db.Kategooriatabel.ToList();
            foreach (var nimi in Tooted.Select(t => t.Kategooria).Distinct())
            {
                if (!kategooriad.Any(k => string.Equals(k.nimetus, nimi, StringComparison.OrdinalIgnoreCase)))
                {
                    var uus = new Kategooria { nimetus = nimi };
                    db.Kategooriatabel.Add(uus);
                    kategooriad.Add(uus);
                }
            }
            db.SaveChanges(); // uued kategooriad saavad Id

            // 2) Kõik algtooted
            foreach (var t in Tooted)
            {
                var kategooria = kategooriad.First(k =>
                    string.Equals(k.nimetus, t.Kategooria, StringComparison.OrdinalIgnoreCase));
                db.Toodetabel.Add(new Toode
                {
                    Toodenimetus = t.Nimi,
                    Kogus = t.Kogus,
                    Hind = t.Hind,
                    KategooriaId = kategooria.Id,
                    Pilt = AlgPildid.Url(t.Nimi)
                });
            }
            db.SaveChanges();
        }

        /// <summary>Algtoodetele, millel pilti pole, pannakse pildi link (nt kui lingid lisati hiljem).</summary>
        private static void LisaPuuduvadPildid(AppDbContext db, List<Toode> olemas)
        {
            bool muudetud = false;
            foreach (var toode in olemas.Where(o => string.IsNullOrWhiteSpace(o.Pilt)))
            {
                string pilt = AlgPildid.Url(toode.Toodenimetus);
                if (pilt.Length == 0) continue;
                toode.Pilt = pilt;
                muudetud = true;
            }
            if (muudetud) db.SaveChanges();
        }
    }
}
