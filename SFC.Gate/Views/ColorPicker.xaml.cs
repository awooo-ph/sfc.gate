using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SFC.Gate.Views
{
    /// <summary>
    /// floateraction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
        }
        
        //public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        //    "SelectedColor", typeof(SolidColorBrush), typeof(ColorPicker), new PropertyMetadata(new SolidColorBrush(Colors.Black), SelectedColorChanged));

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor", typeof(SolidColorBrush), typeof(ColorPicker), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedColorChanged));

        private static void SelectedColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var color = (SolidColorBrush) dependencyObject.GetValue(SelectedColorProperty);
            dependencyObject.SetValue(AlphaProperty,color.Color.ScA);
            dependencyObject.SetValue(RedProperty, color.Color.ScR);
            dependencyObject.SetValue(GreenProperty, color.Color.ScG);
            dependencyObject.SetValue(BlueProperty, color.Color.ScB);

        }

        public SolidColorBrush SelectedColor
        {
            get { return (SolidColorBrush) GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(string), typeof(ColorPicker), new PropertyMetadata("Black", ValueChanged));

        private static void ValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
           // var color = (SolidColorBrush)dependencyObject.GetValue(SelectedColorProperty);
            
        }

        public string Value
        {
            get { return (string) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        
        public static readonly DependencyProperty BlueProperty = DependencyProperty.Register(
            "Blue", typeof(float), typeof(ColorPicker), new PropertyMetadata(default(float), ColorChanged));

        private static void ColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var a =(float) dependencyObject.GetValue(AlphaProperty);
            var r =(float) dependencyObject.GetValue(RedProperty);
            var g =(float) dependencyObject.GetValue(GreenProperty);
            var b =(float) dependencyObject.GetValue(BlueProperty);
            
            var color = new SolidColorBrush(Color.FromScRgb(a,r,g,b)); 
            dependencyObject.SetValue(SelectedColorProperty, color);
        }

        public float Blue
        {
            get { return (float) GetValue(BlueProperty); }
            set { SetValue(BlueProperty, value); }
        }

        public static readonly DependencyProperty RedProperty = DependencyProperty.Register(
            "Red", typeof(float), typeof(ColorPicker), new PropertyMetadata(default(float), ColorChanged));

        public float Red
        {
            get { return (float) GetValue(RedProperty); }
            set { SetValue(RedProperty, value); }
        }

        public static readonly DependencyProperty GreenProperty = DependencyProperty.Register(
            "Green", typeof(float), typeof(ColorPicker), new PropertyMetadata(default(float), ColorChanged));

        public float Green
        {
            get { return (float) GetValue(GreenProperty); }
            set { SetValue(GreenProperty, value); }
        }

        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register(
            "Alpha", typeof(float), typeof(ColorPicker), new PropertyMetadata(1f, ColorChanged));

        public float Alpha
        {
            get { return (float) GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }
    }
}
