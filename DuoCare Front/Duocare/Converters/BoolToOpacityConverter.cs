using System.Globalization;

namespace Duocare.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool done = (bool)value;
        return done ? 0.4 : 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
