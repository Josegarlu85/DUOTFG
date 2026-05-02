using System.Globalization;

namespace Duocare.Converters;

public class BoolToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool done = (bool)value;
        return done ? "✔️" : "⭕";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
