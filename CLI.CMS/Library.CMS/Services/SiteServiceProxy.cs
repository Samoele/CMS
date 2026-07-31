using System;
using System.Linq;
using Library.CMS.Models;
using System.Text;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;

namespace Library.CMS.Services
{
    public class SiteServiceProxy
    {   
        //creates instance of Site Service Proxy
        private static SiteServiceProxy? _instance;
        public static SiteServiceProxy Current => _instance ??= new SiteServiceProxy();

        public List<Course> Courses { get; set; } = new List<Course>();
        public List<User> Users { get; set;} = new List<User>();

        //tracks who is currently logged in 
        public User? _currentUser;
        public User? CurrentUser 
        {
            get
            {
                // If an active session already exists, return it
                if (_currentUser != null) return _currentUser;



                return _currentUser;
            }
            set => _currentUser = value;
        }


        private SiteServiceProxy()
        {
            Courses = new List<Course>();
            Users = new List<User>();
            //data for testing
            //SeedData();
        }

        //methods for adding and deleting a student from system (university roster)
        public void AddStudent(Student newStudent)
        {
            if (newStudent == null) return;

            //ensures classification defaults to Freshman if left blank
            if (string.IsNullOrWhiteSpace(newStudent.Classification))
            {
                newStudent.Classification = "Freshman";
            }

            //auto generate new unique ID if ID is 0 or already taken
            var existingStudents = Users.OfType<Student>().ToList();

            if (newStudent.Id <= 0 || existingStudents.Any(s => s.Id == newStudent.Id))
            {
                //finds max current ID (e.g. if Alice is 1 and Bob is 2, next ID is 3)
                int maxId = existingStudents.Any() ? existingStudents.Max(s => s.Id) : 0;
                newStudent.Id = maxId + 1;
            }

            //appends the new student to the system/university Users roster
            Users.Add(newStudent);
        }

        public void DeleteStudent(int studentId)
        {
            // Find the student in the global Users list
            var studentToRemove = Users.OfType<Student>().FirstOrDefault(s => s.Id == studentId);
            if (studentToRemove != null)
            {
                Users.Remove(studentToRemove);
            }
        }

        //filters user list to return only objects that are derived Student models
        public List<Student> GetStudents()
        {
            return Users.OfType<Student>().ToList(); //gets all users and returns them as list
        }

        //Sets the active global user session (used during login testing)
        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        //sets active app user to a specific student by their unique Id
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

        //finds course directly by its unique id and returns null if not found
        public Course? GetCourseById(int id)
        {
            return Courses.FirstOrDefault(c => c.Id == id);
        }

        //Retrieves all courses  
        public List<Course> GetCourses()
        {
            return Courses;
        }

        //adds a new course to the system and assigns it a unique ID

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
                // generates unique ID for the module within course's module list
                int nextId = course.Modules.Count > 0 ? course.Modules.Max(m => m.Id) + 1 : 1;
                module.Id = nextId;

                course.Modules.Add(module);
            }
        }

        
        //finds specific module inside a course using stable IDs
        public Module? GetModuleFromCourse(int courseId, int moduleId)
        {
            var course = GetCourseById(courseId);
            return course?.Modules.FirstOrDefault(m => m.Id == moduleId);
        }


        //enrollment of students in a course roster
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
                
                //create a unique ID for the assignment within course's assignment list
                int nextId = course.Assignments.Count > 0 ? course.Assignments.Max(a => a.Id) + 1 : 1;
                assignment.Id = nextId;

                course.Assignments.Add(assignment);
                return true;
            }
            return false;
            
        }

        //Remove assignment from a course by its assignment ID
        public bool RemoveAssignmentFromCourse(int courseId, int assignmentId)
        {
            var course = GetCourseById(courseId);
            if (course != null)
            {
                var assignmentToRemove = course.Assignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignmentToRemove != null)
                {
                    course.Assignments.Remove(assignmentToRemove);
                    return true;
                }
            }
            return false;
        }

        //retrieves all courses with a student in it
        public List<Course> GetCoursesForStudent(int studentId)
        {
            return Courses.Where(c => c.Roster.Any(s => s.Id == studentId)).ToList();
        }

        //submits an attempt for a specific assignment
        public void SubmitAssignment(int courseId, int assignmentId, Submission submission)
        {
            var course = GetCourseById(courseId);
            var assignment = course?.Assignments.FirstOrDefault(a => a.Id == assignmentId);
            
            if (assignment != null)
            {

                assignment.Submissions ??= new List<Submission>(); 
                //sets unique submission ID for this assignment submission
                int nextId = assignment.Submissions.Count > 0 ? assignment.Submissions.Max(s => s.Id) + 1 : 1;
                submission.Id = nextId;
                
                assignment.Submissions.Add(submission);
            }
        }

        //request to clone a course
        public void CloneCourse(int originalCourseId, string newCode, string newName)
        {
            var original = GetCourseById(originalCourseId);
            if (original == null) return;

            //initialize new course container
            var clonedCourse = new Course
            {
                Id = Courses.Count > 0 ? Courses.Max(c => c.Id) + 1 : 1,
                Code = newCode,
                Name = newName,
                Semester = original.Semester,
                Roster = new List<Student>(), //enforce clean slate for roster
                Assignments = new List<Assignment>(),
                Modules = new List<Module>(),
                AssignmentGroup = new List<AssignmentGroup>()
            };

            //performs a deep clone of the course assignments
            foreach (var assign in original.Assignments)
            {
                clonedCourse.Assignments.Add(assign.Clone());
            }

            //deep clone of the modules
            foreach (var mod in original.Modules)
            {
                clonedCourse.Modules.Add(mod.Clone());
            }

            //performs a deep clone of assignment groups, connects newly cloned assignments
            foreach (var group in original.AssignmentGroup)
            {
                clonedCourse.AssignmentGroup.Add(group.Clone(clonedCourse.Assignments));
            }

            Courses.Add(clonedCourse);
        }

        // method to import and return info to the UI
    public ImportResult ImportRosterFromCsv(int courseId, string csvContent)
    {
        var result = new ImportResult();
        var targetCourse = GetCourses().FirstOrDefault(c => c.Id == courseId);

        if (targetCourse == null) return result;

        targetCourse.Roster ??= new List<Student>();
        var systemStudents = GetStudents() ?? new List<Student>();

        using var reader = new StringReader(csvContent);
        string? line;
        bool isFirstLine = true;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Header Check
            if (isFirstLine && (line.ToLower().Contains("id") || line.ToLower().Contains("name")))
            {
                isFirstLine = false;
                continue;
            }
            isFirstLine = false;

            var parts = line.Split(',');
            if (parts.Length < 2) continue;

            if (int.TryParse(parts[0].Trim(), out int studentId))
            {
                string studentName = parts[1].Trim();
                result.TotalProcessed++;

                // checks if student already in course roster
                if (targetCourse.Roster.Any(s => s.Id == studentId))
                {
                    result.DuplicateCount++;
                    continue;
                }

                //find or add student to system
                var existingStudent = systemStudents.FirstOrDefault(s => s.Id == studentId);
                if (existingStudent == null)
                {
                    existingStudent = new Student { Id = studentId, Name = studentName };
                    AddStudent(existingStudent); 
                }

                targetCourse.Roster.Add(existingStudent);
                result.AddedCount++;
            }
        }

        return result;
    }

    public string ExportRosterToCsv(int courseId)
    {
        var targetCourse = GetCourses().FirstOrDefault(c => c.Id == courseId);
        if (targetCourse == null || targetCourse.Roster == null || !targetCourse.Roster.Any())
        {
            return string.Empty;
        }

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Id,Name");

        foreach (var student in targetCourse.Roster)
        {
            csvBuilder.AppendLine($"{student.Id},{student.Name}");
        }

        return csvBuilder.ToString();
    }

    //Import and export methods for assignments
    //Import methods for assignments
    public ImportResult ImportAssignmentCSV(int courseId, string csvContent)
    {
        var result = new ImportResult();
        var targetCourse = GetCourses().FirstOrDefault(c => c.Id == courseId);

        if (targetCourse == null) return result;

        targetCourse.Assignments ??= new List<Assignment>();

        using var reader = new StringReader(csvContent);
        string? line;
        bool isFirstLine = true;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            //skips CSV header row category rows on import back to app
            if (isFirstLine && (line.ToLower().Contains("name") || line.ToLower().Contains("description")))
            {
                isFirstLine = false;
                continue;
            }
            isFirstLine = false;

            var parts = line.Split(',');
            if (parts.Length < 2) continue; //min req assg: Name, Description

            string name = parts[0].Trim();
            string description = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            double points = 100.0;
            if (parts.Length > 2 && double.TryParse(parts[2].Trim(), out double parsedPoints))
            {
                points = parsedPoints;
            }

            DateTime dueDate = DateTime.Now.AddDays(7);
            if (parts.Length > 3 && DateTime.TryParse(parts[3].Trim(), out DateTime parsedDate))
            {
                dueDate = parsedDate;
            }

            result.TotalProcessed++;

            //checks duplicate assignments in the same course with the exact same name
            if (targetCourse.Assignments.Any(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                result.DuplicateCount++;
                continue;
            }

            int nextId = targetCourse.Assignments.Any() ? targetCourse.Assignments.Max(a => a.Id) + 1 : 1;

            var newAssignment = new Assignment
            {
                Id = nextId,
                Name = name,
                Description = description,
                TotalPoints = points,
                DueDate = dueDate
            };

            targetCourse.Assignments.Add(newAssignment);
            result.AddedCount++;
        }

        return result;
    }

    //export methods for assignments 
    public string ExportAssignmentCSV(int courseId, int assignmentId)
    {
        var targetCourse = GetCourses().FirstOrDefault(c => c.Id == courseId);
        var assignment = targetCourse?.Assignments?.FirstOrDefault(a => a.Id == assignmentId);

        if (assignment == null) return string.Empty;

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Name,Description");
        csvBuilder.AppendLine($"\"{assignment.Name}\",\"{assignment.Description}\"");

        return csvBuilder.ToString();
    }

    //methods for getting a student's score for assignment (gradebook)
    public double GetStudentScore(int courseId, int studentId, int assignmentId)
    {
        //finds target assignment in the course
                
        var targetCourse = GetCourses().FirstOrDefault(c => c.Id == courseId);
        var assignment = targetCourse?.Assignments?.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null) return 0.0;

        // find student's submission
        var submission = assignment.Submissions?.FirstOrDefault(s => s.StudentId == studentId);

        //returns a grade if submitted and graded, otherwise default to 0.0 
        if (submission != null && submission.IsGraded)
        {
            return submission.Grade ?? 0.0;
        }

        return 0.0; 
    }

    //get a student's overall grade
    public double CalculateStudentOverallGrade(int courseId, int studentId)
    {
        
        var targetCourse = GetCourseById(courseId);
        if (targetCourse == null) return 0.0;

        var assignments = targetCourse.Assignments ?? new List<Assignment>();
        if (!assignments.Any()) return 0.0;

        double totalEarnedPoints = 0.0;
        double totalPossiblePoints = 0.0;

        foreach (var assignment in assignments)
        {
            double maxPoints = assignment.TotalPoints > 0 ? assignment.TotalPoints : 100.0;
            
            var submission = assignment.Submissions?.FirstOrDefault(s => s.StudentId == studentId);
            double score = (submission != null && submission.IsGraded) ? (submission.Grade ?? 0.0) : 0.0;

            totalEarnedPoints += score;
            totalPossiblePoints += maxPoints;
        }

        if (totalPossiblePoints == 0) return 0.0;

        return totalEarnedPoints / totalPossiblePoints * 100.0;
    }

    //map a student's grade from a percentage to a letter grade depending on letter ranges
    public string GetLetterGradeForPercentage(int courseId, double percentage)
    {
        var targetCourse = GetCourseById(courseId);
        if (targetCourse == null) return "N/A";

        if (percentage >= targetCourse.GradeScaleA) return "A";
        if (percentage >= targetCourse.GradeScaleB) return "B";
        if (percentage >= targetCourse.GradeScaleC) return "C";
        if (percentage >= targetCourse.GradeScaleD) return "D";
        return "F";
    }


    public string GetStudentLetterGrade(int courseId, int studentId)
    {
        double percentage = CalculateStudentOverallGrade(courseId, studentId);
        return GetLetterGradeForPercentage(courseId, percentage);
    }

    //DATABASE CONNECTION METHODS TO LINK TO StudentWebService, reroutes for the http requests
    //instantiate web service inside SiteServiceProxy
    private readonly StudentWebService _studentWebService = new StudentWebService();

    //fetches all students from db and update Users list
    public async Task RefreshStudentsFromDatabaseAsync()
    {
        var remoteStudents = await _studentWebService.FetchAllStudentsAsync();

        if (remoteStudents != null && remoteStudents.Any())
        {
            // remove existing Students from Users list to avoid duplicates
            Users.RemoveAll(u => u is Student);

            //adds Student objects fetched from mongoDb 
            Users.AddRange(remoteStudents);
        }
    }

    //adds a student to MongoDB Atlas and syncs user list
    public async Task<bool> AddStudentAsync(Student student)
    {
        //ensure unique ID is generated if ID 0 or invalid
        if (student.Id <= 0)
        {
            var existingStudents = GetStudents(); //Users.OfType<Student>()
            int maxId = existingStudents.Any() ? existingStudents.Max(s => s.Id) : 0;
            student.Id = maxId + 1;
        }
        
        bool success = await _studentWebService.CreateStudentAsync(student);
        
        if (success)
        {
            //adds to local users list so app updates immediately
            Users.Add(student);
        }
        
        return success;
    }

    //updates a student in MongoDB Atlas AND syncs locally
    public async Task<bool> UpdateStudentAsync(Student updatedStudent)
    {
        bool success = await _studentWebService.UpdateStudentAsync(updatedStudent);
        
        if (success)
        {
            var existingIndex = Users.FindIndex(u => u.Id == updatedStudent.Id);
            if (existingIndex != -1)
            {
                Users[existingIndex] = updatedStudent;
            }
        }
        
        return success;
    }

    //delets a student from MongoDB Atlas and syncs locally
    public async Task<bool> DeleteStudentAsync(int studentId)
    {
        bool success = await _studentWebService.DeleteStudentAsync(studentId);
        
        if (success)
        {
            Users.RemoveAll(u => u.Id == studentId);
        }
        
        return success;
    }

    //DATABASE CONNECTION METHODS TO LINK TO CourseWebService, reroutes for the http requests
    private readonly CourseWebService _courseWebService = new CourseWebService();

    //refresh courses from database
    public async Task RefreshCoursesFromDatabaseAsync()
    {
        // get courses from API
        var remoteCourses = await _courseWebService.FetchAllCoursesAsync();

        if (remoteCourses != null)
        {
            //reconnect Roster students to current Users list
            foreach (var course in remoteCourses)
            {
                if (course.Roster != null)
                {
                    //matches enrolled students by ID
                    var updatedRoster = new List<Student>();
                    foreach (var rosterStudent in course.Roster)
                    {
                        var matchedStudent = Users.OfType<Student>().FirstOrDefault(s => s.Id == rosterStudent.Id);
                        updatedRoster.Add(matchedStudent ?? rosterStudent);
                    }
                    course.Roster = updatedRoster;
                }
            }

            Courses = remoteCourses;
        }
    }

    //adds course with generated ID
    public async Task<bool> AddCourseAsync(Course course)
    {
        if (course.Id <= 0)
        {
            int maxId = Courses.Any() ? Courses.Max(c => c.Id) : 0;
            course.Id = maxId + 1;
        }

        bool success = await _courseWebService.CreateCourseAsync(course);
        if (success)
        {
            Courses.Add(course);
        }
        return success;
    }

    //update courses (roster changes, assignments, descriptions)
    public async Task<bool> UpdateCourseAsync(Course updatedCourse)
    {
        bool success = await _courseWebService.UpdateCourseAsync(updatedCourse);
        if (success)
        {
            var existingIndex = Courses.FindIndex(c => c.Id == updatedCourse.Id);
            if (existingIndex != -1)
            {
                Courses[existingIndex] = updatedCourse;
            }
        }
        return success;
    }

    //delete course
    public async Task<bool> DeleteCourseAsync(int courseId)
    {
        bool success = await _courseWebService.DeleteCourseAsync(courseId);
        if (success)
        {
            Courses.RemoveAll(c => c.Id == courseId);
        }
        return success;
    }









        


    












    }



    // Result object to return import info to the UI (class for roster and assignments import)
    public class ImportResult
    {
        public int TotalProcessed { get; set; }
        public int AddedCount { get; set; }
        public int DuplicateCount { get; set; }
    }


    

}