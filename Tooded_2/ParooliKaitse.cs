using System.Security.Cryptography;
using System.Text;

namespace Tooded_2
{
    // Rakenduse parooli ei saa räsida (BCrypt), sest seda on vaja SMTP-le päris kujul anda.
    // Seepärast krüpteerime selle Windowsi DPAPI-ga: dekrüpteerida saab ainult
    // sama Windowsi kasutaja samas arvutis.
    public static class ParooliKaitse
    {
        private static readonly byte[] Lisaentroopia = Encoding.UTF8.GetBytes("Tooded_2.EpostiParool");

        public static string Krupteeri(string parool)
        {
            if (string.IsNullOrEmpty(parool)) return string.Empty;
            byte[] andmed = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(parool), Lisaentroopia, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(andmed);
        }

        public static string Dekrupteeri(string krupteeritud)
        {
            if (string.IsNullOrEmpty(krupteeritud)) return string.Empty;
            byte[] andmed = ProtectedData.Unprotect(
                Convert.FromBase64String(krupteeritud), Lisaentroopia, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(andmed);
        }
    }
}
