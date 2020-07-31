using Ranka.Utils;
using System;
using System.Diagnostics;
using System.IO;

namespace Ranka.API
{
    /*
     * Tiny class for handling YoutubeDL
     */

    public class SmolYT
    {
        private readonly string _rootdir = AppDomain.CurrentDomain.BaseDirectory + "output/";
        private readonly string _url;
        private readonly string _output;

        public SmolYT(string url)
        {
            if (!url.StartsWith("https"))
                url = $"ytsearch:{url}";

            _url = url;

            if (!Directory.Exists(_rootdir)) Directory.CreateDirectory(_rootdir);

            _output = _rootdir + "%(title)s.%(ext)s";
        }

        public SmolYTObject GetVideo()
        {
            SmolYTObject smol = new SmolYTObject();
            Process youtube;
            try
            {
                Console.WriteLine("Getting metadata...");
                MidoFile metadata = YouTubeMetaData(_url);
                Console.WriteLine("Metadata fetched, downloading...");
                ProcessStartInfo ytinfo = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-f \"bestvideo[filesize < 7M,ext=mp4] + bestaudio[filesize < 7M]\" -o \"{_output}\" \"{metadata.FileName}\"",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                youtube = Process.Start(ytinfo);
                youtube.WaitForExit();
                Console.WriteLine("Done! Returning data");

                smol.file = $"{_rootdir}/{metadata.Title}.mp4";
                smol.meta = metadata;

                return smol;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SmolYTObject GetAudio()
        {
            SmolYTObject smol = new SmolYTObject();
            Process youtube;
            try
            {
                Console.WriteLine("Getting metadata...");
                MidoFile metadata = YouTubeMetaData(_url);
                Console.WriteLine("Metadata fetched, downloading...");
                ProcessStartInfo ytinfo = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-f \"bestvideo[filesize < 7M] + bestaudio[filesize < 7M]\" -x --audio-format \"mp3\" -o \"{_output}\"  \"{metadata.FileName}\"",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                youtube = Process.Start(ytinfo);
                youtube.WaitForExit();
                Console.WriteLine("Done! Returning data");

                smol.file = $"{_rootdir}/{metadata.Title}.mp3";
                smol.meta = metadata;

                return smol;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CleanUp()
        {
            DirectoryInfo di = new DirectoryInfo(_rootdir);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        private MidoFile YouTubeMetaData(string input)
        {
            Process youtubedl;
            MidoFile midoFile = new MidoFile();
            var youtubePath = "https://www.youtube.com/watch?v=";
            ProcessStartInfo youtubedlMetaData = new ProcessStartInfo()
            {
                FileName = "youtube-dl",
                Arguments = $" -s -e --get-id --get-description --get-thumbnail {input}",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            youtubedl = Process.Start(youtubedlMetaData);
            youtubedl.WaitForExit();

            string[] output = youtubedl.StandardOutput.ReadToEnd().Split('\n');

            if (output.Length > 0)
            {
                midoFile.Title = output[0];
                midoFile.FileName = youtubePath + output[1];
                midoFile.Description = output[3];
                midoFile.Thumbnail = output[2];
            }
            return midoFile;
        }
    }

    public class SmolYTObject
    {
        public string file { get; set; }
        public MidoFile meta { get; set; }
    }
}