using HtmlAgilityPack;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ranka.API
{
    /// <summary>
    /// Fetches Instagram results from a query
    /// </summary>
    public class Instafetcher
    {
        private readonly string _query;

        private readonly string rootdir = AppDomain.CurrentDomain.BaseDirectory + "/screenshots";

        public Instafetcher(string query)
        {
            _query = query;
        }

        public async Task<List<InstafetcherObject>> GetSearchResults(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            await page.WaitForSelectorAsync(".XTCLo").ConfigureAwait(false);
            await page.FocusAsync(".XTCLo").ConfigureAwait(false);
            await page.Keyboard.TypeAsync(_query).ConfigureAwait(false);
            await page.WaitForSelectorAsync(".fuqBx").ConfigureAwait(false);
            await page.FocusAsync(".fuqBx").ConfigureAwait(false);
            try
            {
                var content = await page.GetContentAsync().ConfigureAwait(false);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

                var results = doc.DocumentNode.SelectNodes("/html/body/div[1]/section/nav/div[2]/div/div/div[2]/div[3]/div[2]/div");

                var array = results.Elements().ToArray();

                List<InstafetcherObject> output = new List<InstafetcherObject>();

                foreach (var item in array)
                {
                    InstafetcherObject obj = new InstafetcherObject();
                    if (item.Attributes["href"] == null)
                    {
                        throw new Exception("No results!");
                    }
                    obj.Url = item.Attributes["href"].Value;
                    output.Add(obj);
                }
                Console.WriteLine("Results fetched");
                return output;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GetScreenshot(List<InstafetcherObject> list, int index, Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            var _ = list.ToArray<InstafetcherObject>();

            var selected = _[index];

            await page.GoToAsync($"https://www.instagram.com{selected.Url}").ConfigureAwait(false);
            await page.WaitForTimeoutAsync(500).ConfigureAwait(false);

            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);

            await page.ScreenshotAsync($"{rootdir}/tmp_screenshot.png").ConfigureAwait(false);
            Console.WriteLine("Screenshot grabbed!");
            return $"{rootdir}/tmp_screenshot.png";
        }

        public async Task<string> GetScreenshot(List<InstafetcherObject> list, int index, Page page, int timeout)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            var _ = list.ToArray<InstafetcherObject>();

            var selected = _[index];

            await page.GoToAsync($"https://www.instagram.com{selected.Url}").ConfigureAwait(false);
            await page.WaitForTimeoutAsync(timeout).ConfigureAwait(false);

            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);

            await page.ScreenshotAsync($"{rootdir}/tmp_screenshot.png").ConfigureAwait(false);
            Console.WriteLine("Screenshot grabbed!");
            return $"{rootdir}/tmp_screenshot.png";
        }

        public async Task Cleanup(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            await page.CloseAsync().ConfigureAwait(false);
            if (File.Exists($"{rootdir}/tmp_screenshot.png")) File.Delete($"{rootdir}/tmp_screenshot.png");
            Console.WriteLine("Temporary files wiped. Page destroyed.");
        }
    }

    public class InstafetcherObject
    {
        public string Url { get; set; }
        public string Thumbnail { get; set; }
    }
}