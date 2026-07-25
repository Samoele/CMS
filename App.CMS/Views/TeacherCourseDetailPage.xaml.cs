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

        // Tab Switching Handlers
        private void OnRosterTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: true, assignments: false, modules: false);
            HighlightButton(BtnRoster, BtnAssignments, BtnModules);
        }

        private void OnAssignmentsTabClicked(object sender, EventArgs e)
        {
            SetTabVisibility(roster: false, assignments: true, modules: false);
            HighlightButton(BtnAssignments, BtnRoster, BtnModules);
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
    }
}