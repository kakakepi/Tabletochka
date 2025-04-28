using Microsoft.EntityFrameworkCore;

namespace T4bJl3T04K4
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var optionsBuilder = new DbContextOptionsBuilder<T4bJl3T04K4Db>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tabletochka;Username=postgres;Password=MNXAKER123;Search Path=tabletochka");

            using (var db = new T4bJl3T04K4Db(optionsBuilder.Options))
            {
                var loginForm = new AutorisationForm(db);
                Application.Run(loginForm);
            }
        }
    }
}