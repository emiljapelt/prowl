using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace prowl
{
    class ProductWatchList
    {
        private List<Product> products;
        string savepath;

        static void Main(string[] args)
        {
            if(args.Length < 1) 
            {
                System.Console.WriteLine("No command given");
                System.Console.WriteLine("Use command [help | h] for a list of commands");
                System.Console.WriteLine();
                return;
            }

            ProductWatchList pwl = new ProductWatchList();

            switch(args[0].ToLower())
            {
                case "a":
                case "add":
                    HandleResult(pwl.Add(args));
                    break;
                case "clr":
                case "clear":
                    HandleResult(pwl.Clear());
                    break;
                case "rm":
                case "remove":
                    HandleResult(pwl.Remove(args));
                    break;
                case "c":
                case "check":
                    HandleResult(pwl.Check(args));
                    break;
                case "o":
                case "open":
                    HandleResult(pwl.Open(args));
                    break;
                case "h":
                case "help":
                    Console.WriteLine("[add | a] <name> <url>: Add a product from an url to your watch list, saving it with the given name");
                    Console.WriteLine("[remove | rm] <name>: Remove a product saved under the given name, from your watch list");
                    Console.WriteLine("[clear | clr]: Remove all products from your watch list");
                    Console.WriteLine("[check | c] <name>?: View how the price has changed for products, with names starting with <name>. If <name> is empty, all products are checked");
                    Console.WriteLine("[open | o] <name>?: Open products, with names starting with <name>, in the default browser. If <name> is empty, all products are opened");
                    break;
                default:
                    Console.WriteLine("Unknown command: " + args[0]);
                    Console.WriteLine("Use command [help | h] for a list of commands");
                    break;
            }
        }

        public ProductWatchList()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase).LocalPath;
            savepath = location.Substring(0,location.Length-9) + "prowl_data.json";

            if (File.Exists(savepath))
            {
                using (StreamReader reader = new StreamReader(savepath))
                {
                    string data = reader.ReadToEnd();
                    products = JsonConvert.DeserializeObject<List<Product>>(data);
                }
            }
            else 
            {
                products = new List<Product>();
            }
        }

        public MethodResult Check(string[] args)
        {
            string filter;
            try { filter = args[1].ToLower(); } catch (Exception) { filter = ""; }

            int count = 0;
            foreach(Product product in products)
            {
                if(product.name.ToLower().StartsWith(filter)) 
                {
                    count++;
                    var res = product.CheckProduct();
                    ColoredWriteLine(res.color, res.text, res.text.Length - 1, res.text.Length);
                    Save();
                }
            }
            Save();
            if (count == 0) return Failure("No products to be checked");
            else return Success();
        }

        public MethodResult Open(string[] args)
        {
            string filter;
            try { filter = args[1].ToLower(); } catch (Exception) { filter = ""; }

            int count = 0;
            foreach(Product product in products)
            {
                if(product.name.ToLower().StartsWith(filter)) 
                {
                    count++;
                    OpenUrl(product.url);
                }
            }
            if (count == 0) return Failure("No products to open");
            else return Success();
        }

        public MethodResult Add(string[] args)
        {
            if(args.Length < 3) return Failure("[add | a] needs 2 parameters: <name> <url>");

            var name = args[1];
            var url = args[2];

            foreach(Product p in products) if(p.name == name) return Failure("Name already in use");
            try
            {
                products.Add(new Product(name, url));
                Save();
                return Success("Product added");
            }
            catch (Exception)
            {
                return Failure("Something went wrong");
            }
        }

        public MethodResult Remove(string[] args)
        {
            if(args.Length < 2) return Failure("[remove | rm] needs 1 parameter: <name>");

            var name = args[1];

            foreach(Product product in products)
            {
                if(product.name == name) 
                {
                    products.Remove(product);
                    Save();
                    return Success("Product was removed");
                }
            }
            return Failure($"No product with name \"{name}\"");
        }

        public MethodResult Clear()
        {
            products = new List<Product>();
            Save();
            return Success("List cleared");
        }

        public void Save()
        {
            File.WriteAllText(savepath, "");

            string result = JsonConvert.SerializeObject(products);
            using (var tw = new StreamWriter(savepath, true))
            {
                tw.WriteLine(result);
                tw.Close();
            }
        }

        // https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ColoredWriteLine(ConsoleColor color, string text, int startIndex, int endIndex)
        {
            Console.Write(text.Substring(0, startIndex));
            Console.ForegroundColor = color;
            Console.Write(text.Substring(startIndex, endIndex - startIndex));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text.Substring(endIndex, text.Length - endIndex));
        }

        private MethodResult Success(string msg = "") { return new Success(msg); }

        private MethodResult Failure(string msg) { return new Failure(msg); }

        private static void HandleResult(MethodResult result)
        {
            if (result is Failure) Console.WriteLine($"Failure: {result.Message}");
            else Console.WriteLine(result.Message);
        }
    }
}

