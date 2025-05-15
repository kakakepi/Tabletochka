using Microsoft.VisualStudio.TestTools.UnitTesting;
using T4bJl3T04K4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static T4bJl3T04K4.Tabletochka;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Reflection;
using System.Windows.Forms;

namespace T4bJl3T04K4.Tests
{
    [TestClass()]
    public class TabletochkaTests
    {
        private Tabletochka _tabletochka;
        private T4bJl3T04K4Db _db;

        [TestInitialize]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<T4bJl3T04K4Db>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _db = new T4bJl3T04K4Db(options);
            _tabletochka = new Tabletochka(_db);
        }

        [TestMethod()]
        public async Task LoginAsync_NonExistingUser_NotAddLoginHistory()
        {
            var nonExistingUser = new LoginData 
            { 
                username = "qwerty", 
                password = "12345678"
            };

            await _tabletochka.LoginAsync(nonExistingUser);

            var countLoginHistory = _db.LoginHistories.Count();
            Assert.AreEqual(0, countLoginHistory);
        }

        [TestMethod()]
        public async Task LoginAsync_WrongPassword_AddLoginHistoryUnsuccessful()
        {
            var correctPassword = "12345678";
            var wrongPassword = "12345679";
            var salt = _tabletochka.GenerateSalt();
            var passwordHash = _tabletochka.HashPassword(correctPassword, salt);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "qwerty",
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

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            var testUser = new LoginData
            {
                username = "qwerty",
                password = wrongPassword
            };

            await _tabletochka.LoginAsync(testUser);

            var loginHistory = await _db.LoginHistories
                .FirstOrDefaultAsync(l => l.UserId == newUser.Id);
            Assert.IsNotNull(loginHistory);
            Assert.IsFalse(loginHistory.IsSuccessful);
        }

        [TestMethod()]
        public async Task LoginAsync_correctPassword_AddLoginHistorySuccessful()
        {
            var correctPassword = "12345678";
            var salt = _tabletochka.GenerateSalt();
            var passwordHash = _tabletochka.HashPassword(correctPassword, salt);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "qwerty",
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

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            var testUser = new LoginData
            {
                username = "qwerty",
                password = correctPassword
            };

            await _tabletochka.LoginAsync(testUser);

            var loginHistory = await _db.LoginHistories
                .FirstOrDefaultAsync(l => l.UserId == newUser.Id);
            Assert.IsNotNull(loginHistory);
            Assert.IsTrue(loginHistory.IsSuccessful);
        }
        
        [TestMethod()]
        public async Task RegisterAsync_NonExistingUser_AddNewUserToDataBase()
        {
            var newUsername = "qwerty";
            var registerData = new RegisterData
            {
                username = newUsername,
                password = "12345678",
                repeatPassword = "12345678"
            };
            await _tabletochka.RegisterAsync(registerData);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == newUsername);

            Assert.IsNotNull(user);
        }

        [TestMethod()]
        public async Task RegisterAsync_DuplicateUser_NotAddNewUserToDataBase()
        {
            var newUsername = "qwerty";

            var salt = _tabletochka.GenerateSalt();
            var passwordHash = _tabletochka.HashPassword("12345678", salt);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = newUsername,
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

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            var registerData = new RegisterData
            {
                username = newUsername,
                password = "12345679",
                repeatPassword = "12345679"
            };

            await _tabletochka.RegisterAsync(registerData);

            var userCount = await _db.Users
                .CountAsync(u => u.Username == newUsername);

            Assert.AreEqual(1, userCount);
        }

        [TestMethod()]
        public async Task UpdateProfileAsync_NewProfileDataWithoutPassword_UpdateProfileData()
        {
            var salt = _tabletochka.GenerateSalt();
            var passwordHash = _tabletochka.HashPassword("12345678", salt);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "oldUsername",
                FirstName = "oldFirstName",
                LastName = "oldLastName",
                Gender = true,
                DateOfBirth = new DateTime(2006, 3, 7),
                PasswordHash = passwordHash,
                Salt = salt,
                Picture = "",
            };

            await _db.AddAsync(user);
            await _db.SaveChangesAsync();

            var profileData = new ProfileData
            {
                Id = user.Id,
                Username = "newUsername",
                Firstname = "newFirstname",
                Lastname = "newLastname",
                Gender = "female",
                Birthdate = "2006-03-06",
                OldPassword = "",
                NewPassword = "",
            };

            await _tabletochka.UpdateProfileAsync(profileData);

            var updatedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(profileData.Username, updatedUser.Username);
            Assert.AreEqual(profileData.Firstname, updatedUser.FirstName);
            Assert.AreEqual(profileData.Lastname, updatedUser.LastName);
            Assert.IsFalse(updatedUser.Gender); 
            Assert.AreEqual(DateTime.Parse(profileData.Birthdate), updatedUser.DateOfBirth);
            Assert.AreEqual(passwordHash, updatedUser.PasswordHash);
        }

        [TestMethod()]
        public async Task UpdateProfileAsync_NewProfileDataWithCorrectOldPassword_UpdatePassword()
        {
            var oldPassword = "12345678";
            var newPassword = "87654321";
            var salt = _tabletochka.GenerateSalt();
            var oldPasswordHash = _tabletochka.HashPassword(oldPassword, salt);
            var newPasswordHash = _tabletochka.HashPassword(newPassword, salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "oldUsername",
                FirstName = "oldFirstName",
                LastName = "oldLastName",
                Gender = true,
                DateOfBirth = new DateTime(2006, 3, 7),
                PasswordHash = oldPasswordHash,
                Salt = salt,
                Picture = "",
            };

            await _db.AddAsync(user);
            await _db.SaveChangesAsync();

            var profileData = new ProfileData
            {
                Id = user.Id,
                Username = "newUsername",
                Firstname = "newFirstname",
                Lastname = "newLastname",
                Gender = "female",
                Birthdate = "2006-03-06",
                OldPassword = oldPassword,
                NewPassword = newPassword,
            };

            await _tabletochka.UpdateProfileAsync(profileData);

            var updatedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(profileData.Username, updatedUser.Username);
            Assert.AreEqual(profileData.Firstname, updatedUser.FirstName);
            Assert.AreEqual(profileData.Lastname, updatedUser.LastName);
            Assert.IsFalse(updatedUser.Gender);
            Assert.AreEqual(DateTime.Parse(profileData.Birthdate), updatedUser.DateOfBirth);
            Assert.AreEqual(updatedUser.PasswordHash, newPasswordHash);
        }

        [TestMethod()]
        public async Task UpdateProfileAsync_NewProfileDataWithWrongOldPassword_NotUpdatePassword()
        {
            var oldPassword = "12345678";
            var newPassword = "87654321";
            var salt = _tabletochka.GenerateSalt();
            var oldPasswordHash = _tabletochka.HashPassword(oldPassword, salt);
            var newPasswordHash = _tabletochka.HashPassword(newPassword, salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "oldUsername",
                FirstName = "oldFirstName",
                LastName = "oldLastName",
                Gender = true,
                DateOfBirth = new DateTime(2006, 3, 7),
                PasswordHash = oldPasswordHash,
                Salt = salt,
                Picture = "",
            };

            await _db.AddAsync(user);
            await _db.SaveChangesAsync();

            var profileData = new ProfileData
            {
                Id = user.Id,
                Username = "newUsername",
                Firstname = "newFirstname",
                Lastname = "newLastname",
                Gender = "female",
                Birthdate = "2006-03-06",
                OldPassword = "18273645",
                NewPassword = newPassword,
            };

            await _tabletochka.UpdateProfileAsync(profileData);

            var updatedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(profileData.Username, updatedUser.Username);
            Assert.AreEqual(profileData.Firstname, updatedUser.FirstName);
            Assert.AreEqual(profileData.Lastname, updatedUser.LastName);
            Assert.IsFalse(updatedUser.Gender);
            Assert.AreEqual(DateTime.Parse(profileData.Birthdate), updatedUser.DateOfBirth);
            Assert.AreEqual(updatedUser.PasswordHash, oldPasswordHash);
        }

        [TestMethod()]
        public async Task UpdateProfileAsync_DuplicateProfileData_NotUpdateProfileData()
        {
            var username = "latypdin";
            var firstSalt = _tabletochka.GenerateSalt();
            var secondSalt = _tabletochka.GenerateSalt();
            var firstPasswordHash = _tabletochka.HashPassword("12345678", firstSalt);
            var secondPasswordHash = _tabletochka.HashPassword("09876543", secondSalt);

            var firstUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                FirstName = "Dina",
                LastName = "Latypova",
                Gender = false,
                DateOfBirth = new DateTime(1996, 9, 23),
                PasswordHash = firstPasswordHash,
                Salt = firstSalt,
                Picture = "",
            };

            var secondUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "quintrrr",
                FirstName = "Arina",
                LastName = "Gomza",
                Gender = false,
                DateOfBirth = new DateTime(2006, 3, 6),
                PasswordHash = secondPasswordHash,
                Salt = secondSalt,
                Picture = "",
            };

            await _db.AddAsync(firstUser);
            await _db.AddAsync(secondUser);
            await _db.SaveChangesAsync();

            var profileData = new ProfileData
            {
                Id = secondUser.Id,
                Username = username,
                Firstname = "Arina",
                Lastname = "Semenistyy",
                Gender = "female",
                Birthdate = "2006-03-06",
                OldPassword = "",
                NewPassword = "",
            };

            await _tabletochka.UpdateProfileAsync(profileData);

            var userCount = await _db.Users.CountAsync(u => u.Username == username);

            Assert.AreEqual(1, userCount);
        }

        [TestMethod()]
        public async Task UploadPhotoAsync_Photo_UploadPhoto()
        {
            string base64image = "R0lGODlhAQABAAAAACw=";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "latypdin",
                FirstName = "Dina",
                LastName = "Latypova",
                Gender = false,
                DateOfBirth = new DateTime(1996, 9, 23),
                PasswordHash = "",
                Salt = "",
                Picture = "",
            };

            await _db.AddAsync(user);
            await _db.SaveChangesAsync();

            var idField = typeof(Tabletochka)
                .GetField("currentUserId", BindingFlags.Instance | BindingFlags.NonPublic);
            idField.SetValue(_tabletochka, user.Id);

            await _tabletochka.UploadPhotoAsync(base64image);

            var userWithPicture = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.IsNotNull(userWithPicture);
            Assert.AreEqual(userWithPicture.Picture, base64image);
        }

        [TestMethod()]
        public async Task DeletePhotoAsync_Photo_DeletePhoto()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "latypdin",
                FirstName = "Dina",
                LastName = "Latypova",
                Gender = false,
                DateOfBirth = new DateTime(1996, 9, 23),
                PasswordHash = "",
                Salt = "",
                Picture = "R0lGODlhAQABAAAAACw=",
            };

            await _db.AddAsync(user);
            await _db.SaveChangesAsync();

            var idField = typeof(Tabletochka)
                .GetField("currentUserId", BindingFlags.Instance | BindingFlags.NonPublic);
            idField.SetValue(_tabletochka, user.Id);

            await _tabletochka.DeletePhotoAsync();

            var userWithoutPicture = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.IsNotNull(userWithoutPicture);
            Assert.IsNull(userWithoutPicture.Picture);
        }

        [TestMethod()]
        public void GenerateSalt_DifferentSalts_DifferentValues()
        {
            var firstSalt = _tabletochka.GenerateSalt();
            var secondSalt = _tabletochka.GenerateSalt();

            Assert.AreNotEqual(firstSalt, secondSalt);
        }

        [TestMethod()]
        public void HashPassword_SameSaltAndSamePassword_SameHashPassword()
        {
            var password = "12345678";
            var salt = _tabletochka.GenerateSalt();
            var firstHashPassword = _tabletochka.HashPassword(password, salt);
            var secondHashPassword = _tabletochka.HashPassword(password, salt);

            Assert.AreEqual(firstHashPassword, secondHashPassword);
        }
    }
}