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
                Console.WriteLine("2. Copy a Course");
                Console.WriteLine("3. Select an Existing Course");
                Console.WriteLine("4. See Courses by Semester");
                Console.WriteLine("5. Return to Main Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-4): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Logic to add a new course
                        CreateNewCourseForm(proxy);
                        break;
                    case "2":
                        CopyCourseForm(proxy);
                        break;
                    case "3":
                        // Logic to select an existing course
                        SelectExistingCourseForm(proxy);
                        break;
                    case "4":
                    //Logic to filter courses by semester
                        FilterCoursesForm(proxy);
                        break;
                    case "5": //exit the menu
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
        Console.Write("Enter Course Section (e.g., 001): ");
    string section = (Console.ReadLine() ?? string.Empty).Trim();

        //Require input for a semester
        string semester = string.Empty;
            while (string.IsNullOrWhiteSpace(semester))
            {
                Console.Write("Enter Semester/Term (e.g Fall 2026): ");
                semester = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(semester))
                {
                    Console.WriteLine("Error. Semester is required to register a course. Please try again.");

                }
            }
        
            //checks that code, name and description are not blank
        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(section))
        {
            var newCourse = new Course
            {
                Code = code, 
                Name = name,
                Description = description,
                Semester = semester,
                Section = section
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

            // Print out all available courses with their IDs
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
                Console.WriteLine("7. Create Assignment Group");
                Console.WriteLine("8. Return to Main Teacher Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-8): ");

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
                        //runs assg menu
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
                        //runs assignment groups menu
                        RunAssignmentGroupsMenu(course, proxy);
                        break;
                    case "8":
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
            
            //assign module name
            Console.Write("Enter Module Name: ");
            string moduleName = Console.ReadLine() ?? "Untitled Module";
            if (string.IsNullOrWhiteSpace(moduleName)) moduleName = "Untitled Module";

            //use initial input as a description property on the Module itself
            Console.Write("Enter module description: ");
            string initialContent = Console.ReadLine() ?? string.Empty;

            var newModule = new Module
            {
                Name = moduleName,
                Description = initialContent
            };

            
            if (!string.IsNullOrWhiteSpace(initialContent))
            {
                newModule.Content.Add(new PageItem
                {
                    Id = 1,
                    Name = "Module Overview",
                    Body = initialContent
                });
            }

            // call proxy to add the module to the course
            proxy.AddModuleToCourse(course.Id, newModule);

            Console.WriteLine($"\nSuccess! Module '{moduleName}' added.");
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
                            Console.WriteLine($"   - [{contentItem.GetType().Name.Replace("Item", "").ToUpper()}] {contentItem.Name}");
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
                        //check if modules exist first
                        if (course.Modules.Count == 0)
                        {
                            Console.WriteLine("\nNo modules exist yet to add content to! Create a module first.");
                            Console.ReadKey();
                            break;
                        }

                        //ask user which module they want to manage
                        Console.Write("Enter the Module ID you want to add content to: ");
                        if (int.TryParse(Console.ReadLine(), out int targetModuleId))
                        {
                            //get Module instance
                            var selectedModule = course.Modules.FirstOrDefault(m => m.Id == targetModuleId);

                            if (selectedModule != null)
                            {
                                //pass specific content to module
                                AddContentToModuleForm(course, selectedModule);
                            }
                            else
                            {
                                Console.WriteLine($"\nModule with ID {targetModuleId} not found.");
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid ID selection format.");
                            Console.ReadKey();
                        }
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

        private static void AddContentToModuleForm(Course course, Module module)
        {
            bool addingContent = true;

            while (addingContent)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  Add Content to Module: {module.Name}");
                Console.WriteLine("======================================");
                Console.WriteLine("1. Add a Page (Text Block)");
                Console.WriteLine("2. Add a File (Link/Path)");
                Console.WriteLine("3. Embed an Existing Assignment");
                Console.WriteLine("4. Finish / Return to Module Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-4): ");

                string choice = (Console.ReadLine() ?? string.Empty).Trim();

                //calculate next ID for the local module content items
                int nextItemId = module.Content.Count > 0 ? module.Content.Max(i => i.Id) + 1 : 1;

                switch (choice)
                {
                    case "1": // Create a Page
                        Console.Clear();
                        Console.WriteLine("--- Create Module Page ---");
                        Console.Write("Enter Page Title: ");
                        string pageTitle = Console.ReadLine() ?? "Untitled Page";
                        Console.Write("Enter Page Content Body:\n");
                        string pageBody = Console.ReadLine() ?? string.Empty;

                        var newPage = new PageItem 
                        { 
                            Id = nextItemId, 
                            Name = pageTitle, 
                            Body = pageBody 
                        };
                        module.Content.Add(newPage);
                        Console.WriteLine("\nSuccess! Page item appended to module.");
                        Console.ReadKey();
                        break;

                    case "2": // creates a file
                        Console.Clear();
                        Console.WriteLine("--- Link Module File ---");
                        Console.Write("Enter Display Name for File: ");
                        string fileName = Console.ReadLine() ?? "Untitled File";
                        Console.Write("Enter File Path (e.g., syllabus.pdf): ");
                        string filePath = Console.ReadLine() ?? string.Empty;

                        var newFile = new FileItem 
                        { 
                            Id = nextItemId, 
                            Name = fileName, 
                            FilePath = filePath 
                        };
                        module.Content.Add(newFile);
                        Console.WriteLine("\nSuccess! File reference appended to module.");
                        Console.ReadKey();
                        break;

                    case "3": // link assignment
                        Console.Clear();
                        Console.WriteLine("--- Link Course Assignment ---");
                        if (course.Assignments.Count == 0)
                        {
                            Console.WriteLine("No assignments exist in this course yet. Create one first!");
                            Console.ReadKey();
                            break;
                        }

                        Console.WriteLine("Available Course Assignments:");
                        foreach (var assign in course.Assignments)
                        {
                            Console.WriteLine($"  [ID: {assign.Id}] {assign.Name}");
                        }
                        Console.WriteLine("--------------------------------------");
                        Console.Write("Enter the Assignment ID to link: ");
                        
                        if (int.TryParse(Console.ReadLine(), out int assignId))
                        {
                            var targetAssign = course.Assignments.FirstOrDefault(a => a.Id == assignId);
                            if (targetAssign != null)
                            {
                                var newAssignItem = new AssignmentItem(targetAssign)
                                {
                                    Id = nextItemId
                                };
                                module.Content.Add(newAssignItem);
                                Console.WriteLine($"\nSuccess! Embedded assignment '{targetAssign.Name}' into module.");
                            }
                            else
                            {
                                Console.WriteLine("\nAssignment ID not found.");
                            }
                        }
                        Console.ReadKey();
                        break;

                    case "4":
                        addingContent = false;
                        break;

                    default:
                        Console.WriteLine("\nInvalid choice. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }


        //Form for modifying content in a module
        private static void ModifyContentInModuleForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Modify Module Content ({course.Code}) ---");

            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules exist in this course.");
                Console.ReadKey();
                return;
            }

            //select module by ID
            Console.Write("Enter the Module ID to modify content within: ");
            if (!int.TryParse(Console.ReadLine(), out int moduleId))
            {
                Console.WriteLine("Invalid ID format.");
                Console.ReadKey();
                return;
            }

            var module = course.Modules.FirstOrDefault(m => m.Id == moduleId);
            if (module == null)
            {
                Console.WriteLine("Module not found.");
                Console.ReadKey();
                return;
            }

            if (module.Content.Count == 0)
            {
                Console.WriteLine("\nThis module has no content items to modify.");
                Console.ReadKey();
                return;
            }

            //show items inside the module
            Console.WriteLine("\nCurrent Content Items:");
            foreach (var item in module.Content)
            {
                string typeLabel = item switch
                {
                    PageItem => "[PAGE]",
                    FileItem => "[FILE]",
                    AssignmentItem => "[ASSIGNMENT]",
                    _ => "[ITEM]"
                };
                Console.WriteLine($"  [Item ID: {item.Id}] {typeLabel} {item.Name}");
            }
            Console.WriteLine("--------------------------------------");

            //select item to modify
            Console.Write("Enter the Item ID to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int itemId))
            {
                Console.WriteLine("Invalid ID format.");
                Console.ReadKey();
                return;
            }

            var targetItem = module.Content.FirstOrDefault(i => i.Id == itemId);
            if (targetItem == null)
            {
                Console.WriteLine("Content item not found.");
                Console.ReadKey();
                return;
            }

            //modify item based on type of item
            Console.Clear();
            Console.WriteLine($"--- Editing Item: {targetItem.Name} ---");
            
            Console.WriteLine($"Current Name: {targetItem.Name}");
            Console.Write("Enter new name (leave blank to keep current): ");
            string newName = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(newName))
            {
                targetItem.Name = newName;
                //if embedded assignment item, then also updates the linked assignment reference
                if (targetItem is AssignmentItem assignItem && assignItem.LinkedAssignment != null)
                {
                    assignItem.LinkedAssignment.Name = newName;
                }
            }

            //change of item if it is a page
            if (targetItem is PageItem page)
            {
                Console.WriteLine($"\nCurrent Body Text: {page.Body}");
                Console.Write("Enter new body text (leave blank to keep current):\n");
                string newBody = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(newBody))
                {
                    page.Body = newBody;
                }
            }
            //change if item is a file
            else if (targetItem is FileItem file)
            {
                Console.WriteLine($"\nCurrent File Path: {file.FilePath}");
                Console.Write("Enter new file path (leave blank to keep current): ");
                string newPath = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    file.FilePath = newPath;
                }
            }//changge if the file is an assignment
            else if (targetItem is AssignmentItem assignmentItem)
            {
                Console.WriteLine("\nNote: This is an embedded link to a course assignment.");
                Console.WriteLine("To edit the assignment, use the main Assignment Management menu.");
            }

            Console.WriteLine("\nSuccess! Content item updated.");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }


        //Method for deleting a module in module menu

        //New method to account for specific types
        private static void RemoveContentFromModuleForm(Course course, SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine($"--- Remove Module Content ({course.Code}) ---");

            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules exist in this course.");
                Console.ReadKey();
                return;
            }

            //select module by ID
            Console.Write("Enter the Module ID to remove content from: ");
            if (!int.TryParse(Console.ReadLine(), out int moduleId))
            {
                Console.WriteLine("Invalid ID format.");
                Console.ReadKey();
                return;
            }

            var module = course.Modules.FirstOrDefault(m => m.Id == moduleId);
            if (module == null)
            {
                Console.WriteLine("Module not found.");
                Console.ReadKey();
                return;
            }

            if (module.Content.Count == 0)
            {
                Console.WriteLine("\nThis module has no content items to remove.");
                Console.ReadKey();
                return;
            }

            //display available items to delete
            Console.WriteLine("\nCurrent Content Items:");
            foreach (var item in module.Content)
            {
                string typeLabel = item switch
                {
                    PageItem => "[PAGE]",
                    FileItem => "[FILE]",
                    AssignmentItem => "[ASSIGNMENT]",
                    _ => "[ITEM]"
                };
                Console.WriteLine($"  [Item ID: {item.Id}] {typeLabel} {item.Name}");
            }
            Console.WriteLine("--------------------------------------");

            //select item by ID to delete
            Console.Write("Enter the Item ID to REMOVE: ");
            if (int.TryParse(Console.ReadLine(), out int itemId))
            {
                var targetItem = module.Content.FirstOrDefault(i => i.Id == itemId);

                if (targetItem != null)
                {
                    //remove the actual object reference from the collection
                    module.Content.Remove(targetItem);
                    Console.WriteLine($"\nSuccess! Removed item '{targetItem.Name}' from module.");
                }
                else
                {
                    Console.WriteLine("\nItem ID not found within this module.");
                }
            }
            else
            {
                Console.WriteLine("\nInvalid ID selection format.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }





        //View modules new format 
        private static void ViewCourseModules(Course course)
        {
            Console.Clear();
            Console.WriteLine($"======================================");
            Console.WriteLine($"        {course.Code} Course Modules   ");
            Console.WriteLine($"======================================");

            if (course.Modules.Count == 0)
            {
                Console.WriteLine("No modules exist for this course yet.");
            }
            else
            {
                foreach (var mod in course.Modules)
                {
                    Console.WriteLine($"\n[Module ID: {mod.Id}] {mod.Name}");
                    Console.WriteLine($"Description: {mod.Description}");
                    Console.WriteLine("  Content Items:");
                    
                    if (mod.Content.Count == 0)
                    {
                        Console.WriteLine("    (No content items inside this module)");
                    }
                    else
                    {
                        foreach (var item in mod.Content)
                        {
                            // Use pattern matching to visually label item types clearly
                            string typeLabel = item switch
                            {
                                PageItem => "[PAGE]",
                                FileItem => "[FILE]",
                                AssignmentItem => "[ASSIGNMENT]",
                                _ => "[ITEM]"
                            };

                            Console.WriteLine($"    - {typeLabel} {item.Name}");
                        }
                    }
                    Console.WriteLine(new string('-', 38));
                }
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
            //fetch all university students from database (future db)
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

        //method for grading student submissions in courses
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


        //Menu for assignment groups inside courses
        //AssignmentGroups
        public static void RunAssignmentGroupsMenu(Course course, SiteServiceProxy proxy)
        {
            bool inGroupMenu = true;

            while (inGroupMenu)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($"  {course.Code} - Assignment Groups");
                Console.WriteLine("======================================");

                if (course.AssignmentGroup.Count == 0)
                {
                    Console.WriteLine("No assignment groups exist yet.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-5} | {"Group Name",-20} | {"Weight",-8} | {"Assignments Count"}");
                    Console.WriteLine(new string('-', 55));
                    foreach (var group in course.AssignmentGroup)
                    {
                        Console.WriteLine($"{group.Id,-5} | {group.Name,-20} | {group.Weight,-8}% | {group.Assignments.Count}");
                    }
                }
                Console.WriteLine(new string('-', 55));
                Console.WriteLine("1. Create New Assignment Group");
                Console.WriteLine("2. Edit Assignment Group");
                Console.WriteLine("3. Delete Assignment Group");
                Console.WriteLine("4. Add an Assignment to a Group");
                Console.WriteLine("5. Return to Course Menu");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-5): ");

                string choice = (Console.ReadLine() ?? string.Empty).Trim();

                switch (choice)
                {
                    case "1":
                        CreateGroupForm(course);
                        break;
                    case "2":
                        EditGroupForm(course);
                        break;
                    case "3":
                        DeleteGroupForm(course);
                        break;
                    case "4":
                        AddAssignmentToGroupForm(course);
                        break;
                    case "5":
                        inGroupMenu = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        //CRUD For assignment groups
        private static void CreateGroupForm(Course course)
        {
            Console.Clear();
            Console.WriteLine("--- Create Assignment Group ---");
            Console.Write("Enter Group Name (e.g., Exams): ");
            string name = Console.ReadLine() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(name)) return;

            Console.Write("Enter Weight Percentage (e.g., 30 for 30%): ");
            if (!double.TryParse(Console.ReadLine(), out double weight) || weight < 0)
            {
                weight = 0;
            }

            int nextId = course.AssignmentGroup.Count > 0 ? course.AssignmentGroup.Max(g => g.Id) + 1 : 1;
            course.AssignmentGroup.Add(new AssignmentGroup { Id = nextId, Name = name, Weight = weight });
            
            Console.WriteLine($"\nSuccess! Group '{name}' created.");
            Console.ReadKey();
        }

        private static void EditGroupForm(Course course)
        {
            Console.Clear();
            Console.WriteLine("--- Edit Assignment Group ---");
            Console.Write("Enter Group ID to edit: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var group = course.AssignmentGroup.FirstOrDefault(g => g.Id == id);
                if (group != null)
                {
                    Console.Write($"Enter new name (leave blank for '{group.Name}'): ");
                    string inputName = Console.ReadLine() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(inputName)) group.Name = inputName;

                    Console.Write($"Enter new weight (leave blank for {group.Weight}%): ");
                    string inputWeight = Console.ReadLine() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(inputWeight) && double.TryParse(inputWeight, out double targetWeight))
                    {
                        group.Weight = targetWeight;
                    }
                    Console.WriteLine("\nGroup updated successfully!");
                }
                else Console.WriteLine("\nGroup not found.");
            }
            Console.ReadKey();
        }

        private static void DeleteGroupForm(Course course)
        {
            Console.Clear();
            Console.WriteLine("--- Delete Assignment Group ---");
            Console.Write("Enter Group ID to remove: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var group = course.AssignmentGroup.FirstOrDefault(g => g.Id == id);
                if (group != null)
                {
                    course.AssignmentGroup.Remove(group);
                    Console.WriteLine("\nGroup permanently removed.");
                }
                else Console.WriteLine("\nGroup not found.");
            }
            Console.ReadKey();
        }
        //CRUD for assignment groups ends

        //Adding an assignment to the group
        private static void AddAssignmentToGroupForm(Course course)
        {
            Console.Clear();
            Console.WriteLine("--- Add Assignment to Group ---");
            
            if (course.AssignmentGroup.Count == 0 || course.Assignments.Count == 0)
            {
                Console.WriteLine("Ensure you have at least one Assignment Group and one Assignment created.");
                Console.ReadKey();
                return;
            }

            //choose group available
            Console.WriteLine("Available Groups:");
            foreach (var g in course.AssignmentGroup) Console.WriteLine($"  [ID: {g.Id}] {g.Name}");
            Console.Write("Enter Group ID: ");
            if (!int.TryParse(Console.ReadLine(), out int groupId)) return;
            var group = course.AssignmentGroup.FirstOrDefault(g => g.Id == groupId);
            if (group == null) return;

            //choose available assignment
            Console.Clear();
            Console.WriteLine($"--- Adding Assignment to Group: {group.Name} ---");
            Console.WriteLine("Available Course Assignments:");
            foreach (var a in course.Assignments) Console.WriteLine($"  [ID: {a.Id}] {a.Name}");
            Console.Write("Enter Assignment ID: ");
            if (!int.TryParse(Console.ReadLine(), out int assignId)) return;
            var assignment = course.Assignments.FirstOrDefault(a => a.Id == assignId);

            if (assignment != null)
            {
                //checks and avoids for duplicate assignments
                if (!group.Assignments.Any(a => a.Id == assignment.Id))
                {
                    group.Assignments.Add(assignment);
                    Console.WriteLine($"\nSuccess! Added assignment '{assignment.Name}' to group '{group.Name}'.");
                }
                else
                {
                    Console.WriteLine("\nThis assignment is already grouped here.");
                }
            }
            else Console.WriteLine("\nAssignment not found.");
            Console.ReadKey();
        }

        //CLI menu for cloning a course
        public static void CopyCourseForm(SiteServiceProxy proxy)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("          Clone / Copy Course         ");
            Console.WriteLine("======================================");

            var courses = proxy.GetCourses();
            if (courses.Count == 0)
            {
                Console.WriteLine("No existing courses available to clone.");
                Console.ReadKey();
                return;
            }

            foreach (var c in courses)
            {
                Console.WriteLine($"  [ID: {c.Id}] {c.Code} - {c.Name}");
            }
            Console.WriteLine("--------------------------------------");
            Console.Write("Enter the ID of the course you want to copy: ");
            
            if (int.TryParse(Console.ReadLine(), out int targetId))
            {
                var targetCourse = proxy.GetCourseById(targetId);
                if (targetCourse != null)
                {
                    Console.Write("Enter NEW Course Code (e.g., COP4813): ");
                    string newCode = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Enter NEW Course Name: ");
                    string newName = Console.ReadLine() ?? string.Empty;
                    

                    if (!string.IsNullOrWhiteSpace(newCode) && !string.IsNullOrWhiteSpace(newName))
                    {
                        proxy.CloneCourse(targetId, newCode, newName);
                        Console.WriteLine($"\nSuccess! Deep Copy of '{targetCourse.Code}' created as '{newCode}'.");
                    }
                    else
                    {
                        Console.WriteLine("\nError: Code and Name inputs cannot be left blank.");
                    }
                }
                else Console.WriteLine("\nCourse not found.");
            }
            else Console.WriteLine("\nInvalid ID selection format.");

            Console.ReadKey();
        }


        //SORTING OF COURSES IN TEACHER VIEW
        public static void FilterCoursesForm(SiteServiceProxy proxy)
        {
            bool viewing = true;
            string? currentFilter = null; //null = "Show All Semesters"

            while (viewing)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine("          Instructor Course Directory ");
                string filterLabel = currentFilter == null ? "All Terms" : $"Filtered by: {currentFilter}";
                Console.WriteLine($"          Current View: {filterLabel}");
                Console.WriteLine("======================================");

                var allCourses = proxy.GetCourses();

                if (allCourses.Count == 0)
                {
                    Console.WriteLine("No courses exist in the system database yet.");
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey();
                    return;
                }

                //apply dinamic filtering
                var filteredCourses = currentFilter == null 
                    ? allCourses 
                    : allCourses.Where(c => c.Semester.Equals(currentFilter, StringComparison.OrdinalIgnoreCase)).ToList();

                if (filteredCourses.Count == 0)
                {
                    Console.WriteLine($"No courses found registered under term '{currentFilter}'.");
                }
                else
                {
                    //orrganize and sort them dynamically by semester using LINQ GroupBy
                    var groupedBySemester = filteredCourses
                        .GroupBy(c => c.Semester)
                        .OrderBy(g => g.Key); //sort them alphabetically

                    foreach (var semesterGroup in groupedBySemester)
                    {
                        Console.WriteLine($"\n🗓️  SEMESTER: {semesterGroup.Key.ToUpper()}");
                        Console.WriteLine(new string('-', 45));
                        
                        foreach (var course in semesterGroup.OrderBy(c => c.Code))
                        {
                            Console.WriteLine($"  [ID: {course.Id,-3}] {course.Code,-8} {course.Section,-4} | {course.Name}");
                        }
                    }
                }

                Console.WriteLine("\n" + new string('=', 38));
                Console.WriteLine("1. Filter by a Specific Semester/Term");
                Console.WriteLine("2. Clear Filter (Show All Semesters)");
                Console.WriteLine("3. Return to Main Dashboard");
                Console.WriteLine("======================================");
                Console.Write("Enter choice (1-3): ");

                string choice = (Console.ReadLine() ?? string.Empty).Trim();

                switch (choice)
                {
                    case "1":
                        //show available terms to help user choose
                        var availableTerms = allCourses.Select(c => c.Semester).Distinct().ToList();
                        Console.Clear();
                        Console.WriteLine("Available Active Terms:");
                        foreach (var term in availableTerms) Console.WriteLine($"  - {term}");
                        Console.WriteLine("--------------------------------------");
                        Console.Write("Enter term name exactly to filter: ");
                        string targetFilter = (Console.ReadLine() ?? string.Empty).Trim();
                        if (!string.IsNullOrWhiteSpace(targetFilter))
                        {
                            currentFilter = targetFilter;
                        }
                        break;

                    case "2":
                        currentFilter = null; //clear out the filter state
                        Console.WriteLine("\nFilters cleared successfully.");
                        break;

                    case "3":
                        viewing = false;
                        break;
                }
            }
        }












































    }
}