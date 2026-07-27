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










    }
}