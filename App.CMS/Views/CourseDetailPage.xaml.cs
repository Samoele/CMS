using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Library.CMS.Models; 
using Library.CMS.Services;

namespace App.CMS.Views
{
    public partial class CourseDetailPage : ContentPage
    {
        private Course _selectedCourse;
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
            RefreshStudentGradeSummary();
        }

        //method for updated announcements
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // update course data (assignments, grades, announcements) from Mdb
            await SiteServiceProxy.Current.RefreshCoursesFromDatabaseAsync();

            // get current course
            if (_selectedCourse != null)
            {
                _selectedCourse = SiteServiceProxy.Current.GetCourseById(_selectedCourse.Id);

                if (_selectedCourse != null && SiteServiceProxy.Current.CurrentUser is Student currentStudent)
                {
                    
                    AssignmentsListView.ItemsSource = null;
                    AssignmentsListView.ItemsSource = _selectedCourse.Assignments;

                    // calculate overall grade
                    double percentage = SiteServiceProxy.Current.CalculateStudentOverallGrade(_selectedCourse.Id, currentStudent.Id);
                    string letterGrade = SiteServiceProxy.Current.GetStudentLetterGrade(_selectedCourse.Id, currentStudent.Id);

                    // updatete Grade badge
                    OverallGradeLabel.Text = $"{percentage:F1}% ({letterGrade})";
                }
            }
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


            //binds and refreshes Announcements //changed for announcements to update
            var sortedAnnouncements = (_selectedCourse.Announcements ?? new List<Announcement>())
                .OrderByDescending(a => a.DatePosted).ToList();

            StudentAnnouncementsCollectionView.ItemsSource = null;
            StudentAnnouncementsCollectionView.ItemsSource = sortedAnnouncements;

            // Show card only if an announcement exist
            StudentAnnouncementsCard.IsVisible = sortedAnnouncements.Any();


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

            PopulateStudentGradebookTable();
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
            

            // Reset form and file state
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



        private void RefreshStudentGradeSummary()
        {
            if (_selectedCourse == null || _selectedStudent == null) return;

            // get overall numerical percentage and letter grade from proxy
            double overallPercentage = SiteServiceProxy.Current.CalculateStudentOverallGrade(_selectedCourse.Id, _selectedStudent.Id);
            string letterGrade = SiteServiceProxy.Current.GetStudentLetterGrade(_selectedCourse.Id, _selectedStudent.Id);

            AssignmentsListView.ItemsSource = null;
            AssignmentsListView.ItemsSource = _selectedCourse.Assignments;
            // update student view
            if (LetterGradeLabel != null)
            {
                LetterGradeLabel.Text = $"{overallPercentage:F1}% ({letterGrade})";
            }
        }

        private void PopulateStudentGradebookTable()
        {
            if (_selectedCourse == null) return;

            var currentStudent = _selectedStudent ?? (SiteServiceProxy.Current?.CurrentUser as Student);
            if (currentStudent == null) return;

            // current course reference
            var liveCourse = SiteServiceProxy.Current.GetCourseById(_selectedCourse.Id);
            if (liveCourse == null) return;

            // compute overall grade and letter grade
            double overallPercentage = SiteServiceProxy.Current.CalculateStudentOverallGrade(liveCourse.Id, currentStudent.Id);
            string overallLetter = SiteServiceProxy.Current.GetStudentLetterGrade(liveCourse.Id, currentStudent.Id);

            //Update for the top right letter badge
            if (LetterGradeLabel != null)
            {
                LetterGradeLabel.Text = overallLetter;
            }
            if (NumericGradeLabel != null)
            {
                NumericGradeLabel.Text = $"{overallPercentage:F1}%";
            }

            
            // Update badge and scale breakdown card
            OverallGradeLabel.Text = $"{overallPercentage:F1}% ({overallLetter})";
            GradingScaleBreakdownLabel.Text = $"Scale: A ≥ {liveCourse.GradeScaleA}% | B ≥ {liveCourse.GradeScaleB}% | C ≥ {liveCourse.GradeScaleC}% | D ≥ {liveCourse.GradeScaleD}%";
                        // clears existing table rows
            StudentGradebookRowsContainer.Children.Clear();

            var assignments = liveCourse.Assignments ?? new List<Assignment>();

            if (!assignments.Any())
            {
                StudentGradebookRowsContainer.Children.Add(new Label 
                { 
                    Text = "No assignments found for this course.", 
                    FontSize = 13, 
                    TextColor = Microsoft.Maui.Graphics.Colors.Gray,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                return;
            }

            // build a row for every assignment in the course
            foreach (var assignment in assignments)
            {
                var submission = assignment.Submissions?.FirstOrDefault(s => s.StudentId == currentStudent.Id);

                string scoreText = "--";
                string pctText = "--";
                string letterText = "Ungraded";
                Color letterColor = Microsoft.Maui.Graphics.Colors.Gray;

                if (submission != null && submission.IsGraded && submission.Grade.HasValue)
                {
                    double maxPts = assignment.TotalPoints > 0 ? assignment.TotalPoints : 100.0;
                    double earnedPts = submission.Grade.Value;
                    double pct = (earnedPts / maxPts) * 100.0;

                    scoreText = $"{earnedPts}/{maxPts}";
                    pctText = $"{pct:F1}%";
                    
                    //map percentage to scale setting from liveCourse
                    letterText = SiteServiceProxy.Current.GetLetterGradeForPercentage(liveCourse.Id, pct);
                    letterColor = Microsoft.Maui.Graphics.Color.FromArgb("#16A34A"); // Green for graded
                }

                // table rows layout
                var rowBorder = new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                    Padding = new Thickness(10),
                    BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                    Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#E2E8F0")
                };

                var rowGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = new GridLength(80) },
                        new ColumnDefinition { Width = new GridLength(80) },
                        new ColumnDefinition { Width = new GridLength(80) }
                    }
                };

                var nameLabel = new Label { Text = assignment.Name, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#0F172A"), VerticalOptions = LayoutOptions.Center };
                var scoreLbl = new Label { Text = scoreText, FontSize = 13, TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#334155"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                var pctLbl = new Label { Text = pctText, FontSize = 13, TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#334155"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                var letterLbl = new Label { Text = letterText, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = letterColor, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

                Grid.SetColumn(nameLabel, 0);
                Grid.SetColumn(scoreLbl, 1);
                Grid.SetColumn(pctLbl, 2);
                Grid.SetColumn(letterLbl, 3);

                rowGrid.Children.Add(nameLabel);
                rowGrid.Children.Add(scoreLbl);
                rowGrid.Children.Add(pctLbl);
                rowGrid.Children.Add(letterLbl);

                rowBorder.Content = rowGrid;
                StudentGradebookRowsContainer.Children.Add(rowBorder);
            }
        }





    }
}