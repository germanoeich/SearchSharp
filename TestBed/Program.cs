using SearchSharp;
using System;

namespace TestBed
{
    class Program
    {
        static void Main(string[] args)
        {
            Search s = new Search();
            s.GetAllFiles();
            Console.Read();
        }
    }
}