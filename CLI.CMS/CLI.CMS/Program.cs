using System;
using Library.CMS.Models;
using CLI.CMS.Handlers;
using Library.CMS.Services;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] Empty)
        {
            var proxy = SiteServiceProxy.Current; // Get the singleton instance of SiteServiceProxy
            bool keepRunning = true;
            while (keepRunning)
            {
                Console.WriteLine("Welcome to Course Management System CLI!");
                Console.WriteLine("Please select your role:");
                Console.WriteLine("1. Student");
                Console.WriteLine("2. Teacher");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice (1-3): ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        RunStudentProxyMenu();
                        break;
                    case "2":
                        TeacherMenuHandlers.RunTeacherMenu();
                        break;
                    case "3":
                        keepRunning = false;
                        Console.WriteLine("Exiting the application. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }


            }
        }

        static void RunStudentProxyMenu()
        {
            // Get the singleton instance of SiteServiceProxy

        }


        





















        //end of file

    }

}