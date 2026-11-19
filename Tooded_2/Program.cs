namespace Tooded_2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Algandmed (kategooriad ja tooted), kui andmebaasis neid veel pole
            try
            {
                Seeder.Kaivita();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Keel.T("Algandmete lisamine ebaõnnestus: ",
                    "Не удалось добавить начальные данные: ",
                    "Failed to add initial data: ") + (ex.InnerException?.Message ?? ex.Message));
            }

            // Kui user.txt-s on salvestatud kasutaja, avame kohe Avavormi
            bool sisseLogitud = false;
            try
            {
                sisseLogitud = Session.TryLoad();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Keel.T("Andmebaasiga ei saa ühendust: ",
                    "Не удалось подключиться к базе данных: ",
                    "Cannot connect to the database: ") + ex.Message);
            }

            if (!sisseLogitud)
            {
                using var login = new Login();
                if (login.ShowDialog() != DialogResult.OK)
                    return; // kasutaja sulges sisselogimise akna
            }

            Application.Run(new Avavorm());
        }
    }
}
