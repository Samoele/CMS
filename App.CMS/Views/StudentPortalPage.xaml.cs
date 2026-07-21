using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Linq;
using Library.CMS.Services; // Ensure this matches your Library namespace
using Library.CMS.Models;   // Ensure this matches your Library namespace

namespace App.CMS.Views
{
    public partial class StudentPortalPage : ContentPage
    {
        // Reference your existing service logic
        private readonly SiteServiceProxy _studentService;

        public StudentPortalPage()
        {
            InitializeComponent();
            _studentService = SiteServiceProxy.Current; //current instance of siteservice proxy
            
            LoadStudents();
        }

        private void LoadStudents()
        {
            // get available students (alice and boib)
            var students = _studentService.GetStudents(); 
            StudentPicker.ItemsSource = students;
        }

        private void OnStudentSelected(object sender, EventArgs e)
        {
            if (StudentPicker.SelectedItem is Student selectedStudent)
            {
                // Update visibility and set courses source
                CoursesHeaderLabel.Text = $"{selectedStudent.Name}'s Enrolled Courses";
                CoursesHeaderLabel.IsVisible = true;

                var enrolledCourses = _studentService.Courses
                .Where(c => c.Roster != null && c.Roster.Any(s => s.Id == selectedStudent.Id))
                .ToList();

                CoursesCollectionView.ItemsSource = enrolledCourses;
            }
        }

        private async void OnCourseClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course selectedCourse)
            {
                // Action handler when interacting with a specific course
                await DisplayAlert("Course Actions", $"Opening portal for {selectedCourse.Name}...", "OK");
            }
        }
    }
}