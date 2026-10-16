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
        // Testimiseks võid selle ka otse siia kirjutada, aga ära siis commiti seda.
        public static string SaatjaParool = "jcfl usxf hbxw ansj";

        public static async Task SaadaAsync(string kellele, string teema, string html)
        {
            if (string.IsNullOrWhiteSpace(SaatjaParool))
                throw new InvalidOperationException(
                    "SMTP parool puudub. Määra keskkonnamuutuja TOODED_SMTP_PAROOL " +
                    "või kirjuta parool EpostiSaatja.cs faili.");

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

            using var smtp = new SmtpClient(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(SaatjaEmail, SaatjaParool)
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