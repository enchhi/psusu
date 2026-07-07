using System.Globalization;
using System.IO;

namespace DataConcentrator
{
    // Cuvanje traceword-a (maska log kategorija) u NUMERICKOM formatu u fajlu (spec F7).
    public static class TraceWordStore
    {
        public static long Load(string path, long defaultValue)
        {
            try
            {
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path).Trim();
                    if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                        return v;
                }
            }
            catch
            {
                // ako fajl ne moze da se procita, vrati default
            }
            return defaultValue;
        }

        public static void Save(string path, long value)
        {
            File.WriteAllText(path, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
