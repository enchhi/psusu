using System;
using System.Windows;

namespace ScadaGUI
{
    // F1: Light/Dark tema. Stilovi (Styles.xaml) su uvek merged (App.xaml) i koriste
    // DynamicResource za boje. Ovde samo menjamo paletu (Light.xaml/Dark.xaml), pa se
    // ceo app - ukljucujuci vec otvorene prozore - presvuce UZIVO.
    public static class ThemeManager
    {
        public static bool IsDark { get; private set; }

        private static ResourceDictionary colorDict;

        public static void Apply(bool dark)
        {
            IsDark = dark;
            var res = Application.Current.Resources;

            if (colorDict != null)
                res.MergedDictionaries.Remove(colorDict);

            var uri = new Uri("Themes/" + (dark ? "Dark" : "Light") + ".xaml", UriKind.Relative);
            colorDict = new ResourceDictionary { Source = uri };
            res.MergedDictionaries.Add(colorDict);
        }
    }
}
