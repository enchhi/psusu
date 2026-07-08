using System.Globalization;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class WriteValueWindow : DialogWindow
    {
        private readonly Tag output;

        public WriteValueWindow(Tag outputTag)
        {
            InitializeComponent();
            ShowMockButton = true;
            output = outputTag;
            InfoText.Text = "Upis u " + output.Name + " (" + output.Type + ", " + output.IOAddress + ")";
            ValueBox.Text = output.CurrentValue.ToString(CultureInfo.InvariantCulture);
        }

        // Dev pomoc: predlozi validnu nasumicnu vrednost (DO -> 0/1, AO -> unutar opsega ako postoji).
        private void Mock_Click(object sender, RoutedEventArgs e)
        {
            if (output.Type == TagType.DO)
                ValueBox.Text = MockData.Bool() ? "1" : "0";
            else if (output is AnalogOutput ao && ao.LowLimit < ao.HighLimit)
                ValueBox.Text = MockData.Double(ao.LowLimit, ao.HighLimit).ToString();
            else
                ValueBox.Text = MockData.Double(0, 100).ToString();
        }

        protected override void OnMock() => Mock_Click(this, null);

        private void Write_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(ValueBox.Text, out var v))
            {
                MessageBox.Show("Unesite broj.");
                return;
            }
            if (output.Type == TagType.DO && v != 0 && v != 1)
            {
                MessageBox.Show("Digitalni izlaz prima samo 0 ili 1.");
                return;
            }

            DataConcentratorService.Instance.WriteValue(output, v);
            Close();
        }
    }
}
