using System.IO;

namespace Tooded_2
{
    // Hoiab sisseloginud kasutaja andmeid ja salvestab need faili user.txt
    public static class Session
    {
        private static readonly string FailiTee = Path.Combine(AppContext.BaseDirectory, "user.txt");

        public static int UserId { get; private set; }
        public static string? Username { get; private set; }
        public static string? Role { get; private set; }

        public static bool IsAdmin => Role == "Admin";

        public static void Login(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            Role = user.Role;
            File.WriteAllLines(FailiTee, new[] { user.Username, user.Role });
        }

        // Proovib laadida varem salvestatud sessiooni. Tagastab true, kui õnnestus.
        public static bool TryLoad()
        {
            if (!File.Exists(FailiTee)) return false;

            var read = File.ReadAllLines(FailiTee);
            if (read.Length < 1 || string.IsNullOrWhiteSpace(read[0]))
                return false;

            // Roll võetakse andmebaasist, mitte failist – faili muutmisega ei saa adminiks
            using var db = new AppDbContext();
            var kasutaja = db.Users.FirstOrDefault(u => u.Username == read[0]);
            if (kasutaja == null) return false;

            UserId = kasutaja.Id;
            Username = kasutaja.Username;
            Role = kasutaja.Role;
            return true;
        }

        /// <summary>Loeb kasutaja värsked andmed andmebaasist (raha, täiendused).</summary>
        public static User? LaeKasutaja(AppDbContext db) =>
            db.Users.FirstOrDefault(u => u.Id == UserId);

        public static decimal Raha()
        {
            using var db = new AppDbContext();
            return LaeKasutaja(db)?.Raha ?? 0m;
        }

        public static void Logout()
        {
            UserId = 0;
            Username = null;
            Role = null;
            if (File.Exists(FailiTee)) File.Delete(FailiTee);
        }
    }
}
