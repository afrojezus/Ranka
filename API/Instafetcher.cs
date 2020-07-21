using HtmlAgilityPack;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ranka.API
{
    /// <summary>
    /// Fetches Instagram results from a query
    /// </summary>
    public class Instafetcher
    {
        private readonly string _query;
        private Browser _browser;
        private Page _page;

        public Instafetcher(string query)
        {
            _query = query;
        }

        public async Task Prepare()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            _page = await _browser.NewPageAsync();
            await _page.GoToAsync("https://www.instagram.com/");
            await _page.WaitForSelectorAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(2) > div > label > input");
            await _page.FocusAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(2) > div > label > input");
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("INSTAGRAM_USERNAME"));
            await _page.WaitForSelectorAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(3) > div > label > input");
            await _page.FocusAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(3) > div > label > input");
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("INSTAGRAM_PASSWORD"));
            await _page.ClickAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(4) > button");
            await _page.WaitForNavigationAsync();
            await _page.WaitForSelectorAsync(".XTCLo");
            await _page.FocusAsync(".XTCLo");
            await _page.Keyboard.TypeAsync(_query);
            await _page.WaitForSelectorAsync(".fuqBx");
            await _page.FocusAsync(".fuqBx");
        }

        public async Task<List<InstafetcherObject>> GetSearchResults()
        {
            try
            {
                var content = await _page.GetContentAsync();

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

                await _page.CloseAsync();
                await _browser.CloseAsync();

                _page = null;
                _browser = null;

                return output;
            }
            catch (Exception e)
            {
                await _page.CloseAsync();
                await _browser.CloseAsync();

                _page = null;
                _browser = null;
                throw e;
            }
        }
    }

    public class InstafetcherObject
    {
        public string Url { get; set; }
        public string Thumbnail { get; set; }
    }
}