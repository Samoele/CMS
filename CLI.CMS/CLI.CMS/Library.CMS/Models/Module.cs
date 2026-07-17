using System.Collections.Generic;
using System.Dynamic;

namespace Library.CMS.Models
{
    public class Module
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ContentItem> Content { get; set; } = new List<ContentItem>();
    }
}