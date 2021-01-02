using System.Collections.Generic;

namespace Searchers
{
    public class Webshop
    {
        public static readonly Webshop PriceRunner = new Webshop("www.pricerunner.dk", new PriceRunnerSearcher());
        public static readonly Webshop Proshop = new Webshop("www.proshop.dk", new ProshopSearcher());

        public string Domain { get; private set; }
        public ISearcher Searcher { get; private set; }

        Webshop(string domain, ISearcher searcher) =>
            (Domain, Searcher) = (domain, searcher);

        public static IEnumerable<Webshop> Values
        {
            get
            {
                yield return PriceRunner;
                yield return Proshop;
            }
        }
    }
}