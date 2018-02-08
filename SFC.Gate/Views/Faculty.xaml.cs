using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SFC.Gate.Models;

namespace SFC.Gate.Material.Views
{
    /// <summary>
    /// Interaction logic for Faculty.xaml
    /// </summary>
    public partial class Faculty : UserControl
    {
        public Faculty()
        {
            InitializeComponent();
        }

        private void DataGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header.ToString() == "RFID")
                RfidScanner.ExclusiveCallback = id =>
                {
                    ((Student) e.Row.Item).Rfid = id;
                };
        }

        private void DataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "RFID")
                RfidScanner.ExclusiveCallback = null;
        }
    }
}
