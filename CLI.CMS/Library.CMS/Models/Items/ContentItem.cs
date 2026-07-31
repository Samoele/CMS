namespace Library.CMS.Models
{
    public abstract class ContentItem
    {
        public int Id {get; set;}
        public string Name {get; set;} = string.Empty;

        public abstract void Open();

        public abstract ContentItem Clone();
    }
}