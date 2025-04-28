using System.Security.Cryptography;
using System.Text;

namespace T4bJl3T04K4
{
    public partial class RegistrationForm : Form
    {
        private readonly T4bJl3T04K4Db dataBase;

        public RegistrationForm(T4bJl3T04K4Db dbContext)
        {
            dataBase = dbContext;
            InitializeComponent();
        }
        private void btnRegister_Click(object sender, EventArgs e)
        {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) ||
                    string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Заполните обязательные поля (логин, пароль, имя)");
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Пароли не совпадают");
                    return;
                }

                if (txtPassword.Text.Length < 8)
                {
                    MessageBox.Show("Пароль должен содержать минимум 8 символов");
                    return;
                }

                if (dataBase.Users.Any(u => u.Username == txtUsername.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует");
                    return;
                }

                var salt = GenerateSalt();
                var passwordHash = HashPassword(txtPassword.Text, salt);

                var newUser = new User
                {
                    Username = txtUsername.Text,
                    FirstName = txtFirstName.Text,
                    LastName = txtLastName.Text,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    CreatedAt = DateTime.UtcNow,
                    Admin = false
                };

                dataBase.Users.Add(newUser);
                dataBase.SaveChanges();

                MessageBox.Show("Вы зарегистрировались");
                this.DialogResult = DialogResult.OK;
                this.Close();
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void RegistrationForm_Load(object sender, EventArgs e)
        {

        }

    }
}
