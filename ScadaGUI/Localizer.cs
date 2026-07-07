using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScadaGUI
{
    public enum Lang { Sr, En }

    // F3: lokalizacija - jezik (sr/en), vremenska zona, format datuma. Kontrole se
    // pretplate na Changed i pozovu svoj ApplyLanguage().
    public static class Localizer
    {
        public static Lang Language { get; private set; } = Lang.Sr;
        public static TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.Local;
        public static string DateFormat { get; private set; } = "yyyy-MM-dd HH:mm:ss";

        public static event Action Changed;

        public static void SetLanguage(Lang lang) { Language = lang; Changed?.Invoke(); }
        public static void SetTimeZone(TimeZoneInfo tz) { TimeZone = tz ?? TimeZoneInfo.Local; Changed?.Invoke(); }
        public static void SetDateFormat(string fmt) { if (!string.IsNullOrWhiteSpace(fmt)) DateFormat = fmt; Changed?.Invoke(); }

        public static string T(string key)
        {
            var d = Language == Lang.En ? En : Sr;
            return d.TryGetValue(key, out var v) ? v : key;
        }

        // Prikaz vremena u izabranoj zoni i formatu (timestamp-ovi su snimljeni kao lokalno vreme).
        public static string FormatTime(DateTime dt)
        {
            try
            {
                var local = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
                var converted = TimeZoneInfo.ConvertTime(local, TimeZoneInfo.Local, TimeZone);
                return converted.ToString(DateFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
                return dt.ToString(DateFormat, CultureInfo.InvariantCulture);
            }
        }

        private static readonly Dictionary<string, string> Sr = new Dictionary<string, string>
        {
            { "app.title", "SCADA Aplikacija" },
            { "app.overview", "Pregled tagova" },
            { "btn.add", "Dodaj" }, { "btn.remove", "Ukloni" }, { "btn.write", "Upisi vrednost" },
            { "btn.details", "Detalji" }, { "btn.scan", "Scan on/off" }, { "btn.ack", "Acknowledge" },
            { "btn.report", "Report" }, { "btn.trace", "Trace log" }, { "btn.export", "Export" },
            { "btn.import", "Import" }, { "btn.filter", "Pretraga" }, { "btn.options", "Opcije" },
            { "col.type", "Tip" }, { "col.name", "Naziv" }, { "col.address", "Adresa" },
            { "col.value", "Vrednost" }, { "col.unit", "Jedinica" }, { "col.desc", "Opis" },
            { "tip.selectSignal", "Prvo izaberite signal iz tabele" },
            { "tip.add", "Dodaj novi tag ili alarm" },
            { "tip.remove", "Ukloni selektovani tag" },
            { "tip.write", "Upisi vrednost u izlaz (AO/DO)" },
            { "tip.details", "Prikazi alarme i grafik istorije (AI)" },
            { "tip.scan", "Ukljuci/iskljuci skeniranje (AI/DI)" },
            { "tip.ack", "Prihvati aktivan alarm (AI)" },
            { "tip.report", "Generisi Report .txt" },
            { "tip.trace", "Izbor logova (trace-bitovi)" },
            { "tip.export", "Izvezi konfiguraciju u JSON" },
            { "tip.import", "Uvezi konfiguraciju iz JSON" },
            { "tip.filter", "Pretraga vrednosti iz baze" },
            { "tip.options", "Tema, zvuk, jezik i format vremena" },
            { "role.ro", "[Samo citanje]" }, { "role.rw", "[Citanje/Pisanje]" },
        };

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "app.title", "SCADA Application" },
            { "app.overview", "Tag overview" },
            { "btn.add", "Add" }, { "btn.remove", "Remove" }, { "btn.write", "Write value" },
            { "btn.details", "Details" }, { "btn.scan", "Scan on/off" }, { "btn.ack", "Acknowledge" },
            { "btn.report", "Report" }, { "btn.trace", "Trace log" }, { "btn.export", "Export" },
            { "btn.import", "Import" }, { "btn.filter", "Search" }, { "btn.options", "Options" },
            { "col.type", "Type" }, { "col.name", "Name" }, { "col.address", "Address" },
            { "col.value", "Value" }, { "col.unit", "Unit" }, { "col.desc", "Description" },
            { "tip.selectSignal", "Select a signal from the table first" },
            { "tip.add", "Add a new tag or alarm" },
            { "tip.remove", "Remove the selected tag" },
            { "tip.write", "Write a value to an output (AO/DO)" },
            { "tip.details", "Show alarms and history chart (AI)" },
            { "tip.scan", "Toggle scanning (AI/DI)" },
            { "tip.ack", "Acknowledge an active alarm (AI)" },
            { "tip.report", "Generate Report .txt" },
            { "tip.trace", "Select logs (trace bits)" },
            { "tip.export", "Export configuration to JSON" },
            { "tip.import", "Import configuration from JSON" },
            { "tip.filter", "Search values from the database" },
            { "tip.options", "Theme, sound, language and time format" },
            { "role.ro", "[Read only]" }, { "role.rw", "[Read/Write]" },
        };
    }
}
