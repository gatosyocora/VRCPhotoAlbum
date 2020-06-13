using Gatosyocora.VRCPhotoAlbum.Wrappers;
using KoyashiroKohaku.VrcMetaTool;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public class ImageHelper
    {
        private static BitmapImage _failedImage => LoadBitmapImage(@"pack://application:,,,/Resources/failed.png");

        #region BitmapImage
        public static BitmapImage LoadBitmapImage(string filePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            Stream streamBase;
            if (filePath.StartsWith(@"pack://application:,,,"))
            {
                var streamInfo = Application.GetResourceStream(new Uri(filePath));
                streamBase = streamInfo.Stream;
            }
            else
            {
                streamBase = File.OpenRead(filePath);
            }

            using (var stream = new DisposableStream(streamBase))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.None;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        public static string GetThumbnailImagePath(string filePath, string cacheFolderPath)
        {
            return $"{cacheFolderPath}/{Path.GetFileNameWithoutExtension(filePath)}.jpg";
        }

        public static async Task CreateThumbnailImagePathAsync(string originalFilePath, string thumbnailFilePath)
        {
            await Task.Run(() =>
            {
                using (var stream = File.OpenRead(originalFilePath))
                {
                    var originalImage = Image.FromStream(stream, false, false);
                    var thumbnailImage = originalImage.GetThumbnailImage(originalImage.Width / 8, originalImage.Height / 8, () => { return false; }, IntPtr.Zero);
                    thumbnailImage.Save(thumbnailFilePath, ImageFormat.Jpeg);
                    originalImage.Dispose();
                    thumbnailImage.Dispose();
                }
            });
        }
        #endregion

        #region Bitmap
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

        public static void SaveImage(byte[] imageBuffer, string filePath)
        {
            File.WriteAllBytes(filePath, imageBuffer);
        }
        #endregion

        #region Convert
        public static Bitmap Bytes2Bitmap(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                var bitmap = new Bitmap(ms);
                ms.Close();
                return bitmap;
            }
        }

        public static byte[] Bitmap2Bytes(Bitmap bitmap)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        }
        #endregion

        #region ImageProcessing

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

        public static Bitmap FlipHorizontal(Bitmap image)
        {
            image.RotateFlip(RotateFlipType.Rotate180FlipY);
            return image;
        }

        #endregion

        public static void RotateLeft90AndSave(string filePath, VrcMetaData metaData)
        {
            var image = RotateLeft90(LoadImage(filePath));
            var buffer = Bitmap2Bytes(image);
            if (!(metaData is null)) buffer = VrcMetaDataWriter.Write(buffer, metaData);
            SaveImage(buffer, filePath);
        }

        public static void RotateRight90AndSave(string filePath, VrcMetaData metaData)
        {
            var image = RotateRight90(LoadImage(filePath));
            var buffer = Bitmap2Bytes(image);
            if (!(metaData is null)) buffer = VrcMetaDataWriter.Write(buffer, metaData);
            SaveImage(buffer, filePath);
        }

        public static void FilpHorizontalAndSave(string filePath, VrcMetaData metaData)
        {
            var image = FlipHorizontal(LoadImage(filePath));
            var buffer = Bitmap2Bytes(image);
            if (!(metaData is null)) buffer = VrcMetaDataWriter.Write(buffer, metaData);
            SaveImage(buffer, filePath);
        }
    }
}
