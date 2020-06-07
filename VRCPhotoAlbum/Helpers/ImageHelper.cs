using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using KoyashiroKohaku.VrcMetaToolSharp;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public class ImageHelper
    {

        #region BitmapImage
        public static BitmapImage LoadBitmapImage(string filePath)
        {
            var bitmapImage = new BitmapImage();
            var stream = File.OpenRead(filePath);
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            stream.Close();
            stream.Dispose();
            return bitmapImage;
        }

        public static BitmapImage GetThumbnailImage(string filePath, string cashFolderPath)
        {
            if (!Directory.Exists(cashFolderPath))
            {
                Directory.CreateDirectory(cashFolderPath);
            }

            var thumbnailImageFilePath = $"{cashFolderPath}/tn_" + Path.GetFileName(filePath);

            if (!File.Exists(thumbnailImageFilePath))
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var originalImage = Image.FromStream(stream, false, false);
                    var thumbnailImage = originalImage.GetThumbnailImage(originalImage.Width / 8, originalImage.Height / 8, () => { return false; }, IntPtr.Zero);
                    thumbnailImage.Save(thumbnailImageFilePath, ImageFormat.Png);
                    originalImage.Dispose();
                    thumbnailImage.Dispose();
                }
            }
            return LoadBitmapImage(thumbnailImageFilePath);
        }
        #endregion

        public static Bitmap LoadImage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} is not found.");
            }
            return new Bitmap(filePath);
        }

        public static void SaveImage(Bitmap image, string filePath)
        {
            if (image is null)
            {
                throw new ArgumentNullException("image is null");
            }
            using (image)
            {
                image.Save(filePath, ImageFormat.Png);
            }
        }

        public static void RotateLeft90AndSave(string filePath)
        {
            var image = LoadImage(filePath);
            image = RotateLeft90(image);
            SaveImage(image, filePath);
        }

        public static void RotateRight90AndSave(string filePath)
        {
            var image = LoadImage(filePath);
            image = RotateRight90(image);
            SaveImage(image, filePath);
        }

        public static Bitmap RotateLeft90(string filePath)
        {
            var image = LoadImage(filePath);
            return RotateLeft90(image);
        }

        public static Bitmap RotateRight90(string filePath)
        {
            var image = LoadImage(filePath);
            return RotateRight90(image);
        }

        public static Bitmap RotateLeft90(Bitmap image)
        {
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return image;
        }

        public static Bitmap RotateRight90(Bitmap image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return image;
        }
    }
}
