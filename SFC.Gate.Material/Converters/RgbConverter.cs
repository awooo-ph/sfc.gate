using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SFC.Gate.Converters
{
    class RgbConverter:ConverterBase
    {
        public enum ARGB
        {
            Red, Green, Blue,Alpha
        }

        public ARGB Rgb { get; set; } = ARGB.Red;

        public float Red { get; set; } = 0;
        public float Green { get; set; } = 0;
        public float Blue { get; set; } = 0;
        public float Alpha { get; set; } = 1;
        
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if(value!=null)
            switch (Rgb)
            {
                case ARGB.Red:
                    Red = float.Parse(value + "");
                        break;
                case ARGB.Green:
                    Green = float.Parse(value + "");
                        break;
                case ARGB.Blue:
                    Blue = float.Parse(value+"");
                        break;
                    case ARGB.Alpha:
                        Alpha = float.Parse(value + "");
                        break;
                }
            
            var color = new SolidColorBrush(Color.FromScRgb(Alpha,Red,Green,Blue));
            return color;
        }
    }
}
