using System;
using System.Collections.Generic;
using System.Linq;
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
            SetTabVisibility(roster: true, assignments: false, modules: false);
            HighlightButton(BtnRoster, BtnAssignments, BtnModules);
        }

        private void OnAssignmentsTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: false, assignments: true, modules: false);
            HighlightButton(BtnAssignments, BtnRoster, BtnModules);

            //Refresh assignments list when switching tab
            RefreshAssignmentsView();
        }

        private void OnModulesTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: false, assignments: false, modules: true);
            HighlightButton(BtnModules, BtnRoster, BtnAssignments);

            RefreshModulesView();
        }

        private void SetTabVisibility(bool roster, bool assignments, bool modules)
        {
            RosterSection.IsVisible = roster;
            AssignmentsSection.IsVisible = assignments;
            ModulesSection.IsVisible = modules;
        }

        private void HighlightButton(Button active, Button inactive1, Button inactive2)
        {
            active.BackgroundColor = Color.FromArgb("#0F172A");
            inactive1.BackgroundColor = Color.FromArgb("#475569");
            inactive2.BackgroundColor = Color.FromArgb("#475569");
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
                await DisplayAlert("Validation Error", "Please provide an assignment title.", "OK");
                return;
            }

            if (!int.TryParse(TotalPointsEntry.Text, out int TotalPoints) || TotalPoints <= 0)
            {
                await DisplayAlert("Validation Error", "Please enter a valid positive number for Max Points.", "OK");
                return;
            }

            if (_editingAssignment != null) //if assg exist then edits
            {
                //updates an existing assignment
                _editingAssignment.Name = AssignmentNameEntry.Text.Trim();
                _editingAssignment.Description = AssignmentDescriptionEntry.Text?.Trim();
                _editingAssignment.TotalPoints = TotalPoints;
                _editingAssignment.DueDate = DueDateEntry.Date;

                await DisplayAlert("Success", $"Assignment '{_editingAssignment.Name}' updated successfully.", "OK");
            }
            else
            {
                //create a new assignment and assign a new ID
                int nextId = _selectedCourse.Assignments.Any() 
                    ? _selectedCourse.Assignments.Max(a => a.Id) + 1 : 1;

                var newAssignment = new Assignment
                {
                    Id = nextId,
                    Name = AssignmentNameEntry.Text.Trim(),
                    Description = AssignmentDescriptionEntry.Text?.Trim(),
                    TotalPoints = TotalPoints,
                    DueDate = DueDateEntry.Date,
                    Submissions = new List<Submission>() //initializes submissions container
                };

                _selectedCourse.Assignments.Add(newAssignment);
                await DisplayAlert("Success", $"Assignment '{newAssignment.Name}' added to course.", "OK");
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
            if (sender is Button button && button.BindingContext is Assignment assignment)
            {
                await Navigation.PushAsync(new AssignmentSubmissionsPage(assignment));
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










    }
}