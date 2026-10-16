using System.Globalization;
using System.Net;
using System.Text;

namespace Tooded_2
{
    /// <summary>Koostab ostutšeki HTML-kujul (e-kirja sisuks).</summary>
    public static class TsekiKoostaja
    {
        // ---- Poe andmed tšeki päises (muuda enda omadeks) ----
        public const string PoeNimi = "Shoping 67";
        public const string Ettevote = "Shoping 67 OÜ";
        public const string RegNr = "12345678";
        public const string Aadress = "Tallinn";

        // Käibemaks Eestis (alates 01.07.2025 on 24%)
        public const decimal KmMaar = 24m;

        private static readonly CultureInfo Et = new("et-EE");

        private static string Raha(decimal x) => x.ToString("0.00", Et) + " €";
        private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

        public static string LooHtml(List<OstuRida> ostud, string tsekiNr, DateTime aeg, string klient)
        {
            decimal summa = ostud.Sum(o => o.Summa);
            decimal ilmaKm = Math.Round(summa / (1 + KmMaar / 100m), 2);
            decimal km = summa - ilmaKm;

            var sb = new StringBuilder();
            sb.Append($@"
<div style=""background:#f2f2f2;padding:20px 0;font-family:Arial,Helvetica,sans-serif;"">
<table cellpadding=""0"" cellspacing=""0"" align=""center""
       style=""width:400px;background:#ffffff;border:1px solid #ddd;padding:20px;font-size:13px;color:#222;"">
  <tr><td colspan=""2"" style=""text-align:center;padding:10px 20px 4px;"">
    <div style=""font-size:30px;font-weight:900;letter-spacing:2px;color:#2e7d32;"">{H(PoeNimi)}</div>
  </td></tr>
  <tr><td colspan=""2"" style=""text-align:center;font-size:11px;color:#555;padding:0 20px 14px;"">
    {H(Ettevote)} Reg. nr {H(RegNr)} {H(Aadress)}
  </td></tr>
  <tr><td colspan=""2"" style=""padding:0 20px 12px;"">Tšeki nr {H(tsekiNr)}</td></tr>");

            // ---- Tooted ----
            foreach (var o in ostud)
            {
                string detail = o.Kogus > 1
                    ? $"<span style=\"color:#555;\">{o.Hind.ToString("0.00", Et)} × {o.Kogus}</span>&nbsp;&nbsp;"
                    : "";

                sb.Append($@"
  <tr>
    <td style=""padding:4px 0 4px 20px;"">{H(o.Nimetus)}</td>
    <td style=""padding:4px 20px 4px 0;text-align:right;white-space:nowrap;"">{detail}<b>{Raha(o.Summa)}</b></td>
  </tr>");
            }

            // ---- Käibemaksu tabel ----
            sb.Append($@"
  <tr><td colspan=""2"" style=""padding:12px 20px;""><hr style=""border:0;border-top:1px solid #333;margin:0;""></td></tr>
  <tr><td colspan=""2"" style=""padding:0 20px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:12px;"">
      <tr style=""color:#555;"">
        <td>KM %</td><td style=""text-align:center;"">ilma KM-ta</td>
        <td style=""text-align:center;"">KM</td><td style=""text-align:right;"">koos KM-ga</td>
      </tr>
      <tr>
        <td style=""padding-top:6px;"">{KmMaar:0} %</td>
        <td style=""text-align:center;padding-top:6px;"">{Raha(ilmaKm)}</td>
        <td style=""text-align:center;padding-top:6px;"">{Raha(km)}</td>
        <td style=""text-align:right;padding-top:6px;""><b>{Raha(summa)}</b></td>
      </tr>
    </table>
  </td></tr>
  <tr><td colspan=""2"" style=""padding:12px 20px;""><hr style=""border:0;border-top:1px solid #333;margin:0;""></td></tr>

  <tr><td style=""padding:2px 0 2px 20px;text-align:right;"">KM:</td>
      <td style=""padding:2px 20px 2px 0;text-align:right;""><b>{Raha(km)}</b></td></tr>
  <tr><td style=""padding:2px 0 2px 20px;text-align:right;"">ilma KM-ta:</td>
      <td style=""padding:2px 20px 2px 0;text-align:right;""><b>{Raha(ilmaKm)}</b></td></tr>
  <tr><td style=""padding:10px 0 2px 20px;text-align:right;font-size:15px;""><b>Summa:</b></td>
      <td style=""padding:10px 20px 2px 0;text-align:right;font-size:15px;""><b>{Raha(summa)}</b></td></tr>
  <tr><td style=""padding:2px 0 2px 20px;text-align:right;"">Makstud:</td>
      <td style=""padding:2px 20px 2px 0;text-align:right;""><b>{Raha(summa)}</b></td></tr>
  <tr><td style=""padding:2px 0 2px 20px;text-align:right;"">Tooteid kokku:</td>
      <td style=""padding:2px 20px 2px 0;text-align:right;""><b>{ostud.Sum(o => o.Kogus)} tk</b></td></tr>

  <tr><td colspan=""2"" style=""padding:16px 20px 4px;""><hr style=""border:0;border-top:1px dashed #999;margin:0;""></td></tr>
  <tr><td colspan=""2"" style=""text-align:center;padding:8px 20px;"">Aitäh ostu eest!</td></tr>
  <tr>
    <td style=""padding:8px 0 0 20px;font-size:11px;color:#555;"">Klient: {H(klient)}</td>
    <td style=""padding:8px 20px 0 0;font-size:11px;color:#555;text-align:right;"">{aeg:dd.MM.yyyy HH:mm:ss}</td>
  </tr>
</table>
</div>");

            return sb.ToString();
        }
    }
}