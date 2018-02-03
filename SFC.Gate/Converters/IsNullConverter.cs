using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SFC.Gate.Converters
{
    class IsNullConverter : ConverterBase
    {
        public bool Invert { get; set; }
        private bool _returnVisibility;

        public bool ReturnVisibility
        {
            get { return _returnVisibility; }
            set
            {
                _returnVisibility = value;
                if (value)
                {
                    TrueValue = Visibility.Visible;
                    FalseValue = Visibility.Collapsed;
                }
            }
        }

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;
        
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (Invert)
                return value != null ? TrueValue : FalseValue;
            return value == null ? TrueValue : FalseValue;
        }
    }
}
