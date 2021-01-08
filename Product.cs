using System.Text.RegularExpressions;
using System.Text;
using System;
using Searchers;

namespace prowl
{
    public class Product
    {
        public string name;
        public string url;
        public string domain;
        public float? price;

        public Product(string name, string url)
        {
            this.name = name;
            this.url = url;
            domain = FindDomain();
            CheckProduct();
        }

        private string FindDomain()
        {
            string pattern = @"https:\/\/(.+?)\/.*";
            var matches = Regex.Matches(url, pattern);
            if(matches.Count == 0) throw new Exception("Invalid url");
            return matches[0].Groups[1].Value;
        }

        public string CheckProduct()
        {
            StringBuilder productInformation = new StringBuilder();
            productInformation.Append(name + " [" + domain + "] : ");

            try
            {
                float? oldPrice = price;

                ISearcher searcher = null;
                foreach(var w in Webshop.Values) if(w.Domain == domain) searcher = w.Searcher;
                if(searcher == null) productInformation.Append("Webshop not supported");
                else
                {
                    SearchResult searchResult = searcher.Search(url);

                    if (searchResult.IsAvailable is null) productInformation.Append("Non-product page on supported webshop");
                    else if (!searchResult.IsAvailable ?? false) productInformation.Append("Product not available"); 
                    else 
                    {
                        price = searchResult.Price;
                        
                        if(oldPrice != null) productInformation.Append(oldPrice + " -> ");
                        else productInformation.Append("Null -> ");

                        if(price != null) productInformation.Append(price + " ");
                        else productInformation.Append("Null ");

                        if(oldPrice == price) productInformation.Append("STATIC");
                        else if (oldPrice > price) productInformation.Append("DECREASE");
                        else productInformation.Append("INCREASE");
                    }
                }
            }
            catch (Exception e)
            {
                productInformation.Append(e.Message);
            }

            return productInformation.ToString();
        }
    }
}