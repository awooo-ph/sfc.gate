using System;
using Microsoft.Win32;

namespace SFC.Gate
{
    static class Extensions
    {
        public static string GetPicture()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter =
                @"All Images|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|
                                                            BMP Files|*.BMP;*.DIB;*.RLE|
                                                            JPEG Files|*.JPG;*.JPEG;*.JPE;*.JFIF|
                                                            GIF Files|*.GIF|
                                                            PNG Files|*.PNG";
            dialog.Title = "Select Picture";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (!(dialog.ShowDialog() ?? false)) return null;

            return dialog.FileName;
        }
    }
}
