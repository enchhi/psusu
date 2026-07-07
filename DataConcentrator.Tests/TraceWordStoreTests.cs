using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class TraceWordStoreTests
    {
        [TestMethod]
        public void SaveThenLoad_RoundTrips()
        {
            var path = Path.GetTempFileName();
            try
            {
                TraceWordStore.Save(path, 1234);
                Assert.AreEqual(1234, TraceWordStore.Load(path, 0));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void Load_MissingFile_ReturnsDefault()
        {
            var path = Path.Combine(Path.GetTempPath(), "tw_missing_" + Guid.NewGuid().ToString("N") + ".cfg");
            Assert.AreEqual(999, TraceWordStore.Load(path, 999));
        }

        [TestMethod]
        public void Load_Garbage_ReturnsDefault()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "nije broj");
                Assert.AreEqual(42, TraceWordStore.Load(path, 42));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
