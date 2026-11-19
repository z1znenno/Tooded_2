namespace Tooded_2
{
    /// <summary>Algtoodete piltide lingid internetis (toote nimi -> pildi URL).</summary>
    public static class AlgPildid
    {
        public static readonly Dictionary<string, string> Lingid = new()
        {
            { "Piim 2,5% 1 l",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0e/Milk_glass.jpg/500px-Milk_glass.jpg" },
            { "Juust Eesti 400 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c2/Emmentaler.jpg/500px-Emmentaler.jpg" },
            { "Või 82% 200 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fd/Western-pack-butter.jpg/500px-Western-pack-butter.jpg" },
            { "Maasikajogurt 380 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f8/2019-02-19_20_10_42_A_cup_of_Chobani_Greek_Yogurt_with_Strawberry_on_the_Bottom_before_being_opened_in_the_Franklin_Farm_section_of_Oak_Hill%2C_Fairfax_County%2C_Virginia.jpg/500px-2019-02-19_20_10_42_A_cup_of_Chobani_Greek_Yogurt_with_Strawberry_on_the_Bottom_before_being_opened_in_the_Franklin_Farm_section_of_Oak_Hill%2C_Fairfax_County%2C_Virginia.jpg" },
            { "Rukkileib 500 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3f/Dark_rye_bread.JPG/500px-Dark_rye_bread.JPG" },
            { "Sai 400 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1f/White_bread.jpg/500px-White_bread.jpg" },
            { "Croissant",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2a/Croissant-Petr_Kratochvil.jpg/500px-Croissant-Petr_Kratochvil.jpg" },
            { "Banaanid 1 kg",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/4/44/Bananas_white_background_DS.jpg/500px-Bananas_white_background_DS.jpg" },
            { "Õunad 1 kg",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Red_Apple.jpg/500px-Red_Apple.jpg" },
            { "Tomatid 500 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/8/89/Tomato_je.jpg/500px-Tomato_je.jpg" },
            { "Kurk",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/9/96/ARS_cucumber.jpg/500px-ARS_cucumber.jpg" },
            { "Kohvioad 500 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Roasted_coffee_beans.jpg/500px-Roasted_coffee_beans.jpg" },
            { "Apelsinimahl 1 l",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/0/05/Orangejuice.jpg/500px-Orangejuice.jpg" },
            { "Mineraalvesi 1,5 l",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Plastic_Water_Bottle.jpg/500px-Plastic_Water_Bottle.jpg" },
            { "Piimašokolaad 100 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/9/90/Milka_Alpine_Milk_Chocolate_bar_100g.jpg/500px-Milka_Alpine_Milk_Chocolate_bar_100g.jpg" },
            { "Küpsised 200 g",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/1/16/Choc-Chip-Cookie.jpg/500px-Choc-Chip-Cookie.jpg" },
            { "Juhtmevabad kõrvaklapid",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/0/01/AirPods.jpg/500px-AirPods.jpg" },
            { "Nutikell",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Apple_Watch-.jpg/500px-Apple_Watch-.jpg" },
            { "USB-C laadija 20 W",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/8/86/Baseus_CCCP20UE_20W_USB_charger-0.jpg/500px-Baseus_CCCP20UE_20W_USB_charger-0.jpg" },
            { "Kohvimasin",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6b/Coffee_maker_Krups.jpg/500px-Coffee_maker_Krups.jpg" },
            { "LED lauavalgusti",
              "https://upload.wikimedia.org/wikipedia/commons/thumb/7/71/Concise_bamboo_eye_protection_LED_desk_lamp.jpg/500px-Concise_bamboo_eye_protection_LED_desk_lamp.jpg" },
        };

        public static string Url(string toode) => Lingid.TryGetValue(toode, out var url) ? url : "";
    }
}
