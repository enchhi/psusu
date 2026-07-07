using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DataConcentrator;

namespace ScadaGUI.Converters
{
    // Boja TEKSTA reda kad AI ima aktivan/acknowledge-ovan alarm: uvek TAMNA
    // (citljiva na crvenoj/zutoj u obe teme). Kad nema alarma -> UnsetValue,
    // pa red nasledi TextBrush i normalno prati temu.
    public class AlarmStatusToForegroundConverter : IValueConverter
    {
        private static readonly Brush AlarmText = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnalogInput ai && ai.Alarms != null &&
                ai.Alarms.Any(a => a.State == AlarmState.Active || a.State == AlarmState.Acknowledged))
                return AlarmText;

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
