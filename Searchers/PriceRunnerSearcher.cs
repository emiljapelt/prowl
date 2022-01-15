using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Searchers
{
    public class PriceRunnerSearcher : ISearcher
    {
        public async Task<SearchResult> Search(string url)
        {
            var doc = await WebInterface.GetDocument(url);

            if (doc is null) return null;

            string pricepattern = @"<span currency="".+?"".+?>([0-9\.]+).+?<\/span>";
            string statuspattern = @"<div aria-label=""(På lager|Ikke på lager|Ukendt lagerstatus)""";

            SearchResult searchResult = new SearchResult();

            int i = 0;
            var statusmatches = Regex.Matches(doc, statuspattern);
            var pricematches = Regex.Matches(doc, pricepattern);
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
    }
}