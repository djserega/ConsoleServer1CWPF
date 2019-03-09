using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;


namespace ConsoleServer1C.Converters
{
    /// <summary>
    /// Конвертер значений свойств элементов форм
    /// </summary>
    public class ElementsFormConverter : MarkupExtension, IValueConverter
    {
        private static ElementsFormConverter _instance;

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
            double result = System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            return result < 0 ? 0 : result;
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
            double result = System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            return result < 0 ? 0 : result;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new ElementsFormConverter());
        }
    }
}
