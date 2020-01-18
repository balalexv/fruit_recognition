using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SerialPortMonitor.Views
{
    public class IntegerArrayToStringConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var data = (int[]) value;
            return string.Join(" ", data);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
