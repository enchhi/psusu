using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class AiHistoryStoreTests
    {
        [TestMethod]
        public void Record_And_Get_ReturnsSamples()
        {
            var s = new AiHistoryStore();
            s.Record("T1", new DateTime(2026, 7, 7, 10, 0, 0), 12.3);

            var h = s.Get("T1", 0, 100);

            Assert.AreEqual(1, h.Samples.Count);
            Assert.AreEqual(12.3, h.Samples[0].Value, 1e-9);
        }
    }
}
