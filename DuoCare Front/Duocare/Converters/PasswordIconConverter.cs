using System.Globalization;

namespace Duocare.Converters;

public class PasswordIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            "Coincide" => "✔",
            "No coincide" => "✖",
            _ => ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
