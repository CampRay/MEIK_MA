
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MEIKScreen.Common
{
    public class ImageTools
    {
        /// <summary>
        /// 读取图片文件并保存到字节流中，这样就不会锁定原始的图片文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static BitmapImage GetBitmapImage(string filePath)
        {
            BitmapImage bitmap = new BitmapImage();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    FileInfo fi = new FileInfo(filePath);
                    byte[] bytes = reader.ReadBytes((int)fi.Length);
                    reader.Close();
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(bytes);
                    bitmap.EndInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return bitmap;
        }

        /// <summary>
        /// 读取图片文件的字节流，获取BitmapImage对象
        /// </summary>
        /// <param name="filePath">图片文件路径</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage GetBitmapImage(byte[] streamBytes)
        {
            BitmapImage bitmap = new BitmapImage();
            try
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(streamBytes);
                bitmap.EndInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return bitmap;
        }

        /// <summary>
        /// 读取图片文件并保存到字节流中
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] GetBytesImage(string filePath)
        {            
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    FileInfo fi = new FileInfo(filePath);
                    byte[] bytes = reader.ReadBytes((int)fi.Length);
                    reader.Close();
                    return bytes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }
    }
}