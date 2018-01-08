using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;

namespace SFC.Gate
{
    static class Extensions
    {
        public static string GetPicture()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = @"All Images|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|
                            BMP Files|*.BMP;*.DIB;*.RLE|
                            JPEG Files|*.JPG;*.JPEG;*.JPE;*.JFIF|
                            GIF Files|*.GIF|
                            PNG Files|*.PNG",
                Title = "Select Picture",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            if (!(dialog.ShowDialog() ?? false)) return null;

            return dialog.FileName;
        }

        private static ICommand _openModemsCommand;

        public static ICommand OpenModemsCommand => _openModemsCommand ?? (_openModemsCommand = new DelegateCommand(d =>
        {
            Process.Start("rundll32", "shell32.dll,Control_RunDLL modem.cpl");
        }));
    }
}
