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

            // Kui user.txt-s on salvestatud kasutaja, avame kohe Avavormi
            bool sisseLogitud = false;
            try
            {
                sisseLogitud = Session.TryLoad();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasiga ei saa ühendust: " + ex.Message);
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
