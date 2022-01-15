
namespace Searchers;

public static class WebInterface 
{
    public static HttpClient client = new HttpClient();
    public async static Task<string> GetDocument(string url)
    {
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();
        else return null;
    }
}