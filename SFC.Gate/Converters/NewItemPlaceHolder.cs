using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SFC.Gate.Converters
{
    class NewItemPlaceHolder : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (value == CollectionView.NewItemPlaceholder) return Visibility.Visible;
            return Visibility.Collapsed;
        }
    }
}
