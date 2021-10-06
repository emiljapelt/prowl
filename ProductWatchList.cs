using System;
using System.Collections.Generic;
using System.Collections;
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
        private List<MethodConstruct> methods;
        private string savepath;

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

            foreach(MethodConstruct mc in pwl.methods)
            {
                if (args[0] == mc.shorthand || args[0] == mc.name)
                {
                    if (!mc.params_check(args)) 
                    {
                        Console.WriteLine($"[{mc.name} | {mc.shorthand}] {mc.params_info}");
                        return;
                    }
                    HandleResult(mc.method(args));
                    return;
                }
            }
            Console.WriteLine("Unknown command: " + args[0]);
            Console.WriteLine("Use command [help | h] for a list of commands");
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

            methods = initMethodConstructs(this);
        }

        public static List<MethodConstruct> initMethodConstructs(ProductWatchList pwl)
        {
            return new List<MethodConstruct> {
                // CHECK
                new MethodConstruct {
                    name = "check",
                    shorthand = "c",
                    params_info = "takes 0 or 1 parameters: <filter>?",
                    params_check = args => args.Length == 1 || args.Length == 2,
                    method = args => {
                        string filter;
                        try { filter = args[1].ToLower(); } catch (Exception) { filter = ""; }

                        int count = 0;
                        foreach(Product product in pwl.products)
                        {
                            if(product.name.ToLower().StartsWith(filter)) 
                            {
                                count++;
                                var res = product.CheckProduct();
                                ColoredWriteLine(res.color, res.text, res.text.Length - 1, res.text.Length);
                                pwl.Save();
                            }
                        }
                        pwl.Save();
                        if (count == 0) return Failure("No products to be checked");
                        else return Success();
                    }
                },
                // OPEN
                new MethodConstruct {
                    name = "open",
                    shorthand = "o",
                    params_info = "takes 0 or 1 parameters: <filter>?",
                    params_check = args => args.Length == 1 || args.Length == 2,
                    method = args => {
                        string filter;
                        try { filter = args[1].ToLower(); } catch (Exception) { filter = ""; }

                        int count = 0;
                        foreach(Product product in pwl.products)
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
                },
                // ADD
                new MethodConstruct {
                    name = "add",
                    shorthand = "a",
                    params_info = "takes 2 parameters: <name> <url>",
                    params_check = args => args.Length == 3,
                    method = args => {
                        var name = args[1];
                        var url = args[2];

                        foreach(Product p in pwl.products) if(p.name == name) return Failure("Name already in use");
                        try
                        {
                            pwl.products.Add(new Product(name, url));
                            pwl.Save();
                            return Success("Product added");
                        }
                        catch (Exception)
                        {
                            return Failure("Something went wrong");
                        }
                    }
                },
                // REMOVE
                new MethodConstruct {
                    name = "remove",
                    shorthand = "rm",
                    params_info = "takes 1 parameter: <name>",
                    params_check = args => args.Length == 2,
                    method = args => {
                        var name = args[1];

                        foreach(Product product in pwl.products)
                        {
                            if(product.name == name) 
                            {
                                pwl.products.Remove(product);
                                pwl.Save();
                                return Success("Product was removed");
                            }
                        }
                        return Failure($"No product with name \"{name}\"");
                    }
                },
                // CLEAR
                new MethodConstruct {
                    name = "clear",
                    shorthand = "clr",
                    params_info = "takes 0 parameters",
                    params_check = args => args.Length == 1,
                    method = args => {
                        pwl.products = new List<Product>();
                        pwl.Save();
                        return Success("List cleared");
                    }
                },
                // HELP
                new MethodConstruct {
                    name = "help",
                    shorthand = "h",
                    params_info = "takes 0 parameters",
                    params_check = args => args.Length == 1,
                    method = args => {
                        Console.WriteLine("[add | a] <name> <url>: Add a product from an url to your watch list, saving it with the given name");
                        Console.WriteLine("[remove | rm] <name>: Remove a product saved under the given name, from your watch list");
                        Console.WriteLine("[clear | clr]: Remove all products from your watch list");
                        Console.WriteLine("[check | c] <filter>?: View how the price has changed for products, with names starting with <filter>. If <filter> is empty, all products are checked");
                        Console.WriteLine("[open | o] <filter>?: Open products, with names starting with <filter>, in the default browser. If <filter> is empty, all products are opened");
                        return Success();
                    }
                }
            };
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
        private static void OpenUrl(string url)
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

        private static void ColoredWriteLine(ConsoleColor color, string text, int startIndex, int endIndex)
        {
            Console.Write(text.Substring(0, startIndex));
            Console.ForegroundColor = color;
            Console.Write(text.Substring(startIndex, endIndex - startIndex));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text.Substring(endIndex, text.Length - endIndex));
        }

        private static MethodResult Success(string msg = "") { return new Success(msg); }

        private static MethodResult Failure(string msg) { return new Failure(msg); }

        private static void HandleResult(MethodResult result)
        {
            if (result is Failure) Console.WriteLine($"Failure: {result.Message}");
            else Console.WriteLine(result.Message);
        }
    }
}

