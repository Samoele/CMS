using System;

namespace Library.CMS.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public double? Grade { get; set; }

        public bool IsGraded => Grade.HasValue;
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}