using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace Searchers
{
    public class ProshopSearcher : ISearcher
    {
        public float? Search(string url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (string.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else 
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string pricepattern = @"""Price"": (\d+\.\d+),";
                string statuspattern = @"<div class=""site-stock-text site-inline"">(.+?)(,|-|–).+<\/div>";
                string data = readStream.ReadToEnd();

                var pricematches = Regex.Matches(data, pricepattern);
                if (pricematches.Count == 0) throw new Exception("Non-product page on supported webshop");

                var status = Regex.Matches(data, statuspattern)[0].Groups[1].Value;
                switch(status)
                {
                    case "På lager ":
                    case "Fjernlager":
                        break;
                    case "Bestillingsvare":
                    case "Bestilt ":
                        throw new Exception("Product not available");
                }

                return float.Parse(pricematches[0].Groups[1].Value, CultureInfo.InvariantCulture);
            }
            return null;
        }
    }
}