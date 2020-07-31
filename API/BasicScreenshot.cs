using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ranka.API
{
    public class BasicScreenshot
    {
        private readonly string _url;
        private readonly string screenshotDir = AppDomain.CurrentDomain.BaseDirectory + "/screenshots";

        public BasicScreenshot(string url)
        {
            // TODO: Use an online database as blacklist
            /*string[] blacklists = RankaFileHandler.FileToStrings($"{AppDomain.CurrentDomain.BaseDirectory}/blacklist.txt");

            if (blacklists.Any(url.Contains))
            {
                throw new Exception("That site is blacklisted. （︶^︶）");
            }*/
            _url = url;
        }

        public async Task<string> GetScreenshot(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            await page.GoToAsync(_url).ConfigureAwait(false);
            await page.WaitForTimeoutAsync(2000).ConfigureAwait(false);
            if (!Directory.Exists(screenshotDir)) Directory.CreateDirectory(screenshotDir);
            await page.ScreenshotAsync($"{screenshotDir}/tmp_screenshot.png").ConfigureAwait(false);
            Console.WriteLine("Screenshot grabbed!");
            return $"{screenshotDir}/tmp_screenshot.png";
        }

        public async Task Cleanup(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            await page.CloseAsync().ConfigureAwait(false);
            if (File.Exists($"{screenshotDir}/tmp_screenshot.png")) File.Delete($"{screenshotDir}/tmp_screenshot.png");
            Console.WriteLine("Temporary files wiped. Page destroyed.");
        }
    }
}