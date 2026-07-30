using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using Library.CMS.Models;
using Library.CMS.Services;

namespace App.CMS.Views
{
    public partial class TeacherCourseDetailPage : ContentPage
    {
        private readonly Course _selectedCourse;

        public TeacherCourseDetailPage(Course course)
        {
            InitializeComponent();
            _selectedCourse = course;

            PopulateCourseHeader();
            RefreshRosterView();
        }

        private void PopulateCourseHeader()
        {
            CourseNameLabel.Text = _selectedCourse.Name;
            CourseCodeLabel.Text = $"Course Code: {_selectedCourse.Code}";
            CourseDescriptionLabel.Text = _selectedCourse.Description ?? "No description available.";
        }

        private void RefreshRosterView()
        {
            //get enrolled students
            var enrolled = _selectedCourse?.Roster ?? new List<Student>();
            RosterCollectionView.ItemsSource = null;
            RosterCollectionView.ItemsSource = enrolled;

            //get all university students (e.g., Alice & Bob)
            var allStudents = SiteServiceProxy.Current?.GetStudents() ?? new List<Student>();

            //filter available students not yet enrolled
            var available = allStudents.Where(s => !enrolled.Any(e => e.Id == s.Id)).ToList();
            AvailableStudentsPicker.ItemsSource = available;
        }

        private async void OnEnrollStudentClicked(object sender, EventArgs e)
        {
            if (AvailableStudentsPicker.SelectedItem is Student studentToEnroll)
            {
                SiteServiceProxy.Current.EnrollStudent(_selectedCourse.Id, studentToEnroll.Id);
                await DisplayAlert("Success", $"{studentToEnroll.Name} enrolled into {_selectedCourse.Code}.", "OK");
                RefreshRosterView();
            }
            else
            {
                await DisplayAlert("Selection Required", "Please pick a student to enroll.", "OK");
            }
        }

        private async void OnRemoveStudentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Student studentToRemove)
            {
                bool confirm = await DisplayAlert("Confirm Removal", $"Remove {studentToRemove.Name} from {_selectedCourse.Code}?", "Yes", "No");
                if (confirm)
                {
                    SiteServiceProxy.Current.UnenrollStudent(_selectedCourse.Id, studentToRemove.Id);
                    RefreshRosterView();
                }
            }
        }

        //tab switching handlers
        private void OnRosterTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: true, assignments: false, modules: false, announcements: false, gradebook: false);
            HighlightButton(BtnRoster, BtnAssignments, BtnModules, BtnAnnouncements);
            RefreshRosterView();
        }

        private void OnAssignmentsTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: false, assignments: true, modules: false, announcements: false, gradebook: false);
            HighlightButton(BtnAssignments, BtnRoster, BtnModules);

            //Refresh assignments list when switching tab
            RefreshAssignmentsView();
        }

        private void OnModulesTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: false, assignments: false, modules: true, announcements: false, gradebook: false);
            HighlightButton(BtnModules, BtnRoster, BtnAssignments);

            RefreshModulesView();
        }

        private void SetTabVisibility(bool roster, bool assignments, bool modules, bool announcements, bool gradebook)
        {
            RosterSection.IsVisible = roster;
            AssignmentsSection.IsVisible = assignments;
            ModulesSection.IsVisible = modules;
            AnnouncementsSection.IsVisible = announcements;
            GradebookSection.IsVisible = gradebook;

            if (SettingsSection != null)
            {
                SettingsSection.IsVisible = false;
            }
        }

        private void HighlightButton(Button active, params Button[] inactives) //fix for 4th button without breaking methods
        {
            if (active != null) //NULL CHECK//FIXES settings button error
            {
                active.BackgroundColor = Color.FromArgb("#2563EB");
            }

            foreach (var btn in inactives)
            {
                if (btn != null)
                {
                    btn.BackgroundColor = Color.FromArgb("#475569");
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        //method for CRUD assignments
        private Assignment? _editingAssignment = null;

        private void RefreshAssignmentsView()
        {
            //ensure course assignments list is initialized
            _selectedCourse.Assignments ??= new List<Assignment>();

            AssignmentsCollectionView.ItemsSource = null;
            AssignmentsCollectionView.ItemsSource = _selectedCourse.Assignments;
        }


        //save an assignment action
        private async void OnSaveAssignmentClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AssignmentNameEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please provide a title.", "OK");
                return;
            }

            if (!int.TryParse(TotalPointsEntry.Text, out int TotalPoints) || TotalPoints <= 0)
            {
                await DisplayAlert("Validation Error", "Please enter a valid positive number for Max Points.", "OK");
                return;
            }

            if (_editingAssignment != null) //editing existing assignment or quiz (updated for quizzes)
            {
                _editingAssignment.Name = AssignmentNameEntry.Text.Trim();
                _editingAssignment.IsQuiz = _isCreatingQuiz;
                _editingAssignment.QuizQuestion = _isCreatingQuiz ? (QuizQuestionEditor.Text?.Trim() ?? string.Empty) : string.Empty;
                _editingAssignment.Description = _isCreatingQuiz ? string.Empty : (AssignmentDescriptionEntry.Text?.Trim() ?? string.Empty);
                _editingAssignment.TotalPoints = TotalPoints;
                _editingAssignment.DueDate = DueDateEntry.Date;

                string itemType = _isCreatingQuiz ? "Quiz" : "Assignment";
                await DisplayAlert("Success", $"{itemType} '{_editingAssignment.Name}' updated successfully.", "OK");
            }
            else // new assignment or quiz
            {
                int nextId = _selectedCourse.Assignments.Any() 
                    ? _selectedCourse.Assignments.Max(a => a.Id) + 1 : 1;

                var newAssignment = new Assignment
                {
                    Id = nextId,
                    Name = AssignmentNameEntry.Text.Trim(),
                    IsQuiz = _isCreatingQuiz,
                    QuizQuestion = _isCreatingQuiz ? (QuizQuestionEditor.Text?.Trim() ?? string.Empty) : string.Empty,
                    Description = _isCreatingQuiz ? string.Empty : (AssignmentDescriptionEntry.Text?.Trim() ?? string.Empty),
                    TotalPoints = TotalPoints,
                    DueDate = DueDateEntry.Date,
                    Submissions = new List<Submission>()
                };

                _selectedCourse.Assignments.Add(newAssignment);

                string itemType = _isCreatingQuiz ? "Quiz" : "Assignment";
                await DisplayAlert("Success", $"{itemType} '{newAssignment.Name}' added to course.", "OK");
            }

            ResetAssignmentForm();
            RefreshAssignmentsView();
        }

        //editing of an assignment
        private void OnEditAssignmentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Assignment assignmentToEdit)
            {
                _editingAssignment = assignmentToEdit;
                AssignmentFormHeader.Text = $"Edit Assignment: {assignmentToEdit.Name}";
                AssignmentNameEntry.Text = assignmentToEdit.Name;
                AssignmentDescriptionEntry.Text = assignmentToEdit.Description;
                TotalPointsEntry.Text = assignmentToEdit.TotalPoints.ToString();
                DueDateEntry.Date = assignmentToEdit.DueDate;

                SaveAssignmentBtn.Text = "Update Assignment";
                CancelAssignmentEditBtn.IsVisible = true;
            }
        }

        //deleting an assignments
        private async void OnDeleteAssignmentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Assignment assignmentToDelete)
            {
                //permanent deletion verification
                bool confirm = await DisplayAlert(
                    "⚠️ Confirm Deletion",
                    $"Are you sure you want to delete assignment '{assignmentToDelete.Name}'?\n\nThis will permanently delete ALL student submissions and grades for this assignment.",
                    "Delete",
                    "Cancel");

                if (confirm)
                {
                    //CASCADE OF DELETION: Erases full submissions list and removes the assignment from course
                    assignmentToDelete.Submissions?.Clear();
                    _selectedCourse.Assignments.Remove(assignmentToDelete);

                    await DisplayAlert("Deleted", $"Assignment '{assignmentToDelete.Name}' and its submissions were removed.", "OK");
                    
                    // Reset form if we were currently editing the deleted assignment
                    if (_editingAssignment == assignmentToDelete)
                    {
                        ResetAssignmentForm();
                    }

                    RefreshAssignmentsView();
                }
            }
        }

        private void OnCancelAssignmentEditClicked(object sender, EventArgs e) => ResetAssignmentForm();

        private void ResetAssignmentForm()
        {
            _editingAssignment = null;
            AssignmentFormHeader.Text = "Add New Assignment";
            AssignmentNameEntry.Text = string.Empty;
            AssignmentDescriptionEntry.Text = string.Empty;
            TotalPointsEntry.Text = string.Empty;
            DueDateEntry.Date = DateTime.Today.AddDays(7); // Default due date to next week
            SaveAssignmentBtn.Text = "Save Assignment";
            CancelAssignmentEditBtn.IsVisible = false;
        }

        //View submissions by students
        private async void OnViewSubmissionsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Assignment assignment)
            {
                try
                {
                    await Navigation.PushAsync(new AssignmentSubmissionsPage(assignment));
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Navigation Crash Error", 
                        $"Message: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "OK");
                }
            }
        }

        //Module Management methods
        private Module? _editingModule = null;

        private void RefreshModulesView()
        {
            _selectedCourse.Modules ??= new List<Module>();
            ModulesCollectionView.ItemsSource = null;
            ModulesCollectionView.ItemsSource = _selectedCourse.Modules;
        }

        private async void OnSaveModuleClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ModuleNameEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please provide a module name.", "OK");
                return;
            }

            if (_editingModule != null)
            {
                // Update Module
                _editingModule.Name = ModuleNameEntry.Text.Trim();
                _editingModule.Description = ModuleDescriptionEntry.Text?.Trim() ?? string.Empty;
                await DisplayAlert("Success", $"Module '{_editingModule.Name}' updated successfully.", "OK");
            }
            else
            {
                //adds module with auto increment ID
                int nextId = _selectedCourse.Modules.Any() 
                    ? _selectedCourse.Modules.Max(m => m.Id) + 1 : 1;

                var newModule = new Module
                {
                    Id = nextId,
                    Name = ModuleNameEntry.Text.Trim(),
                    Description = ModuleDescriptionEntry.Text?.Trim() ?? string.Empty,
                    Content = new List<ContentItem>()
                };

                _selectedCourse.Modules.Add(newModule);
                await DisplayAlert("Success", $"Module '{newModule.Name}' created.", "OK");
            }

            ResetModuleForm();
            RefreshModulesView();
        }

        private void OnEditModuleClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Module moduleToEdit)
            {
                _editingModule = moduleToEdit;
                ModuleFormHeader.Text = $"Edit Module: {moduleToEdit.Name}";
                ModuleNameEntry.Text = moduleToEdit.Name;
                ModuleDescriptionEntry.Text = moduleToEdit.Description;

                SaveModuleBtn.Text = "Update Module";
                CancelModuleEditBtn.IsVisible = true;
            }
        }

        private async void OnDeleteModuleClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Module moduleToDelete)
            {
                bool confirm = await DisplayAlert(
                    "⚠️ Confirm Deletion",
                    $"Are you sure you want to delete module '{moduleToDelete.Name}'?\n\nThis will permanently delete all content items within this module.",
                    "Delete Module",
                    "Cancel");

                if (confirm)
                {
                    moduleToDelete.Content?.Clear(); //clears nested content
                    _selectedCourse.Modules.Remove(moduleToDelete);

                    if (_editingModule == moduleToDelete)
                    {
                        ResetModuleForm();
                    }

                    await DisplayAlert("Deleted", $"Module '{moduleToDelete.Name}' removed.", "OK");
                    RefreshModulesView();
                }
            }
        }

        private void OnCancelModuleEditClicked(object sender, EventArgs e) => ResetModuleForm();

        private void ResetModuleForm()
        {
            _editingModule = null;
            ModuleFormHeader.Text = "Add New Module";
            ModuleNameEntry.Text = string.Empty;
            ModuleDescriptionEntry.Text = string.Empty;
            SaveModuleBtn.Text = "Save Module";
            CancelModuleEditBtn.IsVisible = false;
        }

        //Content management inside modules
        private async void OnAddContentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Module targetModule)
            {
                //Let teacher select Content Type betweeen page and file
                string action = await DisplayActionSheet(
                    "Select Content Type", 
                    "Cancel", 
                    null, 
                    "📄 Page (Text / Reading)", 
                    "📁 File (Document / Link Path)");

                if (action == "Cancel" || string.IsNullOrWhiteSpace(action)) return;

                //Prompts for Content Title
                string name = await DisplayPromptAsync("New Content Item", "Enter Item Title/Name:");
                if (string.IsNullOrWhiteSpace(name)) return;

                targetModule.Content ??= new List<ContentItem>();
                int nextId = targetModule.Content.Any() ? targetModule.Content.Max(c => c.Id) + 1 : 1;

                ContentItem newItem;

                if (action.Contains("Page"))
                {
                    // Prompt for Page Body
                    string body = await DisplayPromptAsync("Page Content", "Enter the page body text/content:");
                    
                    newItem = new PageItem
                    {
                        Id = nextId,
                        Name = name.Trim(),
                        Body = body?.Trim() ?? string.Empty
                    };
                }
                else
                {
                    // Prompt for FilePath
                    string filePath = await DisplayPromptAsync("File Details", "Enter file path or URL:");

                    newItem = new FileItem
                    {
                        Id = nextId,
                        Name = name.Trim(),
                        FilePath = filePath?.Trim() ?? string.Empty
                    };
                }

                targetModule.Content.Add(newItem);

                await DisplayAlert("Success", $"Added '{newItem.Name}' to module '{targetModule.Name}'.", "OK");
                RefreshModulesView();
            }
        }
        private async void OnEditContentItemClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ContentItem contentToEdit)
            {
                string newName = await DisplayPromptAsync("Edit Content Item", "Update Name:", initialValue: contentToEdit.Name);
                if (string.IsNullOrWhiteSpace(newName)) return;

                contentToEdit.Name = newName.Trim();

                if (contentToEdit is PageItem page)
                {
                    string newBody = await DisplayPromptAsync("Edit Page Body", "Update Body Text:", initialValue: page.Body);
                    page.Body = newBody?.Trim() ?? string.Empty;
                }
                else if (contentToEdit is FileItem file)
                {
                    string newPath = await DisplayPromptAsync("Edit File Path", "Update File Path / URL:", initialValue: file.FilePath);
                    file.FilePath = newPath?.Trim() ?? string.Empty;
                }

                await DisplayAlert("Updated", $"'{contentToEdit.Name}' was updated successfully.", "OK");
                RefreshModulesView();
            }
        }

        private async void OnDeleteContentItemClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ContentItem contentToDelete)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Remove content item '{contentToDelete.Name}'?", "Delete", "Cancel");
                if (confirm)
                {
                    //Finds parent module and removes an item
                    var parentModule = _selectedCourse.Modules.FirstOrDefault(m => m.Content != null && m.Content.Contains(contentToDelete));
                    parentModule?.Content.Remove(contentToDelete);

                    await DisplayAlert("Deleted", $"'{contentToDelete.Name}' removed.", "OK");
                    RefreshModulesView();
                }
            }
        }

        //View content latest button added to verify material in module items
        private async void OnViewContentItemClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ContentItem item)
            {
                if (item is PageItem page)
                {
                    string contentBody = string.IsNullOrWhiteSpace(page.Body) 
                        ? "[No body text provided for this page]" 
                        : page.Body;

                    await DisplayAlert($"📄 Page: {page.Name}", contentBody, "Close");
                }
                else if (item is FileItem file)
                {
                    string pathText = string.IsNullOrWhiteSpace(file.FilePath) 
                        ? "[No file path or URL provided]" 
                        : file.FilePath;

                    await DisplayAlert($"📁 File: {file.Name}", $"File Location / URL:\n\n{pathText}", "Close");
                }
            }
        }


        //Copying assignment from course to another course
        private async void OnCopyAssignmentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Assignment assignmentToCopy)
            {
                //get all available courses (future to be changed for course assigned to specific professors)
                var allCourses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();
                
                var targetCourses = allCourses
                    .Where(c => c.Id != _selectedCourse.Id).ToList();

                if (!targetCourses.Any())
                {
                    await DisplayAlert("Copy Assignment", "No other courses are available to copy this assignment to.", "OK");
                    return;
                }

                //course options for ActionSheet
                string[] courseOptions = targetCourses
                    .Select(c => $"{c.Code} - {c.Name}").ToArray();

                string selectedChoice = await DisplayActionSheet(
                    $"Copy '{assignmentToCopy.Name}' to Course:",
                    "Cancel",
                    null,
                    courseOptions);

                if (selectedChoice == "Cancel" || string.IsNullOrWhiteSpace(selectedChoice)) return;

                // finds target course
                var selectedTargetCourse = targetCourses.FirstOrDefault(c => $"{c.Code} - {c.Name}" == selectedChoice);

                if (selectedTargetCourse != null)
                {
                    selectedTargetCourse.Assignments ??= new List<Assignment>();

                    // clone assignment with Clone assignment model
                    var clonedAssignment = assignmentToCopy.Clone();

                    //reassign ID so it has unique within the target course's assignments list
                    clonedAssignment.Id = selectedTargetCourse.Assignments.Any()
                        ? selectedTargetCourse.Assignments.Max(a => a.Id) + 1 : 1;

                    selectedTargetCourse.Assignments.Add(clonedAssignment);

                    await DisplayAlert("Success", 
                        $"Assignment '{assignmentToCopy.Name}' was successfully copied to {selectedTargetCourse.Code}!", 
                        "OK");
                }
            }
        }



        //Export and import logic
        private async void OnImportRosterClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Student Roster CSV File"
                });

                if (result == null) return;

                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string csvContent = await reader.ReadToEndAsync();

                //Passes raw text to SiteServiceProxy
                var importSummary = SiteServiceProxy.Current.ImportRosterFromCsv(_selectedCourse.Id, csvContent);

                RefreshRosterView();

                await DisplayAlert("Import Complete", 
                    $"Roster import process finished:\n\n" +
                    $"• Records processed: {importSummary.TotalProcessed}\n" +
                    $"• New students added: {importSummary.AddedCount}\n" +
                    $"• Duplicates skipped: {importSummary.DuplicateCount}", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Import Error", $"Failed to import roster:\n{ex.Message}", "OK");
            }
        }

        private async void OnExportRosterClicked(object sender, EventArgs e)
        {
            try
            {
                //fetch CSV payload directly from Proxy
                string csvContent = SiteServiceProxy.Current.ExportRosterToCsv(_selectedCourse.Id);

                if (string.IsNullOrEmpty(csvContent))
                {
                    await DisplayAlert("Export Warning", "This course currently has no enrolled students to export.", "OK");
                    return;
                }

                string fileName = $"{_selectedCourse.Code}_Roster_{DateTime.Now:yyyyMMdd}.csv";
                string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

                await File.WriteAllTextAsync(filePath, csvContent);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Export Roster for {_selectedCourse.Code}",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Export Error", $"Failed to export roster:\n{ex.Message}", "OK");
            }
        }

        //announcements section methods

        private void OnAnnouncementsTabClicked(object sender, EventArgs e)
        {
            // Hide other sections, show AnnouncementsSection
            RosterSection.IsVisible = false;
            AssignmentsSection.IsVisible = false;
            ModulesSection.IsVisible = false;
            AnnouncementsSection.IsVisible = true;

            HighlightButton(BtnAnnouncements, BtnRoster, BtnAssignments, BtnModules);
            RefreshAnnouncementsView();
        }


        private void RefreshAnnouncementsView()
        {
            _selectedCourse.Announcements ??= new List<Announcement>();

            AnnouncementsCollectionView.ItemsSource = null;
            AnnouncementsCollectionView.ItemsSource = _selectedCourse.Announcements
                .OrderByDescending(a => a.DatePosted)
                .ToList();
        }

        private async void OnPostAnnouncementClicked(object sender, EventArgs e)
        {
            string title = AnnouncementTitleEntry.Text?.Trim() ?? string.Empty;
            string content = AnnouncementContentEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                await DisplayAlert("Validation Error", "Please provide both a title and content.", "OK");
                return;
            }

            _selectedCourse.Announcements ??= new List<Announcement>();

            int nextId = _selectedCourse.Announcements.Any() 
                ? _selectedCourse.Announcements.Max(a => a.Id) + 1 : 1;

            _selectedCourse.Announcements.Add(new Announcement
            {
                Id = nextId,
                Title = title,
                Content = content,
                DatePosted = DateTime.Now
                
            });
                

            //Update central proxy state so all pages see the change!
            var currentCourses = SiteServiceProxy.Current?.GetCourses();
            var existingCourse = currentCourses?.FirstOrDefault(c => c.Id == _selectedCourse.Id);
            if (existingCourse != null)
            {
                existingCourse.Announcements = _selectedCourse.Announcements;
            }

    

            AnnouncementTitleEntry.Text = string.Empty;
            AnnouncementContentEntry.Text = string.Empty;

            RefreshAnnouncementsView();
            await DisplayAlert("Success", "Announcement posted!", "OK");
        }


        //Import and Export event handlers for Assignments
        //Import event handler
        private async void OnImportAssignmentClicked(object sender, EventArgs e)
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".csv", ".txt" } },
                        { DevicePlatform.MacCatalyst, new[] { "csv", "txt" } },
                        { DevicePlatform.Android, new[] { "text/csv", "text/plain" } },
                        { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } }
                    });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Assignment CSV File",
                    FileTypes = customFileType
                });

                if (result == null) return; //cancelled operation

                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string csvContent = await reader.ReadToEndAsync();

                var importSummary = SiteServiceProxy.Current?.ImportAssignmentCSV(_selectedCourse.Id, csvContent);

                //refresh assignments list
                RefreshAssignmentsView();

                //import summary report
                await DisplayAlert("Import Complete", 
                    $"Assignment import finished:\n\n" +
                    $"• Processed: {importSummary?.TotalProcessed ?? 0}\n" +
                    $"• Added: {importSummary?.AddedCount ?? 0}\n" +
                    $"• Skipped (Duplicates): {importSummary?.DuplicateCount ?? 0}", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Import Error", $"Failed to import assignment:\n{ex.Message}", "OK");
            }
        }


        //Export event handler for assignments teacher view
        private async void OnExportAssignmentClicked(object sender, EventArgs e)
        {
            try
            {
                if (_selectedCourse?.Assignments == null || !_selectedCourse.Assignments.Any())
                {
                    await DisplayAlert("Export Warning", "This course currently has no assignments to export.", "OK");
                    return;
                }

                //asks teacher which assignment to export instead of button on list
                var assignmentNames = _selectedCourse.Assignments.Select(a => a.Name).ToArray();
                string selectedName = await DisplayActionSheet(
                    "Select Assignment to Export", 
                    "Cancel", 
                    null, 
                    assignmentNames);

                if (selectedName == "Cancel" || string.IsNullOrEmpty(selectedName)) return;

                var selectedAssignment = _selectedCourse.Assignments.FirstOrDefault(a => a.Name == selectedName);
                if (selectedAssignment == null) return;

                string csvContent = SiteServiceProxy.Current?.ExportAssignmentCSV(_selectedCourse.Id, selectedAssignment.Id) ?? string.Empty;

                if (string.IsNullOrEmpty(csvContent))
                {
                    await DisplayAlert("Export Error", "Could not generate CSV for the selected assignment.", "OK");
                    return;
                }

                string fileName = $"{_selectedCourse.Code}_{selectedAssignment.Name.Replace(" ", "_")}.csv";
                string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

                await File.WriteAllTextAsync(filePath, csvContent);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Export {selectedAssignment.Name}",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Export Error", $"Failed to export assignment:\n{ex.Message}", "OK");
            }
        }

        //Course Settings button event handlers for grade ranges, weighted percentages and course editing
        private async void OnCourseSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                // hide every other sections
                RosterSection.IsVisible = false;
                AssignmentsSection.IsVisible = false;
                ModulesSection.IsVisible = false;
                AnnouncementsSection.IsVisible = false;

                HighlightButton(null, BtnRoster, BtnAssignments, BtnModules, BtnAnnouncements);

                SettingsSection.IsVisible = true;

                PopulateSettingsData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Settings Error", $"Error opening settings:\n{ex.Message}\n\n{ex.StackTrace}", "OK");
            }
        }

        private void OnCloseSettingsClicked(object sender, EventArgs e)
        {
            // Hide settings
            SettingsSection.IsVisible = false;

            // Return to default section roster//can be changed later
            RosterSection.IsVisible = true;
            HighlightButton(BtnRoster, BtnAssignments, BtnModules, BtnAnnouncements);
        }

        private void PopulateSettingsData()
        {
            if (_selectedCourse == null) return;

            //Loads grade scale
            EntryGradeA.Text = _selectedCourse.GradeScaleA.ToString();
            EntryGradeB.Text = _selectedCourse.GradeScaleB.ToString();
            EntryGradeC.Text = _selectedCourse.GradeScaleC.ToString();
            EntryGradeD.Text = _selectedCourse.GradeScaleD.ToString();

            //Loads assignment weights
            WeightsCollectionView.ItemsSource = null;
            WeightsCollectionView.ItemsSource = _selectedCourse.Assignments;

            //Loads basic course info (name, code, description)
            EditCourseNameEntry.Text = _selectedCourse.Name;
            EditCourseCodeEntry.Text = _selectedCourse.Code;
            EditCourseDescEntry.Text = _selectedCourse.Description;
        }
            
        //event handler for saving grade scale
        private async void OnSaveGradeScaleClicked(object sender, EventArgs e)
        {
            if (double.TryParse(EntryGradeA.Text, out double a) &&
                double.TryParse(EntryGradeB.Text, out double b) &&
                double.TryParse(EntryGradeC.Text, out double c) &&
                double.TryParse(EntryGradeD.Text, out double d))
            {
                _selectedCourse.GradeScaleA = a;
                _selectedCourse.GradeScaleB = b;
                _selectedCourse.GradeScaleC = c;
                _selectedCourse.GradeScaleD = d;

                await DisplayAlert("Success", "Grade thresholds updated!", "OK");
            }
            else
            {
                await DisplayAlert("Validation Error", "Please enter valid numeric percentage values.", "OK");
            }
        }

        private async void OnSaveWeightsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Success", "Assignment weights updated!", "OK");
        }

        private async void OnSaveCourseInfoClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditCourseNameEntry.Text) || string.IsNullOrWhiteSpace(EditCourseCodeEntry.Text))
            {
                await DisplayAlert("Validation Error", "Course title and code cannot be empty.", "OK");
                return;
            }

            _selectedCourse.Name = EditCourseNameEntry.Text.Trim();
            _selectedCourse.Code = EditCourseCodeEntry.Text.Trim();
            _selectedCourse.Description = EditCourseDescEntry.Text?.Trim();

            CourseNameLabel.Text = _selectedCourse.Name;
            CourseCodeLabel.Text = $"Course Code: {_selectedCourse.Code}";

            await DisplayAlert("Success", "Course information updated successfully!", "OK");

            //update with SSProxy so teacher dashboard gets updated info
            var courses = SiteServiceProxy.Current?.GetCourses();
            var courseInService = courses?.FirstOrDefault(c => c.Id == _selectedCourse.Id);
            if (courseInService != null)
            {
                courseInService.Name = _selectedCourse.Name;
                courseInService.Code = _selectedCourse.Code;
                courseInService.Description = _selectedCourse.Description;
            }

            // Update local page labels //does not update and must check to fix
            CourseNameLabel.Text = _selectedCourse.Name;
            CourseCodeLabel.Text = $"Course Code: {_selectedCourse.Code}";

        }

        //export event handlers for gradebook
        private void OnGradebookTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: false, assignments: false, modules: false, announcements: false, gradebook: true);
            HighlightButton(BtnGradebook, BtnRoster, BtnAssignments, BtnModules, BtnAnnouncements);
            RefreshGradebookView();
        }

        //GeminiAI Implementation //Help with dynamic table so all enrolled students and assignments are registered
        private void RefreshGradebookView()
        {
            GradebookGrid.Children.Clear();
            GradebookGrid.RowDefinitions.Clear();
            GradebookGrid.ColumnDefinitions.Clear();

            var students = _selectedCourse.Roster ?? new List<Student>();
            var assignments = _selectedCourse.Assignments ?? new List<Assignment>();

            if (!students.Any())
            {
                GradebookGrid.Children.Add(new Label { Text = "No enrolled students found in roster.", TextColor = Color.FromArgb("#64748B") });
                return;
            }

            // Grid Columns: Student name/ assignments/ overall grade
            GradebookGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Student Column
            foreach (var a in assignments)
            {
                GradebookGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
            GradebookGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Grade display Column

            // build Header Row 
            GradebookGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var headerName = new Label { Text = "Student Name", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") };
            Grid.SetRow(headerName, 0);
            Grid.SetColumn(headerName, 0);
            GradebookGrid.Children.Add(headerName);

            int colIndex = 1;
            foreach (var assignment in assignments)
            {
                var headerAssg = new Label { Text = assignment.Name, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2563EB") };
                Grid.SetRow(headerAssg, 0);
                Grid.SetColumn(headerAssg, colIndex++);
                GradebookGrid.Children.Add(headerAssg);
            }

            var headerOverall = new Label { Text = "Overall Grade", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#059669") };
            Grid.SetRow(headerOverall, 0);
            Grid.SetColumn(headerOverall, colIndex);
            GradebookGrid.Children.Add(headerOverall);

            // Build Student Rows
            int rowIndex = 1;
            foreach (var student in students)
            {
                GradebookGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Student Name Label
                var lblName = new Label { Text = student.Name, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#334155"), VerticalOptions = LayoutOptions.Center };
                Grid.SetRow(lblName, rowIndex);
                Grid.SetColumn(lblName, 0);
                GradebookGrid.Children.Add(lblName);

                int studentColIndex = 1;
                double totalEarned = 0;
                double totalPossible = 0;

                foreach (var assignment in assignments)
                {
                    // Retrieve score or NA if graded or not
                    // Fetches grade and assignment to insert on table
                    double score = SiteServiceProxy.Current?.GetStudentScore(_selectedCourse.Id, student.Id, assignment.Id) ?? 0.0;
                    double maxPoints = assignment.TotalPoints > 0 ? assignment.TotalPoints : 100;

                    totalEarned += score;
                    totalPossible += maxPoints;

                    var lblScore = new Label { Text = $"{score}/{maxPoints}", TextColor = Color.FromArgb("#475569"), VerticalOptions = LayoutOptions.Center };
                    Grid.SetRow(lblScore, rowIndex);
                    Grid.SetColumn(lblScore, studentColIndex++);
                    GradebookGrid.Children.Add(lblScore);
                }

                //calculates overall grade
                double overallPct = totalPossible > 0 ? (totalEarned / totalPossible) * 100.0 : 0.0;
                var lblOverall = new Label { Text = $"{overallPct:F1}%", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0D9488"), VerticalOptions = LayoutOptions.Center };
                Grid.SetRow(lblOverall, rowIndex);
                Grid.SetColumn(lblOverall, studentColIndex);
                GradebookGrid.Children.Add(lblOverall);

                rowIndex++;
            }
        }

        //exports gradebook to CSV
        private async void OnExportGradebookClicked(object sender, EventArgs e)
        {
            try
            {
                var students = _selectedCourse.Roster ?? new List<Student>();
                var assignments = _selectedCourse.Assignments ?? new List<Assignment>();

                if (!students.Any())
                {
                    await DisplayAlert("Export Warning", "There are no students in the roster to export.", "OK");
                    return;
                }

                var csvBuilder = new StringBuilder();

                //CSV Header Row: Student Name, Assignment 1, Assignment 2, ..., Overall Grade
                var headerColumns = new List<string> { "Student Name" };
                headerColumns.AddRange(assignments.Select(a => $"\"{a.Name}\""));
                headerColumns.Add("Overall Grade (%)");
                csvBuilder.AppendLine(string.Join(",", headerColumns));

                //Student Data Rows
                foreach (var student in students)
                {
                    var rowValues = new List<string> { $"\"{student.Name}\"" };
                    double totalEarned = 0;
                    double totalPossible = 0;

                    foreach (var assignment in assignments)
                    {
                        double score = SiteServiceProxy.Current?.GetStudentScore(_selectedCourse.Id, student.Id, assignment.Id) ?? 0.0;
                        double maxPoints = assignment.TotalPoints > 0 ? assignment.TotalPoints : 100;

                        totalEarned += score;
                        totalPossible += maxPoints;

                        rowValues.Add(score.ToString());
                    }

                    double overallPct = totalPossible > 0 ? totalEarned / totalPossible * 100.0 : 0.0;
                    rowValues.Add($"{overallPct:F1}");

                    csvBuilder.AppendLine(string.Join(",", rowValues));
                }

                //saves and shares file
                string fileName = $"{_selectedCourse.Code}_Gradebook.csv";
                string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

                await File.WriteAllTextAsync(filePath, csvBuilder.ToString());

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Export {_selectedCourse.Code} Gradebook",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Export Error", $"Failed to export gradebook:\n{ex.Message}", "OK");
            }
        }

        //Quiz in assignment event handlers
        private bool _isCreatingQuiz = false;
        private void OnSelectStandardTypeClicked(object sender, EventArgs e)
        {
            _isCreatingQuiz = false;
            BtnSelectStandard.BackgroundColor = Color.FromArgb("#2563EB");
            BtnSelectStandard.TextColor = Colors.White;

            BtnSelectQuiz.BackgroundColor = Color.FromArgb("#E2E8F0");
            BtnSelectQuiz.TextColor = Color.FromArgb("#334155");

            AssignmentDescriptionEntry.IsVisible = true;
            QuizQuestionEditor.IsVisible = false;
        }

        private void OnSelectQuizTypeClicked(object sender, EventArgs e)
        {
            _isCreatingQuiz = true;
            BtnSelectQuiz.BackgroundColor = Color.FromArgb("#2563EB");
            BtnSelectQuiz.TextColor = Colors.White;

            BtnSelectStandard.BackgroundColor = Color.FromArgb("#E2E8F0");
            BtnSelectStandard.TextColor = Color.FromArgb("#334155");

            AssignmentDescriptionEntry.IsVisible = false;
            QuizQuestionEditor.IsVisible = true;
        }

        











    }
}