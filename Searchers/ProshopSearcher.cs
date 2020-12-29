using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

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
                return float.Parse(Regex.Matches(data, pattern)[0].Groups[1].Value, CultureInfo.InvariantCulture);
            }
            return null;
        }
    }
}