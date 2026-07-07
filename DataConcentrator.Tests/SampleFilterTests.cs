using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class SampleFilterTests
    {
        private static List<AnalogSample> Data() => new List<AnalogSample>
        {
            new AnalogSample { TagName = "A", Value = 10, Timestamp = new DateTime(2026, 7, 7, 10, 0, 0) },
            new AnalogSample { TagName = "A", Value = 60, Timestamp = new DateTime(2026, 7, 7, 11, 0, 0) },
            new AnalogSample { TagName = "B", Value = 30, Timestamp = new DateTime(2026, 7, 7, 12, 0, 0) },
        };

        [TestMethod]
        public void NoFilters_ReturnsAll()
            => Assert.AreEqual(3, SampleFilter.Apply(Data(), null, null, null, null, null).Count);

        [TestMethod]
        public void FilterByTag()
            => Assert.AreEqual(2, SampleFilter.Apply(Data(), "A", null, null, null, null).Count);

        [TestMethod]
        public void FilterByValueRange()
            => Assert.AreEqual(1, SampleFilter.Apply(Data(), null, null, null, 50, 100).Count); // samo 60

        [TestMethod]
        public void FilterByTimeRange()
        {
            var res = SampleFilter.Apply(Data(), null,
                new DateTime(2026, 7, 7, 10, 30, 0), new DateTime(2026, 7, 7, 11, 30, 0), null, null);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(60, res[0].Value, 1e-9);
        }

        [TestMethod]
        public void CombinedTagAndValue()
        {
            var res = SampleFilter.Apply(Data(), "A", null, null, 0, 20);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(10, res[0].Value, 1e-9);
        }
    }
}
