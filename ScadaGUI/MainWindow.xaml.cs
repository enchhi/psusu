using System;
using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        private readonly DataConcentratorService dc = DataConcentratorService.Instance;

        public MainWindow()
        {
            InitializeComponent();

            // F7: primeni sacuvan traceword (default = sve kategorije ukljucene).
            Logger.Instance.TraceWord = TraceWordStore.Load(App.TraceWordPath, long.MaxValue);

            try
            {
                dc.LoadFromDb();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(LogCategory.Error, "Greska pri ucitavanju baze: " + ex.Message);
                MessageBox.Show("Greska pri ucitavanju baze:\n" + ex.Message,
                    "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TagsGrid.ItemsSource = dc.Tags;
            dc.AlarmActivated += OnAlarmActivated;
        }

        private Tag Selected => TagsGrid.SelectedItem as Tag;

        // Alarm se aktivira iz scan niti -> osvezi prikaz na UI niti.
        private void OnAlarmActivated(int alarmId)
        {
            Dispatcher.BeginInvoke(new Action(() => TagsGrid.Items.Refresh()));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            new AddWindow { Owner = this }.ShowDialog();
            TagsGrid.Items.Refresh();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Selected == null) return;
            dc.RemoveTag(Selected.Name);
        }

        private void WriteValue_Click(object sender, RoutedEventArgs e)
        {
            var tag = Selected;
            if (tag == null) return;
            if (tag.Type != TagType.AO && tag.Type != TagType.DO)
            {
                MessageBox.Show("Vrednost se upisuje samo u izlazne (AO/DO) tagove.");
                return;
            }
            new WriteValueWindow(tag) { Owner = this }.ShowDialog();
            TagsGrid.Items.Refresh();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (Selected is AnalogInput ai)
                new DetailsWindow(ai) { Owner = this }.ShowDialog();
            else
                MessageBox.Show("Detalji su dostupni samo za analogne ulaze (AI).");
        }

        private void ToggleScan_Click(object sender, RoutedEventArgs e)
        {
            var tag = Selected;
            if (tag == null) return;

            bool isInput = tag is AnalogInput || tag is DigitalInput;
            if (!isInput)
            {
                MessageBox.Show("Skeniranje je dostupno samo za ulazne (AI/DI) tagove.");
                return;
            }

            bool onScan = (tag is AnalogInput ai && ai.OnScan) || (tag is DigitalInput di && di.OnScan);
            if (onScan) dc.StopScan(tag.Name);
            else dc.StartScan(tag);
        }

        private void Acknowledge_Click(object sender, RoutedEventArgs e)
        {
            if (Selected is AnalogInput ai)
            {
                foreach (var a in ai.Alarms.Where(x => x.State == AlarmState.Active).ToList())
                    dc.Acknowledge(a.Id);
                TagsGrid.Items.Refresh();
            }
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = "report.txt"
            };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dlg.FileName, dc.GenerateReport());
                MessageBox.Show("Report sacuvan: " + dlg.FileName);
            }
        }

        private void TraceSettings_Click(object sender, RoutedEventArgs e)
        {
            new TraceSettingsWindow { Owner = this }.ShowDialog();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                FileName = "konfiguracija.json"
            };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dlg.FileName, dc.ExportConfigJson());
                MessageBox.Show("Konfiguracija izvezena: " + dlg.FileName);
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON (*.json)|*.json" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    int added = dc.ImportConfigJson(System.IO.File.ReadAllText(dlg.FileName));
                    TagsGrid.Items.Refresh();
                    MessageBox.Show("Uvezeno tagova: " + added);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Greska pri importu: " + ex.Message);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dc.Shutdown();
        }
    }
}
