using ConsoleServer1C.Settings;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ConsoleServer1C.Converters
{
    /// <summary>
    /// Класс выделения цветом ячеек у которых сработало превышение порогового значения времени захвата СУБД
    /// </summary>
    public class ExceededThresholdDbProcTookConverter : IValueConverter
    {
        /// <summary>
        /// Конвертер значения
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="targetType">Тип</param>
        /// <param name="parameter">Параметр</param>
        /// <param name="culture">Культура</param>
        /// <returns>Результат конвертации</returns>
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

        /// <summary>
        /// Конвертер обратного значения
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="targetType">Тип</param>
        /// <param name="parameter">Параметр</param>
        /// <param name="culture">Культура</param>
        /// <returns>Результат конвертации</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// Конвертация object в тип float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float ToFloat(object value)
        {
            return System.Convert.ToSingle(value);
        }
    }
}
