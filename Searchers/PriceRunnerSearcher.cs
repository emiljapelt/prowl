using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace Searchers
{
    public class PriceRunnerSearcher : ISearcher
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

                string pricepattern = @"class=""_1GHOTiLjmw _1VgCKncEcl _1081IF0fJa _3QPmcfztEB _3ZcFJUe-1R"">([0-9.]+)";
                string statuspattern = @"<div aria-label=""(På lager|Ikke på lager|Ukendt lagerstatus)"" class=""_2tE8-5-ApS _2AeDEJjmCK _1rd_Ri2xHf"">";
                string data = readStream.ReadToEnd();

                int i = 0;
                var statusmatches = Regex.Matches(data, statuspattern);
                var pricematches = Regex.Matches(data, pricepattern);
                if (pricematches.Count == 0) throw new Exception("Non-product page on supported webshop");
                foreach(Match m in statusmatches)
                {
                    if(m.Groups[1].Value != "På lager") i++;
                    else break;
                }
                if (i == statusmatches.Count) throw new Exception("Product not available");

                string price = pricematches[i].Groups[1].Value;
                string cleanprice = Regex.Replace(price, @"\.", "");
                return float.Parse(cleanprice);
            }
            return null;
        }
    }
}