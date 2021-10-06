using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Searchers
{
    public class ProshopSearcher : ISearcher
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

                string pricepattern = @"""Price"": (\d+\.\d+),";
                string statuspattern = @"<div class=""site-stock-text site-inline"">(.+?)(,|-|–).+<\/div>";
                string data = readStream.ReadToEnd();
                SearchResult searchResult = new SearchResult();

                var pricematches = Regex.Matches(data, pricepattern);
                if (pricematches.Count == 0) { return searchResult; }
                
                var status = Regex.Matches(data, statuspattern)[0].Groups[1].Value;
                switch(status)
                {
                    case "På lager ":
                    case "P&#229; lager ":
                    case "Fjernlager":
                        searchResult.IsAvailable = true;
                        break;
                    case "Bestillingsvare":
                    case "Bestilt ":
                        searchResult.IsAvailable = false;
                        return searchResult;
                }

                searchResult.Price = float.Parse(pricematches[0].Groups[1].Value, CultureInfo.InvariantCulture);
                return searchResult;
            }
            return null;
        }
    }
}