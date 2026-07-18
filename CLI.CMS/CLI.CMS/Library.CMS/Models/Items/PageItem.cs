using System;


namespace Library.CMS.Models
{
    public class PageItem : ContentItem
    {
        public string Body {get; set;} = string.Empty;

        public override void Open()
        {
            Console.Clear();
            Console.WriteLine($"======================================");
            Console.WriteLine($" PAGE: {Name}");
            Console.WriteLine($"======================================");
            Console.WriteLine(Body);
            Console.WriteLine($"======================================");
            Console.WriteLine("\nPress any key to close page...");
            Console.ReadKey();
        }
    }
}