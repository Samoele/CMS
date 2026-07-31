namespace Library.CMS.Models
{
    public class Announcement 
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; } = DateTime.Now;
    }
}