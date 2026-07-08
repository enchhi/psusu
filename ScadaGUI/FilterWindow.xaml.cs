using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    // F4: prozor za pretragu AI vrednosti iz baze + Generate TXT.
    public partial class FilterWindow : DialogWindow
    {
        private List<AnalogSample> results = new List<AnalogSample>();

        public FilterWindow()
        {
            InitializeComponent();
            ShowMockButton = true;
        }

        protected override void OnMock() => Mock_Click(this, null);

        // Dev pomoc: popuni filter nasumicnim postojecim AI tagom i sirokim vremenskim opsegom.
        // Vrednosti se ostave prazne (uslov se ignorise) da pretraga vrati sto vise rezultata.
        private void Mock_Click(object sender, RoutedEventArgs e)
        {
            var aiNames = DataConcentratorService.Instance.Tags.OfType<AnalogInput>().Select(a => a.Name).ToList();
            TagBox.Text = aiNames.Count > 0 ? MockData.Pick(aiNames) : "";

            var now = DateTime.Now;
            FromBox.Text = now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            ToBox.Text = now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            MinBox.Text = "";
            MaxBox.Text = "";
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tag = string.IsNullOrWhiteSpace(TagBox.Text) ? null : TagBox.Text.Trim();
                DateTime? from = ParseDate(FromBox.Text, "Vreme od");
                DateTime? to = ParseDate(ToBox.Text, "Vreme do");
                double? min = ParseNum(MinBox.Text, "Vrednost od");
                double? max = ParseNum(MaxBox.Text, "Vrednost do");

                results = DataConcentratorService.Instance.SearchSamples(tag, from, to, min, max);
                ResultsGrid.ItemsSource = results;
                CountText.Text = "Rezultata: " + results.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Neispravan filter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GenerateTxt_Click(object sender, RoutedEventArgs e)
        {
            if (results.Count == 0)
            {
                MessageBox.Show("Nema rezultata za izvoz. Prvo pokreni pretragu.");
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Text (*.txt)|*.txt", FileName = "pretraga.txt" };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dlg.FileName, SampleTxtGenerator.Generate(results));
                MessageBox.Show("Sacuvano: " + dlg.FileName);
            }
        }

        private static DateTime? ParseDate(string s, string field)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
            throw new FormatException(field + " mora biti u formatu yyyy-MM-dd HH:mm:ss.");
        }

        private static double? ParseNum(string s, string field)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s, out var v)) return v;
            throw new FormatException(field + " mora biti broj.");
        }
    }
}
