using System;

namespace Library.CMS.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        //File upload extensions on submissions model
        //File attachment properties
        public string? FileName { get; set; }
        public byte[]? FileData { get; set; }

        //Helper property for XAML IsVisible binding
        public bool HasFile => !string.IsNullOrEmpty(FileName) && FileData != null && FileData.Length > 0;

        public double? Grade { get; set; }
        public string Comment { get; set; }

        public bool IsGraded => Grade.HasValue;
        public bool HasComment => !string.IsNullOrWhiteSpace(Comment);
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}