using Microsoft.VisualStudio.TestTools.UnitTesting;
using PLCSimulator;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class PlcSimulatorTests
    {
        [TestMethod]
        public void AllAddresses_AreKnown_NoMinusOne()
        {
            var plc = new PLCSimulatorManager();
            for (int i = 1; i <= 16; i++)
            {
                string addr = "ADDR" + i.ToString("000"); // ADDR001..ADDR016
                Assert.AreNotEqual(-1, plc.GetAnalogValue(addr), addr + " nije poznata");
            }
        }

        [TestMethod]
        public void SetAndGet_AnalogOutput_RoundTrips()
        {
            var plc = new PLCSimulatorManager();
            plc.SetAnalogValue("ADDR005", 42.5);
            Assert.AreEqual(42.5, plc.GetAnalogValue("ADDR005"), 1e-9);
        }

        [TestMethod]
        public void UnknownAddress_ReturnsMinusOne()
        {
            var plc = new PLCSimulatorManager();
            Assert.AreEqual(-1, plc.GetAnalogValue("ADDR999"));
        }
    }
}
