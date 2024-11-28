namespace Scrape_Headlines.Site_Classes
{
    public class Headline
    {
        public string site { get; set; }
        public string url { get; set; }
        public string headline_text { get; set; }
        public DateTime date { get; set; }

        // if the date is unparsable we just put this
        public string date_string { get; set; }
        public string author { get; set; }

        public DateTime last_read { get; set; }
    }
}
