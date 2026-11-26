using System.Globalization;

namespace Tooded_EFCore;

public static class ToodeValidaator
{
    public static bool OnKorrektneNimi(string? nimi) =>
        !string.IsNullOrWhiteSpace(nimi);

    // Kogus peab olema täisarv, 0 või rohkem
    public static bool LoeKogus(string? tekst, out int kogus) =>
        int.TryParse(tekst, out kogus) && kogus >= 0;

    // Hind: lubatud on nii koma kui punkt (2,50 või 2.50)
    public static bool LoeHind(string? tekst, out decimal hind) =>
        decimal.TryParse((tekst ?? "").Replace(',', '.'), NumberStyles.Number,
                         CultureInfo.InvariantCulture, out hind) && hind >= 0;
}