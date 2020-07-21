using NYoutubeDL;
using NYoutubeDL.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ranka.API
{
    /*
     * Tiny class for handling YoutubeDL
     */

    public class SmolYT
    {
        private readonly string _rootdir = AppDomain.CurrentDomain.BaseDirectory + "/output/";

        private readonly YoutubeDL _youtube;

        public SmolYT(string url)
        {
            if (!url.Contains("http"))
                url = $"ytsearch:\"{url}\"";

            _youtube = new YoutubeDL
            {
                VideoUrl = url
            };
            _youtube.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            _youtube.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
            if (Directory.Exists(_rootdir)) _youtube.Options.FilesystemOptions.Output = _rootdir + "output.%(ext)s";
            else
            {
                Directory.CreateDirectory(_rootdir);
                _youtube.Options.FilesystemOptions.Output = _rootdir + "output.%(ext)s";
            }
            _youtube.Options.FilesystemOptions.RmCacheDir = true;
        }

        public async Task<string> GetVideo()
        {
            _youtube.Options.PostProcessingOptions.RecodeFormat = Enums.VideoFormat.mp4;
            try
            {
                await _youtube.PrepareDownloadAsync();

                await _youtube.DownloadAsync();

                var info = await _youtube.GetDownloadInfoAsync();

                return AppDomain.CurrentDomain.BaseDirectory + "/output/output.mp4";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<string> GetAudio()
        {
            _youtube.Options.PostProcessingOptions.AudioFormat = Enums.AudioFormat.mp3;
            _youtube.Options.PostProcessingOptions.ExtractAudio = true;
            try
            {
                await _youtube.PrepareDownloadAsync();

                await _youtube.DownloadAsync();

                var info = await _youtube.GetDownloadInfoAsync();

                return AppDomain.CurrentDomain.BaseDirectory + "/output/output.mp3";
            }
            catch (Exception e)
            {
                throw e;
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
    }
}