using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class AddWindow : Window
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
