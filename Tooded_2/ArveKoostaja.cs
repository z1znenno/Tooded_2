using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Tooded_2
{
    /// <summary>Koostab ostu kohta PDF-arve (A4), mis lisatakse e-kirjale manusena.</summary>
    public static class ArveKoostaja
    {
        // Samad värvid mis tšekil
        private const string Tumesinine = "#0e2240";
        private const string Oranz = "#ff6600";
        private const string Hall = "#555555";
        private const string HeleHall = "#f4f5f7";
        private const string Joon = "#d9dce1";

        // Windowsi fondid, milles on olemas nii eesti (õäöüšž) kui ka vene tähed
        private static readonly string[] Fondid = { "Arial", "Segoe UI", "Tahoma" };

        private static readonly CultureInfo Et = new("et-EE");

        static ArveKoostaja()
        {
            // Tasuta kogukonnalitsents (õppeprojekt)
            QuestPDF.Settings.License = LicenseType.Community;

            // Alates QuestPDF 2026.9 ei kasutata süsteemi fonte vaikimisi – lubame need,
            // sest kaasasolevas vaikefondis ei pruugi kõiki eesti ja vene tähti olla.
            QuestPDF.Settings.UseSystemFonts = true;
            // Kui mõnda fonti arvutis pole, võetakse nimekirjast järgmine (mitte viga)
            QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
        }

        private static string Raha(decimal x) => x.ToString("0.00", Et) + " €";

        /// <summary>Tagastab valmis PDF-faili sisu baitidena.</summary>
        public static byte[] LooPdf(List<OstuRida> ostud, string arveNr, DateTime aeg,
            string klient, string klientEmail)
        {
            return LooDokument(ostud, arveNr, aeg, klient, klientEmail).GeneratePdf();
        }

        // Eraldi meetod, et sama dokumenti saaks kontrollimiseks ka pildiks joonistada
        internal static Document LooDokument(List<OstuRida> ostud, string arveNr, DateTime aeg,
            string klient, string klientEmail)
        {
            // Sama arvutus mis tšekil: hinnad sisaldavad juba käibemaksu
            decimal summa = ostud.Sum(o => o.Summa);
            decimal ilmaKm = Math.Round(summa / (1 + TsekiKoostaja.KmMaar / 100m), 2);
            decimal km = summa - ilmaKm;

            // Arve tekstid ostu hetkel valitud keeles
            string pealkiri = Keel.T("Arve", "Счёт", "Invoice");
            string arveNrSilt = Keel.T("Arve nr", "Счёт №", "Invoice no.");
            string kuupaevSilt = Keel.T("Kuupäev", "Дата", "Date");
            string regNr = Keel.T("Reg. nr", "Рег. №", "Reg. no.");
            string muujaSilt = Keel.T("Müüja", "Продавец", "Seller");
            string ostjaSilt = Keel.T("Ostja", "Покупатель", "Buyer");
            string epostSilt = Keel.T("E-post", "Эл. почта", "E-mail");
            string nrSilt = Keel.T("Nr", "№", "No.");
            string toodeSilt = Keel.T("Toode", "Товар", "Product");
            string kogusSilt = Keel.T("Kogus", "Кол-во", "Qty");
            string hindSilt = Keel.T("Ühiku hind", "Цена за ед.", "Unit price");
            string reaSummaSilt = Keel.T("Summa", "Сумма", "Amount");
            string ilmaKmSilt = Keel.T("Summa ilma KM-ta", "Сумма без НДС", "Amount excl. VAT");
            string kmSilt = Keel.T("Käibemaks", "НДС", "VAT");
            string kokkuSilt = Keel.T("Kokku tasuda", "Итого к оплате", "Total");
            string makstud = Keel.T("Arve on tasutud.", "Счёт оплачен.", "This invoice has been paid.");
            string hinnadKm = Keel.T("Kõik hinnad sisaldavad käibemaksu.",
                "Все цены включают НДС.", "All prices include VAT.");
            string aitah = Keel.T("Aitäh ostu eest!", "Спасибо за покупку!", "Thank you for your purchase!");
            string lkSilt = Keel.T("Lk", "Стр.", "Page");

            return Document.Create(dok =>
            {
                dok.Page(leht =>
                {
                    leht.Size(PageSizes.A4);
                    leht.Margin(40);
                    leht.DefaultTextStyle(t => t.FontFamily(Fondid).FontSize(10).FontColor("#222222"));

                    // ---- Päis: pood vasakul, arve number ja kuupäev paremal ----
                    leht.Header().PaddingBottom(18).Column(pais =>
                    {
                        pais.Item().Row(rida =>
                        {
                            rida.RelativeItem().Column(v =>
                            {
                                v.Item().Text(TsekiKoostaja.PoeNimi)
                                    .FontSize(26).ExtraBold().LetterSpacing(0.05f).FontColor(Tumesinine);
                                v.Item().PaddingTop(5).Width(60).Height(4).Background(Oranz);
                            });

                            rida.ConstantItem(210).AlignRight().Column(p =>
                            {
                                p.Item().AlignRight().Text(pealkiri.ToUpper(Et))
                                    .FontSize(20).Bold().FontColor(Tumesinine);
                                p.Item().PaddingTop(4).AlignRight().Text(t =>
                                {
                                    t.Span(arveNrSilt + ": ").FontColor(Hall);
                                    t.Span(arveNr).SemiBold();
                                });
                                p.Item().AlignRight().Text(t =>
                                {
                                    t.Span(kuupaevSilt + ": ").FontColor(Hall);
                                    t.Span(aeg.ToString("dd.MM.yyyy HH:mm", Et)).SemiBold();
                                });
                            });
                        });

                        pais.Item().PaddingTop(14).LineHorizontal(1).LineColor(Joon);
                    });

                    leht.Content().Column(sisu =>
                    {
                        // ---- Müüja ja ostja ----
                        sisu.Item().Row(rida =>
                        {
                            rida.RelativeItem().Column(m =>
                            {
                                m.Item().Text(muujaSilt.ToUpper(Et)).FontSize(8).SemiBold().FontColor(Oranz);
                                m.Item().PaddingTop(3).Text(TsekiKoostaja.Ettevote).SemiBold();
                                m.Item().Text($"{regNr} {TsekiKoostaja.RegNr}").FontColor(Hall);
                                m.Item().Text(TsekiKoostaja.Aadress).FontColor(Hall);
                            });

                            rida.ConstantItem(20);

                            rida.RelativeItem().Column(o =>
                            {
                                o.Item().Text(ostjaSilt.ToUpper(Et)).FontSize(8).SemiBold().FontColor(Oranz);
                                o.Item().PaddingTop(3).Text(klient ?? "").SemiBold();
                                if (!string.IsNullOrWhiteSpace(klientEmail))
                                    o.Item().Text($"{epostSilt}: {klientEmail.Trim()}").FontColor(Hall);
                            });
                        });

                        // ---- Toodete tabel ----
                        sisu.Item().PaddingTop(22).Table(tabel =>
                        {
                            tabel.ColumnsDefinition(v =>
                            {
                                v.ConstantColumn(32);   // nr
                                v.RelativeColumn();     // toode
                                v.ConstantColumn(55);   // kogus
                                v.ConstantColumn(85);   // ühiku hind
                                v.ConstantColumn(90);   // summa
                            });

                            // Päis kordub igal lehel, kui tabel ei mahu ühele lehele
                            tabel.Header(p =>
                            {
                                p.Cell().Element(PaiseLahter).Text(nrSilt);
                                p.Cell().Element(PaiseLahter).Text(toodeSilt);
                                p.Cell().Element(PaiseLahter).AlignRight().Text(kogusSilt);
                                p.Cell().Element(PaiseLahter).AlignRight().Text(hindSilt);
                                p.Cell().Element(PaiseLahter).AlignRight().Text(reaSummaSilt);
                            });

                            for (int i = 0; i < ostud.Count; i++)
                            {
                                var o = ostud[i];
                                bool triip = i % 2 == 1;

                                tabel.Cell().Element(c => ReaLahter(c, triip)).Text((i + 1).ToString()).FontColor(Hall);
                                tabel.Cell().Element(c => ReaLahter(c, triip)).Text(o.Nimetus ?? "");
                                tabel.Cell().Element(c => ReaLahter(c, triip)).AlignRight().Text(o.Kogus.ToString());
                                tabel.Cell().Element(c => ReaLahter(c, triip)).AlignRight().Text(Raha(o.Hind));
                                tabel.Cell().Element(c => ReaLahter(c, triip)).AlignRight().Text(Raha(o.Summa)).SemiBold();
                            }
                        });

                        // ---- Kokkuvõte (hoitakse ühel lehel koos) ----
                        sisu.Item().ShowEntire().PaddingTop(16).Column(lopp =>
                        {
                            lopp.Item().AlignRight().Width(250).Column(k =>
                            {
                                k.Item().PaddingVertical(2).Row(r =>
                                {
                                    r.RelativeItem().Text(ilmaKmSilt + ":").FontColor(Hall);
                                    r.ConstantItem(90).AlignRight().Text(Raha(ilmaKm));
                                });
                                k.Item().PaddingVertical(2).Row(r =>
                                {
                                    r.RelativeItem().Text($"{kmSilt} {TsekiKoostaja.KmMaar:0} %:").FontColor(Hall);
                                    r.ConstantItem(90).AlignRight().Text(Raha(km));
                                });
                                k.Item().PaddingTop(6).BorderTop(2).BorderColor(Oranz).PaddingTop(6).Row(r =>
                                {
                                    r.RelativeItem().Text(kokkuSilt + ":").FontSize(13).Bold().FontColor(Tumesinine);
                                    r.ConstantItem(110).AlignRight().Text(Raha(summa))
                                        .FontSize(13).Bold().FontColor(Tumesinine);
                                });
                            });

                            lopp.Item().PaddingTop(22).Background(HeleHall).BorderLeft(3).BorderColor(Oranz)
                                .Padding(10).Column(m =>
                                {
                                    m.Item().Text(makstud).SemiBold();
                                    m.Item().Text(hinnadKm).FontSize(9).FontColor(Hall);
                                });

                            lopp.Item().PaddingTop(26).AlignCenter().Text(aitah)
                                .FontSize(12).SemiBold().FontColor(Tumesinine);
                        });
                    });

                    // ---- Jalus ----
                    leht.Footer().Column(jalus =>
                    {
                        jalus.Item().LineHorizontal(1).LineColor(Joon);
                        jalus.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().Text(
                                    $"{TsekiKoostaja.Ettevote} · {regNr} {TsekiKoostaja.RegNr} · {TsekiKoostaja.Aadress}")
                                .FontSize(8).FontColor(Hall);
                            r.ConstantItem(80).AlignRight().Text(t =>
                            {
                                t.DefaultTextStyle(s => s.FontSize(8).FontColor(Hall));
                                t.Span(lkSilt + " ");
                                t.CurrentPageNumber();
                                t.Span(" / ");
                                t.TotalPages();
                            });
                        });
                    });
                });
            });
        }

        private static IContainer PaiseLahter(IContainer c) =>
            c.Background(Tumesinine).PaddingVertical(7).PaddingHorizontal(8)
             .DefaultTextStyle(t => t.FontColor("#ffffff").SemiBold().FontSize(9));

        private static IContainer ReaLahter(IContainer c, bool triip) =>
            c.Background(triip ? HeleHall : "#ffffff")
             .BorderBottom(1).BorderColor(Joon)
             .PaddingVertical(6).PaddingHorizontal(8);
    }
}
