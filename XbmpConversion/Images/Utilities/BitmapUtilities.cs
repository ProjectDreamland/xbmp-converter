using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using XBMPConverter.Images;

namespace XbmpConversion.Images.Utilities
{
    internal class BitmapUtilities
    {
        public static List<StoredImages> CachedImages { get; } = new List<StoredImages>();


        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public class StoredImages
        {
            public string Path { get; set; }
            public long Size { get; set; }
            public XbmpImage Xbmp { get; set; }
        }
    }
}