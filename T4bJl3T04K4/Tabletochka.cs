using System.Security.Cryptography;
using System.Text;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace T4bJl3T04K4
{
    public partial class Tabletochka : Form
    {
        private readonly T4bJl3T04K4Db dataBase;
        private readonly SemaphoreSlim _dbSemaphore = new(1, 1);
        private Guid currentUserId;

        public Tabletochka(T4bJl3T04K4Db dbContext)
        {
            InitializeComponent();
            dataBase = dbContext;
            InitializeWebViewAsync();
        }

        private async void InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;

            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "LoginRegistration.html");
            webView.Source = new Uri(htmlPath);
        }

        private async void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs receivedArgs)
        {
            var json = receivedArgs.WebMessageAsJson;
            var baseData = System.Text.Json.JsonSerializer.Deserialize<BaseData>(json);

            switch (baseData.action)
            {
                case "login":
                    var loginData = System.Text.Json.JsonSerializer.Deserialize<LoginData>(json);
                    await LoginAsync(loginData);
                    break;

                case "register":
                    var registerData = System.Text.Json.JsonSerializer.Deserialize<RegisterData>(json);
                    await RegisterAsync(registerData);
                    break;
            }
        }

        private async Task LoginAsync(LoginData loginData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Username == loginData.username);
                if (user == null)
                {
                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Такого пользователя не существует"
                        }));
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

                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Неверный логин или пароль"
                        }));
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

                var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "Cabinet.html");
                webView.Source = new Uri(htmlPath);
                webView.CoreWebView2.WebMessageReceived -= WebView_WebMessageReceived;
                webView.CoreWebView2.WebMessageReceived += WebView2_WebMessageReceived;
                webView.NavigationCompleted += WebView_NavigationCompleted;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async Task RegisterAsync(RegisterData registerData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(registerData.username) ||
                    string.IsNullOrWhiteSpace(registerData.password))
                {
                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Пароли не совпадают"
                        }));
                    return;
                }

                if (registerData.password != registerData.repeatPassword)
                {
                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Пароли не совпадают"
                        }));
                    return;
                }

                if (registerData.password.Length < 8)
                {
                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Пароль должен быть не менее 8 символов"
                        }));
                    return;
                }

                if (await dataBase.Users.AnyAsync(u => u.Username == registerData.username))
                {
                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Пользователь с таким именем уже существует"
                        }));
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

        public async Task UpdateProfileAsync(ProfileData profileData)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(usr => usr.Id == profileData.Id);
                if (user == null)
                {
                    webView.CoreWebView2.PostWebMessageAsJson(
                        JsonConvert.SerializeObject(new
                        {
                            type = "error",
                            message = "Пользователь не найден"
                        }));
                    return;
                }

                user.Username = profileData.Username;
                user.FirstName = profileData.Firstname;
                user.LastName = profileData.Lastname;
                user.DateOfBirth = DateTime.Parse(profileData.Birthdate);
                user.Gender = profileData.Gender == "male";
                if (!string.IsNullOrEmpty(profileData.OldPassword) &&
                    !string.IsNullOrEmpty(profileData.NewPassword))
                {
                    var oldPassHash = HashPassword(profileData.OldPassword, user.Salt);
                    if (oldPassHash != user.PasswordHash)
                    {
                        webView.CoreWebView2.PostWebMessageAsJson(
                            JsonConvert.SerializeObject(new
                            {
                                type = "error",
                                message = "Неверный старый пароль"
                            }));
                        return;
                    }
                    user.PasswordHash = HashPassword(profileData.NewPassword, user.Salt);
                }

                await dataBase.SaveChangesAsync();
                webView.CoreWebView2.PostWebMessageAsJson(
                    JsonConvert.SerializeObject(new
                    {
                        type = "success",
                        message = "Профиль обновлен"
                    }));
            }
            catch (Exception ex)
            {
                webView.CoreWebView2.PostWebMessageAsJson(
                    JsonConvert.SerializeObject(new
                    {
                        type = "error",
                        message = $"Ошибка: {ex.Message}"
                    }));
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                var user = await dataBase.Users.FindAsync(currentUserId);
                if (user != null)
                {
                    var userData = new
                    {
                        id = user.Id,
                        username = user.Username,
                        firstname = user.FirstName,
                        lastname = user.LastName,
                        gender = user.Gender,
                        dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                        picture = user.Picture
                    };
                    webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(userData));
                }
            }
        }

        public async Task UploadPhotoAsync(string base64Image)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null) return;

                user.Picture = base64Image;
                await dataBase.SaveChangesAsync();

                webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
                {
                    type = "photo-updated",
                    picture = $"data:image/jpeg;base64,{base64Image}"
                }));
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async Task DeletePhotoAsync()
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var user = await dataBase.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (user == null) return;

                user.Picture = null;
                await dataBase.SaveChangesAsync();

                webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
                {
                    type = "photo-deleted"
                }));
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private string GenerateSalt()
        {
            var salt = new byte[32];
            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
