using System;
using SFC.Gate.ViewModels;

namespace SFC.Gate.Converters
{
    class IsCurrentScreen : ConverterBase
    {
        public IsCurrentScreen(MainViewModel.Screens target)
        {
            Target = target;
        }

        public MainViewModel.Screens Target { get; set; }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (MainViewModel.Instance.IsGuardMode) return false;
            return MainViewModel.Instance.SelectedTab == (int)Target;
        }
    }
}
