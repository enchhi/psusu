using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    // Dozvoljene PLC adrese po tipu taga (odgovara rasporedu u PLCSimulator-u).
    // Koristi ga TagValidator (provera unosa) i AddWindow (punjenje combo-a adresa).
    public static class PlcAddressMap
    {
        private static readonly Dictionary<TagType, string[]> map = new Dictionary<TagType, string[]>
        {
            { TagType.AI, new[] { "ADDR001", "ADDR002", "ADDR003", "ADDR004" } },
            { TagType.AO, new[] { "ADDR005", "ADDR006", "ADDR007", "ADDR008" } },
            { TagType.DI, new[] { "ADDR009", "ADDR011", "ADDR012", "ADDR013" } },
            { TagType.DO, new[] { "ADDR010", "ADDR014", "ADDR015", "ADDR016" } },
        };

        public static IReadOnlyList<string> ForType(TagType type)
            => map.TryGetValue(type, out var list) ? list : new string[0];

        public static bool IsValidFor(TagType type, string address)
            => map.TryGetValue(type, out var list) && list.Contains(address);
    }
}
