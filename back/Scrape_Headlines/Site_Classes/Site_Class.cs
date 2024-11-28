using Microsoft.Playwright;
using Scrape_Headlines.Site_Classes;
using Scrape_Headlines.Utilities;

namespace Scrape_Headlines
{
    public class Site_Scrape
    {
        public IBrowser browser { get; set; }

        public Site_Scrape()
        {
            var playwright = Playwright.CreateAsync().Result;

            var options = new BrowserTypeLaunchOptions { Headless = false };

            browser = playwright.Chromium.LaunchAsync(options).Result;
        }

        public string site_url { get; set; }

        public virtual List<Headline> Scrape_Headlines()
        {
            var items = new List<Headline>();
            //TODO: read the headlines!
            // some are on the page directly
            // some are links to drill into

            return items;
        }

        public virtual string reading_type { get; set; } = "httpclient";

        public (bool is_ok, string html) GetHtmlOrCache(string url, bool use_cache = true)
        {
            var html = "";
            var is_ok = false;
            var cache_file = Path.Combine(@"C:\temp", $"{Utes.MakeValidFilename(url)}.html");
            if (use_cache && File.Exists(cache_file))
            {
                var fi = new FileInfo(cache_file);
                if (fi.LastWriteTime > DateTime.Now.AddMinutes(-10))
                {
                    html = File.ReadAllText(cache_file);
                    return (true, html);
                }
            }
            if (reading_type == "httpclient")
            {
                (is_ok, html) = Utes.HttpGetHtmlFromUrl(url);
            }
            else { }
            if (is_ok)
            {
                File.WriteAllText(cache_file, html);
            }
            return (is_ok, html);
        }

        public IPage current_page { get; set; }

        public IPage Get_CurrentPage(IBrowser browser)
        {
            if (current_page == null)
            {
                var context = browser.NewContextAsync().Result;
                if (context.Pages.Count == 0)
                {
                    current_page = browser.NewPageAsync().Result;
                }
            }
            return current_page;
        }

        public (bool is_ok, string html) ReadHtmlOrCache(string url)
        {
            var html = "";
            var is_ok = true;
            var cache_file = Path.Combine(@"C:\temp", $"{Utes.MakeValidFilename(url)}.html");
            if (File.Exists(cache_file))
            {
                var fi = new FileInfo(cache_file);
                if (fi.LastWriteTime > DateTime.Now.AddMinutes(-10))
                {
                    html = File.ReadAllText(cache_file);
                    return (true, html);
                }
            }
            var page = Get_CurrentPage(browser);
            var task = page.GotoAsync(url);
            var x = task.Result;

            html = page.ContentAsync().Result;

            if (is_ok)
            {
                File.WriteAllText(cache_file, html);
            }

            return (is_ok, html);
        }
    }
}
