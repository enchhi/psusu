using System;
using System.Globalization;
using System.Windows.Data;

namespace ScadaGUI.Converters
{
    // F3: prikaz DateTime-a u izabranoj vremenskoj zoni i formatu.
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DateTime dt ? Localizer.FormatTime(dt) : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
