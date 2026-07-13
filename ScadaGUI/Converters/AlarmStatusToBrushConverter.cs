using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using DataConcentrator;

namespace ScadaGUI.Converters
{
    // Boja reda po stanju alarma AI-a: crveno = aktivan (nije acknowledge-ovan), zuto = acknowledged.
    // Namerno MEKSE nijanse (ne cist #FF0000/#FFFF00) da budu prijatne oku i citljive sa tamnim tekstom
    // u obe teme; jednake su u light/dark jer su semanticke (status), a kontrast obezbedjuje AlarmFg.
    public class AlarmStatusToBrushConverter : IValueConverter
    {
        private static readonly Brush ActiveBrush = Freeze(0xE2, 0x4C, 0x4C);   // umeren alarmni crveni
        private static readonly Brush AckBrush = Freeze(0xF0, 0xA9, 0x3B);      // amber (umesto sirovog zutog)

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnalogInput ai && ai.Alarms != null)
            {
                if (ai.Alarms.Any(a => a.State == AlarmState.Active))
                    return ActiveBrush;
                if (ai.Alarms.Any(a => a.State == AlarmState.Acknowledged))
                    return AckBrush;
            }
            return Brushes.Transparent;
        }

        private static Brush Freeze(byte r, byte g, byte b)
        {
            var br = new SolidColorBrush(Color.FromRgb(r, g, b));
            br.Freeze();
            return br;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
