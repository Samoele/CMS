using System;

namespace Library.CMS.Models
{
    public class FileItem : ContentItem
    {
        public string FilePath { get; set; } = string.Empty;

        public override void Open()
        {
            Console.Clear();
            Console.WriteLine($"======================================");
            Console.WriteLine($" FILE: {Name}");
            Console.WriteLine($"======================================");
            Console.WriteLine($"[System Simulation] Opening file from path: '{FilePath}'...");
            Console.WriteLine("Loading file contents successfully into terminal memory.");
            Console.WriteLine($"======================================");
            Console.WriteLine("\nPress any key to close file...");
            Console.ReadKey();
        }
    }
}