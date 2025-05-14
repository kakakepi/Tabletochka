using Microsoft.VisualStudio.TestTools.UnitTesting;
using T4bJl3T04K4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static T4bJl3T04K4.Tabletochka;

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
                .Where(l => l.UserId == newUser.Id).FirstOrDefaultAsync();
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
                .Where(l => l.UserId == newUser.Id).FirstOrDefaultAsync();
            Assert.IsNotNull(loginHistory);
            Assert.IsTrue(loginHistory.IsSuccessful);
        }
    }
}