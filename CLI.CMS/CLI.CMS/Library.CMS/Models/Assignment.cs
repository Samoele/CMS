using System;
using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AvailablePoints { get; set; }
        public DateTime DueDate { get; set; }
        
        // Track completed work
        public List<Submission> Submissions { get; set; } = new List<Submission>();
    }
}