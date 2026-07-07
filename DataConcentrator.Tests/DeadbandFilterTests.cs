using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class DeadbandFilterTests
    {
        [TestMethod]
        public void ChangeAboveDeadband_IsSignificant()
            => Assert.IsTrue(DeadbandFilter.IsSignificant(10, 13, 2));

        [TestMethod]
        public void ChangeBelowDeadband_IsNotSignificant()
            => Assert.IsFalse(DeadbandFilter.IsSignificant(10, 11, 2));

        [TestMethod]
        public void ChangeEqualsDeadband_IsSignificant()
            => Assert.IsTrue(DeadbandFilter.IsSignificant(10, 12, 2));

        [TestMethod]
        public void ZeroDeadband_AnyRegister()
            => Assert.IsTrue(DeadbandFilter.IsSignificant(10, 10.0001, 0));

        [TestMethod]
        public void FirstRead_NaNOld_IsAlwaysSignificant()
            => Assert.IsTrue(DeadbandFilter.IsSignificant(double.NaN, 5, 100));
    }
}
