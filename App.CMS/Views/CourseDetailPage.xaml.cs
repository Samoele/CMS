using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Library.CMS.Models; 
using Library.CMS.Services;

namespace App.CMS.Views
{
    public partial class CourseDetailPage : ContentPage
    {
        private readonly Course _selectedCourse;
        private readonly Student _selectedStudent;
        private Assignment _currentActiveAssignment;
        private string? _selectedFileName;
        private byte[]? _selectedFileData;


        public CourseDetailPage(Course course, Student student)
        {
            InitializeComponent();
            _selectedCourse = course;
            _selectedStudent = student;

            PopulateCourseDetails();
        }

        //method for updated announcements
        protected override void OnAppearing()
        {
            base.OnAppearing();

            //Fetch the latest version of the course from the  SiteserviceProxy
            var updatedCourse = SiteServiceProxy.Current?.GetCourses().FirstOrDefault(c => c.Id == _selectedCourse.Id);

            if (updatedCourse != null)
            {
                //synchronize announcements list
                _selectedCourse.Announcements = updatedCourse.Announcements;
            }

            
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

            //binds and refreshes Announcements //changed for announcements to update
            var sortedAnnouncements = (_selectedCourse.Announcements ?? new List<Announcement>())
                .OrderByDescending(a => a.DatePosted).ToList();

            StudentAnnouncementsCollectionView.ItemsSource = null;
            StudentAnnouncementsCollectionView.ItemsSource = sortedAnnouncements;

            // Show card only if an announcement exist
            StudentAnnouncementsCard.IsVisible = sortedAnnouncements.Any();


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

        //need to check later as I have duplicate methods (hella inneficient and looks unprofessional to be honest)
        //same method as in Teacherview, will ignore for now and make better structure later
        //
         private void HighlightButton(Button active, params Button[] inactives) 
        {
            active.BackgroundColor = Color.FromArgb("#2563EB");

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

        private async void OnSubmitAssignmentClicked(object sender, EventArgs e)
        {
            string responseText = StudentResponseEditor.Text?.Trim() ?? string.Empty;

            // Validate that either text or a file is provided
            if (string.IsNullOrWhiteSpace(responseText) && string.IsNullOrWhiteSpace(_selectedFileName))
            {
                await DisplayAlert("Validation Error", "Please enter a response or attach a file before submitting.", "OK");
                return;
            }

            if (_currentActiveAssignment == null)
            {
                await DisplayAlert("Error", "No assignment selected.", "OK");
                return;
            }

            var currentStudent = _selectedStudent ?? (SiteServiceProxy.Current?.CurrentUser as Student);

            if (currentStudent == null)
            {
                await DisplayAlert("Error", "No active student logged in. Please log in first.", "OK");
                return;
            }

            _currentActiveAssignment.Submissions ??= new List<Submission>();

            bool alreadySubmitted = _currentActiveAssignment.Submissions
                .Any(s => s.StudentId == currentStudent.Id);

            if (alreadySubmitted)
            {
                await DisplayAlert("Already Submitted", 
                    $"You have already submitted a response for '{_currentActiveAssignment.Name}'. You can only submit once.", 
                    "OK");
                return;
            }

            int nextId = _currentActiveAssignment.Submissions.Any() == true
                ? _currentActiveAssignment.Submissions.Max(s => s.Id) + 1 : 1;

            var newSubmission = new Submission
            {
                Id = nextId,
                StudentId = currentStudent.Id,
                StudentName = currentStudent.Name,
                Content = responseText,
                FileName = _selectedFileName,
                FileData = _selectedFileData,
                SubmissionDate = DateTime.Now
            };

            _currentActiveAssignment.Submissions.Add(newSubmission);

            await DisplayAlert("Success", $"Response successfully submitted for '{_currentActiveAssignment.Name}'!", "OK");

            var liveCourse = SiteServiceProxy.Current.Courses
                .FirstOrDefault(c => c.Assignments.Any(a => a.Id == _currentActiveAssignment.Id));

            var liveAssignment = liveCourse?.Assignments
                .FirstOrDefault(a => a.Id == _currentActiveAssignment.Id);

            if (liveAssignment != null)
            {
                liveAssignment.Submissions ??= new List<Submission>();
                
                //guard against adding the same student submission twice
                if (!liveAssignment.Submissions.Any(s => s.StudentId == currentStudent.Id))
                {
                    liveAssignment.Submissions.Add(newSubmission);
                }
            }
            

            // Reset form UI & File state
            SubmissionCard.IsVisible = false;
            StudentResponseEditor.Text = string.Empty;
            _selectedFileName = null;
            _selectedFileData = null;
            if (SelectedFileLabel != null)
            {
                SelectedFileLabel.Text = string.Empty;
                SelectedFileLabel.IsVisible = false;
            }
            _currentActiveAssignment = null;
        }

        //cleaner check if already submitted assignment
               private void OnCancelSubmissionClicked(object sender, EventArgs e)
        {
            SubmissionCard.IsVisible = false;
            StudentResponseEditor.Text = string.Empty;
            _currentActiveAssignment = null;
        }


        //method for opening content in a module
        private async void OnOpenModuleContentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ContentItem item)
            {
                if (item is PageItem page)
                {
                    string bodyText = string.IsNullOrWhiteSpace(page.Body) 
                        ? "[This page has no content provided yet.]" 
                        : page.Body;

                    await DisplayAlert($"📄 {page.Name}", bodyText, "Close");
                }
                else if (item is FileItem file)
                {
                    string pathText = string.IsNullOrWhiteSpace(file.FilePath) 
                        ? "[No file path or URL provided.]" 
                        : file.FilePath;

                    await DisplayAlert($"📁 {file.Name}", $"File Path / Resource URL:\n\n{pathText}", "Close");
                }
            }
        }


        private async void OnAttachFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    _selectedFileName = result.FileName;

                    using var stream = await result.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    _selectedFileData = memoryStream.ToArray();

                    SelectedFileLabel.Text = $"📎 Attached: {_selectedFileName}";
                    SelectedFileLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("File Error", $"Failed to pick file: {ex.Message}", "OK");
            }
        }





    }
}