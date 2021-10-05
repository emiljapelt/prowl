using System;

namespace prowl
{
    public delegate MethodResult Method(string[] args);
    public class MethodConstruct
    {
        public string name { get; set; }
        public string shorthand { get; set; }
        public string args_info { get; set; }
        public Predicate<string[]> args_check { get; set; }
        public Method method { get; set; }
    }
}