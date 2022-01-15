
namespace Searchers
{
    public interface ISearcher
    {
        public Task<SearchResult> Search(string url);
    }
}