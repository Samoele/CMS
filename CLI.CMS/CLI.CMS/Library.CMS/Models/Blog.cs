using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Blog
    {
        public int Id {get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public List<Item> Posts { get; set; } = new List<Item>();
    }
}