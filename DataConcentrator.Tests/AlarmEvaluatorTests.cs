using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class AlarmEvaluatorTests
    {
        // Above, granica 100, hysteresis 5
        [TestMethod]
        public void Above_Activates_WhenExceeds()
            => Assert.AreEqual(AlarmState.Active,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 101, AlarmState.Inactive));

        [TestMethod]
        public void Above_StaysInactive_BelowLimit()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 99, AlarmState.Inactive));

        [TestMethod]
        public void Above_HysteresisHold_DoesNotClearJustBelowLimit()
            => Assert.AreEqual(AlarmState.Active,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 97, AlarmState.Active)); // 97 > 100-5

        [TestMethod]
        public void Above_Clears_BelowLimitMinusHysteresis()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 94, AlarmState.Active)); // 94 < 95

        [TestMethod]
        public void Acknowledged_Persists_UntilCleared()
            => Assert.AreEqual(AlarmState.Acknowledged,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 101, AlarmState.Acknowledged));

        [TestMethod]
        public void Acknowledged_Clears_WhenReturnsToNormal()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 90, AlarmState.Acknowledged));

        [TestMethod]
        public void Below_Activates_WhenUnder()
            => Assert.AreEqual(AlarmState.Active,
               AlarmEvaluator.NextState(AlarmDirection.Below, 20, 3, 19, AlarmState.Inactive));

        [TestMethod]
        public void Below_Clears_AboveLimitPlusHysteresis()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Below, 20, 3, 24, AlarmState.Active)); // 24 > 23
    }
}
