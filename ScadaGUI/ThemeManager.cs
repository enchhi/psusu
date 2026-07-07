using System;
using System.Windows;
using System.Windows.Media;

namespace ScadaGUI
{
    // F1: Light/Dark tema. Kombinacija:
    //  1) merged ResourceDictionary (implicitni stilovi) -> tema za NOVE prozore,
    //  2) direktno postavljanje Background/Foreground na VEC OTVORENE prozore
    //     (WPF ne prestilizuje vec ucitane prozore kad se resursi promene naknadno).
    public static class ThemeManager
    {
        public static bool IsDark { get; private set; }

        public static void Apply(bool dark)
        {
            IsDark = dark;

            // 1) za nove prozore
            var uri = new Uri("Themes/" + (dark ? "Dark" : "Light") + ".xaml", UriKind.Relative);
            var res = Application.Current.Resources;
            res.MergedDictionaries.Clear();
            res.MergedDictionaries.Add(new ResourceDictionary { Source = uri });

            // 2) za vec otvorene prozore
            Brush bg = dark ? new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)) : Brushes.White;
            Brush fg = dark ? Brushes.White : Brushes.Black;
            foreach (Window w in Application.Current.Windows)
            {
                w.Background = bg;
                w.Foreground = fg;
            }
        }

        // Nove prozore takodje odmah oboji tekucom temom (poziva se iz konstruktora prozora).
        public static void ApplyTo(Window w)
        {
            if (w == null) return;
            w.Background = IsDark ? new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)) : Brushes.White;
            w.Foreground = IsDark ? Brushes.White : Brushes.Black;
        }
    }
}
