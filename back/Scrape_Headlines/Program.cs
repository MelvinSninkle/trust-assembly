using Scrape_Headlines.Sites;
using Scrape_Headlines.Utilities;

namespace Scrape_Headlines
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Info("Started");

            ///var site = new New_York_Times();
            var site = new World_Religion_News();

            var heads = site.Scrape_Headlines();

            Log.Info(heads.ToJsonPretty());

            Log.Info("Finished");
        }
    }
}
