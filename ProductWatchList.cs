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
                System.Console.WriteLine("Use command [help] for a list of available commands");
                return;
            }

            ProductWatchList pwl = new ProductWatchList();

            switch(args[0].ToLower())
            {
                case "add":
                    if(args.Length < 3) 
                    {
                        System.Console.WriteLine("[add] needs 2 parameters: <name> <url>");
                        return;
                    }
                    bool addSucces = pwl.Add(new Product(args[1], args[2]));
                    if(addSucces) System.Console.WriteLine("Succes");
                    else System.Console.WriteLine("Error: Name already used");
                    break;
                case "delete":
                    if(args.Length < 2)
                    {
                        System.Console.WriteLine("[delete] needs 1 parameter: <name>");
                        return;
                    }
                    bool deleteSucces = pwl.Delete(args[1]);
                    if(deleteSucces) System.Console.WriteLine("Succes");
                    else System.Console.WriteLine("Error: No product with this name");
                    break;
                case "checkall":
                    pwl.CheckAll();
                    break;
                case "check":
                    if(args.Length < 2)
                    {
                        System.Console.WriteLine("[check] needs 1 parameter: <name>");
                        return;
                    }
                    bool checkSucces = pwl.Check(args[1]);
                    if(!checkSucces) System.Console.WriteLine("Error: No product with this name");
                    break;
                case "open":
                    if(args.Length < 2)
                    {
                        System.Console.WriteLine("[open] needs 1 parameter: <name>");
                        return;
                    }
                    bool openSucces = pwl.Open(args[1]);
                    if(!openSucces) System.Console.WriteLine("Error: No product with this name");
                    break;
                case "help":
                    System.Console.WriteLine("[add] <name> <url>: Add a product from an url to your watch list, saving it with the given name");
                    System.Console.WriteLine("[delete] <name>: Delete a product saved under the given name, from your watch list");
                    System.Console.WriteLine("[checkall]: View how the price has changed for each saved product in your watch list");
                    System.Console.WriteLine("[check] <name>: View how the price has changed for a specific product in your watch list");
                    System.Console.WriteLine("[open] <name>: View the full url of a specific product in your watch list");
                    break;
                default:
                    System.Console.WriteLine("Unknown command: " + args[0]);
                    System.Console.WriteLine("Use command [help] for a list of available commands");
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

        public void CheckAll()
        {
            foreach(Product product in products)
            {
                System.Console.WriteLine(product.CheckProduct());
            }
            Save();
        }

        public bool Check(string name)
        {
            foreach(Product product in products)
            {
                if(product.name == name) 
                {
                    System.Console.WriteLine(product.CheckProduct());
                    Save();
                    return true;
                }
            }
            Save();
            return false;
        }

        public bool Open(string name)
        {
            foreach(Product product in products)
            {
                if(product.name == name) 
                {
                    System.Console.WriteLine(product.name + ": " + product.url);
                    return true;
                }
            }
            return false;
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

