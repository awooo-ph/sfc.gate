using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;
using Devcorner.NIdenticon;
using Devcorner.NIdenticon.BrushGenerators;
using Microsoft.Win32;

namespace SFC.Gate
{
    static class Extensions
    {

        public static void Print(string path)
        {
            var info = new ProcessStartInfo(path);
            //info.Arguments = "\"" + Config.PrinterName + "\"";
            info.CreateNoWindow = true;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.Verb = "PrintTo";
            Process.Start(info);
        }
        
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

        public const string ACCEPTED_EXTENSIONS = @".BMP.JPG.JPEG.GIF.PNG.BMP.DIB.RLE.JPE.JFIF";

        public static bool IsImageFile(string file)
        {
            if (file == null)
                return false;
            var ext = System.IO.Path.GetExtension(file)?.ToUpper();
            return File.Exists(file) && (ACCEPTED_EXTENSIONS.Contains(ext));

        }

        public static byte[] Generate()
        {
            var rnd = new Random();
            var color = Color.FromArgb(255, rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
            var gen = new IdenticonGenerator()
                .WithBlocks(5, 5)
                .WithSize(512, 512)
                .WithBlockGenerators(IdenticonGenerator.ExtendedBlockGeneratorsConfig)
                .WithBackgroundColor(Color.White)
                .WithBrushGenerator(new StaticColorBrushGenerator(color));

            using (var pic = gen.Create("awooo" + DateTime.Now.Ticks))
            {
                using (var stream = new MemoryStream())
                {
                    pic.Save(stream, ImageFormat.Jpeg);
                    return stream.ToArray();
                }
            }

        }

        public static byte[] ResizeImage(string file)
        {
            using (var img = System.Drawing.Image.FromFile(file))
            {
                using (var bmp = Resize(img, 777, Color.White))
                {
                    using (var bin = new MemoryStream())
                    {
                        bmp.Save(bin, ImageFormat.Jpeg);
                        return bin.ToArray();
                    }
                }
            }
        }

        public static Image Resize(Image imgPhoto, int size, Color background)
        {
            var sourceWidth = imgPhoto.Width;
            var sourceHeight = imgPhoto.Height;
            var sourceX = 0;
            var sourceY = 0;
            var destX = 0;
            var destY = 0;
            var nPercent = 0.0f;

            if (sourceWidth > sourceHeight)
                nPercent = (size / (float) sourceWidth);

            else
                nPercent = (size / (float) sourceHeight);



            var destWidth = (int) (sourceWidth * nPercent);
            var destHeight = (int) (sourceHeight * nPercent);

            var bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            var grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.Clear(background);
            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}
