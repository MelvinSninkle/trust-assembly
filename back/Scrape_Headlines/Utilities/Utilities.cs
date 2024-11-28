namespace Scrape_Headlines.Utilities
{
    public static class Utes
    {
        public static string MakeValidFilename(string input)
        {
            var file_name = input;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                file_name = file_name.Replace(c, '_');
            }
            return file_name;
        }

        public static (bool is_ok, string html) HttpGetHtmlFromUrl(string url)
        {
            var client = new HttpClient();

            //TODO: Intermittently get 403 Forbidden, but it's not clear why

            //Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36
            client.DefaultRequestHeaders.Add(
                "user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
            );
            client.DefaultRequestHeaders.Add(
                "Accept-Language",
                "en-US,en;q=0.9,pt;q=0.8,fr;q=0.7,ar;q=0.6"
            );
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

        public static (bool is_ok, string html) HttpGetHtmlFromBrowser(string url)
        {
            var client = new HttpClient();

            //TODO: Intermittently get 403 Forbidden, but it's not clear why

            //Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36
            client.DefaultRequestHeaders.Add(
                "user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
            );
            client.DefaultRequestHeaders.Add(
                "Accept-Language",
                "en-US,en;q=0.9,pt;q=0.8,fr;q=0.7,ar;q=0.6"
            );
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
}
