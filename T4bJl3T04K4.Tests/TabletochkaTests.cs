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
        public async Task LoginAsync_NonExistingUser_NotAddLoginHistoryAsync()
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

    }
}