using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Relationships
        public List<Student> Roster { get; set; } = new List<Student>();
        public List<Module> Modules { get; set; } = new List<Module>();
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();

        //holds assignment group list of assignments for said group
        public List<AssignmentGroup> AssignmentGroup { get; set; } = new List<AssignmentGroup>();
    }
}