using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Course
    {
        public string Code { get; set; } = string.Empty; // ex: "COP4870"
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<User> EnrolledStudents { get; set; } = new List<User>();
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        public List<Module> Modules { get; set; } = new List<Module>();
    }
}