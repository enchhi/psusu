using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class ConfigSerializerTests
    {
        [TestMethod]
        public void ExportImport_RoundTrips_AllTypes()
        {
            var tags = new List<Tag>
            {
                new AnalogInput { Name = "AI1", IOAddress = "ADDR001", ScanTime = 500, OnScan = true,
                    LowLimit = 0, HighLimit = 100, Units = "C", Deadband = 1, Hysteresis = 2 },
                new AnalogOutput { Name = "AO1", IOAddress = "ADDR005", LowLimit = 0, HighLimit = 50,
                    Units = "%", InitialValue = 10 },
                new DigitalInput { Name = "DI1", IOAddress = "ADDR009", ScanTime = 1000, OnScan = false },
                new DigitalOutput { Name = "DO1", IOAddress = "ADDR010", InitialValue = 1 }
            };

            var json = ConfigSerializer.Export(tags);
            var back = ConfigSerializer.Import(json);

            Assert.AreEqual(4, back.Count);

            var ai = back.OfType<AnalogInput>().Single();
            Assert.AreEqual("AI1", ai.Name);
            Assert.AreEqual("ADDR001", ai.IOAddress);
            Assert.AreEqual(500, ai.ScanTime);
            Assert.AreEqual(100, ai.HighLimit, 1e-9);
            Assert.AreEqual("C", ai.Units);
            Assert.IsTrue(ai.OnScan);
            Assert.AreEqual(2, ai.Hysteresis, 1e-9);

            var ao = back.OfType<AnalogOutput>().Single();
            Assert.AreEqual(10, ao.InitialValue, 1e-9);
            Assert.AreEqual("%", ao.Units);

            var di = back.OfType<DigitalInput>().Single();
            Assert.AreEqual(1000, di.ScanTime);

            var dof = back.OfType<DigitalOutput>().Single();
            Assert.AreEqual(1, dof.InitialValue, 1e-9);
        }
    }
}
