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


        //logic to clone the modules including items in them
        public Module Clone()
        {
            var clonedModule = new Module
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Content = new List<ContentItem>()
            };

            foreach (var item in this.Content)
            {
                clonedModule.Content.Add(item.Clone());
            }

            return clonedModule;
        }
    }
}