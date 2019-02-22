using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;


namespace ConsoleServer1C.Converters
{
    public class ElementsFormConverter : MarkupExtension, IValueConverter
    {
        private static ElementsFormConverter _instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            return result < 0 ? 0 : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            return result < 0 ? 0 : result;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new ElementsFormConverter());
        }
    }
}
