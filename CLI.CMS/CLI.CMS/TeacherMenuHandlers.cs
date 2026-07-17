using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Library.CMS.Models;
using Library.CMS.Services;

//Handlers and methods for teacher menu commands

namespace CLI.CMS.Handlers
{
    public static class TeacherMenuHandlers
    {
        public static void RunTeacherMenu()
        {
            var proxy = SiteServiceProxy.Current;
            bool inTeacherMenu = true;
            while (inTeacherMenu)
            {
                Console.Clear();
                Console.WriteLine("            Teacher Menu              ");
                Console.WriteLine("======================================");
                Console.WriteLine("1. Add a New Course");
                Console.WriteLine("2. Select an Existing Course");
                Console.WriteLine("3. Return to Main Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-3): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Logic to add a new course
                        CreateNewCourseForm(proxy);
                        break;
                    case "2":
                        // Logic to select an existing course
                        SelectExistingCourseForm(proxy);
                        break;
                    case "3": //exit the menu
                        inTeacherMenu = false;
                        break;
                        
                        
                }

            }
        }


        //Create a new course from teacher menu
        static void CreateNewCourseForm(SiteServiceProxy proxy)
        {
        Console.WriteLine("Creating a New Course");
        Console.WriteLine("=====================");
        Console.Write("Enter Course Code: ");
        string? code = Console.ReadLine();
        Console.Write("Enter Course Name: ");
        string? name = Console.ReadLine();
        Console.Write("Enter Course Description: ");
        string? description = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(description))
        {
            var newCourse = new Course
            {
                Code = code,
                Name = name,
                Description = description
            };

            proxy.AddCourse(newCourse);
            Console.WriteLine($"Course '{name}' added successfully!");
            Console.WriteLine($"\nSuccess! Course '{name}' created with ID: {newCourse.Id}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("All fields are required. Please try again.");
        }
        }

        //Select existing course form teachers
        static void SelectExistingCourseForm(SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("Select an Existing Course");

            if (proxy.Courses.Count == 0)
            {
                Console.WriteLine("No courses available. Please add a course first.");
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
                return;
            }

            // Print out all available courses with their stable structural IDs
            Console.WriteLine("Available Courses:");
            foreach (var course in proxy.Courses)
            {
                Console.WriteLine($"[ID: {course.Id}] {course.Code} - {course.Name}");
            }
            Console.WriteLine("--------------------------------------");

            Console.Write("Enter the exact ID of the course you want to manage: ");
            if (int.TryParse(Console.ReadLine(), out int targetId))
            {
                // Query the data store using the unique ID directly
                var selectedCourse = proxy.GetCourseById(targetId);

                if (selectedCourse != null)
                {
                    Console.WriteLine($"\nSuccessfully selected: {selectedCourse.Name}!");
                    RunCourseManagementMenu(selectedCourse, proxy);                }
                else
                {
                    Console.WriteLine($"\nError: No course found matching ID {targetId}.");
                }
            }
            else
            {
                Console.WriteLine("\nInvalid ID entry. Must be an integer format.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();

        }

        //Course management menu for teachers to manage a specific course (assignments, modules, enrollment, grading)
        static void RunCourseManagementMenu(Course course, SiteServiceProxy proxy)
        {
            bool inCourseMenu = true;

            while (inCourseMenu)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  Managing: {course.Code} - {course.Name}");
                Console.WriteLine("======================================");
                Console.WriteLine($"Description: {course.Description}");
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("1. Update Course Description");
                Console.WriteLine("2. Delete This Course");
                Console.WriteLine("3. Manage Roster (Unenroll Student)");
                Console.WriteLine("4. Manage Assignments (Add/Edit)");
                Console.WriteLine("5. Manage Modules (Add/Edit Content)");
                Console.WriteLine("6. Grade Submissions");
                Console.WriteLine("7. Return to Teacher Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-7): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        //Updates description of a course
                        UpdateCourseDescriptionForm(course);
                        Console.ReadKey();
                        break;
                    case "2":
                        // Deletes description of a course
                        DeleteCourseForm(course, proxy);
                        inCourseMenu = false; // Exit the course management menu after deletion
                        break;
                    case "3":
                        //covers enrolling and unenrolling students
                        RosterManagementMenu(course, proxy);
                        Console.ReadKey();
                        break;
                    case "4":
                        //will map to Issues #13 & #17
                        RunAssignmentsMenu(course, proxy);
                        Console.ReadKey();
                        break;
                    case "5":
                        //runs module management menu in a course
                        RunModulesMenu(course, proxy);
                        break;
                    case "6":
                        //manages grade submissions window for assignments in a course
                        GradeSubmissionsForm(course);
                        Console.ReadKey();
                        break;
                    case "7":
                        inCourseMenu = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid choice. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }


        // Update the course description for a specific course
        static void UpdateCourseDescriptionForm(Course course)
        {
            Console.Clear();
            Console.WriteLine("--- Update Course Description ---");
            Console.WriteLine($"Current Description: {course.Description}");
            Console.WriteLine("----------------------------------");
            
            Console.Write("Enter new description: ");
            string newDescription = Console.ReadLine() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(newDescription))
            {
                course.Description = newDescription;
                Console.WriteLine("\nSuccess! Course description updated.");
            }
            else
            {
                Console.WriteLine("\nUpdate cancelled. Description cannot be empty.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }


        //Deleting a course from the system
        static void DeleteCourseForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("--- Delete Course ---");
            Console.WriteLine($"Are you absolutely sure you want to delete {course.Code}: {course.Name}?");
            Console.WriteLine("This action cannot be undone.");
            Console.WriteLine("----------------------------------");
            Console.Write("Type 'YES' to confirm deletion: ");
            
            string? confirmation = Console.ReadLine();

            if (confirmation?.Trim().ToUpper() == "YES")
            {
                bool success = proxy.DeleteCourse(course.Id);
                if (success)
                {
                    Console.WriteLine("\nSuccess! The course has been permanently removed.");
                }
                else
                {
                    Console.WriteLine("\nError: Course could not be found or removed.");
                }
            }
            else
            {
                Console.WriteLine("\nDeletion cancelled. Returning to menu.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

    //add a new module to a specific course by ID
        static void AddModuleForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Add Module to {course.Code} ---");
            
            var newModule = new Module();
            
            // In our specification, Module only has an Id and a List<string> Content.
            // Let's allow them to add an initial content string to start off.
            Console.Write("Enter initial module content or description: ");
            string initialContent = Console.ReadLine() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(initialContent))
            {
                newModule.Content.Add(initialContent);
            }

            proxy.AddModuleToCourse(course.Id, newModule);

            Console.WriteLine($"\nSuccess! Module created with ID: {newModule.Id} and added to {course.Code}.");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }


        //Module management menu for teachers to manage modules in specific course
        static void RunModulesMenu(Course course, SiteServiceProxy proxy)
        {
            bool inModulesMenu = true;

            while (inModulesMenu)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  {course.Code} - Module Management");
                Console.WriteLine("======================================");
                
                if (course.Modules.Count == 0)
                {
                    Console.WriteLine("No modules exist in this course yet.");
                }
                else
                {
                    Console.WriteLine("Current Modules:");
                    foreach (var mod in course.Modules)
                    {
                        Console.WriteLine($"[Module ID: {mod.Id}]");
                        foreach (var contentItem in mod.Content)
                        {
                            Console.WriteLine($"  - {contentItem}");
                        }
                    }
                }
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("1. Add a New Module");
                Console.WriteLine("2. Add Content to a Module");
                Console.WriteLine("3. Modify Content in a Module");
                Console.WriteLine("4. Remove Content from a Module");
                Console.WriteLine("5. Return to Course Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-5): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Adds a module inside a course
                        AddModuleForm(course, proxy);
                        break;
                    case "2":
                        // Adds text vcontent to a module inside a course
                        AddContentToModuleForm(course, proxy);
                        break;
                    case "3":
                        // Modify content in a module
                        ModifyContentInModuleForm(course, proxy);
                        break;
                    case "4":
                        // Remove content from module
                        RemoveContentFromModuleForm(course, proxy);
                        break;
                    case "5":
                        inModulesMenu = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid choice. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void AddContentToModuleForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Add Content to a Module ({course.Code}) ---");

            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules exist in this course yet. Create a module first.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            // List available modules so the teacher can pick one
            Console.WriteLine("Available Modules:");
            foreach (var mod in course.Modules)
            {
                Console.WriteLine($"[Module ID: {mod.Id}] (Contains {mod.Content.Count} items)");
            }
            Console.WriteLine("--------------------------------------");

            Console.Write("Enter the ID of the module to add content to: ");
            if (int.TryParse(Console.ReadLine(), out int targetModuleId))
            {
                var targetModule = proxy.GetModuleFromCourse(course.Id, targetModuleId);

                if (targetModule != null)
                {
                    Console.Write("Enter the content text to add: ");
                    string contentText = Console.ReadLine() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(contentText))
                    {
                        targetModule.Content.Add(contentText);
                        Console.WriteLine("\nSuccess! Content item added to the module.");
                    }
                    else
                    {
                        Console.WriteLine("\nCancelled. Content cannot be blank.");
                    }
                }
                else
                {
                    Console.WriteLine($"\nError: Module with ID {targetModuleId} not found.");
                }
            }
            else
            {
                Console.WriteLine("\nInvalid ID entry.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }


        //Form for modifying content in a module
        static void ModifyContentInModuleForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Modify Module Content ({course.Code}) ---");

            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules exist in this course yet.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            // List available modules
            Console.WriteLine("Available Modules:");
            foreach (var mod in course.Modules)
            {
                Console.WriteLine($"[Module ID: {mod.Id}] (Contains {mod.Content.Count} items)");
            }
            Console.WriteLine("--------------------------------------");

            Console.Write("Enter the ID of the module to modify: ");
            if (int.TryParse(Console.ReadLine(), out int targetModuleId))
            {
                var targetModule = proxy.GetModuleFromCourse(course.Id, targetModuleId);

                if (targetModule != null)
                {
                    if (targetModule.Content.Count == 0)
                    {
                        Console.WriteLine("\nThis module has no content items to modify.");
                    }
                    else
                    {
                        Console.WriteLine("\nContent Items:");
                        for (int i = 0; i < targetModule.Content.Count; i++)
                        {
                            Console.WriteLine($"[{i}] {targetModule.Content[i]}");
                        }
                        Console.WriteLine("--------------------------------------");

                        Console.Write("Enter the index number of the item to modify: ");
                        if (int.TryParse(Console.ReadLine(), out int targetIndex) && targetIndex >= 0 && targetIndex < targetModule.Content.Count)
                        {
                            Console.WriteLine($"\nCurrent text: \"{targetModule.Content[targetIndex]}\"");
                            Console.Write("Enter the new text: ");
                            string updatedText = Console.ReadLine() ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(updatedText))
                            {
                                targetModule.Content[targetIndex] = updatedText;
                                Console.WriteLine("\nSuccess! Content item updated.");
                            }
                            else
                            {
                                Console.WriteLine("\nCancelled. Content cannot be blank.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid index selection.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"\nError: Module with ID {targetModuleId} not found.");
                }
            }
            else
            {
                Console.WriteLine("\nInvalid ID entry.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }


        //Method for deleting a module in module menu
        static void RemoveContentFromModuleForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Remove Module Content ({course.Code}) ---");

            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules exist in this course yet.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            // List available modules
            Console.WriteLine("Available Modules:");
            foreach (var mod in course.Modules)
            {
                Console.WriteLine($"[Module ID: {mod.Id}] (Contains {mod.Content.Count} items)");
            }
            Console.WriteLine("--------------------------------------");

            Console.Write("Enter the ID of the module to remove content from: ");
            if (int.TryParse(Console.ReadLine(), out int targetModuleId))
            {
                var targetModule = proxy.GetModuleFromCourse(course.Id, targetModuleId);

                if (targetModule != null)
                {
                    if (targetModule.Content.Count == 0)
                    {
                        Console.WriteLine("\nThis module has no content items to remove.");
                    }
                    else
                    {
                        Console.WriteLine("\nContent Items:");
                        for (int i = 0; i < targetModule.Content.Count; i++)
                        {
                            Console.WriteLine($"[{i}] {targetModule.Content[i]}");
                        }
                        Console.WriteLine("--------------------------------------");

                        Console.Write("Enter the index number of the item to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int targetIndex) && targetIndex >= 0 && targetIndex < targetModule.Content.Count)
                        {
                            string removedText = targetModule.Content[targetIndex];
                            targetModule.Content.RemoveAt(targetIndex);
                            
                            Console.WriteLine($"\nSuccess! Permanently removed: \"{removedText}\"");
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid index selection.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"\nError: Module with ID {targetModuleId} not found.");
                }
            }
            else
            {
                Console.WriteLine("\nInvalid ID entry.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        //Roster management menu for teachers to manage student enrollment in a specific course
        private static void RosterManagementMenu(Course course, SiteServiceProxy proxy)
        {
            bool inRosterMenu = true;
            while (inRosterMenu)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  {course.Code} - Roster Management");
                Console.WriteLine("======================================");


                if (course.Roster.Count == 0)
                {
                    Console.WriteLine("\n No students are currently enrolled in this course.");
                }
                else
                {
                    Console.WriteLine("\nCurrent Roster:");
                    foreach (var student in course.Roster)
                    {
                        Console.WriteLine($"ID: {student.Id} - {student.Name} - {student.Code} - {student.Classification}");
                    }
                }
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("[1] View Roster");
                Console.WriteLine("[2] Enroll Student");
                Console.WriteLine("[3] Remove Student");
                Console.WriteLine("[4] Return to Course Management Menu");
                Console.WriteLine("======================================");

                Console.Write("Enter choice (1-4):");
                string? choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                    //Logic to view the course roster
                        ViewCourseRoster(course);
                        break;
                    case "2":
                    //Logic to enroll a student into the course
                        EnrollStudentForm(course, proxy);
                        break;
                    case "3":
                    //Logic to remove a student from the course
                        UnenrollStudentForm(course, proxy);
                        break;
                    case "4":
                    //Logic to return to course management menu
                        inRosterMenu = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid choice. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }

            }

        }

        private static void EnrollStudentForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("--Enroll a Student--");
            //fetch all university students from our database (future db)
            var allStudents = proxy.GetStudents();

            if (allStudents.Count == 0)
            {
                Console.WriteLine("No students registered in the database.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Available University Students");
            foreach (var s in allStudents)
            {
                if (!course.Roster.Any(r => r.Id == s.Id))
                {
                    //Don't show them if already in the course
                    //If Roster has same ID, then do not show those students
                    Console.WriteLine($"ID: {s.Id} - {s.Name} - {s.Classification}");
                }
            }
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("Enter the Student ID to enroll:");
            if (int.TryParse(Console.ReadLine(), out int studentId))
            {
                if (proxy.EnrollStudent(course.Id, studentId))
                {
                    Console.WriteLine("\nStudent succesfully added to Course Roster");
                }
                else
                {
                    Console.WriteLine("\nFailed to enroll student. Invalid ID");

                }
            }
            Console.ReadKey();
        }

        //Menu options for unenrolling a student
        private static void UnenrollStudentForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("--Unenroll a Student--");
            if (course.Roster.Count == 0)
            {
                Console.WriteLine("No students enrolled in the course");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Enter the ID of the student to be removed from the course: ");
            if (int.TryParse(Console.ReadLine(), out int studentId))
            {
                if (proxy.UnenrollStudent(course.Id, studentId))
                {
                    Console.WriteLine("\nSuccess! Student succesfully removed from the course");
                }
                else
                {
                    Console.WriteLine("Error: Student ID not found in the course roster");
                }
            }
            Console.ReadKey();


        }

        //Logic to view full roster in a course
        private static void ViewCourseRoster(Course course)
        {
            Console.Clear();
            Console.WriteLine($"======================================");
            Console.WriteLine($"      {course.Code} Official Roster    ");
            Console.WriteLine($"======================================");

            if (course.Roster.Count == 0)
            {
                Console.WriteLine("No students are currently enrolled in this course.");
            }
            else
            {
                // Formatting table like headers for better visual clarity
                Console.WriteLine(string.Format("{0,-6} | {1,-18} | {2,-15}", "ID", "Name", "Classification"));
                Console.WriteLine("------------------------------------------------");
                
                foreach (var student in course.Roster)
                {
                    Console.WriteLine(string.Format("{0,-6} | {1,-18} | {2,-15}", 
                        student.Id, 
                        student.Name, 
                        student.Classification));
                }
            }

            Console.WriteLine("======================================");
            Console.WriteLine("\nPress any key to return to Roster Menu...");
            Console.ReadKey();
        }


        private static void RunAssignmentsMenu(Course course, SiteServiceProxy proxy)
        {
            bool inAssignmentsMenu = true;

            while (inAssignmentsMenu)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  {course.Code} - Assignment Management");
                Console.WriteLine("======================================");

                if (course.Assignments.Count == 0)
                {
                    Console.WriteLine("No assignments exist in this course yet.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-5} | {"Name",-15} | {"Description",-20} | {"Due Date",-12} | {"Total Points",-12}");
                    Console.WriteLine(new string('-', 76)); //same as long line of dashes
                    foreach (var assignment in course.Assignments)
                    {

                        string nameDisplay = assignment.Name.Length > 15 
                            ? assignment.Name.Substring(0, 12) + "..." 
                            : assignment.Name;
                
                        string descDisplay = assignment.Description.Length > 20 
                            ? assignment.Description.Substring(0, 17) + "..." 
                            : assignment.Description;

                        string dateDisplay = assignment.DueDate.ToString("MM/dd/yyyy");

                        Console.WriteLine($"{assignment.Id,-5} | {nameDisplay,-15} | {descDisplay,-20} | {dateDisplay,-12} | {assignment.TotalPoints,-12}");
                            
                    }
                }
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("1. Add a New Assignment");
                Console.WriteLine("2. Edit an Existing Assignment");
                Console.WriteLine("3. Delete an Assignment");
                Console.WriteLine("4. Return to Course Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-4): ");

                string choice = (Console.ReadLine() ?? string.Empty).Trim();

                switch (choice)
                {
                    case "1":
                        CreateAssignmentForm(course, proxy);
                        break;
                    case "2":
                        EditAssignmentForm(course, proxy);
                        Console.ReadKey();
                        break;
                    case "3":
                        DeleteAssignmentForm(course, proxy);
                        Console.ReadKey();
                        break;
                    case "4":
                        inAssignmentsMenu = false;
                        break;
                    default:
                        Console.WriteLine($"\nInvalid choice '{choice}'. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }

    private static void CreateAssignmentForm(Course course, SiteServiceProxy proxy)
    {
        Console.Clear();
        Console.WriteLine($"--- Create New Assignment for {course.Code} ---");

        Console.Write("Enter Assignment Name: ");
        string name = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("\nAssignment creation cancelled. Name cannot be blank.");
            Console.ReadKey();
            return;
        }

        Console.Write("Enter Assignment Description: ");
        string description = Console.ReadLine() ?? string.Empty;

        Console.Write("Enter Maximum Points Possible: ");
        if (!int.TryParse(Console.ReadLine(), out int maxPoints) || maxPoints < 0)
        {
            Console.WriteLine("\nInvalid points entered. Defaulting to 100 points.");
            maxPoints = 100;
        }

        // Get due date accurately using DateTime
        Console.Write("Enter Due Date (MM/dd/yyyy): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime dueDate))
        {
            Console.WriteLine("\nInvalid date format. Defaulting to one week from today.");
            dueDate = DateTime.Today.AddDays(7);
        }        


        var newAssignment = new Assignment
        {
            Name = name,
            Description = description,
            DueDate = dueDate,
            TotalPoints = maxPoints
        };

        proxy.AddAssignment(course.Id, newAssignment);

        Console.WriteLine($"\nSuccess! Assignment '{name}' created with local ID: {newAssignment.Id}");
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }

    private static void EditAssignmentForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--Edit Assignment ({course.Code}) ---");

            if (course.Assignments.Count == 0)
            {
                Console.WriteLine("No assignments exist in this course yet.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
                
            }

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("Enter the ID of assignment to edit: ");
            if (int.TryParse(Console.ReadLine(), out int targetId))
            {
                var assignment = course.Assignments.FirstOrDefault(a => a.Id == targetId);

                if (assignment != null)
                {
                    Console.Clear();
                    Console.WriteLine($"--- Editing Assignment [ID: {assignment.Id}] ---");
                    
                    //edit name of assignment
                    Console.WriteLine($"Current Name: {assignment.Name}");
                    Console.Write("Enter new name (leave blank to keep current): ");
                    string newName = Console.ReadLine() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        assignment.Name = newName;
                    }

                    //edit assignment desc
                    Console.WriteLine($"\nCurrent Description: {assignment.Description}");
                    Console.Write("Enter new description (leave blank to keep current): ");
                    string newDesc = Console.ReadLine() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(newDesc))
                    {
                        assignment.Description = newDesc;
                    }

                    //edit total points of assignment
                    Console.WriteLine($"\nCurrent Max Points: {assignment.TotalPoints}");
                    Console.Write("Enter new max points (leave blank to keep current): ");
                    string pointsInput = Console.ReadLine() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(pointsInput))
                    {
                        if (int.TryParse(pointsInput, out int newPoints) && newPoints >= 0)
                        {
                            assignment.TotalPoints = newPoints;
                        }
                        else
                        {
                            Console.WriteLine("Invalid entry. Points left unchanged.");
                        }
                        
                    }

                    Console.WriteLine("Success! Assignment has been edited.");
                    
                    
                }
                else
                {
                    Console.WriteLine($"\nError: Assignment with ID {targetId} not found.");
                }

            }
            else
            {
                Console.WriteLine("\nInvalid ID selection format.");
            }
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
        }

        private static void DeleteAssignmentForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Delete Assignment ({course.Code}) ---");

            if (course.Assignments.Count == 0)
            {
                Console.WriteLine("No assignments exist in this course to delete.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter the ID of the assignment you want to delete: ");
            if (int.TryParse(Console.ReadLine(), out int targetId))
            {
                var assignment = course.Assignments.FirstOrDefault(a => a.Id == targetId);

                if (assignment != null)
                {
                    Console.Clear();
                    Console.WriteLine($"WARNING: You are about to permanently delete assignment: '{assignment.Name}'");
                    Console.Write("Type 'YES' to confirm deletion: ");
                    string confirmation = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();

                    if (confirmation == "YES")
                    {
                        proxy.RemoveAssignmentFromCourse(course.Id, targetId);
                        Console.WriteLine("\nSuccess! The assignment has been permanently deleted.");
                    }
                    else
                    {
                        Console.WriteLine("\nDeletion cancelled.");
                    }
                }
                else
                {
                    Console.WriteLine($"\nError: Assignment with ID {targetId} not found.");
                }
            }
            else
            {
                Console.WriteLine("\nInvalid ID selection format.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        //methodfor grading student submissions in courses
        private static void GradeSubmissionsForm(Course course)
        {
            Console.Clear();
            Console.WriteLine($"--- Grade Submissions ({course.Code}) ---");

            var gradedAssignments = course.Assignments.Where(a => a.Submissions.Count > 0).ToList();

            if (gradedAssignments.Count == 0)
            {
                Console.WriteLine("No submissions exist in any assignments for this course.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Assignments with submissions:");
            foreach (var assign in gradedAssignments)
            {
                int ungradedCount = assign.Submissions.Count(s => !s.IsGraded);
                Console.WriteLine($"  [ID: {assign.Id}] {assign.Name} ({ungradedCount} ungraded submissions)");
            }
            Console.WriteLine("--------------------------------------");
            Console.Write("Enter the ID of the assignment to grade: ");

            if (int.TryParse(Console.ReadLine(), out int assignId))
            {
                var assignment = course.Assignments.FirstOrDefault(a => a.Id == assignId);
                if (assignment != null && assignment.Submissions.Count > 0)
                {
                    Console.Clear();
                    Console.WriteLine($"--- Submissions for: {assignment.Name} ---");
                    
                    foreach (var sub in assignment.Submissions)
                    {
                        string gradeText = sub.IsGraded ? $"{sub.Grade}/{assignment.TotalPoints}" : "UNGRADED";
                        Console.WriteLine($"[Submission ID: {sub.Id}] Student: {sub.StudentName} (ID: {sub.StudentId})");
                        Console.WriteLine($"  Submission Content: \"{sub.Content}\"");
                        Console.WriteLine($"  Current Grade: {gradeText}");
                        Console.WriteLine("--------------------------------------");
                    }

                    Console.Write("Enter the Submission ID to grade: ");
                    if (int.TryParse(Console.ReadLine(), out int subId))
                    {
                        var targetSub = assignment.Submissions.FirstOrDefault(s => s.Id == subId);
                        if (targetSub != null)
                        {
                            Console.Write($"Enter grade for {targetSub.StudentName} (Max {assignment.TotalPoints}): ");
                            if (double.TryParse(Console.ReadLine(), out double grade) && grade >= 0 && grade <= assignment.TotalPoints)
                            {
                                targetSub.Grade = grade;
                                Console.WriteLine($"\nSuccess! Assigned grade {grade}/{assignment.TotalPoints} to {targetSub.StudentName}.");
                            }
                            else
                            {
                                Console.WriteLine("\nInvalid grade format or point values out of bounds.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nSubmission ID not found.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\nAssignment ID not found or has no active submissions.");
                }
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }










































    }
}