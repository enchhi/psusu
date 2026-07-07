using System;
using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    // F4: filtriranje AI uzoraka. Prazan (null) uslov se ignorise.
    public static class SampleFilter
    {
        public static List<AnalogSample> Apply(IEnumerable<AnalogSample> samples,
            string tagName, DateTime? from, DateTime? to, double? min, double? max)
        {
            IEnumerable<AnalogSample> q = samples;

            if (!string.IsNullOrWhiteSpace(tagName))
                q = q.Where(s => s.TagName == tagName);
            if (from.HasValue)
                q = q.Where(s => s.Timestamp >= from.Value);
            if (to.HasValue)
                q = q.Where(s => s.Timestamp <= to.Value);
            if (min.HasValue)
                q = q.Where(s => s.Value >= min.Value);
            if (max.HasValue)
                q = q.Where(s => s.Value <= max.Value);

            return q.ToList();
        }
    }
}
