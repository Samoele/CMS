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

        //OnAppearing method to reload live students from database
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //fetch live student records from db through ssproxy
            await SiteServiceProxy.Current.RefreshStudentsFromDatabaseAsync();
            await SiteServiceProxy.Current.RefreshCoursesFromDatabaseAsync();

            //bind picker control to display students
            if (StudentPicker != null)
            {
                StudentPicker.ItemsSource = null;
                StudentPicker.ItemsSource = SiteServiceProxy.Current.GetStudents();
                
            }
        }

        private void OnStudentPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            var selectedStudent = picker?.SelectedItem as Student;

            if (selectedStudent != null)
            {
                //set active session user in ssproxy to guarantee update on other app pages
                SiteServiceProxy.Current.CurrentUser = selectedStudent;
            
            }
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
                if (StudentPicker.SelectedItem is Student currentStudent)
                {
                    //navigates tp course selected passing the selected Course and Student
                    await Navigation.PushAsync(new CourseDetailPage(selectedCourse, currentStudent));
                }
            }
        }

        private async void OnReturnToMainMenuClicked(object sender, EventArgs e)
        {
            // Pop back to the previous screen on the navigation stack
            await Navigation.PopAsync();
        }






    }
}