using System.Text.RegularExpressions;
using System.Text;
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
            price = FindPrice();
        }

        private string FindDomain()
        {
            string pattern = @"https:\/\/(.+?)\/.+";
            return Regex.Matches(url, pattern)[0].Groups[1].Value;
        }

        public string CheckProduct()
        {
            StringBuilder productInformation = new StringBuilder();
            productInformation.Append(name + " [" + domain + "] : ");
            if(price != null) productInformation.Append(price + " -> ");
            else productInformation.Append("No support -> ");

            float? oldPrice = price;
            price = FindPrice();
            if(price != null) productInformation.Append(price + " ");
            else productInformation.Append("No support ");

            if(oldPrice == price) productInformation.Append("STATIC");
            else if (oldPrice > price) productInformation.Append("DECREASE");
            else productInformation.Append("INCREASE");

            return productInformation.ToString();
        }

        private float? FindPrice()
        {
            ISearcher searcher = null;
            switch(domain)
            {
                case "www.proshop.dk":
                    searcher = new ProshopSearcher();
                    break;
                case "www.pricerunner.dk":
                    searcher = new PriveRunnerSearcher();
                    break;
            }
            if(searcher != null) return searcher.Search(url);
            else return null;
        }
    }
}