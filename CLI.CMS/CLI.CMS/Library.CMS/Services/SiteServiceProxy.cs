using System;
using System.Collections.Generic; //Includes add and delete methods for lists built in
using System.Linq;
using Library.CMS.Models;

namespace Library.CMS.Services
{
    public class SiteServiceProxy
    {   
        //Creates instance of Site Service Proxy
        private static SiteServiceProxy? _instance;
        public static SiteServiceProxy Current => _instance ??= new SiteServiceProxy();

        public List<Course> Courses { get; set; } = new List<Course>();
        public List<User> Users { get; set;} = new List<User>();

        //track who is currently logged in 
        public User? CurrentUser { get; set;} 

        private SiteServiceProxy()
        {
            Courses = new List<Course>();
            Users = new List<User>();
            //data for testing
            SeedData();
        }

        private void SeedData()
        {
            Users.Add(new Student { Id = 1, Name = "Alice", Code = "S001", Classification = "Freshman"});
            Users.Add(new Student { Id = 2, Name = "Bob", Code = "S002", Classification = "Senior"});
            Users.Add(new Instructor { Id = 3, Name = "Dr. Smith", Code = "I001", YearsOfExperience = 10});

        }


        /// Filters our user list to return only objects that are derived Student models
        public List<Student> GetStudents()
        {
            return Users.OfType<Student>().ToList(); //Uses LINQ library to get all users that are of type Student and returns them as a list
        }

        /// Sets the active application user to a specific student by their unique Id.
        public bool ProxyAsStudent(int studentId)
        {
            var student = Users.OfType<Student>().FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                CurrentUser = student;
                return true;
            }
            return false;
        }

        // Finds a course directly by its unique id and returns null if not found
        public Course? GetCourseById(int id)
        {
            return Courses.FirstOrDefault(c => c.Id == id);
        }

        //Adds a new course to the system and assigns it a unique ID

        public void AddCourse(Course course)
        {
            // Generate a unique ID based on the highest existing ID + 1
            int nextId = Courses.Count > 0 ? Courses.Max(c => c.Id) + 1 : 1;
            course.Id = nextId;
            
            Courses.Add(course);
        }

        public bool DeleteCourse(int courseId)
        {
            var courseToRemove = Courses.FirstOrDefault(c => c.Id == courseId);
            if (courseToRemove != null)
            {
                Courses.Remove(courseToRemove);
                return true;
            }
            return false;
        }

        //method to add module to a course
        public void AddModuleToCourse(int courseId, Module module)
        {
            var course = GetCourseById(courseId);
            if (course != null)
            {
                // Generate a unique ID for the module within this course's module list
                int nextId = course.Modules.Count > 0 ? course.Modules.Max(m => m.Id) + 1 : 1;
                module.Id = nextId;

                course.Modules.Add(module);
            }
        }

        
        // Finds a specific module inside a course using stable IDs.
        public Module? GetModuleFromCourse(int courseId, int moduleId)
        {
            var course = GetCourseById(courseId);
            return course?.Modules.FirstOrDefault(m => m.Id == moduleId);
        }


        //Enrollment of students in a course roster
        public bool EnrollStudent(int courseId, int studentId)
        {
            var course = GetCourseById(courseId);
            var student = Users.OfType<Student>().FirstOrDefault(s => s.Id == studentId);

            if (course != null && student != null)
            {
                course.Roster.Add(student);
                return true;
                
            }
            return false;

        }

        public bool UnenrollStudent(int courseId, int studentId) //methods for enrollment need CLI menu
        {
            var course = GetCourseById(courseId);
            if (course != null)
            {
                var studentToRemove = course.Roster.FirstOrDefault(s => s.Id == studentId);
                if (studentToRemove != null)
                {
                    course.Roster.Remove(studentToRemove); //Removes student from course roster
                    return true;
                }
            }
            return false;
        }

        public bool AddAssignment(int courseId, Assignment assignment)
        {
            var course =  GetCourseById(courseId);
            if (course != null)
            {
                //create a unique ID for the assignment within this course's assignment list
                int nextId = course.Assignments.Count > 0 ? course.Assignments.Max(a => a.Id) + 1 : 1;
                assignment.Id = nextId;

                course.Assignments.Add(assignment);
                return true;
            }
            return false;
            
        }


        


    












    }
}