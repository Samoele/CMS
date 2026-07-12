using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Page
    {
        public int Id { get; set;}
        public string Title {get; set;} = string.Empty;
        public string Description { get; set;} = string.Empty;

        //List of items contained in the page
        public List<Item> Items {get; set;} = new List<Item>();

    }
    
}



    

