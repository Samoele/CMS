using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Section { get; set; } = string.Empty; //different sections

        //Semester property for course creation
        public string Semester { get; set; } = string.Empty;

        //default grade ranges
        public double GradeScaleA { get; set; } = 90.0;
        public double GradeScaleB { get; set; } = 80.0;
        public double GradeScaleC { get; set; } = 70.0;
        public double GradeScaleD { get; set; } = 60.0;
        public double GradeScaleF { get; set; } = 50.0;
        
        //relationships
        public List<Student> Roster { get; set; } = new List<Student>();
        public List<Module> Modules { get; set; } = new List<Module>();
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();

        //new list initialized for announcements
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();

        //holds assignment group list of assignments for said group
        public List<AssignmentGroup> AssignmentGroup { get; set; } = new List<AssignmentGroup>();
    }
}