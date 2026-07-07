using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataConcentrator
{
    // Generise Report .txt: za svaki AI, ocitavanja koja su bila u opsegu (HighLimit + LowLimit) / 2 +- 5.
    public static class ReportGenerator
    {
        private const double Band = 5.0;

        public static string Generate(IEnumerable<AiHistory> histories)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SCADA Report - vrednosti analognih ulaza u opsegu (High+Low)/2 +- 5");
            sb.AppendLine();

            foreach (var h in histories)
            {
                double mid = (h.HighLimit + h.LowLimit) / 2.0;
                foreach (var s in h.Samples)
                {
                    if (Math.Abs(s.Value - mid) <= Band)
                    {
                        sb.AppendLine(
                            h.TagName + " | " +
                            s.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " | " +
                            s.Value.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
