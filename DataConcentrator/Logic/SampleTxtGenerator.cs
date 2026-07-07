using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataConcentrator
{
    // F4: .txt sa imenima tagova, vremenima i vrednostima uzoraka koji su prosli filter.
    public static class SampleTxtGenerator
    {
        public static string Generate(IEnumerable<AnalogSample> samples)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Ime taga | Vreme | Vrednost");
            foreach (var s in samples)
            {
                sb.AppendLine(
                    s.TagName + " | " +
                    s.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " | " +
                    s.Value.ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
}
