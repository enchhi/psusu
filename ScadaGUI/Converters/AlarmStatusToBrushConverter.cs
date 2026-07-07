using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using DataConcentrator;

namespace ScadaGUI.Converters
{
    // Boja reda po stanju alarma AI-a: crveno = aktivan (nije acknowledge-ovan), zuto = acknowledged.
    public class AlarmStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnalogInput ai && ai.Alarms != null)
            {
                if (ai.Alarms.Any(a => a.State == AlarmState.Active))
                    return Brushes.Red;
                if (ai.Alarms.Any(a => a.State == AlarmState.Acknowledged))
                    return Brushes.Yellow;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
