using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    // F2: min/max/average nad skupom vrednosti.
    public class Stats
    {
        public int Count { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }
    }

    public static class SampleStats
    {
        public static Stats Compute(IEnumerable<double> values)
        {
            var list = values?.ToList() ?? new List<double>();
            if (list.Count == 0)
                return new Stats { Count = 0 };

            return new Stats
            {
                Count = list.Count,
                Min = list.Min(),
                Max = list.Max(),
                Average = list.Average()
            };
        }
    }
}
