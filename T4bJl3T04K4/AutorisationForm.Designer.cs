namespace T4bJl3T04K4
{
    partial class AutorisationForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            LoginTextBox = new TextBox();
            PasswordTextBox = new TextBox();
            loginLabel = new Label();
            passwordLabel = new Label();
            btnLogin = new Button();
            btnRegister = new Button();
            SuspendLayout();
            // 
            // LoginTextBox
            // 
            LoginTextBox.BorderStyle = BorderStyle.None;
            LoginTextBox.Location = new Point(611, 381);
            LoginTextBox.Margin = new Padding(4, 2, 4, 2);
            LoginTextBox.Name = "LoginTextBox";
            LoginTextBox.Size = new Size(246, 32);
            LoginTextBox.TabIndex = 0;
            // 
            // PasswordTextBox
            // 
            PasswordTextBox.BorderStyle = BorderStyle.None;
            PasswordTextBox.Location = new Point(611, 462);
            PasswordTextBox.Margin = new Padding(4, 2, 4, 2);
            PasswordTextBox.Name = "PasswordTextBox";
            PasswordTextBox.Size = new Size(246, 32);
            PasswordTextBox.TabIndex = 1;
            // 
            // loginLabel
            // 
            loginLabel.AutoSize = true;
            loginLabel.Location = new Point(611, 347);
            loginLabel.Name = "loginLabel";
            loginLabel.Size = new Size(81, 32);
            loginLabel.TabIndex = 2;
            loginLabel.Text = "Логин";
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Location = new Point(611, 428);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(96, 32);
            passwordLabel.TabIndex = 3;
            passwordLabel.Text = "Пароль";
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(611, 533);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(246, 46);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Войти";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(611, 620);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(246, 46);
            btnRegister.TabIndex = 5;
            btnRegister.Text = "Зарегистрироваться";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // AutorisationForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1532, 964);
            Controls.Add(btnRegister);
            Controls.Add(btnLogin);
            Controls.Add(passwordLabel);
            Controls.Add(loginLabel);
            Controls.Add(PasswordTextBox);
            Controls.Add(LoginTextBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 2, 4, 2);
            Name = "AutorisationForm";
            Text = "Авторизация";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox LoginTextBox;
        private TextBox PasswordTextBox;
        private Label loginLabel;
        private Label passwordLabel;
        private Button btnLogin;
        private Button btnRegister;
    }
}
