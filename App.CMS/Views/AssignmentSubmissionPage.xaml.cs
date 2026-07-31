using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Library.CMS.Models;
using Library.CMS.Services;
using System.Collections.ObjectModel;


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
            if (_assignment != null)
            {
                // live assg from SiteServiceProxy
                var liveAssignment = SiteServiceProxy.Current.Courses
                    .SelectMany(c => c.Assignments)
                    .FirstOrDefault(a => a.Id == _assignment.Id);

                var submissions = liveAssignment?.Submissions ?? _assignment.Submissions ?? new List<Submission>();

                // filter duplicates for only one submission per student
                var uniqueSubmissions = submissions
                    .GroupBy(s => s.StudentId)
                    .Select(g => g.First())
                    .ToList();

                
                SubmissionsCollectionView.ItemsSource = null;
                SubmissionsCollectionView.ItemsSource = new ObservableCollection<Submission>(uniqueSubmissions);
            }
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

                //Updated model properties
                submission.Grade = numericGrade;
                submission.Comment = commentResult?.Trim();

                await DisplayAlert("Saved", $"Grade and feedback saved for {submission.StudentName}.", "OK");
                RefreshSubmissionsList();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

        private async void OnOpenAttachedFileClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Submission submission)
            {
                if (!submission.HasFile || submission.FileData == null || submission.FileData.Length == 0)
                {
                    await DisplayAlert("File Error", "No file turned in with this submission.", "OK");
                    return;
                }

                try
                {
                    string tempFolder = FileSystem.CacheDirectory;
                    string tempFilePath = Path.Combine(tempFolder, submission.FileName!);

                    await File.WriteAllBytesAsync(tempFilePath, submission.FileData);

                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(tempFilePath)
                    });
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error Opening File", $"Could not open file: {ex.Message}", "OK");
                }
            }
        }







    }
}