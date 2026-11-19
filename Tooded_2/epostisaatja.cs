using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Tooded_2
{
    /// <summary>Saadab e-kirju Gmaili (või muu SMTP serveri) kaudu.</summary>
    public static class EpostiSaatja
    {
        // ---- SMTP seaded ----
        public const string SmtpServer = "smtp.gmail.com";
        public const int SmtpPort = 587;

        // Poe postkast – ainus aadress, millelt tšekid ja arved saadetakse
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

        /// <summary>
        /// Saadab kirja alati poe postkastist (SaatjaEmail). Kui <paramref name="manus"/> on antud,
        /// lisatakse see kirjale PDF-manusena nimega <paramref name="manuseNimi"/>.
        /// </summary>
        public static async Task SaadaAsync(string kellele, string teema, string html,
            byte[]? manus = null, string? manuseNimi = null)
        {
            if (!PoeEpostSeadistatud)
                throw new InvalidOperationException(Keel.T(
                    "Poe e-post pole seadistatud (User Secrets: Smtp:Parool puudub).",
                    "Почта магазина не настроена (User Secrets: отсутствует Smtp:Parool).",
                    "The shop's e-mail is not set up (User Secrets: Smtp:Parool is missing)."));

            // MailMessage.Dispose vabastab ka manused ja nende vood
            using var kiri = new MailMessage
            {
                From = new MailAddress(SaatjaEmail, SaatjaNimi),
                Subject = teema,
                Body = html,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            kiri.To.Add(kellele);

            if (manus != null)
            {
                string nimi = string.IsNullOrWhiteSpace(manuseNimi) ? "arve.pdf" : manuseNimi.Trim();
                var voog = new MemoryStream(manus, writable: false);
                try
                {
                    kiri.Attachments.Add(new Attachment(voog, nimi, MediaTypeNames.Application.Pdf)
                    {
                        NameEncoding = Encoding.UTF8
                    });
                }
                catch
                {
                    voog.Dispose(); // manust ei õnnestunud lisada – kiri seda voogu ei vabasta
                    throw;
                }
            }

            using var smtp = new SmtpClient(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                // Gmail näitab rakenduse parooli tühikutega – need tuleb eemaldada
                Credentials = new NetworkCredential(SaatjaEmail, SaatjaParool.Replace(" ", ""))
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