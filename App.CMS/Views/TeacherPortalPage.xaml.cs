using System;
using Microsoft.Maui.Controls;
using Library.CMS.Models;
using Library.CMS.Services;

namespace App.CMS.Views
{
    public partial class TeacherPortalPage : ContentPage
    {
        public TeacherPortalPage()
        {
            InitializeComponent();
        }


        private void LoadTeacherCourses()
        {
            //fetch courses from site service proxy
            var courses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();

            // Assigning null and then to courses makes MAUI update the TeacherCoursesCollectionView update immediately
            TeacherCoursesCollectionView.ItemsSource = null;
            TeacherCoursesCollectionView.ItemsSource = courses;
        }


        private void OnShowCoursesClicked(object sender, EventArgs e)
        {
            AddCourseFormCard.IsVisible = false;
            CoursesSection.IsVisible = true;
            LoadTeacherCourses();
        }


        private async void OnManageCourseClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                // retrieves course 
                Course? selectedCourse = button.CommandParameter as Course ?? button.BindingContext as Course;

                if (selectedCourse != null)
                {
                    await Navigation.PushAsync(new TeacherCourseDetailPage(selectedCourse));
                }
                else
                {
                    await DisplayAlert("Error", "Could not retrieve course details from selection.", "OK");
                }
            }
        }

        private async void OnManageStudentsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageStudentsPage());
        }
        private async void OnReturnToMainMenuClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        private void OnShowAddCourseFormClicked(object sender, EventArgs e)
        {
            CoursesSection.IsVisible = false;
            AddCourseFormCard.IsVisible = true;
        }

        private void OnCancelAddCourseClicked(object sender, EventArgs e)
        {
            AddCourseFormCard.IsVisible = false;
        }

        private async void OnSaveNewCourseClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CourseNameEntry.Text) || string.IsNullOrWhiteSpace(CourseCodeEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please provide both a course name and a course code.", "OK");
                return;
            }

            //find active courses and calculate id based on max ID
            var courses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();
            int nextId = courses.Any() ? courses.Max(c => c.Id) + 1 : 1;

            var newCourse = new Course
            {
                Id = nextId,
                Name = CourseNameEntry.Text.Trim(),
                Code = CourseCodeEntry.Text.Trim(),
                Description = CourseDescriptionEntry.Text?.Trim() ?? string.Empty,
                Assignments = new List<Assignment>(),
                Modules = new List<Module>(),
                Roster = new List<Student>()
            };

            //adding course using siteserviceproxy 
            SiteServiceProxy.Current?.AddCourse(newCourse); 

            await DisplayAlert("Success", $"Course '{newCourse.Code} - {newCourse.Name}' created successfully!", "OK");

            //clear form fields
            CourseNameEntry.Text = string.Empty;
            CourseCodeEntry.Text = string.Empty;
            CourseDescriptionEntry.Text = string.Empty;

            // change view back to list and refresh with loadteachercourses() existing method
            AddCourseFormCard.IsVisible = false;
            CoursesSection.IsVisible = true;
            
            // load courses
            LoadTeacherCourses();
        }

        private async void OnDeleteCourseClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course courseToDelete)
            {
                bool confirm = await DisplayAlert(
                    "⚠️ Confirm Deletion",
                    $"Are you sure you want to permanently delete course '{courseToDelete.Code} - {courseToDelete.Name}'?",
                    "Delete",
                    "Cancel");

                if (confirm)
                {
                    // delete course using siteServiceProxy
                    SiteServiceProxy.Current?.DeleteCourse(courseToDelete.Id); //note: deletion must always be with ID

                    await DisplayAlert("Deleted", $"Course '{courseToDelete.Code}' removed.", "OK");
                    
                    //reload courses
                    LoadTeacherCourses();
                }
            }
        }



    }
}