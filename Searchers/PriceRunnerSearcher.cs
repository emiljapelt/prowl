using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Searchers
{
    public class PriveRunnerSearcher : ISearcher
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
                string statuspattern = @"<div aria-label=""(På lager|Ikke på lager)"" class=""_2tE8-5-ApS _2AeDEJjmCK _1rd_Ri2xHf"">";
                string data = readStream.ReadToEnd();

                int i = 0;
                foreach(Match m in Regex.Matches(data, statuspattern))
                {
                    if(m.Groups[1].Value == "Ikke på lager") i++;
                    else break;
                }
                
                string price = Regex.Matches(data, pricepattern)[i].Groups[1].Value;
                string cleanprice = Regex.Replace(price, @"\.", "");
                return float.Parse(cleanprice);
            }
            return null;
        }
    }
}