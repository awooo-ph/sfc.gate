using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SFC.Gate.Converters
{
    class SizeConverter : ConverterBase
    {
        public double MinWidth { get; set; }

        public SizeConverter()
        {
            MinWidth = 800;
        }

        public SizeConverter(double width)
        {
            MinWidth = width;
        }

        protected override object Convert(object value, Type targetType, object parameter)
        {
          
            var dVal = (double) value;
            if (dVal < MinWidth) return dVal - 74;
            
            return (dVal-74)/2;
        }
    }
}
