using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ScadaGUI
{
    // F1 (tema, zvuk) + F3 (jezik, format datuma, vremenska zona).
    public partial class SettingsWindow : Window
    {
        private bool loaded;
        private List<TimeZoneInfo> timeZones;

        public SettingsWindow()
        {
            InitializeComponent();

            // --- tema ---
            if (ThemeManager.IsDark) DarkRadio.IsChecked = true;
            else LightRadio.IsChecked = true;

            // --- zvuk ---
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

            // --- jezik ---
            LangCombo.SelectedIndex = Localizer.Language == Lang.En ? 1 : 0;

            // --- format datuma ---
            foreach (ComboBoxItem it in DateFormatCombo.Items)
            {
                if ((string)it.Content == Localizer.DateFormat) { DateFormatCombo.SelectedItem = it; break; }
            }
            if (DateFormatCombo.SelectedItem == null) DateFormatCombo.SelectedIndex = 0;

            // --- vremenska zona ---
            timeZones = TimeZoneInfo.GetSystemTimeZones().ToList();
            TimeZoneCombo.ItemsSource = timeZones.Select(z => z.DisplayName).ToList();
            int idx = timeZones.FindIndex(z => z.Id == Localizer.TimeZone.Id);
            if (idx < 0) idx = timeZones.FindIndex(z => z.Id == TimeZoneInfo.Local.Id);
            TimeZoneCombo.SelectedIndex = idx;

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

        private void Lang_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!loaded) return;
            Localizer.SetLanguage(LangCombo.SelectedIndex == 1 ? Lang.En : Lang.Sr);
        }

        private void DateFormat_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!loaded) return;
            if (DateFormatCombo.SelectedItem is ComboBoxItem it)
                Localizer.SetDateFormat((string)it.Content);
        }

        private void TimeZone_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!loaded) return;
            int i = TimeZoneCombo.SelectedIndex;
            if (i >= 0 && i < timeZones.Count)
                Localizer.SetTimeZone(timeZones[i]);
        }

        private void TestSound_Click(object sender, RoutedEventArgs e) => AlarmSound.Start();
        private void StopSound_Click(object sender, RoutedEventArgs e) => AlarmSound.Stop();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
