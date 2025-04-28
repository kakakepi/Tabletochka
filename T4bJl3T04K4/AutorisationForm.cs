using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;


namespace T4bJl3T04K4
{
    public partial class AutorisationForm : Form
    {
        private readonly T4bJl3T04K4Db dataBase;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public User CurrentUser { get; private set; }

        public AutorisationForm(T4bJl3T04K4Db dbContext)
        {
            dataBase = dbContext;
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var username = LoginTextBox.Text;
                var password = PasswordTextBox.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("??????? ????? ? ??????");
                    return;
                }

                var user = dataBase.Users.FirstOrDefault(user => user.Username == username);
                if (user == null)
                {
                    MessageBox.Show("???????????? ?? ??????");
                    return;
                }

                var inputHash = HashPassword(password, user.Salt);
                if (inputHash != user.PasswordHash)
                {
                    MessageBox.Show("???????? ??????");
                    return;
                }

                dataBase.LoginHistories.Add(new LoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IsSuccessful = true
                });
                dataBase.SaveChanges();

                CurrentUser = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"?????? ?????: {ex.Message}");
            }
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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var regForm = new RegistrationForm(dataBase);
            if (regForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("?????? ?? ?????? ????? ? ?????? ???????? ???????");
            }
        }
    }
}
