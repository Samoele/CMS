using System;
using System.Linq;
using Library.CMS.Models;
using Library.CMS.Services;

namespace ConsoleApp1.Handlers
{
    public static class StudentMenuHandlers
    {
        public static void RunStudentMenu()
        {
            var proxy = SiteServiceProxy.Current;
            var students = proxy.GetStudents();

            if (students.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("--- Student Login ---");
                Console.WriteLine("No student profiles exist in the system yet.");
                Console.WriteLine("Please log in as a Teacher first to add/register a student.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            // Simulate logging in as a specific student (Issue #6 Role Selection)
            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("          Student Login Portal        ");
            Console.WriteLine("======================================");
            foreach (var s in students)
            {
                Console.WriteLine($"[ID: {s.Id}] {s.Name}");
            }
            Console.WriteLine("--------------------------------------");
            Console.Write("Enter your Student ID to log in: ");

            if (int.TryParse(Console.ReadLine(), out int studentId))
            {
                var currentStudent = students.FirstOrDefault(s => s.Id == studentId);
                if (currentStudent != null)
                {
                    RunStudentDashboard(currentStudent, proxy);
                }
                else
                {
                    Console.WriteLine("\nInvalid Student ID.");
                    Console.ReadKey();
                }
            }
        }

        private static void RunStudentDashboard(Student student, SiteServiceProxy proxy)
        {
            bool loggedIn = true;

            while (loggedIn)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  Welcome back, {student.Name}!  ");
                Console.WriteLine($"  Classification: {student.Classification}");
                Console.WriteLine("======================================");
                Console.WriteLine("1. View My Enrolled Courses");
                Console.WriteLine("2. Access a Course & Submit Assignments");
                Console.WriteLine("3. Log Out / Return to Main Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-3): ");

                string choice = (Console.ReadLine() ?? string.Empty).Trim();

                switch (choice)
                {
                    case "1":
                        ViewMyCourses(student, proxy);
                        break;
                    case "2":
                        AccessCoursePortal(student, proxy);
                        break;
                    case "3":
                        loggedIn = false;
                        break;
                    default:
                        Console.WriteLine($"\nInvalid choice '{choice}'. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ViewMyCourses(Student student, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("--- My Enrolled Courses ---");
            var myCourses = proxy.GetCoursesForStudent(student.Id);

            if (myCourses.Count == 0)
            {
                Console.WriteLine("You are not currently enrolled in any courses.");
            }
            else
            {
                foreach (var course in myCourses)
                {
                    Console.WriteLine($"  - {course.Code}: {course.Name}");
                }
            }
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void AccessCoursePortal(Student student, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("--- Select an Enrolled Course ---");
            var myCourses = proxy.GetCoursesForStudent(student.Id);

            if (myCourses.Count == 0)
            {
                Console.WriteLine("You are not currently enrolled in any courses.");
                Console.ReadKey();
                return;
            }

            foreach (var course in myCourses)
            {
                Console.WriteLine($"[ID: {course.Id}] {course.Code} - {course.Name}");
            }
            Console.WriteLine("--------------------------------------");
            Console.Write("Enter Course ID to access: ");
            
            if (int.TryParse(Console.ReadLine(), out int courseId) && myCourses.Any(c => c.Id == courseId))
            {
                RunCoursePortal(student, courseId, proxy);
            }
            else
            {
                Console.WriteLine("\nInvalid Selection.");
                Console.ReadKey();
            }
        }

        private static void RunCoursePortal(Student student, int courseId, SiteServiceProxy proxy)
        {
            var course = proxy.GetCourseById(courseId);
            if (course == null) return;

            bool inPortal = true;
            while (inPortal)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  Portal: {course.Code} - {course.Name}");
                Console.WriteLine("======================================");
                Console.WriteLine("1. View Modules & Course Content");
                Console.WriteLine("2. View & Submit Assignments");
                Console.WriteLine("3. Leave Course Portal");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-3): ");

                string choice = (Console.ReadLine() ?? string.Empty).Trim();

                switch (choice)
                {
                    case "1":
                        ViewCourseModules(course);
                        break;
                    case "2":
                        SubmitAssignmentMenu(student, course, proxy); // Issue #14
                        break;
                    case "3":
                        inPortal = false;
                        break;
                }
            }
        }

        private static void ViewCourseModules(Course course)
        {
            Console.Clear();
            Console.WriteLine($"--- {course.Code} Modules ---");
            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules published for this course yet.");
            }
            else
            {
                foreach (var mod in course.Modules)
                {
                    Console.WriteLine($"\n[Module ID: {mod.Id}]");
                    foreach (var item in mod.Content)
                    {
                        Console.WriteLine($"  - {item}");
                    }
                }
            }
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void SubmitAssignmentMenu(Student student, Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Assignments for {course.Code} ---");

            if (course.Assignments.Count == 0)
            {
                Console.WriteLine("No assignments exist in this course.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            // Print existing assignments
            Console.WriteLine($"{"ID",-5} | {"Name",-20} | {"Due Date",-12} | {"My Grade",-10}");
            Console.WriteLine(new string('-', 55));
            foreach (var assignment in course.Assignments)
            {
                // Find if this student already submitted it
                var existingSubmission = assignment.Submissions.FirstOrDefault(s => s.StudentId == student.Id);
                string gradeDisplay = existingSubmission != null 
                    ? (existingSubmission.IsGraded ? $"{existingSubmission.Grade}/{assignment.TotalPoints}" : "Ungraded") 
                    : "Unsubmitted";

                Console.WriteLine($"{assignment.Id,-5} | {assignment.Name,-20} | {assignment.DueDate.ToString("MM/dd/yyyy"),-12} | {gradeDisplay,-10}");
            }
            Console.WriteLine(new string('-', 55));
            Console.Write("Enter Assignment ID to view/submit, or press Enter to return: ");
            string input = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(input, out int assignId))
            {
                var targetAssign = course.Assignments.FirstOrDefault(a => a.Id == assignId);
                if (targetAssign != null)
                {
                    var existingSub = targetAssign.Submissions.FirstOrDefault(s => s.StudentId == student.Id);
                    if (existingSub != null)
                    {
                        Console.WriteLine($"\nYou already submitted this! Content: '{existingSub.Content}'");
                        Console.ReadKey();
                        return;
                    }

                    Console.Clear();
                    Console.WriteLine($"--- Submitting Assignment: {targetAssign.Name} ---");
                    Console.WriteLine($"Description: {targetAssign.Description}");
                    Console.WriteLine($"Max Points: {targetAssign.TotalPoints}");
                    Console.WriteLine("--------------------------------------");
                    Console.Write("Enter your submission text: ");
                    string submissionText = Console.ReadLine() ?? string.Empty;

                    var newSub = new Submission
                    {
                        StudentId = student.Id,
                        StudentName = student.Name,
                        Content = submissionText
                    };

                    proxy.SubmitAssignment(course.Id, targetAssign.Id, newSub);
                    Console.WriteLine("\nSuccess! Your assignment has been submitted.");
                    Console.ReadKey();
                }
            }
        }
    }
}