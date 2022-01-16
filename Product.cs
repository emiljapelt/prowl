using System.Text.RegularExpressions;
using System.Text;
using Searchers;

namespace prowl;

public class Product
{
    public string name;
    public string url;
    public string domain;
    public float? price;
    public float? lowest_price;
    public float? highest_price;
    public DateTime added;

    private readonly char up = '\x2191';
    private readonly char down = '\x2193';
    private readonly char right = '\x2192';
    private readonly char indicator = '\x2195';

    public Product(string name, string url)
    {
        this.name = name;
        this.url = url;
        domain = FindDomain();
        added = DateTime.Now;
        CheckProduct().Wait();
    }

    private string FindDomain()
    {
        string pattern = @"https:\/\/(.+?)\/.*";
        var matches = Regex.Matches(url, pattern);
        if(matches.Count == 0) throw new Exception("Invalid url");
        return matches[0].Groups[1].Value;
    }

    public async Task<(string text, ConsoleColor color)> CheckProduct()
    {
        var totaldays = (DateTime.Now - added).TotalDays;
        StringBuilder productInformation = new StringBuilder();
        productInformation.Append($"{name} [{domain}] w{((int)totaldays)/7} : ");

        ConsoleColor color = ConsoleColor.White;

        try
        {
            float? oldPrice = price;

            ISearcher searcher = null;
            foreach(var w in Webshop.Values) if(w.Domain == domain) searcher = w.Searcher;
            if(searcher == null) productInformation.Append("Webshop not supported");
            else
            {
                SearchResult searchResult = await searcher.Search(url);

                if (searchResult.IsAvailable is null) productInformation.Append("Non-product page on supported webshop");
                else if (!searchResult.IsAvailable ?? false) productInformation.Append("Product not available"); 
                else 
                {
                    price = searchResult.Price;
                    if (lowest_price is null) lowest_price = price;
                    if (highest_price is null) highest_price = price;
                    
                    if (price < lowest_price) 
                    {
                        lowest_price = price;
                        productInformation.Append($"L{down} {lowest_price} / ");
                    }
                    else 
                    {
                        productInformation.Append($"L {lowest_price} / ");
                    }

                    if (price < oldPrice)
                    {
                        productInformation.Append($"C{down} {oldPrice} {right} {price} / ");
                        color = ConsoleColor.Green;
                    }
                    else if (price > oldPrice)
                    {
                        productInformation.Append($"C{up} {oldPrice} {right} {price} / ");
                        color = ConsoleColor.Red;
                    }
                    else 
                    {
                        productInformation.Append($"C {price} / ");
                    }

                    if (price > highest_price) 
                    {
                        highest_price = price;
                        productInformation.Append($"H{up} {highest_price} ");
                    }
                    else 
                    {
                        productInformation.Append($"H {highest_price} ");
                    }

                    productInformation.Append($" {indicator}");
                }
            }
        }
        catch (Exception e)
        {
            productInformation.Append(e.Message);
        }

        return (productInformation.ToString(), color);
    }
}