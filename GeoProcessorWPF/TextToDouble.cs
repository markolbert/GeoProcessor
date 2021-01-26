using System;
using System.Globalization;
using System.Windows.Data;

namespace J4JSoftware.GeoProcessor
{
    public class TextToDouble : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if( double.TryParse( value.ToString(), out var result ) )
                return result;

            return 1.0;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value.ToString() ?? string.Empty;
        }
    }
}
