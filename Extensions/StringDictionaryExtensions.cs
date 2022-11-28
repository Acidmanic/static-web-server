using System.Collections.Generic;

namespace angular_server.Extensions
{
    public static class StringDictionaryExtensions
    {
        public static string ToQueryString(this Dictionary<string, string> dictionary)
        {
            var query = "";

            if (dictionary != null)
            {
                if (dictionary.Count > 0)
                {
                    query = "?";
                }

                var sep = "";

                foreach (var keyValue in dictionary)
                {
                    query += sep + keyValue.Key + "=" + keyValue.Value;
                    sep = "&";
                }
            }

            return query;
        }

        public static string ToQueryString(this Dictionary<string, IEnumerable<string>> dictionary)
        {
            var query = "";

            if (dictionary != null)
            {
                if (dictionary.Count > 0)
                {
                    query = "?";
                }

                var sep = "";

                foreach (var keyValues in dictionary)
                {
                    foreach (var value in keyValues.Value)
                    {
                        query += sep + keyValues.Key + "=" + value;
                        sep = "&";
                    }
                }
            }

            return query;
        }
    }
}