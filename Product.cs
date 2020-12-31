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
        public Webshop? webshop;
        public float? price;

        public Product(string name, string url)
        {
            this.name = name;
            this.url = url;
            domain = FindDomain();
            webshop = webshop.GetEnum(domain);
            try
            {
                price = FindPrice();
            }
            catch
            {
                price = null;
            }
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
                price = FindPrice();
                if(oldPrice != null) productInformation.Append(price + " -> ");
                else productInformation.Append("Null -> ");

                if(price != null) productInformation.Append(price + " ");
                else productInformation.Append("Null ");

                if(oldPrice == price) productInformation.Append("STATIC");
                else if (oldPrice > price) productInformation.Append("DECREASE");
                else productInformation.Append("INCREASE");
            }
            catch (Exception e)
            {
                productInformation.Append(e.Message);
            }

            return productInformation.ToString();
        }

        private float? FindPrice()
        {
            if (webshop == null) webshop = webshop.GetEnum(domain);
            ISearcher searcher = webshop.GetSearcher();;
            if(searcher != null) return searcher.Search(url);
            else throw new Exception("Webshop not supported");
        }
    }
}