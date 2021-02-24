using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace Searchers
{
    public class PriceRunnerSearcher : ISearcher
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

                string pricepattern = @"<span currency="".+?"".+?>([0-9\.]+).+?<\/span>";
                string statuspattern = @"<div aria-label=""(På lager|Ikke på lager|Ukendt lagerstatus)""";
                string data = readStream.ReadToEnd();
                SearchResult searchResult = new SearchResult();

                int i = 0;
                var statusmatches = Regex.Matches(data, statuspattern);
                var pricematches = Regex.Matches(data, pricepattern);
                if (pricematches.Count == 0) { return searchResult; }

                foreach(Match m in statusmatches)
                {
                    if(m.Groups[1].Value != "På lager") i++;
                    else break;
                }
                if (i == statusmatches.Count) { searchResult.IsAvailable = false; return searchResult; }
                else searchResult.IsAvailable = true;

                string price = pricematches[i].Groups[1].Value;
                string cleanprice = Regex.Replace(price, @"\.", "");
                searchResult.Price = float.Parse(cleanprice);
                return searchResult;
            }
            return null;
        }
    }
}