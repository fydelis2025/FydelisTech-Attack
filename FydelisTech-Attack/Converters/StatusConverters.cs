using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FydelisTech_Attack.WPF.Converters
{

    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? "✅ Vivo" : "❌ Morto";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true
                ? new SolidColorBrush(Color.FromRgb(0, 255, 65))   // Verde
                : new SolidColorBrush(Color.FromRgb(139, 148, 158)); // Cinza
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HttpStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int statusCode)
                return new SolidColorBrush(Color.FromRgb(139, 148, 158));

            return statusCode switch
            {
                >= 200 and < 300 => new SolidColorBrush(Color.FromRgb(0, 255, 65)),     // Verde
                >= 300 and < 400 => new SolidColorBrush(Color.FromRgb(0, 212, 255)),    // Ciano
                >= 400 and < 500 => new SolidColorBrush(Color.FromRgb(255, 215, 0)),    // Amarelo
                >= 500 => new SolidColorBrush(Color.FromRgb(255, 51, 51)),              // Vermelho
                _ => new SolidColorBrush(Color.FromRgb(139, 148, 158))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }
    }
}