using System;

namespace Library.CMS.Models

{
    public class Item{
        public int Id { get; set;}
        public string Title { get; set;} = string.Empty;
        public string Content { get; set;} = string.Empty;
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;
        public ItemType Type { get; set;}
    }

    public enum ItemType
    {
        Text,
        Assignment,
        Submission,
        Announcement,
        
    }
}