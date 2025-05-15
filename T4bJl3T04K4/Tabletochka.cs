using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using NLog; // Логирование с помощью NLog
using System.Threading;   // Для SemaphoreSlim

namespace T4bJl3T04K4
{
    public partial class Tabletochka : Form
    {
        // Статический логгер для данного класса.
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Используется общий экземпляр DbContext, переданный из Program.
        private readonly T4bJl3T04K4Db dataBase;
        // После авторизации текущий пользователь хранится по его идентификатору.
        private Guid currentUserId;
        // Семафор для синхронного доступа к dataBase.
        private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

        // Конструктор формы принимает уже созданный DbContext.
        public Tabletochka(T4bJl3T04K4Db db)
        {
            InitializeComponent();
            dataBase = db;
            logger.Info("Форма Tabletochka инициализирована.");
            InitializeWebViewAsync();
        }

        private async void InitializeWebViewAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);
                webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
                var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "LoginRegistration.html");
                webView.Source = new Uri(htmlPath);
                logger.Info("WebView2 успешно инициализирован и загружена страница LoginRegistration.html.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при инициализации WebView2.");
                SendError("Ошибка инициализации: " + ex.Message);
            }
        }

        private async void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs receivedArgs)
        {
            var json = receivedArgs.WebMessageAsJson;
            var baseData = System.Text.Json.JsonSerializer.Deserialize<BaseData>(json);
            switch (baseData.action)
            {
                case "login":
                    // Ожидается, что LoginData содержит поля username и password.
                    var loginData = System.Text.Json.JsonSerializer.Deserialize<LoginData>(json);
                    logger.Info("Получено сообщение для входа. Пользователь: {0}", loginData.username);
                    await LoginAsync(loginData);
                    break;
                case "register":
                    var registerData = System.Text.Json.JsonSerializer.Deserialize<RegisterData>(json);
                    logger.Info("Получено сообщение для регистрации. Новый пользователь: {0}", registerData.username);
                    await RegisterAsync(registerData);
                    break;
            }
        }

        /// <summary>
        /// Авторизация происходит по username.
        /// </summary>
        private async Task LoginAsync(LoginData loginData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(loginData.username))
                {
                    SendError("Имя пользователя не может быть пустым");
                    return;
                }

                // Поиск пользователя по username.
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Username == loginData.username);
                if (user == null)
                {
                    logger.Warn("Пользователь с именем {0} не найден.", loginData.username);
                    SendError("Пользователь не найден");
                    return;
                }

                var inputHash = HashPassword(loginData.password, user.Salt);
                if (inputHash != user.PasswordHash)
                {
                    // Регистрируем неудачную попытку входа.
                    dataBase.LoginHistories.Add(new LoginHistory
                    {
                        UserId = user.Id,
                        LoginTime = DateTime.UtcNow,
                        IsSuccessful = false
                    });
                    await dataBase.SaveChangesAsync();
                    logger.Warn("Ошибка входа: неверный пароль для пользователя {0}", loginData.username);
                    SendError("Неверное имя пользователя или пароль");
                    return;
                }
                // Записываем успешную авторизацию.
                currentUserId = user.Id;
                dataBase.LoginHistories.Add(new LoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IsSuccessful = true
                });
                await dataBase.SaveChangesAsync();
                logger.Info("Пользователь {0} успешно авторизовался.", loginData.username);
                LoadUserCabinet();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при авторизации пользователя {0}", loginData.username);
                SendError("Ошибка входа: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        /// <summary>
        /// Регистрация пользователя.
        /// Поиск дублирования осуществляется по username.
        /// После регистрации отправляется сообщение с идентификатором нового пользователя.
        /// </summary>
        private async Task RegisterAsync(RegisterData registerData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(registerData.username) || string.IsNullOrWhiteSpace(registerData.password))
                {
                    SendError("Имя пользователя и пароль не могут быть пустыми");
                    return;
                }
                if (registerData.password != registerData.repeatPassword)
                {
                    SendError("Пароли не совпадают");
                    return;
                }
                if (registerData.password.Length < 8)
                {
                    SendError("Пароль должен быть не менее 8 символов");
                    return;
                }
                if (await dataBase.Users.AnyAsync(u => u.Username == registerData.username))
                {
                    logger.Warn("Попытка регистрации с уже существующим именем пользователя {0}.", registerData.username);
                    SendError("Пользователь с таким именем уже существует");
                    return;
                }
                var salt = GenerateSalt();
                var passwordHash = HashPassword(registerData.password, salt);
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = registerData.username,
                    FirstName = "",
                    LastName = "",
                    Gender = false,
                    Picture = string.Empty,
                    DateOfBirth = DateTime.UtcNow,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    CreatedAt = DateTime.UtcNow,
                    Admin = false
                };
                await dataBase.Users.AddAsync(newUser);
                await dataBase.SaveChangesAsync();
                logger.Info("Новый пользователь {0} успешно зарегистрирован. Id: {1}", registerData.username, newUser.Id);
                // Передаём пользователю его ID для будущей авторизации.
                SendSuccess("Регистрация прошла успешно. Ваш идентификатор: " + newUser.Id);
            }
            catch (DbUpdateException dbEx)
            {
                var innerMsg = dbEx.InnerException?.Message;
                logger.Error(dbEx, "Ошибка регистрации при сохранении пользователя {0}.", registerData.username);
                SendError("Ошибка регистрации (DBUpdate): " + (innerMsg ?? dbEx.Message));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Общая ошибка регистрации для пользователя {0}.", registerData.username);
                SendError("Ошибка регистрации: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        /// <summary>
        /// Обновление профиля пользователя по currentUserId.
        /// </summary>
        public async Task UpdateProfileAsync(ProfileData profileData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null)
                {
                    SendError("Пользователь не найден");
                    return;
                }
                user.Username = profileData.Username;
                user.FirstName = profileData.Firstname;
                user.LastName = profileData.Lastname;
                user.Gender = !string.IsNullOrEmpty(profileData.Gender) &&
                                profileData.Gender.Equals("male", StringComparison.OrdinalIgnoreCase);
                user.DateOfBirth = DateTime.Parse(profileData.Birthdate);
                if (!string.IsNullOrEmpty(profileData.OldPassword) && !string.IsNullOrEmpty(profileData.NewPassword))
                {
                    var oldPassHash = HashPassword(profileData.OldPassword, user.Salt);
                    if (oldPassHash != user.PasswordHash)
                    {
                        SendError("Неверный старый пароль");
                        return;
                    }
                    user.PasswordHash = HashPassword(profileData.NewPassword, user.Salt);
                }
                await dataBase.SaveChangesAsync();
                logger.Info("Профиль пользователя {0} успешно обновлен.", user.Username);
                var userData = new
                {
                    id = user.Id,
                    username = user.Username,
                    firstname = user.FirstName,
                    lastname = user.LastName,
                    gender = user.Gender,
                    dateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : "",
                    picture = user.Picture
                };
                webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
                {
                    type = "success",
                    message = "Профиль обновлен",
                    user = userData
                }));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка обновления профиля для пользователя с id: {0}", currentUserId);
                SendError("Ошибка обновления профиля: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        /// <summary>
        /// Загрузка фото осуществляется для пользователя по currentUserId.
        /// </summary>
        public async Task UploadPhotoAsync(string base64Image)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null) return;
                user.Picture = base64Image;
                await dataBase.SaveChangesAsync();
                logger.Info("Фото пользователя {0} успешно обновлено.", user.Username);
                webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
                {
                    type = "photo-updated",
                    picture = $"data:image/jpeg;base64,{base64Image}"
                }));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка загрузки фото для пользователя с id: {0}", currentUserId);
                SendError("Ошибка загрузки фото: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        /// <summary>
        /// Удаление фото пользователя по currentUserId.
        /// </summary>
        private async Task DeletePhotoAsync()
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null) return;
                user.Picture = null;
                await dataBase.SaveChangesAsync();
                logger.Info("Фото пользователя {0} успешно удалено.", user.Username);
                webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
                {
                    type = "photo-deleted"
                }));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка удаления фото для пользователя с id: {0}", currentUserId);
                SendError("Ошибка удаления фото: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async void WebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = e.WebMessageAsJson;
            var baseData = JsonConvert.DeserializeObject<BaseProfileAction>(json);
            switch (baseData.action)
            {
                case "updateProfile":
                    var profileData = JsonConvert.DeserializeObject<ProfileData>(json);
                    await UpdateProfileAsync(profileData);
                    break;
                case "uploadPhoto":
                    var uploadData = JsonConvert.DeserializeObject<UploadPhotoData>(json);
                    await UploadPhotoAsync(uploadData.file);
                    break;
                case "deletePhoto":
                    await DeletePhotoAsync();
                    break;
            }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && currentUserId != Guid.Empty)
            {
                await _dbSemaphore.WaitAsync();
                try
                {
                    var user = await dataBase.Users.FindAsync(currentUserId);
                    if (user != null)
                    {
                        var userData = new
                        {
                            id = user.Id,
                            username = user.Username,
                            firstname = user.FirstName ?? "",
                            lastname = user.LastName ?? "",
                            gender = user.Gender,
                            dateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : "",
                            picture = user.Picture ?? ""
                        };
                        webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(userData));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ошибка загрузки данных пользователя с id: {0}", currentUserId);
                    SendError("Ошибка загрузки данных пользователя: " + ex.Message);
                }
                finally
                {
                    _dbSemaphore.Release();
                }
            }
        }

        /// <summary>
        /// Генерация случайной соли для хэширования пароля.
        /// </summary>
        private string GenerateSalt()
        {
            var salt = new byte[32];
            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// Вычисление хэша пароля с использованием соли.
        /// </summary>
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }
        private void SendError(string message)
        {
            webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { type = "error", message }));
        }

        private void SendSuccess(string message)
        {
            webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { type = "success", message }));
        }

        /// <summary>
        /// Загрузка кабинета пользователя (страница Cabinet.html) и перенастройка обработчиков WebView.
        /// </summary>
        private void LoadUserCabinet()
        {
            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "Cabinet.html");
            webView.Source = new Uri(htmlPath);
            webView.CoreWebView2.WebMessageReceived -= WebView_WebMessageReceived;
            webView.CoreWebView2.WebMessageReceived += WebView2_WebMessageReceived;
            webView.NavigationCompleted += WebView_NavigationCompleted;
            logger.Info("Переход на страницу кабинета пользователя.");
        }
    }
}
