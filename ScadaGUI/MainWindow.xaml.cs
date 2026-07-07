using System;
using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        private readonly DataConcentratorService dc = DataConcentratorService.Instance;
        private System.Windows.Threading.DispatcherTimer inactivityTimer;
        private System.Windows.Threading.DispatcherTimer soundTimer;

        // true ako je admin izlogovan zbog neaktivnosti (App onda ponovo prikaze login).
        public bool LoggedOutByTimeout { get; private set; }

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

            Localizer.Changed += ApplyLanguage;                 // F3: promena jezika/TZ/formata
            TagsGrid.SelectionChanged += (s, ev) => UpdateUi(); // polish: kontekstualna dugmad
            ApplyLanguage();                                    // postavi tekst + dugmad + tooltipove

            SetupInactivityLogout();   // F5: auto-logout admina posle 5 min neaktivnosti
            SetupAlarmSound();         // F1: zvuk dok ima aktivnog alarma
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

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            new FilterWindow { Owner = this }.ShowDialog();
        }

        // F3: postavi sav tekst (jezik) + dugmad + tooltipove.
        private void ApplyLanguage()
        {
            HeaderText.Text = Localizer.T("app.title") + "   —   " + Session.Username + " (" + Session.CurrentRole + ")";
            AddBtn.Content = Localizer.T("btn.add");
            RemoveBtn.Content = Localizer.T("btn.remove");
            WriteBtn.Content = Localizer.T("btn.write");
            DetailsBtn.Content = Localizer.T("btn.details");
            ScanBtn.Content = Localizer.T("btn.scan");
            AckBtn.Content = Localizer.T("btn.ack");
            ReportBtn.Content = Localizer.T("btn.report");
            TraceBtn.Content = Localizer.T("btn.trace");
            ExportBtn.Content = Localizer.T("btn.export");
            ImportBtn.Content = Localizer.T("btn.import");
            FilterBtn.Content = Localizer.T("btn.filter");
            OptionsBtn.Content = Localizer.T("btn.options");

            TypeCol.Header = Localizer.T("col.type");
            NameCol.Header = Localizer.T("col.name");
            AddressCol.Header = Localizer.T("col.address");
            ValueCol.Header = Localizer.T("col.value");
            UnitCol.Header = Localizer.T("col.unit");
            DescCol.Header = Localizer.T("col.desc");

            UpdateUi();
        }

        // F5 (uloga) + polish (selekcija): dugme radi samo kad je upotrebljivo; tooltip objasnjava.
        private void UpdateUi()
        {
            bool admin = Session.IsAdmin;
            var sel = Selected;
            bool hasSel = sel != null;
            bool isInput = sel is AnalogInput || sel is DigitalInput;
            bool isOutput = sel != null && (sel.Type == TagType.AO || sel.Type == TagType.DO);
            bool isAi = sel is AnalogInput;

            SetBtn(AddBtn, admin, admin ? "tip.add" : "role.ro");
            SetBtn(RemoveBtn, admin && hasSel, !admin ? "role.ro" : (hasSel ? "tip.remove" : "tip.selectSignal"));
            SetBtn(WriteBtn, admin && isOutput, !admin ? "role.ro" : (isOutput ? "tip.write" : "tip.selectSignal"));
            SetBtn(ScanBtn, admin && isInput, !admin ? "role.ro" : (isInput ? "tip.scan" : "tip.selectSignal"));
            SetBtn(AckBtn, admin && isAi, !admin ? "role.ro" : (isAi ? "tip.ack" : "tip.selectSignal"));
            SetBtn(TraceBtn, admin, admin ? "tip.trace" : "role.ro");
            SetBtn(ImportBtn, admin, admin ? "tip.import" : "role.ro");

            SetBtn(DetailsBtn, isAi, isAi ? "tip.details" : "tip.selectSignal");
            SetBtn(ReportBtn, true, "tip.report");
            SetBtn(ExportBtn, true, "tip.export");
            SetBtn(FilterBtn, true, "tip.filter");
            SetBtn(OptionsBtn, true, "tip.options");

            Title = Localizer.T("app.title") + "  -  " + Session.Username + " (" + Session.CurrentRole + ")  "
                    + Localizer.T(admin ? "role.rw" : "role.ro");
        }

        private static void SetBtn(System.Windows.Controls.Button btn, bool enabled, string tipKey)
        {
            btn.IsEnabled = enabled;
            btn.ToolTip = Localizer.T(tipKey);
            System.Windows.Controls.ToolTipService.SetShowOnDisabled(btn, true); // tooltip i kad je onemoguceno
        }

        // F5: auto-logout admina posle 5 min neaktivnosti.
        private void SetupInactivityLogout()
        {
            if (!Session.IsAdmin) return;

            inactivityTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            inactivityTimer.Tick += (s, e) =>
            {
                inactivityTimer.Stop();
                LoggedOutByTimeout = true;
                Logger.Instance.Log(LogCategory.Login, "Auto-logout admina " + Session.Username + " (neaktivnost).");
                MessageBox.Show("Izlogovani ste zbog neaktivnosti (5 minuta).", "Auto-logout");
                Close();
            };
            inactivityTimer.Start();

            PreviewMouseMove += (s, e) => ResetInactivity();
            PreviewMouseDown += (s, e) => ResetInactivity();
            PreviewKeyDown += (s, e) => ResetInactivity();
        }

        private void ResetInactivity()
        {
            if (inactivityTimer == null) return;
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        // F1: zvuk alarma - svake sekunde proveri ima li aktivnog (neacknowledge-ovanog) alarma.
        private void SetupAlarmSound()
        {
            soundTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            soundTimer.Tick += (s, e) => UpdateAlarmSound();
            soundTimer.Start();
        }

        private void UpdateAlarmSound()
        {
            bool anyActive = dc.Tags.OfType<AnalogInput>()
                .SelectMany(a => a.Alarms)
                .Any(al => al.State == AlarmState.Active);

            if (anyActive) AlarmSound.Start();
            else AlarmSound.Stop();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow { Owner = this }.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Localizer.Changed -= ApplyLanguage;
            soundTimer?.Stop();
            inactivityTimer?.Stop();
            AlarmSound.Stop();
            dc.Shutdown();
        }
    }
}
