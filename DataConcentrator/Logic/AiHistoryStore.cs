using System;
using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    // Jedno ocitavanje vrednosti u vremenu.
    public class ValueSample
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    // Istorija jednog AI taga (za Report i prikaz).
    public class AiHistory
    {
        public string TagName { get; set; }
        public double LowLimit { get; set; }
        public double HighLimit { get; set; }
        public IReadOnlyList<ValueSample> Samples { get; set; }
    }

    // In-memory istorija AI vrednosti prikupljena tokom rada aplikacije.
    // (Bazni core drzi tacno 3 tabele u bazi; ovo NIJE u bazi - koristi ga Report.)
    public class AiHistoryStore
    {
        private readonly Dictionary<string, List<ValueSample>> data =
            new Dictionary<string, List<ValueSample>>();
        private readonly object sync = new object();

        public void Record(string tagName, DateTime ts, double value)
        {
            lock (sync)
            {
                if (!data.TryGetValue(tagName, out var list))
                {
                    list = new List<ValueSample>();
                    data[tagName] = list;
                }
                list.Add(new ValueSample { Timestamp = ts, Value = value });
            }
        }

        public AiHistory Get(string tagName, double low, double high)
        {
            lock (sync)
            {
                var samples = data.TryGetValue(tagName, out var list)
                    ? list.ToList()                 // kopija radi bezbednosti niti
                    : new List<ValueSample>();
                return new AiHistory { TagName = tagName, LowLimit = low, HighLimit = high, Samples = samples };
            }
        }

        public IEnumerable<AiHistory> All(IEnumerable<AnalogInput> ais)
        {
            foreach (var ai in ais)
            {
                yield return Get(ai.Name, ai.LowLimit, ai.HighLimit);
            }
        }
    }
}
