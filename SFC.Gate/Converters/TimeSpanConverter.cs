using System;
using System.Windows.Data;

namespace SFC.Gate.Converters
{
    class TimeSpanConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is TimeSpan span)) return Binding.DoNothing;
            if (span.TotalMilliseconds < 777) return "";
            if (span.TotalMinutes < 1)
                return $"{span.TotalSeconds:#,##0.000} SECONDS";
            if (span.TotalHours < 1)
                return $"{(long) span.TotalMinutes} MINUTES";
            
            var hours =(long) span.TotalMinutes/60;
            var h = hours > 1 ? $"{hours} HOURS" : "1 HOUR";
            return $"{h}";
            
        }
    }
}
