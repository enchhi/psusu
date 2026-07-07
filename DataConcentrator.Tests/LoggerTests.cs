using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Log_Writes_Timestamp_Category_Message()
        {
            var sw = new StringWriter();
            var t = new DateTime(2026, 7, 7, 10, 0, 0);
            var log = new Logger(sw, () => t, long.MaxValue);

            log.Log(LogCategory.Login, "user admin");

            StringAssert.Contains(sw.ToString(), "2026-07-07 10:00:00");
            StringAssert.Contains(sw.ToString(), "Login");
            StringAssert.Contains(sw.ToString(), "user admin");
        }

        [TestMethod]
        public void Log_Skips_WhenTraceBitOff()
        {
            var sw = new StringWriter();
            // samo Login bit ukljucen
            var log = new Logger(sw, () => DateTime.Now, (long)LogCategory.Login);

            log.Log(LogCategory.Scan, "scan tick");

            Assert.AreEqual(0, sw.ToString().Length);
        }
    }
}
