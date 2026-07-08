using System.Windows;
using System.Windows.Controls;

namespace ScadaGUI
{
    // Bazni prozor za dijaloge: custom title bar (definisan stilom u Styles.xaml preko WindowChrome).
    // OnApplyTemplate povezuje dugme za zatvaranje iz template-a.
    public class DialogWindow : Window
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Close") is Button close)
                close.Click += (s, e) => Close();
        }
    }
}
