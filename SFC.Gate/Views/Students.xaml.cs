using System;
using System.Collections.Generic;
using System.IO;
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
using SFC.Gate.Configurations;
using SFC.Gate.Material.ViewModels;
using SFC.Gate.Models;

namespace SFC.Gate.Material.Views
{
    /// <summary>
    /// Interaction logic for Students.xaml
    /// </summary>
    public partial class Students : UserControl
    {
        public Students()
        {
            InitializeComponent();
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //ActivityButton.Visibility = Visibility.Collapsed;
            base.OnRenderSizeChanged(sizeInfo);
            //return;
            //if (!sizeInfo.WidthChanged) return;
            //if (ActualWidth < 777)
            //{
            //    Grid.SetColumnSpan(StudentsCard, 2);
            //    StudentsViewModel.Instance.StudentActivityOpen = false;
            //    ActivityButton.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    Grid.SetColumnSpan(StudentsCard,1);
            //    StudentsViewModel.Instance.StudentActivityOpen = false;
            //    ActivityButton.Visibility = Visibility.Collapsed;
            //}
        }

        private void DataGrid_OnInitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            var i = e.NewItem;
        }

        private void StudentsDrop(object sender, DragEventArgs e)
        {
            var stud = ((FrameworkElement) e.OriginalSource).DataContext as Student;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && stud != null)
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                var file = files?.FirstOrDefault();
                if (file == null) return;
                if (Extensions.IsImageFile(file))
                {
                    var pic = stud.Picture;
                    stud.Update(nameof(Student.Picture), Extensions.ResizeImage(file));
                    Log.Add("UPDATE", $"{stud.Fullname}'s picture was changed.", "Students", stud.Id);
                    MainViewModel.ShowMessage($"{stud.Fullname}'s picture was changed.","UNDO", () =>
                    {
                        stud.Update(nameof(Student.Picture),pic);
                        Log.Add("REVERT", $"{stud.Fullname}'s picture changed was undone.", "Students", stud.Id);
                    });
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void StudentsDragEnter(object sender, DragEventArgs e)
        {

            var stud = ((FrameworkElement) e.OriginalSource).DataContext as Student;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && stud != null)
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                var file = files?.FirstOrDefault();
                if (Extensions.IsImageFile(file))
                {
                    e.Effects = DragDropEffects.All;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void DataGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if(e.Column.Header.ToString()=="RFID")
            RfidScanner.ExclusiveCallback = id =>
            {
                ((Student)e.Row.Item).Rfid = id;
            };
        }

        private void DataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "RFID")
                RfidScanner.ExclusiveCallback = null;
        }
      
    }
}
