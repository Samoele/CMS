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
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadTeacherCourses();
        }

        private void LoadTeacherCourses()
        {
            //fetch courses from site service proxy
            var courses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();
            TeacherCoursesCollectionView.ItemsSource = courses;
        }

        private async void OnManageCourseClicked(object sender, EventArgs e)
        {
            
            Button? button = sender as Button;

            //checks that button exists and its BindingContext is a Course //fixes error button unsassigned value
            if (button != null && button.BindingContext is Course selectedCourse)
            {
                await Navigation.PushAsync(new TeacherCourseDetailPage(selectedCourse));
            }
            else
            {
                await DisplayAlert("Error", "Could not retrieve course details from selection.", "OK");
            }
        }
        private async void OnReturnToMainMenuClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}