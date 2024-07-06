using AsusFanControl.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AsusFanControl.Infrastructure
{
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return System.Convert.ToInt32(enumValue);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && targetType.IsEnum)
            {
                return Enum.Parse(targetType, intValue.ToString());
            }
            return null;
        }
    }
}