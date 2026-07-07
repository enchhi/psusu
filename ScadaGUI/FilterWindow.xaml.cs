using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    // F4: prozor za pretragu AI vrednosti iz baze + Generate TXT.
    public partial class FilterWindow : Window
    {
        private List<AnalogSample> results = new List<AnalogSample>();

        public FilterWindow()
        {
            InitializeComponent();
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
