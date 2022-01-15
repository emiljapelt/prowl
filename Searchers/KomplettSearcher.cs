using System.Text.RegularExpressions;

namespace Searchers
{
    public class KomplettSearcher : ISearcher
    {
        public async Task<SearchResult> Search(string url)
        {
            var doc = await WebInterface.GetDocument(url);

            if (doc is null) return null;

            string productcheckpattern = @"https:\/\/www\.komplett\.dk\/product";
            string pricepattern = @"<span class=""product-price-now"">(.+?),-<\/span>";
            string statuspattern = @"<i class=""icon icon-sm stockstatus-instock""";

            SearchResult searchResult = new SearchResult();

            var productcheckmatches = Regex.Matches(doc, productcheckpattern);
            if (productcheckmatches.Count == 0) return searchResult;

            var statusmatches = Regex.Matches(doc, statuspattern);
            if (statusmatches.Count == 0) 
            {
                searchResult.IsAvailable = false;
                return searchResult;
            }
            searchResult.IsAvailable = true;

            var pricematches = Regex.Matches(doc, pricepattern);
            searchResult.Price = float.Parse(Regex.Replace(pricematches[0].Groups[1].Value, @"\D", ""));
            return searchResult;
        }
    }
}