using System;
using System.Windows.Controls;

namespace SFC.Gate.Views
{
    /// <summary>
    /// Interaction logic for GuardMode.xaml
    /// </summary>
    public partial class GuardMode : UserControl
    {
        public GuardMode()
        {
            
            InitializeComponent();
            DateText.Text = DateTime.Now.ToString("yy-MMM-dd ddd");
        }
    }
}
