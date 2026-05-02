using System.Globalization;

namespace Duocare.Converters;

public class PasswordColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            "Coincide" => Color.FromArgb("#22C55E"),   // Verde bonito
            "No coincide" => Color.FromArgb("#EF4444"), // Rojo elegante
            _ => Color.FromArgb("#A0AEC0")              // Gris suave
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
