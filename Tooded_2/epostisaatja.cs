using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Tooded_2
{
    /// <summary>Saadab e-kirju Gmaili (või muu SMTP serveri) kaudu.</summary>
    public static class EpostiSaatja
    {
        // ---- SMTP seaded ----
        public const string SmtpServer = "smtp.gmail.com";
        public const int SmtpPort = 587;

        // E-post, millelt tšekid saadetakse
        public static string SaatjaEmail = "order1print@gmail.com";
        public static string SaatjaNimi = TsekiKoostaja.PoeNimi;

        // Gmaili RAKENDUSE parool (16 tähemärki), mitte tavaline parool.
        // Loetakse keskkonnamuutujast, et parool ei satuks GitHubi.
        // Parool loetakse User Secrets'ist (ei ole projekti kaustas ega GitHubis):
        //   dotnet user-secrets set "Smtp:Parool" "xxxx xxxx xxxx xxxx"
        // või Visual Studios: projekt -> parem klikk -> Manage User Secrets.
        // Varuvariant: keskkonnamuutuja TOODED_SMTP_PAROOL.
        public static string SaatjaParool = LoeParool();

        private static string LoeParool()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(EpostiSaatja).Assembly, optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                return config["Smtp:Parool"]
                       ?? config["TOODED_SMTP_PAROOL"]
                       ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static bool PoeEpostSeadistatud => !string.IsNullOrWhiteSpace(SaatjaParool);

        public static Task SaadaAsync(string kellele, string teema, string html)
        {
            if (!PoeEpostSeadistatud)
                throw new InvalidOperationException(
                    "Sinu kontol pole rakenduse parooli ja poe e-post pole seadistatud " +
                    "(User Secrets: Smtp:Parool puudub).");
            return SaadaAsync(kellele, teema, html, SaatjaEmail, SaatjaParool);
        }

        /// <summary>Saadab kirja kasutaja enda kontolt (tema e-post + rakenduse parool).</summary>
        public static async Task SaadaAsync(string kellele, string teema, string html,
            string saatjaEmail, string saatjaParool)
        {
            if (string.IsNullOrWhiteSpace(saatjaParool))
                throw new InvalidOperationException("Rakenduse parool puudub.");

            using var kiri = new MailMessage
            {
                From = new MailAddress(saatjaEmail, SaatjaNimi),
                Subject = teema,
                Body = html,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            kiri.To.Add(kellele);

            using var smtp = new SmtpClient(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                // Gmail näitab rakenduse parooli tühikutega – need tuleb eemaldada
                Credentials = new NetworkCredential(saatjaEmail, saatjaParool.Replace(" ", ""))
            };

            await smtp.SendMailAsync(kiri);
        }

        /// <summary>Kontrollib, kas e-posti aadress on õige kujuga.</summary>
        public static bool OnKorrektne(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var a = new MailAddress(email.Trim());
                return a.Address == email.Trim() && a.Host.Contains('.');
            }
            catch
            {
                return false;
            }
        }
    }
}