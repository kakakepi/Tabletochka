using System.Diagnostics;
using T4bJl3T04K4.Properties;

namespace T4bJl3T04K4
{
    /// <summary>
    /// Вспомогательный класс для чтения переменных окружения из файла .env
    /// </summary>
    /// <remarks>
    /// Реализует следующие функции:
    /// - Проверку существования файла .env
    /// - Чтение и парсинг файла .env
    /// - Установку переменных окружения в текущем процессе
    /// - Обработку ошибок формата файла
    /// </remarks>
    class EnvReader
    {
        /// <summary>
        /// Загружает переменные окружения из указанного файла
        /// </summary>
        /// <param name="filePath">Путь к файлу .env</param>
        /// <remarks>
        /// Логика работы метода:
        /// 1. Проверяет существование файла (если не существует - показывает ошибку)
        /// 2. Читает файл построчно
        /// 3. Пропускает пустые строки и комментарии (начинающиеся с #)
        /// 4. Разбивает каждую строку по первому символу '='
        /// 5. Устанавливает переменные окружения в текущем процессе
        /// 6. Игнорирует строки некорректного формата
        /// </remarks>
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(Resources.fileDoesntExist, Resources.errorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Process.GetCurrentProcess().Kill();
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                {
                    continue;
                }
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

}
