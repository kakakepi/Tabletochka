using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace T4bJl3T04K4
{
    public partial class LoginRegistrationForm : Form
    {
        private readonly T4bJl3T04K4Db dataBase;

        public LoginRegistrationForm(T4bJl3T04K4Db dbContext)
        {
            InitializeComponent();
            dataBase = dbContext;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var htmlPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Properties", "HTML", "LoginRegistration.html");
            webView.Source = new Uri(htmlPath);
        }

        private void WebView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs receivedArgs)
        {
            var json = receivedArgs.WebMessageAsJson;
            var baseData = JsonSerializer.Deserialize<BaseData>(json);

            switch (baseData.action)
            {
                case "login":
                    var loginData = JsonSerializer.Deserialize<LoginData>(json);
                    Login(loginData);
                    break;

                case "register":
                    var registerData = JsonSerializer.Deserialize<RegisterData>(json);
                    Register(registerData);
                    break;
            }

        }
        private void Login(LoginData loginData)
        {
            var user = dataBase.Users.FirstOrDefault(userData => userData.Username == loginData.username);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден");
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
                dataBase.SaveChanges();
                MessageBox.Show("Неверный пароль");
                return;
            }

            dataBase.LoginHistories.Add(new LoginHistory
            {
                UserId = user.Id,
                LoginTime = DateTime.UtcNow,
                IsSuccessful = true
            });
            dataBase.SaveChanges();

            MessageBox.Show($"Добро пожаловать, {user.Username}!");
        }

        private void Register(RegisterData registerData)
        {
            if (string.IsNullOrWhiteSpace(registerData.username) ||
        string.IsNullOrWhiteSpace(registerData.password))
            {
                MessageBox.Show("Заполните обязательные поля");
                return;
            }

            if (registerData.password != registerData.repeatPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            if (registerData.password.Length < 8)
            {
                MessageBox.Show("Пароль должен быть не менее 8 символов");
                return;
            }

            if (dataBase.Users.Any(userData => userData.Username == registerData.username))
            {
                MessageBox.Show("Пользователь с таким именем уже существует");
                return;
            }

            var salt = GenerateSalt();
            var passwordHash = HashPassword(registerData.password, salt);

            var newUser = new User
            {
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

            dataBase.Users.Add(newUser);
            dataBase.SaveChanges();
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
