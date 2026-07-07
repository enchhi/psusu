using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class TagValidatorTests
    {
        [TestMethod]
        public void ValidAnalogInput_Passes()
        {
            var ai = new AnalogInput
            {
                Name = "T1", IOAddress = "ADDR001", ScanTime = 100,
                LowLimit = 0, HighLimit = 100, Units = "C", Deadband = 1, Hysteresis = 1
            };
            Assert.IsTrue(TagValidator.Validate(ai).IsValid);
        }

        [TestMethod]
        public void MissingName_Fails()
        {
            var ai = new AnalogInput
            {
                Name = "", IOAddress = "ADDR001", ScanTime = 100,
                LowLimit = 0, HighLimit = 100, Units = "C"
            };
            Assert.IsFalse(TagValidator.Validate(ai).IsValid);
        }

        [TestMethod]
        public void LowNotLessThanHigh_Fails()
        {
            var ai = new AnalogInput
            {
                Name = "T", IOAddress = "ADDR001", ScanTime = 100,
                LowLimit = 100, HighLimit = 50, Units = "C"
            };
            Assert.IsFalse(TagValidator.Validate(ai).IsValid);
        }

        [TestMethod]
        public void NonPositiveScanTime_Fails()
        {
            var di = new DigitalInput { Name = "D", IOAddress = "ADDR009", ScanTime = 0 };
            Assert.IsFalse(TagValidator.Validate(di).IsValid);
        }

        [TestMethod]
        public void DigitalOutput_InitialOutsideBinary_Fails()
        {
            var dof = new DigitalOutput { Name = "D", IOAddress = "ADDR010", InitialValue = 5 };
            Assert.IsFalse(TagValidator.Validate(dof).IsValid);
        }

        [TestMethod]
        public void AnalogOutput_EmptyUnits_Fails()
        {
            var ao = new AnalogOutput { Name = "A", IOAddress = "ADDR005", LowLimit = 0, HighLimit = 10, Units = "" };
            Assert.IsFalse(TagValidator.Validate(ao).IsValid);
        }
    }
}
