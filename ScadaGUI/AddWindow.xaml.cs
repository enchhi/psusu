using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class AddWindow : DialogWindow
    {
        private readonly DataConcentratorService dc = DataConcentratorService.Instance;

        public AddWindow()
        {
            InitializeComponent();
            TypeCombo.SelectedIndex = 0; // AI (pokrece i TypeCombo_SelectionChanged)
        }

        private string SelectedType => (TypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string t = SelectedType;
            bool isAlarm = t == "Alarm";
            bool isTag = !isAlarm && t != null;

            CommonPanel.Visibility = Vis(isTag);
            InputPanel.Visibility = Vis(t == "AI" || t == "DI");
            AnalogPanel.Visibility = Vis(t == "AI" || t == "AO");
            AiPanel.Visibility = Vis(t == "AI");
            OutputPanel.Visibility = Vis(t == "AO" || t == "DO");
            AlarmPanel.Visibility = Vis(isAlarm);

            if (isTag && Enum.TryParse(t, out TagType tagType))
            {
                AddressCombo.ItemsSource = PlcAddressMap.ForType(tagType);
                if (AddressCombo.Items.Count > 0) AddressCombo.SelectedIndex = 0;
            }

            if (isAlarm)
            {
                AlarmAiCombo.ItemsSource = dc.Tags.OfType<AnalogInput>().Select(a => a.Name).ToList();
                if (AlarmAiCombo.Items.Count > 0) AlarmAiCombo.SelectedIndex = 0;
                DirectionCombo.SelectedIndex = 0;
            }
        }

        private static Visibility Vis(bool b) => b ? Visibility.Visible : Visibility.Collapsed;

        // Dev pomoc: popuni polja validnim nasumicnim podacima iz pool-a (vidi MockData).
        private void Mock_Click(object sender, RoutedEventArgs e)
        {
            string t = SelectedType;
            if (t == null) return;
            if (t == "Alarm") { MockAlarm(); return; }

            // Zajednicko: jedinstveno ime, opis i slobodna adresa za tip.
            NameBox.Text = MockData.UniqueName(MockData.NamesFor(t), dc.Tags.Select(x => x.Name));
            DescBox.Text = MockData.Pick(MockData.Descriptions);
            if (Enum.TryParse(t, out TagType type))
            {
                string addr = MockData.FreeAddress(type, dc.Tags.Where(x => x.Type == type).Select(x => x.IOAddress));
                if (addr != null) AddressCombo.SelectedItem = addr;
            }

            // Ulazni (AI/DI): scan time + on scan.
            if (t == "AI" || t == "DI")
            {
                ScanTimeBox.Text = MockData.Pick(MockData.ScanTimes).ToString();
                OnScanBox.IsChecked = true;
            }

            // Analogni (AI/AO): low < high, units; za AO i initial unutar opsega.
            if (t == "AI" || t == "AO")
            {
                var lh = MockData.LowHigh();
                double low = lh[0], high = lh[1];
                LowBox.Text = low.ToString();
                HighBox.Text = high.ToString();
                UnitsBox.Text = MockData.Pick(MockData.Units);
                if (t == "AO") InitialBox.Text = MockData.Double(low, high).ToString();
            }

            // Samo AI: deadband + hysteresis.
            if (t == "AI")
            {
                DeadbandBox.Text = MockData.Pick(MockData.Deadbands).ToString();
                HystBox.Text = MockData.Pick(MockData.Hystereses).ToString();
            }

            // DO: initial 0 ili 1.
            if (t == "DO") InitialBox.Text = MockData.Bool() ? "1" : "0";
        }

        private void MockAlarm()
        {
            if (AlarmAiCombo.Items.Count == 0)
            {
                MessageBox.Show("Nema nijednog AI taga. Prvo dodaj AI da bi mock alarm imao na sta da se veze.");
                return;
            }
            AlarmAiCombo.SelectedIndex = MockData.Int(0, AlarmAiCombo.Items.Count - 1);
            LimitBox.Text = MockData.Double(10, 90).ToString();
            DirectionCombo.SelectedIndex = MockData.Bool() ? 0 : 1;
            AlarmMessageBox.Text = MockData.Pick(MockData.AlarmMessages);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedType == "Alarm") SaveAlarm();
                else SaveTag(SelectedType);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Neispravan unos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveTag(string t)
        {
            Tag tag;
            switch (t)
            {
                case "AI":
                    tag = new AnalogInput
                    {
                        ScanTime = ParseInt(ScanTimeBox.Text, "Scan time"),
                        OnScan = OnScanBox.IsChecked == true,
                        LowLimit = ParseDouble(LowBox.Text, "Low limit"),
                        HighLimit = ParseDouble(HighBox.Text, "High limit"),
                        Units = UnitsBox.Text,
                        Deadband = ParseDouble(DeadbandBox.Text, "Deadband"),
                        Hysteresis = ParseDouble(HystBox.Text, "Hysteresis")
                    };
                    break;
                case "AO":
                    tag = new AnalogOutput
                    {
                        LowLimit = ParseDouble(LowBox.Text, "Low limit"),
                        HighLimit = ParseDouble(HighBox.Text, "High limit"),
                        Units = UnitsBox.Text,
                        InitialValue = ParseDouble(InitialBox.Text, "Initial value")
                    };
                    break;
                case "DI":
                    tag = new DigitalInput
                    {
                        ScanTime = ParseInt(ScanTimeBox.Text, "Scan time"),
                        OnScan = OnScanBox.IsChecked == true
                    };
                    break;
                case "DO":
                    tag = new DigitalOutput
                    {
                        InitialValue = ParseDouble(InitialBox.Text, "Initial value")
                    };
                    break;
                default:
                    throw new InvalidOperationException("Izaberite tip.");
            }

            tag.Name = NameBox.Text;
            tag.Description = DescBox.Text;
            tag.IOAddress = AddressCombo.SelectedItem as string;

            dc.AddTag(tag); // baca ArgumentException sa porukama ako nije validan

            // pocetnu vrednost izlaza odmah upisi u PLC
            if (tag is AnalogOutput ao) dc.WriteValue(ao, ao.InitialValue);
            else if (tag is DigitalOutput dof) dc.WriteValue(dof, dof.InitialValue);
        }

        private void SaveAlarm()
        {
            string aiName = AlarmAiCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(aiName))
                throw new InvalidOperationException("Ne postoji nijedan AI tag za alarm.");

            var dir = (DirectionCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Below"
                ? AlarmDirection.Below
                : AlarmDirection.Above;

            var alarm = new Alarm
            {
                Name = "Alarm_" + aiName,
                TagName = aiName,
                LimitValue = ParseDouble(LimitBox.Text, "Granica"),
                Direction = dir,
                Message = string.IsNullOrWhiteSpace(AlarmMessageBox.Text) ? ("Alarm na " + aiName) : AlarmMessageBox.Text,
                State = AlarmState.Inactive
            };

            dc.AddAlarm(alarm);
        }

        private static int ParseInt(string s, string field)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            if (!int.TryParse(s, out var v)) throw new FormatException(field + " mora biti ceo broj.");
            return v;
        }

        private static double ParseDouble(string s, string field)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            if (!double.TryParse(s, out var v)) throw new FormatException(field + " mora biti broj.");
            return v;
        }
    }
}
