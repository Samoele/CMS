using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Library.CMS.Models;

namespace App.CMS.Views
{
    public partial class AssignmentSubmissionsPage : ContentPage
    {
        private readonly Assignment _assignment;

        public AssignmentSubmissionsPage(Assignment assignment)
        {
            InitializeComponent();
            _assignment = assignment;

            AssignmentTitleLabel.Text = $"📝 Submissions for: {_assignment.Name}";
            AssignmentMetaLabel.Text = $"Total Points: {_assignment.TotalPoints} | Due: {_assignment.DueDate:MM/dd/yyyy}";

            RefreshSubmissionsList();
        }

        private void RefreshSubmissionsList()
        {
            _assignment.Submissions ??= new List<Submission>();

            SubmissionsCollectionView.ItemsSource = null;
            SubmissionsCollectionView.ItemsSource = _assignment.Submissions;
        }

        private async void OnGradeSubmissionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Submission submission)
            {
                //Prompt teacher for a grade
                string currentGradeStr = submission.Grade?.ToString() ?? string.Empty;
                string gradeResult = await DisplayPromptAsync(
                    "Grade Submission",
                    $"Enter grade for {submission.StudentName} (Max: {_assignment.TotalPoints}):",
                    initialValue: currentGradeStr,
                    keyboard: Keyboard.Numeric);

                if (gradeResult == null) return; // Cancelled

                if (!double.TryParse(gradeResult, out double numericGrade) || numericGrade < 0 || numericGrade > _assignment.TotalPoints)
                {
                    await DisplayAlert("Invalid Grade", $"Please enter a number between 0 and {_assignment.TotalPoints}.", "OK");
                    return;
                }

                //prompt Teacher for a submission comment
                string commentResult = await DisplayPromptAsync(
                    "Teacher Feedback",
                    $"Leave feedback for {submission.StudentName}:",
                    initialValue: submission.Comment ?? string.Empty);

                //Update model properties
                submission.Grade = numericGrade;
                submission.Comment = commentResult?.Trim();

                await DisplayAlert("Saved", $"Grade and feedback saved for {submission.StudentName}.", "OK");
                RefreshSubmissionsList();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    }
}