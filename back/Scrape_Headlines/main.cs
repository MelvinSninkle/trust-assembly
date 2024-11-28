using Scrape_Headlines.Sites;
using Scrape_Headlines.Utilities;

namespace Scrape_Headlines
{
    internal class main
    {
        static void Main(string[] args)
        {
            Log.Info("Started");

            // make sure playwright is installed
            // Joe knows all: https://forum.linqpad.net/discussion/2710/using-playwright-in-linqpad
            Microsoft.Playwright.Program.Main(new[] { "install" }); // Install Playwright if not already installed

            ///var site = new New_York_Times();
            //TODO: wrl and all shoudl just use playwright
            var site = new World_Religion_News();

            var heads = site.Scrape_Headlines();

            Log.Info(heads.ToJsonPretty());

            Log.Info("Finished");
        }
    }
}
