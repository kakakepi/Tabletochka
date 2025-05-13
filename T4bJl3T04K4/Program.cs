using System.Diagnostics;
using System.Resources;
using Microsoft.EntityFrameworkCore;
using T4bJl3T04K4.Properties;

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
            EnvReader.Load("../../../../.env");
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_HOST")))
            {
                MessageBox.Show(Resources.conString, Resources.errorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Process.GetCurrentProcess().Kill();
            }
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var username = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var database = Environment.GetEnvironmentVariable("DB_NAME");

            var connectionString = $"Host={host};Port={port};Username={username};" +
                    $"Password={password};Database={database}";
            var optionsBuilder = new DbContextOptionsBuilder<T4bJl3T04K4Db>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var db = new T4bJl3T04K4Db(optionsBuilder.Options))
            {
                Application.Run(new Tabletochka(db));
            }
        }
    }
}