using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ScadaGUI
{
    // F1: Light/Dark tema. Boje se postavljaju DIREKTNO na Application.Resources
    // (pouzdano azurira sve DynamicResource reference, ukljucujuci pozadine prozora),
    // + direktno na sve vec otvorene prozore za svaki slucaj.
    public static class ThemeManager
    {
        public static bool IsDark { get; private set; }

        // key -> { light, dark }
        private static readonly Dictionary<string, string[]> Palette = new Dictionary<string, string[]>
        {
            { "BgBrush",            new[] { "#F4F5FB", "#1E1E2A" } },
            { "SurfaceBrush",       new[] { "#FFFFFF", "#262636" } },
            { "AccentBrush",        new[] { "#6C5CE7", "#8B7BFF" } },
            { "AccentHoverBrush",   new[] { "#5A4BD1", "#9C8FFF" } },
            { "AccentPressedBrush", new[] { "#4E41BE", "#6C5CE7" } },
            { "TextBrush",          new[] { "#2B2D42", "#E8E9F3" } },
            { "MutedTextBrush",     new[] { "#7A7F99", "#A6A9C0" } },
            { "BorderBrush",        new[] { "#E3E6F0", "#34344A" } },
            { "HeaderBrush",        new[] { "#6C5CE7", "#8B7BFF" } }, // header = accent (belo na plavom)
            { "InputBgBrush",       new[] { "#FFFFFF", "#2A2A3C" } },
            { "GridLineBrush",      new[] { "#EDEFF7", "#303048" } },
            { "SelectionBrush",     new[] { "#E7E3FB", "#3A3560" } },
            { "DisabledBrush",      new[] { "#E7E8EE", "#3A3A4C" } },
            { "ScrollThumbBrush",   new[] { "#C4C7D4", "#4C4C66" } },
        };

        public static void Apply(bool dark)
        {
            IsDark = dark;
            var r = Application.Current.Resources;

            foreach (var kv in Palette)
                r[kv.Key] = B(dark ? kv.Value[1] : kv.Value[0]);

            foreach (Window w in Application.Current.Windows)
                w.Background = (Brush)r["BgBrush"];
        }

        private static SolidColorBrush B(string hex)
        {
            var c = (Color)ColorConverter.ConvertFromString(hex);
            var br = new SolidColorBrush(c);
            br.Freeze();
            return br;
        }
    }
}
