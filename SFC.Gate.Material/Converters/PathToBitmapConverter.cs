using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace SFC.Gate.Converters {
    class PathToBitmapConverter : ConverterBase {
      

        public PathToBitmapConverter()
        {
      
        }

        public string Default { get; set; } = "pack://application:,,,/default_pic.jpeg";

        private static Dictionary<string, BitmapImage> Cache = new Dictionary<string, BitmapImage>();

        protected override object Convert(object value, Type targetType, object parameter)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            try
            {
                if (value != null && File.Exists((string) value))
                    using (var stream = File.OpenRead((string) value))
                    {
                        bmp.StreamSource = stream;
                        bmp.EndInit();
                        bmp.Freeze();
                    }
                else
                {
                    bmp.UriSource = new Uri(Default); 
                    bmp.EndInit();
                    bmp.Freeze();
                }

            }
            catch
            {
                if (Cache.ContainsKey((string) value+""))
                {
                    bmp = Cache[(string) value+""];
                }
                else
                {
                    return null;
                }
            }

            var key = value?.ToString() ?? "[EMPTY]";
            if (Cache.ContainsKey(key))
                Cache[key] = bmp;
            else
                Cache.Add(key, bmp);

            return bmp;

        }


    }
}
