using Microsoft.Playwright;
using Scrape_Headlines.Site_Classes;
using Scrape_Headlines.Utilities;

namespace Scrape_Headlines.Sites
{
    public class New_York_Times : Site_Scrape
    {
        public New_York_Times()
            : base()
        {
            {
                site_url = @"https://www.nytimes.com/";
            }
        }

        public override List<Headline> Scrape_Headlines()
        {
            // hmm, NYT could use an API, but that is not generic]
            // plain httpclient does not seem to work
            // try playwright...
            //    pwsh bin/Debug/netX/playwright.ps1 install


            // Joe knows all: https://forum.linqpad.net/discussion/2710/using-playwright-in-linqpad
            Microsoft.Playwright.Program.Main(new[] { "install" }); // Install Playwright if not already installed

            var playwright = Playwright.CreateAsync().Result;
            browser = playwright.Chromium.LaunchAsync().Result;

            var page = Get_CurrentPage(browser);

            var task = page.GotoAsync(site_url);
            var x = task.Result;

            var html = page.ContentAsync().Result;

            var items = new List<Headline>();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var doc_node = doc.DocumentNode;

            //

            // now get what look like news stories
            // $x("//section[@class='story-wrapper']")

            var divs = doc_node.SelectNodes("//section[@class='story-wrapper']");
            Log.Info($"{divs.Count} healines");
            foreach (var (div, index) in divs.WithIndex())
            {
                var link = div.SelectSingleNode("a");
                if (link == null)
                {
                    continue;
                }
                var href = link.GetAttributeValue("href", "empty");
                href = href.TrimEnd('/');
                // Log.Info(href);
                var parts = href.Split("/").ToList();
                var title = parts.LastOrDefault();
                Log.Info($"{1 + index}/{divs.Count} Reading: {title}");

                // really need to jump in and get the article
                var item = Read_Article(href);
                items.Add(item);
            }
            return items;
        }

        public Headline Read_Article(string url)
        {
            var art = new Headline();

            var (is_ok, html) = ReadHtmlOrCache(url);

            // for WRN, this is headline
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode;
            var title_node = node.SelectSingleNode("//h1[@data-testid='headline']");
            if (title_node != null)
            {
                art.headline_text = title_node.InnerText;
            }
            //  $x("//span[@class='byline-prefix']/following-sibling::a")
            var bylines = node.SelectNodes("//div[@class='date']/ul/li");
            if (bylines?.Count > 0)
            {
                var author = bylines[0].InnerText.Trim().TrimEnd('-');
                art.author = author;
            }
            // $x("//p[@id='article-summary']/following-sibling::div//time")[0].getAttribute("datetime")
            var time_node = node.SelectSingleNode(
                "//p[@id='article-summary']/following-sibling::div//time"
            );
            if (time_node != null)
            {
                var date_string = time_node.InnerText.Trim();
                art.date_string = date_string;
                var datetime = time_node.GetAttributeValue("datetime", "");
                if (datetime != null)
                {
                    art.date = DateTime.Parse(datetime);
                }
                else if (DateTime.TryParse(date_string, out var the_date))
                {
                    art.date = the_date;
                }
            }
            art.last_read = DateTime.Now;

            return art;
        }

        private (bool is_ok, string html) ReadHtmlOrCache(string url)
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
            var task = page.GotoAsync(site_url);
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
