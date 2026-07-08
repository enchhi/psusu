using System.Windows;
using System.Windows.Controls;

namespace ScadaGUI
{
    // Bazni prozor za dijaloge: custom title bar (stil u Styles.xaml preko WindowChrome).
    // Title bar ima X (zatvaranje) i opciono Mock dugme (gore-desno) za dijaloge koji ga koriste.
    public class DialogWindow : Window
    {
        // Dijalozi koji hoce Mock dugme u title bar-u postave ovo na true (u konstruktoru).
        public bool ShowMockButton { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Close") is Button close)
                close.Click += (s, e) => Close();

            if (GetTemplateChild("PART_Mock") is Button mock)
            {
                mock.Visibility = ShowMockButton ? Visibility.Visible : Visibility.Collapsed;
                mock.Click += (s, e) => OnMock();
            }
        }

        // Override-uju dijalozi koji imaju mock popunjavanje polja.
        protected virtual void OnMock() { }
    }
}
