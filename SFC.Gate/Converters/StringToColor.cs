using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SFC.Gate.Converters
{
    class StringToColor : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (value.ToString()=="") value = "Black";
            return new BrushConverter().ConvertFromString(value + "");
            return ColorConverter.ConvertFromString(value+"");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();           
        }
    }
}
