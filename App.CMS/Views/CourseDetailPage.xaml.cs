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

            //simple grade calculation display
            GradeSummaryLabel.Text = $"Current Evaluation for {_selectedStudent.Name}: Course in Good Standing";
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
    }
}