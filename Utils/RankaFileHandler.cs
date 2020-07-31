using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace Ranka.Utils
{
    public static class RankaFileHandler
    {
        public static string[] FileToStrings(string filename)
        {
            return File.ReadAllLines(filename).Where(x => !x.StartsWith("#")).ToArray();
        }

        public static string DownloadImage(string imageUrl)
        {
            Bitmap bitmap;

            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(imageUrl);
                bitmap = new Bitmap(stream);
                stream.Flush();
                stream.Close();
                bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + "image/input.png", ImageFormat.Png);
                client.Dispose();
                bitmap.Dispose();
                return AppDomain.CurrentDomain.BaseDirectory + "image/input.png";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}