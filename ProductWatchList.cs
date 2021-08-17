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
                    if(args.Length < 3) 
                    {
                        System.Console.WriteLine("[add | a] needs 2 parameters: <name> <url>");
                        return;
                    }
                    try
                    {
                        Product newProduct = new Product(args[1], args[2]);
                        bool addSucces = pwl.Add(newProduct);
                        if(addSucces) System.Console.WriteLine("Product added");
                        else System.Console.WriteLine("Error: Name already used");
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Error: " + e.Message);
                    }
                    break;
                case "clr":
                case "clear":
                    pwl.Clear();
                    System.Console.WriteLine("List cleared");
                    break;
                case "rm":
                case "remove":
                    if(args.Length < 2)
                    {
                        System.Console.WriteLine("[remove | rm] needs 1 parameter: <name>");
                        return;
                    }
                    bool removeSucces = pwl.Remove(args[1]);
                    if(removeSucces) System.Console.WriteLine("Product removed");
                    else System.Console.WriteLine($"Error: No product named \"{args[1]}\"");
                    break;
                case "c":
                case "check":
                    if(args.Length < 2) 
                    {
                        int checks = pwl.Check("");
                        if (checks == 0) System.Console.WriteLine($"Error: No products in list");
                    }
                    else 
                    {
                        int checks = pwl.Check(args[1]);
                        if (checks == 0) System.Console.WriteLine($"Error: No products starting with \"{args[1]}\"");
                    }
                    break;
                case "o":
                case "open":
                    if(args.Length < 2) 
                    {
                        int opened = pwl.Open("");
                        if (opened == 0) System.Console.WriteLine($"Error: No products in list");
                    }
                    else
                    {
                        int opened = pwl.Open(args[1]);
                        if(opened == 0) System.Console.WriteLine($"Error: No products starting with \"{args[1]}\"");
                    }
                    break;
                case "h":
                case "help":
                    System.Console.WriteLine("[add | a] <name> <url>: Add a product from an url to your watch list, saving it with the given name");
                    System.Console.WriteLine("[remove | rm] <name>: Remove a product saved under the given name, from your watch list");
                    System.Console.WriteLine("[clear | clr]: Remove all products from your watch list");
                    System.Console.WriteLine("[check | c] <name>?: View how the price has changed for products, with names starting with <name>. If <name> is empty, all products are checked");
                    System.Console.WriteLine("[open | o] <name>?: Open products, with names starting with <name>, in the default browser. If <name> is empty, all products are opened");
                    break;
                default:
                    System.Console.WriteLine("Unknown command: " + args[0]);
                    System.Console.WriteLine("Use command [help | h] for a list of commands");
                    break;
            }
            System.Console.WriteLine();
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

        public int Check(string name)
        {
            int count = 0;
            foreach(Product product in products)
            {
                if(product.name.ToLower().StartsWith(name.ToLower())) 
                {
                    count++;
                    var res = product.CheckProduct();
                    if (res.EndsWith("INCREASE")) ColoredWriteLine(ConsoleColor.Red, res, res.Length - 8, res.Length);
                    else if (res.EndsWith("DECREASE")) ColoredWriteLine(ConsoleColor.Green, res, res.Length - 8, res.Length);
                    else Console.WriteLine(res);
                    Save();
                }
            }
            Save();
            return count;
        }

        public int Open(string name)
        {
            int count = 0;
            foreach(Product product in products)
            {
                if(product.name.ToLower().StartsWith(name.ToLower())) 
                {
                    count++;
                    OpenUrl(product.url);
                }
            }
            return count;
        }

        public bool Add(Product product)
        {
            foreach(Product p in products)
            {
                if(p.name == product.name) return false;
            }
            products.Add(product);
            Save();
            return true;
        }

        public bool Remove(string name)
        {
            foreach(Product product in products)
            {
                if(product.name == name) 
                {
                    products.Remove(product);
                    Save();
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            products = new List<Product>();
            Save();
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
    }
}

