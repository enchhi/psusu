using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class SampleTxtGeneratorTests
    {
        [TestMethod]
        public void Generate_ContainsTagTimeValue()
        {
            var samples = new List<AnalogSample>
            {
                new AnalogSample { TagName = "A", Value = 12.5, Timestamp = new DateTime(2026, 7, 7, 10, 0, 0) }
            };

            var txt = SampleTxtGenerator.Generate(samples);

            StringAssert.Contains(txt, "A");
            StringAssert.Contains(txt, "2026-07-07 10:00:00");
            StringAssert.Contains(txt, "12.5");
        }
    }
}
