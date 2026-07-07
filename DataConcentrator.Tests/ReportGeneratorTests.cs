using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class ReportGeneratorTests
    {
        [TestMethod]
        public void OnlyValuesInBand_AreIncluded()
        {
            // low=0 high=100 -> sredina 50, opseg [45, 55]
            var h = new AiHistory
            {
                TagName = "T1",
                LowLimit = 0,
                HighLimit = 100,
                Samples = new[]
                {
                    new ValueSample { Timestamp = new DateTime(2026, 7, 7, 10, 0, 0), Value = 50 }, // u opsegu
                    new ValueSample { Timestamp = new DateTime(2026, 7, 7, 10, 0, 1), Value = 80 }, // van opsega
                    new ValueSample { Timestamp = new DateTime(2026, 7, 7, 10, 0, 2), Value = 46 }, // u opsegu
                }
            };

            var txt = ReportGenerator.Generate(new[] { h });

            StringAssert.Contains(txt, "T1");
            StringAssert.Contains(txt, "50");
            StringAssert.Contains(txt, "46");
            Assert.IsFalse(txt.Contains(" 80"));
        }

        [TestMethod]
        public void EmptyHistories_ProducesHeaderNoData()
        {
            var txt = ReportGenerator.Generate(new AiHistory[0]);
            Assert.IsFalse(string.IsNullOrEmpty(txt)); // ima header, ne baca izuzetak
        }
    }
}
