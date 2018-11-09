using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ConsoleServer1C.Converters
{
    public class ExceededThresholdDbProcTookConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float floatValue = ToFloat(value);

            if (floatValue >= AppSettings.ExceededThresholdDbProcTookCritical)
                return new SolidColorBrush(Colors.Red);

            else if (floatValue >= AppSettings.ExceededThresholdDbProcTookHigh)
                return new SolidColorBrush(Colors.LimeGreen);

            else if (floatValue >= AppSettings.ExceededThresholdDbProcTookElevated)
                return new SolidColorBrush(Colors.Yellow);

            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(Colors.Transparent);
        }

        private float ToFloat(object value)
        {
            return System.Convert.ToSingle(value);
        }
    }
}
