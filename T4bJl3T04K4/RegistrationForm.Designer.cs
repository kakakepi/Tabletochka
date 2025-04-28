namespace T4bJl3T04K4
{
    partial class RegistrationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtUsername = new TextBox();
            txtFirstName = new TextBox();
            txtLastName = new TextBox();
            txtPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            nicknameLabel = new Label();
            firstNameLabel = new Label();
            lastNameLabel = new Label();
            passwordLabel = new Label();
            confirmPasswordLabel = new Label();
            btnRegister = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(550, 261);
            txtUsername.Margin = new Padding(6);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(323, 39);
            txtUsername.TabIndex = 0;
            // 
            // txtFirstName
            // 
            txtFirstName.Location = new Point(550, 344);
            txtFirstName.Margin = new Padding(6);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(323, 39);
            txtFirstName.TabIndex = 1;
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(550, 427);
            txtLastName.Margin = new Padding(6);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(323, 39);
            txtLastName.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(550, 510);
            txtPassword.Margin = new Padding(6);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(323, 39);
            txtPassword.TabIndex = 3;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(550, 593);
            txtConfirmPassword.Margin = new Padding(6);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(323, 39);
            txtConfirmPassword.TabIndex = 4;
            // 
            // nicknameLabel
            // 
            nicknameLabel.AutoSize = true;
            nicknameLabel.Location = new Point(550, 223);
            nicknameLabel.Name = "nicknameLabel";
            nicknameLabel.Size = new Size(115, 32);
            nicknameLabel.TabIndex = 5;
            nicknameLabel.Text = "Никнейм";
            // 
            // firstNameLabel
            // 
            firstNameLabel.AutoSize = true;
            firstNameLabel.Location = new Point(550, 306);
            firstNameLabel.Name = "firstNameLabel";
            firstNameLabel.Size = new Size(61, 32);
            firstNameLabel.TabIndex = 6;
            firstNameLabel.Text = "Имя";
            // 
            // lastNameLabel
            // 
            lastNameLabel.AutoSize = true;
            lastNameLabel.Location = new Point(550, 389);
            lastNameLabel.Name = "lastNameLabel";
            lastNameLabel.Size = new Size(113, 32);
            lastNameLabel.TabIndex = 7;
            lastNameLabel.Text = "Фамилия";
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Location = new Point(550, 472);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(96, 32);
            passwordLabel.TabIndex = 8;
            passwordLabel.Text = "Пароль";
            // 
            // confirmPasswordLabel
            // 
            confirmPasswordLabel.AutoSize = true;
            confirmPasswordLabel.Location = new Point(550, 555);
            confirmPasswordLabel.Name = "confirmPasswordLabel";
            confirmPasswordLabel.Size = new Size(219, 32);
            confirmPasswordLabel.TabIndex = 9;
            confirmPasswordLabel.Text = "Повторите пароль";
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(550, 654);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(323, 46);
            btnRegister.TabIndex = 10;
            btnRegister.Text = "Зарегистрироваться";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(550, 749);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(323, 46);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // RegistrationForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1486, 960);
            Controls.Add(btnCancel);
            Controls.Add(btnRegister);
            Controls.Add(confirmPasswordLabel);
            Controls.Add(passwordLabel);
            Controls.Add(lastNameLabel);
            Controls.Add(firstNameLabel);
            Controls.Add(nicknameLabel);
            Controls.Add(txtConfirmPassword);
            Controls.Add(txtPassword);
            Controls.Add(txtLastName);
            Controls.Add(txtFirstName);
            Controls.Add(txtUsername);
            Margin = new Padding(6);
            Name = "RegistrationForm";
            Text = "Registration";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUsername;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Label nicknameLabel;
        private Label firstNameLabel;
        private Label lastNameLabel;
        private Label passwordLabel;
        private Label confirmPasswordLabel;
        private Button btnRegister;
        private Button btnCancel;
    }
}