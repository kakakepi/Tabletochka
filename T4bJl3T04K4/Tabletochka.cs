using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace T4bJl3T04K4
{
    public partial class Tabletochka : Form
    {
        private static readonly Logger logger = LogManager.Setup()
            .LoadConfigurationFromFile("nlog.config")
            .GetCurrentClassLogger();
        private readonly T4bJl3T04K4Db dataBase;
        private Guid currentUserId;
        private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Конструктор формы Tabletochka. Инициализирует компоненты, базу данных и WebView.
        /// </summary>
        public Tabletochka(T4bJl3T04K4Db db)
        {
            logger.Debug(Properties.Resources.EnterConstructor);
            InitializeComponent();
            dataBase = db;
            logger.Info(Properties.Resources.FormInitialized);
            InitializeWebViewAsync();
            logger.Debug(Properties.Resources.ExitConstructor);
        }

        /// <summary>
        /// Асинхронная инициализация WebView, загрузка HTML-страницы.
        /// </summary>
        private async void InitializeWebViewAsync()
        {
            logger.Debug(Properties.Resources.EnterInitializeWebViewAsync);
            try
            {
                await webView.EnsureCoreWebView2Async(null);
                var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "LoginRegistration.html");
                webView.Source = new Uri(htmlPath);
                logger.Info(Properties.Resources.WebViewLoaded);
            }
            catch (Exception ex)
            {
                logger.Error(ex, Properties.Resources.ErrorInitializingWebView);
                SendError(Properties.Resources.ErrorInitializingWebView + ": " + ex.Message);
            }
            logger.Debug(Properties.Resources.ExitInitializeWebViewAsync);
        }

        /// <summary>
        /// Обработчик сообщений из WebView. Выполняет действия на основе полученного JSON.
        /// </summary>
        private async void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs receivedArgs)
        {
            logger.Debug(Properties.Resources.EnterWebMessageReceived);
            var json = receivedArgs.WebMessageAsJson;
            logger.Debug(string.Format(Properties.Resources.ReceivedJsonMessage, json));
            var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var baseData = System.Text.Json.JsonSerializer.Deserialize<BaseData>(json);
            logger.Debug(string.Format(Properties.Resources.ReceivedAction, baseData.action));

            switch (baseData.action)
            {
                case "login":
                    logger.Debug(Properties.Resources.ProcessActionLogin);
                    var loginData = System.Text.Json.JsonSerializer.Deserialize<LoginData>(json);
                    logger.Info(string.Format(Properties.Resources.ReceivedLoginMessage, loginData.username));
                    await LoginAsync(loginData);
                    break;
                case "register":
                    logger.Debug(Properties.Resources.ProcessActionRegister);
                    var registerData = System.Text.Json.JsonSerializer.Deserialize<RegisterData>(json);
                    logger.Info(string.Format(Properties.Resources.ReceivedRegisterMessage, registerData.username));
                    await RegisterAsync(registerData);
                    break;
                case "getSymptoms":
                    logger.Debug(Properties.Resources.ProcessGetSymptoms);
                    var systemName = System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("system").GetString();
                    await HandleGetSymptoms(systemName);
                    break;
                case "diagnose":
                    logger.Debug(Properties.Resources.ProcessDiagnose);
                    var diagnoseData = JsonConvert.DeserializeObject<DiagnoseRequest>(json);
                    await HandleDiagnose(diagnoseData);
                    break;
                case "updateProfile":
                    logger.Debug(Properties.Resources.ProcessUpdateProfile);
                    var profileData = JsonConvert.DeserializeObject<ProfileData>(json);
                    await UpdateProfileAsync(profileData);
                    break;
                case "uploadPhoto":
                    logger.Debug(Properties.Resources.ProcessUploadPhoto);
                    var uploadData = JsonConvert.DeserializeObject<UploadPhotoData>(json);
                    await UploadPhotoAsync(uploadData.file);
                    break;
                case "deletePhoto":
                    logger.Debug(Properties.Resources.ProcessDeletePhoto);
                    await DeletePhotoAsync();
                    break;
                case "deleteAccount":
                    logger.Debug(Properties.Resources.ProcessDeleteAccount);
                    await DeleteAccountAsync();
                    break;
                case "getSearchHistory":
                    logger.Debug(Properties.Resources.ProcessGetSearchHistory);
                    await GetSearchHistoryAsync();
                    break;
                case "logout":
                    logger.Debug(Properties.Resources.ProcessLogout);
                    await LogoutAsync();
                    break;
                case "getDiseaseList":
                    logger.Debug(Properties.Resources.ProcessGetDiseaseList);
                    await HandleGetDiseaseList();
                    break;
                case "getSymptomList":
                    logger.Debug(Properties.Resources.ProcessGetSymptomList);
                    await HandleGetSymptomList();
                    break;
                case "addDisease":
                    logger.Debug(Properties.Resources.ProcessAddDisease);
                    await HandleAddOrUpdateDisease(message, isUpdate: false);
                    break;
                case "updateDisease":
                    logger.Debug(Properties.Resources.ProcessUpdateDisease);
                    await HandleAddOrUpdateDisease(message, isUpdate: true);
                    break;
                case "deleteDisease":
                    logger.Debug(Properties.Resources.ProcessDeleteDisease);
                    await HandleDeleteDisease(message);
                    break;
                case "addSymptom":
                    logger.Debug(Properties.Resources.ProcessAddSymptom);
                    await HandleAddOrUpdateSymptom(message, isUpdate: false);
                    break;
                case "updateSymptom":
                    logger.Debug(Properties.Resources.ProcessUpdateSymptom);
                    await HandleAddOrUpdateSymptom(message, isUpdate: true);
                    break;
                case "deleteSymptom":
                    logger.Debug(Properties.Resources.ProcessDeleteSymptom);
                    await HandleDeleteSymptom(message);
                    break;
                default:
                    logger.Warn(string.Format(Properties.Resources.UnknownAction, baseData.action));
                    break;
            }
            logger.Debug(Properties.Resources.ExitWebMessageReceived);
        }

        /// <summary>
        /// Загружает историю поиска для текущего пользователя из базы данных и отправляет в WebView.
        /// </summary>
        private async Task GetSearchHistoryAsync()
        {
            logger.Debug(Properties.Resources.EnterGetSearchHistoryAsync);
            var histories = await dataBase.SearchHistories
                .Where(sh => sh.UserId == currentUserId)
                .Include(sh => sh.SearchHistorySymptoms)
                .ThenInclude(shs => shs.Symptom)
                .ToListAsync();
            logger.Debug(string.Format(Properties.Resources.SearchHistoryCount, histories.Count, currentUserId));
            var result = histories.Select(sh => new
            {
                searchDate = sh.SearchDate,
                searchHistoryText = string.Join(", ", sh.SearchHistorySymptoms.Select(shs => shs.Symptom.NameRu))
            }).ToList();
            var json = JsonConvert.SerializeObject(new { type = "searchHistoryData", data = result });
            webView.CoreWebView2.PostWebMessageAsJson(json);
            logger.Debug(Properties.Resources.ExitGetSearchHistoryAsync);
        }

        /// <summary>
        /// Выполняет авторизацию пользователя по имени.
        /// </summary>
        public async Task LoginAsync(LoginData loginData)
        {
            logger.Debug(string.Format(Properties.Resources.EnterLoginAsync, loginData.username));
            await _dbSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(loginData.username))
                {
                    logger.Warn(Properties.Resources.EmptyUsername);
                    SendError(Properties.Resources.EmptyUsername);
                    return;
                }
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Username == loginData.username);
                if (user == null)
                {
                    logger.Warn(Properties.Resources.UserNotFound);
                    SendError(Properties.Resources.UserNotFound);
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
                    logger.Warn(Properties.Resources.WrongPassword);
                    SendError(Properties.Resources.WrongPassword);
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
                logger.Info(string.Format(Properties.Resources.LoginSuccess, loginData.username));
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
                logger.Debug("Семафор освобожден в LoginAsync.");
            }
            logger.Debug(string.Format(Properties.Resources.ExitLoginAsync, loginData.username));
        }

        /// <summary>
        /// Регистрирует нового пользователя, проверяя входные данные и дублирование имени.
        /// </summary>
        public async Task RegisterAsync(RegisterData registerData)
        {
            logger.Debug(string.Format(Properties.Resources.EnterRegisterAsync, registerData.username));
            await _dbSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(registerData.username) || string.IsNullOrWhiteSpace(registerData.password))
                {
                    logger.Warn(Properties.Resources.EmptyUsername);
                    SendError(Properties.Resources.EmptyUsername);
                    return;
                }
                if (registerData.password != registerData.repeatPassword)
                {
                    logger.Warn(Properties.Resources.PasswordsNotMatch);
                    SendError(Properties.Resources.PasswordsNotMatch);
                    return;
                }
                if (registerData.password.Length < 8)
                {
                    logger.Warn(Properties.Resources.InsufficientPasswordLength);
                    SendError(Properties.Resources.InsufficientPasswordLength);
                    return;
                }
                if (await dataBase.Users.AnyAsync(u => u.Username == registerData.username))
                {
                    logger.Warn(Properties.Resources.UserAlreadyExists);
                    SendError(Properties.Resources.UserAlreadyExists);
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
                logger.Info(string.Format(Properties.Resources.RegistrationSuccess, newUser.Id));
                SendSuccess(string.Format(Properties.Resources.RegistrationSuccess, newUser.Id));
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
                logger.Debug("Семафор освобожден в RegisterAsync.");
            }
            logger.Debug(string.Format(Properties.Resources.ExitRegisterAsync, registerData.username));
        }

        /// <summary>
        /// Обновляет профиль пользователя, включая смену пароля, если требуется.
        /// </summary>
        public async Task UpdateProfileAsync(ProfileData profileData)
        {
            logger.Debug(string.Format(Properties.Resources.EnterUpdateProfileAsync, profileData.Username));
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null)
                {
                    logger.Warn(string.Format(Properties.Resources.ProfileUserNotFound, currentUserId));
                    SendError(Properties.Resources.UserNotFound);
                    return;
                }
                user.Username = profileData.Username;
                user.FirstName = profileData.Firstname;
                user.LastName = profileData.Lastname;
                user.Gender = !string.IsNullOrEmpty(profileData.Gender) &&
                              profileData.Gender.Equals("male", StringComparison.OrdinalIgnoreCase);
                if (!DateTime.TryParse(profileData.Birthdate, out var parsedDate))
                {
                    logger.Warn(string.Format(Properties.Resources.InvalidBirthdateFormat, profileData.Username));
                    SendError("Неверный формат даты рождения");
                    return;
                }
                user.DateOfBirth = parsedDate.ToUniversalTime();
                if (!string.IsNullOrEmpty(profileData.OldPassword) && !string.IsNullOrEmpty(profileData.NewPassword))
                {
                    var oldPassHash = HashPassword(profileData.OldPassword, user.Salt);
                    if (oldPassHash != user.PasswordHash)
                    {
                        logger.Warn(string.Format(Properties.Resources.WrongOldPassword, profileData.Username));
                        SendError("Неверный старый пароль");
                        return;
                    }
                    user.PasswordHash = HashPassword(profileData.NewPassword, user.Salt);
                    logger.Debug(string.Format(Properties.Resources.PasswordUpdated, profileData.Username));
                }
                await dataBase.SaveChangesAsync();
                logger.Info(string.Format(Properties.Resources.ProfileUpdated, user.Username));
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
                logger.Debug(string.Format(Properties.Resources.ExitUpdateProfileAsync, profileData.Username));
            }
        }

        /// <summary>
        /// Загружает фото пользователя, обновляя запись в базе данных.
        /// </summary>
        public async Task UploadPhotoAsync(string base64Image)
        {
            logger.Debug(string.Format(Properties.Resources.EnterUploadPhotoAsync, base64Image?.Length ?? 0));
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null)
                {
                    logger.Warn(string.Format(Properties.Resources.UserNotFoundUploadPhoto, currentUserId));
                    return;
                }
                user.Picture = base64Image;
                await dataBase.SaveChangesAsync();
                logger.Info(string.Format("Фото обновлено", user.Username));
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
                logger.Debug(Properties.Resources.UploadPhotoSemaphoreReleased);
            }
        }

        /// <summary>
        /// Удаляет фото пользователя, обновляя запись в базе данных.
        /// </summary>
        public async Task DeletePhotoAsync()
        {
            logger.Debug(Properties.Resources.EnterDeletePhotoAsync);
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null)
                {
                    logger.Warn(string.Format(Properties.Resources.UserNotFoundDeletePhoto, currentUserId));
                    return;
                }
                user.Picture = null;
                await dataBase.SaveChangesAsync();
                logger.Info(string.Format("Фото удалено", user.Username));
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
                logger.Debug(Properties.Resources.DeletePhotoSemaphoreReleased);
            }
        }

        /// <summary>
        /// Загружает страницу входа, когда учетная запись удалена.
        /// </summary>
        private void LoadLoginPage()
        {
            // Для простоты оставляем строки без ресурсов
            logger.Debug("Вход в LoadLoginPage.");
            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "LoginRegistration.html");
            webView.Source = new Uri(htmlPath);
            logger.Info("Переход на страницу авторизации после удаления учётной записи.");
            logger.Debug("Выход из LoadLoginPage.");
        }

        /// <summary>
        /// Удаляет учетную запись текущего пользователя и очищает связанные данные.
        /// </summary>
        public async Task DeleteAccountAsync()
        {
            logger.Debug(string.Format(Properties.Resources.EnterDeleteAccountAsync, currentUserId));
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null)
                {
                    logger.Warn(string.Format(Properties.Resources.UserNotFoundDeleteAccount, currentUserId));
                    SendError(Properties.Resources.UserNotFound);
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
                SendSuccess(Properties.Resources.AccountDeletedSuccess);
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
                logger.Debug(Properties.Resources.DeleteAccountSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitDeleteAccountAsync);
        }

        /// <summary>
        /// Обработчик завершения навигации WebView. Отправляет данные пользователя в WebView.
        /// </summary>
        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            logger.Debug(string.Format(Properties.Resources.EnterNavigationCompleted, e.IsSuccess));
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
                            picture = user.Picture ?? string.Empty,
                            admin = user.Admin
                        };
                        webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(userData));
                        logger.Debug("Данные пользователя отправлены в WebView в WebView_NavigationCompleted.");
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
                    logger.Debug(Properties.Resources.NavigationCompletedSemaphoreReleased);
                }
            }
            logger.Debug(Properties.Resources.ExitNavigationCompleted);
        }

        /// <summary>
        /// Генерирует случайную соль для хэширования.
        /// </summary>
        public string GenerateSalt()
        {
            logger.Debug(Properties.Resources.EnterGenerateSalt);
            var salt = new byte[32];
            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(salt);
            }
            var saltString = Convert.ToBase64String(salt);
            logger.Debug(string.Format(Properties.Resources.SaltGenerated, saltString.Length));
            logger.Debug(Properties.Resources.ExitGenerateSalt);
            return saltString;
        }

        /// <summary>
        /// Вычисляет хэш пароля с использованием соли.
        /// </summary>
        public string HashPassword(string password, string salt)
        {
            logger.Debug(Properties.Resources.EnterHashPassword);
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var hashResult = Convert.ToBase64String(bytes);
                logger.Debug(string.Format(Properties.Resources.HashPasswordCompleted, hashResult.Length));
                logger.Debug(Properties.Resources.ExitHashPassword);
                return hashResult;
            }
        }

        /// <summary>
        /// Отправляет в WebView сообщение об ошибке.
        /// </summary>
        protected virtual void SendError(string message)
        {
            logger.Debug(string.Format(Properties.Resources.SendingErrorMessage, message));
            webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { type = "error", message }));
        }

        /// <summary>
        /// Отправляет в WebView сообщение об успешном выполнении операции.
        /// </summary>
        protected virtual void SendSuccess(string message)
        {
            logger.Debug(string.Format(Properties.Resources.SendingSuccessMessage, message));
            webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { type = "success", message }));
        }

        /// <summary>
        /// Загружает страницу кабинета пользователя.
        /// </summary>
        protected virtual void LoadUserCabinet()
        {
            logger.Debug(Properties.Resources.EnterLoadUserCabinet);
            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "Cabinet.html");
            webView.Source = new Uri(htmlPath);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            logger.Info(Properties.Resources.UserCabinetLoaded);
            logger.Debug(Properties.Resources.ExitLoadUserCabinet);
        }

        /// <summary>
        /// Получает симптомы для заданной системы и отправляет данные в WebView.
        /// </summary>
        private async Task HandleGetSymptoms(string system)
        {
            logger.Debug(string.Format(Properties.Resources.EnterHandleGetSymptoms, system));
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
                logger.Debug(string.Format(Properties.Resources.SymptomsRetrieved, symptoms.Count, system));
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
                SendError(string.Format(Properties.Resources.ErrorLoadingSymptoms, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(string.Format(Properties.Resources.ExitHandleGetSymptoms, system));
            }
        }

        /// <summary>
        /// Выполняет диагностику на основе выбранных симптомов.
        /// </summary>
        private async Task HandleDiagnose(DiagnoseRequest request)
        {
            logger.Debug(string.Format(Properties.Resources.EnterHandleDiagnose, request.SelectedSymptomIds?.Count ?? 0));
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
                logger.Debug(string.Format(Properties.Resources.NewSearchHistoryCreated, newSearchHistory.Id));
                foreach (var symptomId in selectedSymptomIds)
                {
                    var shSymptom = new SearchHistorySymptom
                    {
                        SearchHistoryId = newSearchHistory.Id,
                        SymptomId = symptomId
                    };
                    await dataBase.SearchHistorySymptoms.AddAsync(shSymptom);
                    logger.Debug(string.Format(Properties.Resources.AddedSymptomToHistory, symptomId, newSearchHistory.Id));
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
                logger.Debug(string.Format(Properties.Resources.DiagnosesRetrieved, diagnoses.Count));
                var matched = diagnoses
                    .Select(d => new
                    {
                        d.Name,
                        MatchCount = d.SymptomIds.Intersect(selectedSymptomIds).Count(),
                        Total = d.SymptomIds.Count
                    })
                    .Where(d => d.MatchCount > 0)
                    .OrderByDescending(d => d.MatchCount)
                    .ToList();
                logger.Debug(string.Format(Properties.Resources.DiagnosisComplete, matched.Count));
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
                SendError(string.Format(Properties.Resources.ErrorDiagnose, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug("Семафор освобожден в HandleDiagnose.");
            }
        }

        /// <summary>
        /// Выполняет выход из системы.
        /// </summary>
        public async Task LogoutAsync()
        {
            logger.Debug(string.Format(Properties.Resources.EnterLogoutAsync, currentUserId));
            await _dbSemaphore.WaitAsync();
            try
            {
                logger.Info(string.Format(Properties.Resources.UserLoggedOutInfo, currentUserId));
                currentUserId = Guid.Empty;
                LoadLoginPage();
                SendSuccess(Properties.Resources.AccountDeletedSuccess); // Можно заменить на другое сообщение, если требуется
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при выходе пользователя с Id: {0}", currentUserId);
                SendError("Ошибка выхода: " + ex.Message);
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.LogoutSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitLogoutAsync);
        }

        /// <summary>
        /// Получает список болезней из базы данных и отправляет данные в WebView.
        /// </summary>
        private async Task HandleGetDiseaseList()
        {
            logger.Debug(Properties.Resources.EnterHandleGetDiseaseList);
            await _dbSemaphore.WaitAsync();
            try
            {
                var diseases = await dataBase.Diseases
                    .Select(d => new
                    {
                        d.Id,
                        d.NameRu,
                        d.DescriptionRu,
                        SymptomIds = d.DiseaseSymptoms.Select(ds => ds.SymptomId).ToList()
                    })
                    .ToListAsync();
                logger.Debug(string.Format(Properties.Resources.DiseasesRetrieved, diseases.Count));
                var json = JsonConvert.SerializeObject(new
                {
                    type = "diseaseList",
                    data = diseases.Select(d => new
                    {
                        d.Id,
                        Name = d.NameRu,
                        Description = d.DescriptionRu,
                        SymptomIds = string.Join(",", d.SymptomIds)
                    })
                });
                webView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при получении списка болезней");
                SendError(string.Format(Properties.Resources.ErrorGetDiseaseList, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.HandleGetDiseaseListSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitHandleGetDiseaseList);
        }

        /// <summary>
        /// Получает список симптомов из базы данных и отправляет данные в WebView.
        /// </summary>
        private async Task HandleGetSymptomList()
        {
            logger.Debug(Properties.Resources.EnterHandleGetSymptomList);
            await _dbSemaphore.WaitAsync();
            try
            {
                var symptoms = await dataBase.Symptoms
                    .Select(s => new
                    {
                        s.Id,
                        Name = s.NameRu,
                        DiseaseId = s.DiseaseSymptoms.Select(ds => ds.DiseaseId).FirstOrDefault()
                    })
                    .ToListAsync();
                logger.Debug(string.Format(Properties.Resources.SymptomsRetrievedCount, symptoms.Count));
                var json = JsonConvert.SerializeObject(new
                {
                    type = "symptomList",
                    data = symptoms
                });
                webView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при получении списка симптомов");
                SendError(string.Format(Properties.Resources.ErrorGetSymptomList, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.HandleGetSymptomListSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitHandleGetSymptomList);
        }

        /// <summary>
        /// Обрабатывает добавление или обновление болезни.
        /// Это заглушка – реальную логику необходимо реализовать.
        /// </summary>
        private async Task HandleAddOrUpdateDisease(Dictionary<string, object> message, bool isUpdate)
        {
            logger.Debug(string.Format(Properties.Resources.EnterHandleAddOrUpdateDisease, isUpdate));
            await _dbSemaphore.WaitAsync();
            try
            {
                // Реальная логика работы с заболеванием должна быть здесь.
                // В данной заглушке просто логируем действие и отправляем сообщение об успехе.
                if (isUpdate)
                {
                    logger.Debug(string.Format(Properties.Resources.UpdatingDisease, "??"));
                }
                else
                {
                    logger.Debug(string.Format(Properties.Resources.AddingDisease, "??"));
                }
                // Например: 
                logger.Info(Properties.Resources.DiseaseSaved);
                SendSuccess(Properties.Resources.DiseaseSaved);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при сохранении болезни");
                SendError(string.Format(Properties.Resources.ErrorSaveDisease, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.HandleAddOrUpdateDiseaseSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitHandleAddOrUpdateDisease);
        }

        /// <summary>
        /// Обрабатывает удаление болезни.
        /// Это заглушка – реальную логику необходимо реализовать.
        /// </summary>
        private async Task HandleDeleteDisease(Dictionary<string, object> message)
        {
            logger.Debug(Properties.Resources.EnterHandleDeleteDisease);
            await _dbSemaphore.WaitAsync();
            try
            {
                // Заглушка: логирование и сообщение об успехе
                logger.Info(Properties.Resources.DiseaseDeleted);
                SendSuccess(Properties.Resources.DiseaseDeleted);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при удалении болезни");
                SendError(string.Format(Properties.Resources.ErrorDeleteDisease, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.HandleDeleteDiseaseSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitHandleDeleteDisease);
        }

        /// <summary>
        /// Обрабатывает добавление или обновление симптома.
        /// Это заглушка – реальную логику необходимо реализовать.
        /// </summary>
        private async Task HandleAddOrUpdateSymptom(Dictionary<string, object> message, bool isUpdate)
        {
            logger.Debug(string.Format(Properties.Resources.EnterHandleAddOrUpdateSymptom, isUpdate));
            await _dbSemaphore.WaitAsync();
            try
            {
                // Заглушка – логируем действие и отправляем сообщение об успехе
                if (isUpdate)
                {
                    logger.Debug(string.Format(Properties.Resources.UpdatingSymptom, "??"));
                }
                else
                {
                    logger.Debug(string.Format(Properties.Resources.AddingSymptom, "??"));
                }
                logger.Info(Properties.Resources.SymptomSaved);
                SendSuccess(Properties.Resources.SymptomSaved);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при сохранении симптома");
                SendError(string.Format(Properties.Resources.ErrorSaveSymptom, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.HandleAddOrUpdateSymptomSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitHandleAddOrUpdateSymptom);
        }

        /// <summary>
        /// Обрабатывает удаление симптома.
        /// Это заглушка – реальную логику необходимо реализовать.
        /// </summary>
        private async Task HandleDeleteSymptom(Dictionary<string, object> message)
        {
            logger.Debug(Properties.Resources.EnterHandleDeleteSymptom);
            await _dbSemaphore.WaitAsync();
            try
            {
                // Заглушка – логирование и сообщение об успехе
                logger.Info(Properties.Resources.SymptomDeleted);
                SendSuccess(Properties.Resources.SymptomDeleted);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при удалении симптома");
                SendError(string.Format(Properties.Resources.ErrorDeleteSymptom, ex.Message));
            }
            finally
            {
                _dbSemaphore.Release();
                logger.Debug(Properties.Resources.HandleDeleteSymptomSemaphoreReleased);
            }
            logger.Debug(Properties.Resources.ExitHandleDeleteSymptom);
        }
    }
}
