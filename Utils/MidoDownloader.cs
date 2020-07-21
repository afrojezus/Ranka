using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ranka.Utils
{
    // Based on https://github.com/domnguyen/Discord-Bot/blob/master/src/Helpers/AudioDownloader.cs
    public class MidoDownloader
    {
        // Concurrent Library to keep track of the current downloads order.
        private readonly ConcurrentQueue<MidoFile> m_DownloadQueue = new ConcurrentQueue<MidoFile>();

        // Private variables.
        private readonly string m_DownloadPath = AppDomain.CurrentDomain.BaseDirectory + "/mido_tmp";              // Default folder path. This is relative to the running directory of the bot.

        private bool m_IsRunning = false;                   // Flag to check if it's in the middle of downloading already.
        private string m_CurrentlyDownloading = "";         // Currently downloading file.
        private readonly bool m_AllowDuplicates = true;              // Flag for downloading duplicate items.

        // Returns the current downloading folder.
        public string GetDownloadPath() { return m_DownloadPath; }

        // Returns the status of the downloader.
        public bool IsRunning() { return m_IsRunning; }

        // Returns the current download status.
        public string CurrentlyDownloading() { return m_CurrentlyDownloading; }

        // Returns a string with downloaded song names.
        public string[] GetAllItems()
        {
            // Check the files in the directory.
            string[] itemEntries = Directory.GetFiles(m_DownloadPath);
            int itemCount = itemEntries.Length;
            if (itemCount == 0) return new string[] { "There are currently no items downloaded." };
            return itemEntries;
        }

        // Returns a path to the downloaded item, if already downloaded.
        public string GetItem(string item)
        {
            // If it's been downloaded and isn't currently downloading, we can return it.
            try
            {
                if (File.Exists($"{m_DownloadPath}\\{item}") && !m_CurrentlyDownloading.Equals(item))
                    return $"{m_DownloadPath}\\{item}";
            }
            catch { }
            // Check by filename without .mp3.
            try
            {
                if (File.Exists($"{m_DownloadPath}\\{item}.mp3") && !m_CurrentlyDownloading.Equals(item))
                    return $"{m_DownloadPath}\\{item}.mp3";
            }
            catch { }

            // Else we return blank. This means the item doesn't exist in our library.
            return null;
        }

        // Returns a path to the downloaded item, if already downloaded.
        public string GetItem(int index)
        {
            // Check the files in the directory.
            string[] itemEntries = Directory.GetFiles(m_DownloadPath);

            // Return by index.
            if (index < 0 || index >= itemEntries.Length) return null;
            return itemEntries[index].Split(Path.DirectorySeparatorChar).Last();
        }

        // Returns the proper filename by searching the path for an existing file.
        // We use the song title we're searching for, without the .mp3.
        private string GetDuplicateItem(string item)
        {
            int count = 0;

            string filename = Path.Combine(m_DownloadPath, item + ".mp3");

            while (File.Exists(filename))
            {
                filename = Path.Combine(m_DownloadPath, item + "_" + (count++) + ".mp3");
            }

            return filename;
        }

        // Remove any duplicates created by the downloader.
        public void RemoveDuplicateItems()
        {
            ConcurrentDictionary<string, int> duplicates = new ConcurrentDictionary<string, int>();

            // Check the files in the directory.
            string[] itemEntries = Directory.GetFiles(m_DownloadPath);
            foreach (string item in itemEntries)
            {
                string filename = Path.GetFileNameWithoutExtension(item);

                // If it's a duplicate, get it's base name.
                var isDuplicate = int.TryParse(filename.Split('_').Last(), out _);
                if (isDuplicate) filename = filename.Split(new char[] { '_' }, 2)[0];

                // Get the current count, then update the count.
                duplicates.TryRemove(filename, out int count);
                duplicates.TryAdd(filename, ++count);

                try { if (count >= 2) File.Delete(item); }
                catch { Console.WriteLine("Problem while deleting duplicates."); }
            }
        }

        // Gets the next song in the queue for download.
        private MidoFile Pop()
        {
            m_DownloadQueue.TryDequeue(out MidoFile nextSong);
            return nextSong;
        }

        // Adds a song to the queue for download.
        public void Push(MidoFile song) { m_DownloadQueue.Enqueue(song); } // Only add if there's no errors.

        // Starts the download loop and downloads from the front of the queue.
        public async Task StartDownloadAsync()
        {
            if (m_IsRunning) return; // Download is already running, stop to avoid conflicts/race conditions.

            // Loop for downloading.
            m_IsRunning = true;
            while (m_DownloadQueue.Count > 0)
            {
                if (!m_IsRunning) return; // Stop downloading.
                await DownloadAsync(Pop());
            }
            m_IsRunning = false;
        }

        // Downloads the file in the background and sets downloaded to true when done.
        // This can be used to optimize network audio sources.
        private async Task DownloadAsync(MidoFile song)
        {
            // First we check if it's a network file that needs to be downloaded.
            if (!song.IsNetwork) return;

            // Then we check if the file already exists.
            string filename = GetItem(song.Title + ".mp3");
            if (filename != null) // We get the full path.
            {
                if (m_AllowDuplicates) filename = GetDuplicateItem(song.Title);
                else return;
            }
            else // This is our first time seeing it.
            {
                filename = m_DownloadPath + "\\" + song.Title + ".mp3";
            }

            { // Start downloading.
                // Set it as our currently downloading item.
                m_CurrentlyDownloading = filename;
                Console.WriteLine("Currently downloading : " + song.Title);

                // youtube-dl.exe
                Process youtubedl;

                try
                {
                    // Download Video. This replaces the format with the extension .mp3 in the end.
                    ProcessStartInfo youtubedlFile = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-x --audio-format mp3 -o \"{filename.Replace(".mp3", ".%(ext)s")}\" {song.FileName}",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    youtubedl = Process.Start(youtubedlFile);
                    youtubedl.WaitForExit();
                }
                catch
                {
                    // Error while downloading. Remove from folder if exists.
                    Console.WriteLine("Error while downloading " + song.Title);
                    if (GetItem(filename) != null) File.Delete(filename);
                }

                // Update the filename with the local directory, set it to local and downloaded to true.
                // Remember, the title is already set.
                song.FileName = filename;
                song.IsNetwork = false; // Network is now false.
                song.IsDownloaded = true;
                m_CurrentlyDownloading = ""; // Reset our currently downloading item.
            }

            await Task.Delay(0);
        }

        // Stops the download loop.
        public void StopDownload()
        {
            m_IsRunning = false;
        }

        // Verifies that the path is a network path and not a local path. Checks here before extracting.
        // TODO: Add more arguments here, but we'll just check based on http and assume a network link.
        public bool? VerifyNetworkPath(string path)
        {
            if (path == null) return null;
            return path.StartsWith("http");
        }

        // Extracts data from a network path or a search term and allocates it to an MidoFile.
        //
        // Filename - source by local filename or from network link.
        // Title - name of the song.
        // IsNetwork - If it's local or network.
        public async Task<MidoFile> GetAudioFileInfo(string path)
        {
            if (path == null) return null;
            Console.WriteLine("Extracting Meta Data for : " + path);

            // Verify if it's a network path or not.
            bool? verifyURL = VerifyNetworkPath(path);
            if (verifyURL == null)
            {
                Console.WriteLine("Path invalid.");
                return null;
            }

            // Construct audio file.
            MidoFile StreamData = new MidoFile();

            // Search for the path on YouTube.
            if (verifyURL == false)
            {
                Process youtubedl;

                try
                {
                    var youtubePath = "https://www.youtube.com/watch?v=";
                    ProcessStartInfo youtubedlMetaData = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $" -s -e --get-id \"ytsearch:{path}\"",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    youtubedl = Process.Start(youtubedlMetaData);
                    youtubedl.WaitForExit();

                    string[] output = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                    if (output.Length > 0)
                    {
                        StreamData.Title = output[0];
                        StreamData.FileName = youtubePath + output[1];
                    }

                    StreamData.IsNetwork = true;
                }
                catch
                {
                    throw new Exception("Failed to get media. ~(>_<。)＼");
                }
            }
            // Network file.
            else if (verifyURL == true)
            {
                // youtube-dl
                Process youtubedl;

                try
                {
                    // Get Video Title
                    ProcessStartInfo youtubedlMetaData = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-s -e {path}",// Add more flags for more options.
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    youtubedl = Process.Start(youtubedlMetaData);
                    youtubedl.WaitForExit();

                    // Read the output of the simulation
                    string[] output = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                    // Set the file name.
                    StreamData.FileName = path;

                    // Extract each line printed for it's corresponding data.
                    if (output.Length > 0)
                        StreamData.Title = output[0];

                    // Set other properties as follows.
                    StreamData.IsNetwork = true;
                }
                catch
                {
                    throw new Exception("Something inside of me went wrong... ~(>_<。)＼");
                }
            }

            await Task.Delay(0);
            return StreamData;
        }
    }
}