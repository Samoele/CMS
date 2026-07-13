using System;

namespace Library.CMS.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // Equivalent to FSUID
    }

    public class Student : User
    {
        public string Classification { get; set; } = string.Empty; // e.g., Freshman, Senior
    }

    public class Instructor : User
    {
        public int YearsOfExperience { get; set; }
    }
}