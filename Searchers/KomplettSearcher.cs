using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Searchers
{
    public class KomplettSearcher : ISearcher
    {
        public SearchResult Search(string url)
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

                string productcheckpattern = @"https:\/\/www\.komplett\.dk\/product";
                string pricepattern = @"<span class=""product-price-now"">(.+?),-<\/span>";
                string statuspattern = @"<i class=""icon icon-sm stockstatus-instock""";
                string data = readStream.ReadToEnd();
                SearchResult searchResult = new SearchResult();

                var productcheckmatches = Regex.Matches(data, productcheckpattern);
                if (productcheckmatches.Count == 0) return searchResult;

                var statusmatches = Regex.Matches(data, statuspattern);
                if (statusmatches.Count == 0) 
                {
                    searchResult.IsAvailable = false;
                    return searchResult;
                }
                searchResult.IsAvailable = true;

                var pricematches = Regex.Matches(data, pricepattern);
                searchResult.Price = float.Parse(Regex.Replace(pricematches[0].Groups[1].Value, @"\D", ""));
                return searchResult;
            }
            return null;
        }
    }
}