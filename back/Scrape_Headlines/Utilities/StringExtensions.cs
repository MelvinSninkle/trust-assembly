namespace Scrape_Headlines.Utilities
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static void RemoveRange<T>(this List<T> list, IEnumerable<T> values)
        {
            //TODO: would be better if this were functional, meaning it returned a new list rahter than acting on the existing list
            if (list != null && values != null)
            {
                values = values.ToList();
                foreach (var value in values)
                {
                    list.Remove(value);
                }
            }
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        {
            return self.Select((item, index) => (item, index));
        }
    }
}
