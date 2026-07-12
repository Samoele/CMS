using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalPoints { get; set;} = 100;
        public DateTime DueDate { get; set; }
        public List<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public class Submission
    {
        public int StudentId { get; set; }
        public string Content { get; set;} = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public int? Grade {get; set;} //null if not graded, 1 if graded
        public bool IsGraded => Grade.HasValue;
        
    }
}