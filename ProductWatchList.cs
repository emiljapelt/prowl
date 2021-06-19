using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

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
                    bool deleteSucces = pwl.Delete(args[1]);
                    if(deleteSucces) System.Console.WriteLine("Product removed");
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
                    System.Console.WriteLine("[open | o] <name>?: View the full url of products, with names starting with <name>. If <name> is empty, all products are displayed");
                    break;
                default:
                    System.Console.WriteLine("Unknown command: " + args[0]);
                    System.Console.WriteLine("Use command [help | h] for a list of available commands");
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
                    System.Console.WriteLine(product.CheckProduct());
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
                    System.Console.WriteLine(product.name + ": " + product.url);
                    System.Console.WriteLine();
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

        public bool Delete(string name)
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
    }
}

