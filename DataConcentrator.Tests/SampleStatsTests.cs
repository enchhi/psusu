using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class SampleStatsTests
    {
        [TestMethod]
        public void Compute_MinMaxAvg()
        {
            var s = SampleStats.Compute(new double[] { 10, 20, 30 });
            Assert.AreEqual(3, s.Count);
            Assert.AreEqual(10, s.Min, 1e-9);
            Assert.AreEqual(30, s.Max, 1e-9);
            Assert.AreEqual(20, s.Average, 1e-9);
        }

        [TestMethod]
        public void Compute_Empty_CountZero()
        {
            var s = SampleStats.Compute(new double[0]);
            Assert.AreEqual(0, s.Count);
        }
    }
}
