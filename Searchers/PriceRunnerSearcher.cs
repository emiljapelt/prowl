using System.Text.RegularExpressions;

namespace Searchers
{
    public class PriceRunnerSearcher : ISearcher
    {
        public async Task<SearchResult> Search(string url)
        {
            var doc = await WebInterface.GetDocument(url);
            if (doc is null) return null;

            SearchResult searchResult = new SearchResult();

            var matches = Regex.Matches(doc, @"aria-label=""(På lager|Ikke på lager|Ukendt lagerstatus)"".*?>([0-9.]+) <span.*?kr\.");

            if (matches.Count == 0) return searchResult;

            searchResult.IsAvailable = false;

            foreach(Match m in matches) {
                searchResult.Price = float.Parse(Regex.Replace(m.Groups[2].Value, @"\.", ""));
                if (m.Groups[1].Value == "På lager") {
                    searchResult.IsAvailable = true;
                    return searchResult;
                }
            }
            return searchResult;
        }
    }
}