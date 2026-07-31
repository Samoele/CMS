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

        protected override async void OnAppearing()
        {
            base.OnAppearing();


            await SiteServiceProxy.Current.RefreshCoursesFromDatabaseAsync();
            

            TeacherCoursesCollectionView.ItemsSource = null;
            TeacherCoursesCollectionView.ItemsSource = SiteServiceProxy.Current.Courses;
        }



        private async Task LoadTeacherCoursesAsync()
        {
            //fetch courses from site service proxy
            await SiteServiceProxy.Current.RefreshCoursesFromDatabaseAsync();

            // Assigning null and then to courses makes MAUI update the TeacherCoursesCollectionView update immediately
            TeacherCoursesCollectionView.ItemsSource = null;
            TeacherCoursesCollectionView.ItemsSource = SiteServiceProxy.Current.GetCourses();
        }


        private void OnShowCoursesClicked(object sender, EventArgs e)
        {
            AddCourseFormCard.IsVisible = false;
            CoursesSection.IsVisible = true;
            LoadTeacherCoursesAsync();
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

            var newCourse = new Course
            {
                Id = 0, 
                Name = CourseNameEntry.Text.Trim(),
                Code = CourseCodeEntry.Text.Trim(),
                Description = CourseDescriptionEntry.Text?.Trim() ?? string.Empty,
                Assignments = new List<Assignment>(),
                Modules = new List<Module>(),
                Roster = new List<Student>()
            };

            //add course to database on mongodb
            bool success = await SiteServiceProxy.Current.AddCourseAsync(newCourse); 

            if (success)
            {
                await DisplayAlert("Success", $"Course '{newCourse.Code} - {newCourse.Name}' created successfully!", "OK");

                CourseNameEntry.Text = string.Empty;
                CourseCodeEntry.Text = string.Empty;
                CourseDescriptionEntry.Text = string.Empty;

                
                AddCourseFormCard.IsVisible = false;
                CoursesSection.IsVisible = true;
                
                // Refresh local UI view
                await LoadTeacherCoursesAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to save course to Database. Ensure API.CMS is running.", "OK");
            }
        }

        
        private async void OnDeleteCourseClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Course courseToDelete)
            {
                bool confirm = await DisplayAlert(
                    "Confirm Delete", 
                    $"Are you sure you want to delete '{courseToDelete.Code} - {courseToDelete.Name}'?", 
                    "Yes", 
                    "No");

                if (confirm)
                {
                    //removes from database
                    bool success = await SiteServiceProxy.Current.DeleteCourseAsync(courseToDelete.Id);

                    if (success)
                    {
                        await DisplayAlert("Deleted", "Course removed successfully.", "OK");
                        await LoadTeacherCoursesAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Could not delete course from database.", "OK");
                    }
                }
            }
        }



    }
}