using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ranka.Services
{
    // Ensure Ranka only has one instance of Chromium running upon request.
    public class ChromiumService : RankaService
    {
        private Browser _browser;
        private Page _page;

        private int _chromeProcessId;

        private readonly string[] _browserArg =
            {
                "--single-process",
                "--lang=en-GB",
                "--proxy-server=socks5://127.0.0.1:9050",
                "--mute-audio",
                "--no-first-run",
            };

        public async Task<Browser> NewBrowserAsync()
        {
            var process = Process.GetProcessesByName("usr/bin/chromium-browser/chromium-browser-v7"); // Specific to Raspbian Chromium, change this if on different platform.
            if (process.Length > 0)
            {
                foreach (var item in process)
                {
                    item.Kill();
                }
                Console.WriteLine("Previous chromium instances killed, continuing...");
            }

            Console.WriteLine("Launching chromium...");
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = _browserArg,
                ExecutablePath = "/usr/bin/chromium-browser"
            }).ConfigureAwait(false);
            await _browser.DefaultContext.OverridePermissionsAsync("https://www.facebook.com", new List<OverridePermission> { OverridePermission.Geolocation, OverridePermission.Notifications }).ConfigureAwait(false);
            _page = await _browser.NewPageAsync().ConfigureAwait(false);
            _page.DefaultNavigationTimeout = 0;
            _page.DefaultTimeout = 0;
            await _page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1280,
                Height = 1000
            }).ConfigureAwait(false);
            _chromeProcessId = _browser.Process.Id;
            Console.WriteLine($"Chromium instance {_browser.Process.Id} ready");
            return _browser;
        }

        public async Task<Browser> NewBrowserAsync(int height = 1000)
        {
            var process = Process.GetProcessesByName("usr/bin/chromium-browser/chromium-browser-v7"); // Specific to Raspbian Chromium, change this if on different platform.
            if (process.Length > 0)
            {
                foreach (var item in process)
                {
                    item.Kill();
                }
                Console.WriteLine("Previous chromium instances killed, continuing...");
            }

            Console.WriteLine("Launching chromium...");
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = _browserArg,
                ExecutablePath = "/usr/bin/chromium-browser"
            }).ConfigureAwait(false);
            await _browser.DefaultContext.OverridePermissionsAsync("https://www.facebook.com", new List<OverridePermission> { OverridePermission.Geolocation, OverridePermission.Notifications }).ConfigureAwait(false);
            _page = await _browser.NewPageAsync().ConfigureAwait(false);
            _page.DefaultNavigationTimeout = 0;
            _page.DefaultTimeout = 0;
            await _page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1280,
                Height = height
            }).ConfigureAwait(false);
            _chromeProcessId = _browser.Process.Id;
            Console.WriteLine($"Chromium instance {_browser.Process.Id} ready");
            return _browser;
        }

        public Page GetChromiumPage()
        {
            return _page;
        }

        public async Task FBLogin()
        {
            await _page.GoToAsync("https://www.facebook.com/").ConfigureAwait(false);
            await _page.WaitForSelectorAsync("#email").ConfigureAwait(false);
            await _page.FocusAsync("#email").ConfigureAwait(false);
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("FACEBOOK_USERNAME")).ConfigureAwait(false);
            await _page.WaitForSelectorAsync("#pass").ConfigureAwait(false);
            await _page.FocusAsync("#pass").ConfigureAwait(false);
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("FACEBOOK_PASSWORD")).ConfigureAwait(false);
            await _page.WaitForSelectorAsync("#loginbutton").ConfigureAwait(false);
            await _page.ClickAsync("#loginbutton").ConfigureAwait(false);
            await _page.WaitForTimeoutAsync(2000).ConfigureAwait(false);
            Console.WriteLine($"Chromium instance {_browser.Process.Id} has successfully logged into Facebook");
        }

        public async Task InstaLogin()
        {
            await _page.GoToAsync("https://www.instagram.com/").ConfigureAwait(false);
            await _page.WaitForSelectorAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(2) > div > label > input").ConfigureAwait(false);
            await _page.FocusAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(2) > div > label > input").ConfigureAwait(false);
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("INSTAGRAM_USERNAME")).ConfigureAwait(false);
            await _page.WaitForSelectorAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(3) > div > label > input").ConfigureAwait(false);
            await _page.FocusAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(3) > div > label > input").ConfigureAwait(false);
            await _page.Keyboard.TypeAsync(Environment.GetEnvironmentVariable("INSTAGRAM_PASSWORD")).ConfigureAwait(false);
            await _page.ClickAsync("#react-root > section > main > article > div.rgFsT > div:nth-child(1) > div > form > div:nth-child(4) > button").ConfigureAwait(false);
            await _page.WaitForNavigationAsync().ConfigureAwait(false);
            Console.WriteLine($"Chromium instance {_browser.Process.Id} has successfully logged into Instagram");
        }

        public async Task Destroy()
        {
            await _page.CloseAsync().ConfigureAwait(false);
            await _browser.CloseAsync().ConfigureAwait(false);

            var process = Process.GetProcessesByName("usr/bin/chromium-browser/chromium-browser-v7"); // Specific to Raspbian Chromium, change this if on different platform.
            foreach (var item in process)
            {
                item.Kill();
            }
            Console.WriteLine("Chromium has successfully been destroyed");
            DiscordReply("Chromium instances eliminated, now ready for next request.");
        }
    }
}