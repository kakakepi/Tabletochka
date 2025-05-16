using System.Security.Cryptography;
using System.Text;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace T4bJl3T04K4
{
    public partial class Tabletochka : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly T4bJl3T04K4Db dataBase;
        private Guid currentUserId;
        private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);
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
                    var loginData = System.Text.Json.JsonSerializer.Deserialize<LoginData>(json);
                    logger.Info("Получено сообщение для входа. Пользователь: {0}", loginData.username);
                    await LoginAsync(loginData);
                    break;

                case "register":
                    var registerData = System.Text.Json.JsonSerializer.Deserialize<RegisterData>(json);
                    logger.Info("Получено сообщение для регистрации. Новый пользователь: {0}", registerData.username);
                    await RegisterAsync(registerData);
                    break;

                case "getSymptoms":
                    var systemName = System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("system").GetString();
                    await HandleGetSymptoms(systemName);
                    break;


                case "diagnose":
                    var diagnoseData = JsonConvert.DeserializeObject<DiagnoseRequest>(json);
                    await HandleDiagnose(diagnoseData);
                    break;

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

                case "deleteAccount":
                    await DeleteAccountAsync();
                    break;

                case "getSearchHistory":
                    await GetSearchHistoryAsync();
                    break;

                case "logout":
                    await LogoutAsync();
                    break;

            }
        }
        private async Task GetSearchHistoryAsync()
        {
            var histories = await dataBase.SearchHistories
            .Where(sh => sh.UserId == currentUserId)
            .Include(sh => sh.SearchHistorySymptoms)
                .ThenInclude(shs => shs.Symptom)
            .ToListAsync();

            var result = histories.Select(sh => new
            {
                searchDate = sh.SearchDate,
                searchHistoryText = string.Join(", ", sh.SearchHistorySymptoms.Select(shs => shs.Symptom.NameRu))
            }).ToList();

            var json = JsonConvert.SerializeObject(new { type = "searchHistoryData", data = result });
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }

        /// <summary>
        /// Авторизация происходит по username.
        /// </summary>
        public async Task LoginAsync(LoginData loginData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(loginData.username))
                {
                    SendError("Имя пользователя не может быть пустым");
                    return;
                }
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
        public async Task RegisterAsync(RegisterData registerData)
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

                currentUserId = newUser.Id;
                LoadUserCabinet();
                logger.Info("Новый пользователь {0} успешно зарегистрирован. Id: {1}", registerData.username, newUser.Id);
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
                if (!DateTime.TryParse(profileData.Birthdate, out var parsedDate))
                {
                    SendError("Неверный формат даты рождения");
                    return;
                }
                user.DateOfBirth = parsedDate.ToUniversalTime();




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
        public async Task DeletePhotoAsync()
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
        private void LoadLoginPage()
        {
            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "LoginRegistration.html");
            webView.Source = new Uri(htmlPath);
            logger.Info("Переход на страницу авторизации после удаления учётной записи.");
        }

        public async Task DeleteAccountAsync()
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

                var loginHistories = dataBase.LoginHistories.Where(lh => lh.UserId == currentUserId);
                dataBase.LoginHistories.RemoveRange(loginHistories);

                var searchHistories = dataBase.SearchHistories.Where(sh => sh.UserId == currentUserId);
                dataBase.SearchHistories.RemoveRange(searchHistories);
                dataBase.Users.Remove(user);
                await dataBase.SaveChangesAsync();

                logger.Info("Пользователь {0} удален.", user.Username);
                currentUserId = Guid.Empty;

                SendSuccess("Учётная запись удалена");
                LoadLoginPage();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка удаления аккаунта для пользователя с Id: {0}", currentUserId);
                SendError("Ошибка удаления аккаунта: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
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
                            firstname = user.FirstName ?? string.Empty,
                            lastname = user.LastName ?? string.Empty,
                            gender = user.Gender,
                            dateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                            picture = user.Picture ?? string.Empty
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
        public string GenerateSalt()
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
        public string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }
        protected void SendError(string message)
        {
            webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { type = "error", message }));
        }

        protected void SendSuccess(string message)
        {
            webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { type = "success", message }));
        }

        /// <summary>
        /// Загрузка кабинета пользователя (страница Cabinet.html) и перенастройка обработчиков WebView.
        /// </summary>
        protected void LoadUserCabinet()
        {
            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "Cabinet.html");
            webView.Source = new Uri(htmlPath);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            logger.Info("Переход на страницу кабинета пользователя.");
        }
        private async Task HandleGetSymptoms(string system)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var symptoms = await dataBase.SystemsSymptoms
                    .Where(ss => ss.SystemName == system)
                    .Join(dataBase.Symptoms,
                          ss => ss.SymptomId,
                          s => s.Id,
                          (ss, s) => new { id = s.Id, name = s.NameRu })
                    .ToListAsync();

                var json = JsonConvert.SerializeObject(new
                {
                    type = "symptoms",
                    data = symptoms
                });

                webView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при получении симптомов для системы {0}", system);
                SendError("Ошибка загрузки симптомов: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }


        private async Task HandleDiagnose(DiagnoseRequest request)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var selectedSymptomIds = request.SelectedSymptomIds;

                var newSearchHistory = new SearchHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    SearchDate = DateTime.UtcNow
                };
                await dataBase.SearchHistories.AddAsync(newSearchHistory);

                foreach (var symptomId in selectedSymptomIds)
                {
                    var shSymptom = new SearchHistorySymptom
                    {
                        SearchHistoryId = newSearchHistory.Id,
                        SymptomId = symptomId
                    };
                    await dataBase.SearchHistorySymptoms.AddAsync(shSymptom);
                }

                await dataBase.SaveChangesAsync();

                var diagnoses = await dataBase.Diseases
                    .Select(d => new
                    {
                        d.Id,
                        Name = d.NameRu,
                        SymptomIds = d.DiseaseSymptoms.Select(ds => ds.SymptomId).ToList()
                    })
                    .ToListAsync();

                var matched = diagnoses
                    .Select(d => new
                    {
                        d.Name,
                        MatchCount = d.SymptomIds.Intersect(selectedSymptomIds.Select(id => id)).Count(),
                        Total = d.SymptomIds.Count
                    })
                    .Where(d => d.MatchCount > 0)
                    .OrderByDescending(d => d.MatchCount)
                    .ToList();

                var json = JsonConvert.SerializeObject(new
                {
                    type = "diagnosis",
                    results = matched
                });

                webView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при диагностике");
                SendError("Ошибка диагностики: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task LogoutAsync()
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                logger.Info("Пользователь с Id {0} вышел из системы.", currentUserId);
                currentUserId = Guid.Empty;
                LoadLoginPage();
                SendSuccess("Вы успешно вышли из системы");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при выходе пользователя с Id: {0}", currentUserId);
                SendError("Ошибка выхода: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

    }
}
