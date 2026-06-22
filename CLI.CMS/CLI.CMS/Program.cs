// See https://aka.ms/new-console-template for more information

using System;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine("Choose a site to manage:");

            Site site1 = new Site("Site 1");
            List <Site> sites = new List<Site> <Site>
            {
                site1
            };

            int count = 0 ;
            sites.ForEach(site => Console.WriteLine($"{++count}. {s}"));

            var selection = Console.ReadLine();

        }
    }
}


