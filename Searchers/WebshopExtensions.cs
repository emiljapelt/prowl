
namespace Searchers
{
    public static class WebshopExtensions
    {
        public static Webshop? GetEnum(this Webshop? webshop, string domain)
        {
            switch (domain)
            {
                case "www.pricerunner.dk":
                    return Webshop.PriceRunner;
                case "www.proshop.dk":
                    return Webshop.ProShop;
                default:
                    return null;
            }
        }

        public static ISearcher GetSearcher(this Webshop? webshop)
        {
            switch(webshop)
            {
                case Webshop.PriceRunner:
                    return new PriceRunnerSearcher();
                case Webshop.ProShop:
                    return new ProshopSearcher();
                default: 
                    return null;
            }
        }
    }
}