using System;
using System.Collections.Generic;
using System.Linq;
using DataConcentrator;

namespace ScadaGUI
{
    // Dev pomoc: nasumicni ali VALIDNI mock podaci iz fiksnih pool-ova.
    // Koristi ga "Mock" dugme u prozorima da se polja popune jednim klikom
    // (radi lakseg testiranja). Brojevi se formatiraju u tekucoj kulturi da
    // se poklope sa parsiranjem u prozorima (double.TryParse bez kulture).
    internal static class MockData
    {
        private static readonly Random rng = new Random();

        public static T Pick<T>(IReadOnlyList<T> pool) => pool[rng.Next(pool.Count)];
        public static int Int(int minInclusive, int maxInclusive) => rng.Next(minInclusive, maxInclusive + 1);
        public static double Double(double min, double max) => Math.Round(min + rng.NextDouble() * (max - min), 2);
        public static bool Bool() => rng.Next(2) == 0;

        // Citljivi procesni nazivi po tipu taga.
        public static readonly IReadOnlyList<string> AiNames = new[] { "Temperatura", "Pritisak", "Nivo", "Protok", "Vlaznost", "Napon" };
        public static readonly IReadOnlyList<string> AoNames = new[] { "Ventil", "Grejac", "Pumpa", "Regulator", "Zaklopka" };
        public static readonly IReadOnlyList<string> DiNames = new[] { "Senzor", "Prekidac", "Taster", "Kontakt", "GranicniPrekidac" };
        public static readonly IReadOnlyList<string> DoNames = new[] { "Sirena", "Lampa", "Rele", "Motor", "Signalna" };

        public static readonly IReadOnlyList<string> Descriptions = new[]
            { "Pogon A", "Linija 1", "Rezervoar R1", "Kotlarnica", "Hala 2", "Sekcija punjenja", "Glavni razvod" };

        public static readonly IReadOnlyList<string> Units = new[] { "C", "bar", "m3/h", "%", "L", "kPa", "V" };
        public static readonly IReadOnlyList<int> ScanTimes = new[] { 200, 500, 1000, 1500, 2000 };
        public static readonly IReadOnlyList<double> Deadbands = new[] { 0.0, 0.5, 1.0, 2.0 };
        public static readonly IReadOnlyList<double> Hystereses = new[] { 0.0, 1.0, 2.0, 5.0 };
        public static readonly IReadOnlyList<string> AlarmMessages = new[]
            { "Previsoka vrednost", "Kriticno nizak nivo", "Vrednost van opsega", "Granica prekoracena", "Hitno - proveri pogon" };

        public static IReadOnlyList<string> NamesFor(string type)
        {
            switch (type)
            {
                case "AI": return AiNames;
                case "AO": return AoNames;
                case "DI": return DiNames;
                case "DO": return DoNames;
                default: return AiNames;
            }
        }

        // Jedinstveno ime iz pool-a (dodaje broj dok se ne razlikuje od postojecih).
        public static string UniqueName(IReadOnlyList<string> pool, IEnumerable<string> existing)
        {
            var taken = new HashSet<string>(existing ?? Enumerable.Empty<string>());
            for (int i = 0; i < 1000; i++)
            {
                string candidate = Pick(pool) + "_" + Int(1, 999);
                if (!taken.Contains(candidate)) return candidate;
            }
            return Pick(pool) + "_" + Guid.NewGuid().ToString("N").Substring(0, 4);
        }

        // Slobodna PLC adresa za tip (ili bilo koja validna ako su sve zauzete).
        public static string FreeAddress(TagType type, IEnumerable<string> usedAddresses)
        {
            var all = PlcAddressMap.ForType(type);
            var used = new HashSet<string>(usedAddresses ?? Enumerable.Empty<string>());
            var free = all.Where(a => !used.Contains(a)).ToList();
            var source = free.Count > 0 ? free : all.ToList();
            return source.Count > 0 ? source[rng.Next(source.Count)] : null;
        }

        // Par (low, high) gde je uvek low < high. [0] = low, [1] = high.
        public static double[] LowHigh()
        {
            double low = Pick(new[] { -50.0, -20.0, 0.0, 10.0, 20.0 });
            double span = Pick(new[] { 50.0, 80.0, 100.0, 150.0 });
            return new[] { low, low + span };
        }

        // Valjan random password za F5: >=15 karaktera, veliko/malo slovo, specijalni znak; verovatno jedinstven.
        public static string Password()
        {
            return "Mock" + Guid.NewGuid().ToString("N").Substring(0, 10) + "!Aa1";
        }
    }
}
