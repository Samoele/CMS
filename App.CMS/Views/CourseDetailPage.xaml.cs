using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Library.CMS.Models; 

namespace App.CMS.Views
{
    public partial class CourseDetailPage : ContentPage
    {
        private readonly Course _selectedCourse;
        private readonly Student _selectedStudent;

        public CourseDetailPage(Course course, Student student)
        {
            InitializeComponent();
            _selectedCourse = course;
            _selectedStudent = student;

            PopulateCourseDetails();
        }

        private void PopulateCourseDetails()
        {
            //header info
            CourseNameLabel.Text = _selectedCourse.Name;
            CourseCodeLabel.Text = $"Course Code: {_selectedCourse.Code}";
            CourseDescriptionLabel.Text = _selectedCourse.Description ?? "No description provided.";

            //binds lists
            AssignmentsListView.ItemsSource = _selectedCourse.Assignments;
            ModulesListView.ItemsSource = _selectedCourse.Modules;

            //top right grade calculation display
            CalculateAndDisplayGrade();


        }

        //grade calculation based on percentage
        private void CalculateAndDisplayGrade() //temporary grade calculation
        {
            // Example grade evaluation logic
            double totalEarnedPoints = 94.5; // Calculated from student submissions
            double totalPossiblePoints = 100.0;
            double percentage = totalPossiblePoints > 0 ? (totalEarnedPoints / totalPossiblePoints) * 100 : 100.0;

            string letterGrade = GetLetterGrade(percentage);

            // Updates right header display
            LetterGradeLabel.Text = letterGrade;
            NumericGradeLabel.Text = $"{percentage:F1}%";

            // Update Tab Text
            DetailedGradeSummaryLabel.Text = $"{_selectedStudent.Name} currently holds an overall grade of {percentage:F1}% ({letterGrade}) in {_selectedCourse.Code}.";
        }

        private string GetLetterGrade(double percentage)
        {
            if (percentage >= 93) return "A";
            if (percentage >= 90) return "A-";
            if (percentage >= 87) return "B+";
            if (percentage >= 83) return "B";
            if (percentage >= 80) return "B-";
            if (percentage >= 77) return "C+";
            if (percentage >= 73) return "C";
            if (percentage >= 70) return "C-";
            if (percentage >= 60) return "D";
            return "F";
        }

        //tab switch handlers
        private void OnAssignmentsTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(assignments: true, modules: false, grades: false);
            HighlightButton(BtnAssignments, BtnModules, BtnGrades);
        }

        private void OnModulesTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(assignments: false, modules: true, grades: false);
            HighlightButton(BtnModules, BtnAssignments, BtnGrades);
        }

        private void OnGradesTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(assignments: false, modules: false, grades: true);
            HighlightButton(BtnGrades, BtnAssignments, BtnModules);
        }

        private void SetTabVisibility(bool assignments, bool modules, bool grades)
        {
            AssignmentsSection.IsVisible = assignments;
            ModulesSection.IsVisible = modules;
            GradesSection.IsVisible = grades;
        }

        private void HighlightButton(Button active, Button inactive1, Button inactive2)
        {
            active.BackgroundColor = Color.FromArgb("#2563EB");
            inactive1.BackgroundColor = Color.FromArgb("#475569");
            inactive2.BackgroundColor = Color.FromArgb("#475569");
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private Assignment _currentActiveAssignment;
        private void OnOpenSubmissionFormClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Assignment selectedAssignment)
            {
                _currentActiveAssignment = selectedAssignment;
                SelectedAssignmentLabel.Text = $"Submitting: {selectedAssignment.Name}";
                StudentResponseEditor.Text = string.Empty; // Clear previous text
                SubmissionCard.IsVisible = true;
            }
        }

        private async void OnSubmitResponseClicked(object sender, EventArgs e)
        {
            string responseText = StudentResponseEditor.Text?.Trim();

            if (string.IsNullOrWhiteSpace(responseText))
            {
                await DisplayAlert("Validation Error", "Please enter a response before submitting.", "OK");
                return;
            }

            if (_currentActiveAssignment != null)
            {
                // Attach response to model or handle submission logic in your backend
                // _currentActiveAssignment.Submission = responseText;

                await DisplayAlert("Success", $"Response submitted for '{_currentActiveAssignment.Name}'!", "OK");
                
                // Reset form
                SubmissionCard.IsVisible = false;
                StudentResponseEditor.Text = string.Empty;
                _currentActiveAssignment = null;
            }
        }

        private void OnCancelSubmissionClicked(object sender, EventArgs e)
        {
            SubmissionCard.IsVisible = false;
            StudentResponseEditor.Text = string.Empty;
            _currentActiveAssignment = null;
        }




    }
}