using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(AnalogInput ai)
        {
            InitializeComponent();
            HeaderText.Text = "Alarmi za " + ai.Name;
            AlarmsGrid.ItemsSource = ai.Alarms.ToList();
        }
    }
}
