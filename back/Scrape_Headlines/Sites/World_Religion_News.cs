using Scrape_Headlines.Site_Classes;
using Scrape_Headlines.Utilities;

namespace Scrape_Headlines.Sites
{
    public class World_Religion_News : Site_Scrape
    {
        public World_Religion_News()
            : base()
        {
            site_url = @"https://www.worldreligionnews.com/";
        }

        public override List<Headline> Scrape_Headlines()
        {
            var items = new List<Headline>();

            // need to make the site think we are a browser
            //TODO: Intermittently get 403 Forbidden, but it's not clear why

            //            var (is_ok, html) = Utes.HttpGetHtmlFromUrl(site_url);

            var (is_ok, html) = ReadHtmlOrCache(site_url);

            if (!is_ok)
            {
                return items;
            }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var doc_node = doc.DocumentNode;

            //var empty_nodes = doc_node.SelectSingleNode("//*");


            // now get what look like news stories
            // class post-thumb will have links underneath
            //var divs = document.Body.SelectNodes("//div[contains(@class,'post-thumb')]");

            //"//div[@class='post-thumb']"
            var divs = doc_node.SelectNodes("//div[@class='post-thumb']");

            if (divs == null)
            {
                Log.Warn($"Issue reading url: {site_url}\n{html}");
                return items;
            }

            Log.Info(divs.Count());
            foreach (var div in divs)
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
                Log.Info($"{title}");

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
            if (!is_ok)
            {
                return null;
            }
            var times = 0;
            while (
                times++ < 10 && html.Contains("Attention Required! | Cloudflare")
                || html.Contains("DDoS protection by Cloudflare")
                || html.Contains("Please turn JavaScript on and reload the page.")
            )
            {
                Thread.Sleep(1000 * times);
                (is_ok, html) = ReadHtmlOrCache(url);
                Log.Warn($"{times} Cloudflare issue: {url}");
            }
            if (!is_ok)
            {
                return null;
            }

            // for WRN, this is headline
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode;
            var title_node = node.SelectSingleNode("//h1[@class='short_title']");
            if (title_node != null)
            {
                art.headline_text = title_node.InnerText;
            }
            else
            {
                return null;
            }

            //$x("//div[@class='date']/ul/li")
            var bylines = node.SelectNodes("//div[@class='date']/ul/li");
            if (bylines?.Count > 0)
            {
                var author = bylines[0].InnerText.Trim().TrimEnd('-');
                art.author = author;
            }
            if (bylines?.Count > 1)
            {
                var date_string = bylines[1].InnerText.Trim().TrimEnd('-');
                art.date_string = date_string;
                if (DateTime.TryParse(date_string, out var the_date))
                {
                    art.date = the_date;
                }
            }
            art.last_read = DateTime.Now;

            return art;
        }

        public void Parse_Node(string url)
        {
            // we have a url which might have an article on it!
            // it may also have other article links
        }
    }
}
