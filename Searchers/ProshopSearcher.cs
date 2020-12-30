using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace Searchers
{
    public class ProshopSearcher : ISearcher
    {
        public float? Search(string url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (string.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else 
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string pattern = @"""Price"": (\d+\.\d+),";
                string data = readStream.ReadToEnd();
                var matches = Regex.Matches(data, pattern);
                if (matches.Count == 0) throw new Exception("Non-product page on supported webshop");
                return float.Parse(matches[0].Groups[1].Value, CultureInfo.InvariantCulture);
            }
            return null;
        }
    }
}