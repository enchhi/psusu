using System;
using System.Windows;

namespace ScadaGUI
{
    // F1: Light/Dark tema preko implicitnih stilova (ResourceDictionary u App.Resources).
    public static class ThemeManager
    {
        public static bool IsDark { get; private set; }

        public static void Apply(bool dark)
        {
            IsDark = dark;
            var uri = new Uri("Themes/" + (dark ? "Dark" : "Light") + ".xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = uri };

            var res = Application.Current.Resources;
            res.MergedDictionaries.Clear();
            res.MergedDictionaries.Add(dict);
        }
    }
}
