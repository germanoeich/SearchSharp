using SearchSharp;
using System;

namespace TestBed
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var s = new Search();
            s.GetAllFiles();
            Console.Read();
        }
    }
}