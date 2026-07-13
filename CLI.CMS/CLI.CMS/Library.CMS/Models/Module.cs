using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Module
    {
        public int Id { get; set; }
        public List<string> Content { get; set; } = new List<string>();
    }
}