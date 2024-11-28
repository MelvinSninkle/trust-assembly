<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Management.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>Microsoft.Playwright</NuGetReference>
  <NuGetReference>System.Management</NuGetReference>
  <Namespace>System.Management</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.NetworkInformation</Namespace>
  <Namespace>Microsoft.Playwright</Namespace>
</Query>

/*

This uses my favorite down and dirty too, Linqpad: https://forum.linqpad.net/

Though it is getting pretty heavy and should become a proper project soon


Goal- proof of concept that we can read a news site and parse out headlines
First: just world religious news, since it's odd
Then : a simpler site like NYT
Next: Fox news to get some balance
... then see what tech works best to get the most sites
 probably playwright + caching
 
 TODO: choose a place to put the data- AWS Dynamo?
 TODO: choose timing - when to run, when to refresh data
 TODO: error handling. who needs to be notified and how?

*/


void Main()
{

	var js = ""; var xpath = ""; object x = null;

	var now = DateTime.Now;

	//var site = new World_Religion_News();
	var site = new New_York_Times();

	var heads = site.Scrape_Headlines();

	Log.Info(heads.ToJsonPretty());


}


/// <summary>
/// Every news site is unique in how we need to read and parse the headlines
/// </summary>
public class Site_Scrape
{
public IBrowser browser {get;set;}
	
	public string site_url { get; set; }
	virtual public List<Headline> Scrape_Headlines()
	{
		var items = new List<Headline>();
		//TODO: read the headlines!
		// some are on the page directly
		// some are links to drill into

		return items;
	}
	
	virtual public string reading_type {get;set;} = "httpclient";

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
		else
		{
			
		}
		if (is_ok)
		{
			File.WriteAllText(cache_file, html);
		}
		return (is_ok, html);

	}


}
public class Headline
{
	public string url { get; set; }
	public string headline_text { get; set; }
	public DateTime date { get; set; }
	// if the date is unparsable we just put this
	public string date_string { get; set; }
	public string author { get; set; }
	
	
	public DateTime last_read {get;set;}
	
}

public class World_Religion_News : Site_Scrape
{
	public World_Religion_News() : base()
	{
		{
			site_url = @"https://www.worldreligionnews.com/";
		}
	}


	public override List<Headline> Scrape_Headlines()
	{
		var items = new List<Headline>();

		// need to make the site think we are a browser
		//TODO: Intermittently get 403 Forbidden, but it's not clear why

		var (is_ok, html) = Utes.HttpGetHtmlFromUrl(site_url);

		var doc = new HtmlAgilityPack.HtmlDocument();
		doc.LoadHtml(html);
		var doc_node = doc.DocumentNode;

		//var empty_nodes = doc_node.SelectSingleNode("//*");


		// now get what look like news stories
		// class post-thumb will have links underneath
		//var divs = document.Body.SelectNodes("//div[contains(@class,'post-thumb')]");

		//"//div[@class='post-thumb']"
		var divs = doc_node.SelectNodes("//div[@class='post-thumb']");
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
			items.Add(item);



		}
		return items;

	}

	public Headline Read_Article(string url)
	{
		var art = new Headline();

		var (is_ok, html) = Utes.HttpGetHtmlFromUrl(url);
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


public static class Utes
{
	static public string MakeValidFilename(string input)
	{
		var file_name = input;
		foreach (char c in System.IO.Path.GetInvalidFileNameChars())
		{
			file_name = file_name.Replace(c, '_');
		}
		return file_name;
	}
	

	static public (bool is_ok, string html) HttpGetHtmlFromUrl(string url)
	{
		var client = new HttpClient();

		//TODO: Intermittently get 403 Forbidden, but it's not clear why

		//Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36
		client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
		client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,pt;q=0.8,fr;q=0.7,ar;q=0.6");
		Log.Info($"Get: {url}");
		var response = client.GetAsync(url).Result;
		Log.Info($"{(int)response.StatusCode} {response.StatusCode}");


		var times = 0;
		while (times++ < 10 && response.StatusCode == System.Net.HttpStatusCode.Forbidden)
		{
			Thread.Sleep(1000 * times);
			response = client.GetAsync(url).Result;
			Log.Info($"{(int)response.StatusCode} {response.StatusCode}");

		}

		if (response.StatusCode != System.Net.HttpStatusCode.OK)
		{
			return (false, "");
		}
		var html = response.Content.ReadAsStringAsync().Result;
		return (true, html);


	}


	static public (bool is_ok, string html) HttpGetHtmlFromBrowser(string url)
	{
		var client = new HttpClient();

		//TODO: Intermittently get 403 Forbidden, but it's not clear why

		//Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36
		client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
		client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,pt;q=0.8,fr;q=0.7,ar;q=0.6");
		Log.Info($"Get: {url}");
		var response = client.GetAsync(url).Result;
		Log.Info($"{(int)response.StatusCode} {response.StatusCode}");


		var times = 0;
		while (times++ < 10 && response.StatusCode == System.Net.HttpStatusCode.Forbidden)
		{
			Thread.Sleep(1000 * times);
			response = client.GetAsync(url).Result;
			Log.Info($"{(int)response.StatusCode} {response.StatusCode}");

		}

		if (response.StatusCode != System.Net.HttpStatusCode.OK)
		{
			return (false, "");
		}
		var html = response.Content.ReadAsStringAsync().Result;
		return (true, html);


	}

}


public class New_York_Times: Site_Scrape
{
	
	public New_York_Times() : base()
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
		Microsoft.Playwright.Program.Main(new[] { "install" });   // Install Playwright if not already installed

		var playwright = Playwright.CreateAsync().Result;
		browser = playwright.Chromium.LaunchAsync().Result;
		var page = browser.NewPageAsync().Result;
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
			items.Add(item);



		}
		return items;

	}

	public Headline Read_Article(string url)
	{
		var art = new Headline();
		
		var page = browser.NewPageAsync().Result;
		var task = page.GotoAsync(site_url);
		var x = task.Result;

		var html = page.ContentAsync().Result;



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
		var time_node = node.SelectSingleNode("//p[@id='article-summary']/following-sibling::div//time");
		if (time_node!=null)
		{
			var date_string = time_node.InnerText.Trim();
			art.date_string = date_string;
			var datetime = time_node.GetAttributeValue("datetime","");
			if (datetime!=null)
			{
				art.date  = DateTime.Parse( datetime);
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

