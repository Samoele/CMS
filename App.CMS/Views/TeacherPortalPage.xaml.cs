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

        

        //reload courses every time the page becomes visible
        // protected override void OnAppearing()
        // {
        //     base.OnAppearing();
        //     LoadTeacherCourses();
        // }

        private void LoadTeacherCourses()
        {
            //fetch courses from site service proxy
            var courses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();
            TeacherCoursesCollectionView.ItemsSource = courses;
        }


        private void OnShowCoursesClicked(object sender, EventArgs e)
        {
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
    }
}