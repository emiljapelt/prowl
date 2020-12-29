using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace prowl
{
    class ProductWatchList
    {
        private List<Product> products;
        string savepath = "D:\\Programmering\\Dev\\prowl\\Products.json";

        static void Main(string[] args)
        {
            if(args.Length < 1) 
            {
                System.Console.WriteLine("No command given");
                return;
            }

            ProductWatchList pwl = new ProductWatchList();

            switch(args[0].ToLower())
            {
                case "add":
                    if(args.Length < 3) 
                    {
                        System.Console.WriteLine("[Add] needs 2 parameters: <name> <url>");
                        return;
                    }
                    bool addSucces = pwl.Add(new Product(args[1], args[2]));
                    if(addSucces) System.Console.WriteLine("Succes");
                    else System.Console.WriteLine("Error: Name already used");
                    break;
                case "delete":
                    if(args.Length < 2)
                    {
                        System.Console.WriteLine("[Delete] needs 1 parameter: <name>");
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
                        System.Console.WriteLine("[Check] needs 1 parameter: <name>");
                        return;
                    }
                    bool checkSucces = pwl.Check(args[1]);
                    if(!checkSucces) System.Console.WriteLine("Error: No product with this name");
                    break;
                case "open":
                    if(args.Length < 2)
                    {
                        System.Console.WriteLine("[Open] needs 1 parameter: <name>");
                        return;
                    }
                    bool openSucces = pwl.Open(args[1]);
                    if(!openSucces) System.Console.WriteLine("Error: No product with this name");
                    break;
                default:
                    System.Console.WriteLine("Unknown command: " + args[0]);
                    return;
            }
            System.Console.WriteLine();
        }

        public ProductWatchList()
        {
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

