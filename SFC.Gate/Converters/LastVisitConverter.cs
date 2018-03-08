using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFC.Gate.Converters;

namespace SFC.Gate.Converters
{
    class LastVisitConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return ConvertToString(value as DateTime?);
        }

        public static string ConvertToString(DateTime? date)
        {
            
            if (date == null) return "Never";
            var span = DateTime.Now - date.Value;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 74) return "Few minutes";
            if (span.TotalHours < 7) return "Few hours";
            if (span.TotalHours < 48) return "Yesterday";
            if (span.TotalDays < 7) return "Few days";
            if (span.TotalDays < 14) return "Last week";
            if (span.TotalDays < 30) return "Few weeks";
            if (span.TotalDays < 74) return "Last month";
            if (span.TotalDays < 30 * 12) return "Few months";
            if (span.TotalDays < 471) return "Last year";

            return date.Value.ToShortDateString();
        }
    }
}
