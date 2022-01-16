using System.Text.RegularExpressions;
using System.Globalization;

namespace Searchers
{
    public class ProshopSearcher : ISearcher
    {
        public async Task<SearchResult> Search(string url)
        {
            var doc = await WebInterface.GetDocument(url);

            if (doc is null) return null;

            string pricepattern = @"""Price"": (\d+\.\d+),";
            string statuspattern = @"<div class=""site-stock-text site-inline"">(.+?)(,|-|–).+<\/div>";
            
            var searchResult = new SearchResult();

            var pricematches = Regex.Matches(doc, pricepattern);
            if (pricematches.Count == 0) return searchResult;

            var status = Regex.Matches(doc, statuspattern)[0].Groups[1].Value;
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
    }
}