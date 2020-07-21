using HtmlAgilityPack;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ranka.API
{
    public class Facetracker
    {
        private readonly string _query;
        private Browser _browser;
        private Page _page;

        public Facetracker(string query)
        {
            _query = query;
        }

        public async Task Prepare()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
            });
            await _browser.DefaultContext.OverridePermissionsAsync("https://www.facebook.com", new List<OverridePermission> { OverridePermission.Geolocation, OverridePermission.Notifications });
            _page = await _browser.NewPageAsync();
            await _page.GoToAsync("https://www.facebook.com/");
            await _page.WaitForSelectorAsync("#email");
            await _page.FocusAsync("#email");
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("FACEBOOK_USERNAME"));
            await _page.WaitForSelectorAsync("#pass");
            await _page.FocusAsync("#pass");
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("FACEBOOK_PASSWORD"));
            await _page.WaitForSelectorAsync("#loginbutton");
            await _page.ClickAsync("#loginbutton");
            await _page.WaitForNavigationAsync();
            await _page.WaitForTimeoutAsync(2000);
            await _page.GoToAsync($"https://www.facebook.com/search/people/?q={_query}&epa=SERP_TAB");
            await _page.WaitForTimeoutAsync(2000);
        }

        public async Task<List<FacetrackerObject>> GetSearchResults()
        {
            try
            {
                var content = await _page.GetContentAsync();

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
                    Console.WriteLine(imgUrl);
                    FacetrackerObject obj = new FacetrackerObject
                    {
                        Url = a.Attributes["href"].Value,
                        Title = a.Attributes["title"].Value,
                        Thumbnail = imgUrl
                    };
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

    public class FacetrackerObject
    {
        public string Url { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
    }
}