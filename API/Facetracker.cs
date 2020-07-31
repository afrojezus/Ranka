using HtmlAgilityPack;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ranka.API
{
    public class Facetracker
    {
        private readonly string _query;

        private readonly string rootdir = AppDomain.CurrentDomain.BaseDirectory + "/screenshots";

        public Facetracker(string query)
        {
            _query = query;
        }

        public async Task<List<FacetrackerObject>> GetSearchResults(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");

            await page.GoToAsync($"https://www.facebook.com/search/people/?q={_query}&epa=SERP_TAB").ConfigureAwait(false);
            await page.WaitForTimeoutAsync(1500).ConfigureAwait(false);
            try
            {
                var content = await page.GetContentAsync().ConfigureAwait(false);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

                var results = doc.DocumentNode.SelectSingleNode("//*[@id='BrowseResultsContainer']");

                if (results == null)
                {
                    throw new Exception("No results!");
                }

                List<FacetrackerObject> output = new List<FacetrackerObject>();

                foreach (HtmlNode item in results.SelectNodes("./div"))
                {
                    var a = item.SelectSingleNode($"./div/div/div/div[1]/a");
                    var img = a.SelectSingleNode($"./img");

                    var imgUrl = img.Attributes["src"].Value;
                    int index = imgUrl.IndexOf("?");
                    if (index > 0)
                        imgUrl = imgUrl.Substring(0, index);
                    FacetrackerObject obj = new FacetrackerObject
                    {
                        Url = a.Attributes["href"].Value,
                        Title = a.Attributes["title"].Value,
                        Thumbnail = imgUrl
                    };
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

        public async Task<string> GetScreenshot(List<FacetrackerObject> list, int index, Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page), "No chromium instance passed");
            var _ = list.ToArray<FacetrackerObject>();

            var selected = _[index];

            await page.GoToAsync($"{selected.Url}").ConfigureAwait(false);

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

    public class FacetrackerObject
    {
        public string Url { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
    }
}