using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SerialPortMonitor.Views
{
    public class DataColumnsCountConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = System.Convert.ToDouble(value);
            if (count <= 5)
                return count;

            return Math.Ceiling(count / 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
