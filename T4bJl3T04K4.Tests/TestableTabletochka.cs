using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4bJl3T04K4.Tests
{
    class TestableTabletochka(T4bJl3T04K4Db db) : Tabletochka(db)
    {
        protected override void SendError(string message) { }
        protected override void SendSuccess(string message) { }
        protected override void LoadUserCabinet() { }
    }
}
