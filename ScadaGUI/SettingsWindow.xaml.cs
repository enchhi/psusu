using System.Collections.Generic;
using System.Windows;

namespace ScadaGUI
{
    // F1: podesavanje teme (Light/Dark) i zvuka alarma (izbor + jacina).
    public partial class SettingsWindow : Window
    {
        private bool loaded;

        public SettingsWindow()
        {
            InitializeComponent();

            // tema
            if (ThemeManager.IsDark) DarkRadio.IsChecked = true;
            else LightRadio.IsChecked = true;

            // izbor zvuka (Windows sistemski zvukovi)
            SoundCombo.ItemsSource = new List<string>
            {
                @"C:\Windows\Media\Alarm01.wav",
                @"C:\Windows\Media\Alarm02.wav",
                @"C:\Windows\Media\ring.wav",
                @"C:\Windows\Media\chimes.wav",
                @"C:\Windows\Media\notify.wav"
            };
            SoundCombo.SelectedItem = AlarmSound.SoundPath;
            if (SoundCombo.SelectedItem == null) SoundCombo.SelectedIndex = 0;
            SoundCombo.SelectionChanged += (s, e) =>
            {
                if (SoundCombo.SelectedItem is string p) AlarmSound.SoundPath = p;
            };

            VolumeSlider.Value = AlarmSound.Volume;

            loaded = true;
        }

        private void Theme_Changed(object sender, RoutedEventArgs e)
        {
            if (!loaded) return;
            ThemeManager.Apply(DarkRadio.IsChecked == true);
        }

        private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AlarmSound.Volume = e.NewValue;
            AlarmSound.ApplyVolume();
        }

        private void TestSound_Click(object sender, RoutedEventArgs e) => AlarmSound.Start();
        private void StopSound_Click(object sender, RoutedEventArgs e) => AlarmSound.Stop();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
