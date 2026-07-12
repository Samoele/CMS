using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Module
    {
        public string Code { get; set; } = string.Empty; // ex: "COP4870"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Modules { get; set; } = new List<string>();
    }
}