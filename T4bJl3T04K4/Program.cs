using System;
using System.Diagnostics;
using System.Resources;
using Microsoft.EntityFrameworkCore;
using T4bJl3T04K4.Properties;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace T4bJl3T04K4
{
    internal static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Главная точка входа в приложение.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogManager.GetCurrentClassLogger().Error(ex, "Unhandled exception");
                MessageBox.Show(ex?.Message, "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);
            config.AddRuleForAllLevels(consoleTarget);
            LogManager.Configuration = config;

            logger.Info("Приложение запущено.");
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
            logger.Info("Подключение к БД установлено. Используем строку подключения: {0}", connectionString);
            using (var db = new T4bJl3T04K4Db(optionsBuilder.Options))
            {
                Application.Run(new Tabletochka(db));
            }
        }
    }
}
