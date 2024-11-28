using Scrape_Headlines.Site_Classes;
using Scrape_Headlines.Utilities;

namespace Scrape_Headlines.Sites
{
    public class New_York_Times : Site_Scrape
    {
        public New_York_Times()
            : base()
        {
            site_url = @"https://www.nytimes.com/";
        }

        public override List<Headline> Scrape_Headlines()
        {
            var items = new List<Headline>();
            var (is_ok, html) = ReadHtmlOrCache(site_url);

            if (!is_ok)
            {
                return items;
            }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var doc_node = doc.DocumentNode;

            //

            // now get what look like news stories
            // $x("//section[@class='story-wrapper']")

            var divs = doc_node.SelectNodes("//section[@class='story-wrapper']");
            if (divs == null)
            {
                Log.Warn($"Issue reading url: {site_url}\n{html}");
                return items;
            }

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
                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        public Headline Read_Article(string url)
        {
            var art = new Headline { site = site_url, url = url };

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
            else
            {
                return null;
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
    }
}
