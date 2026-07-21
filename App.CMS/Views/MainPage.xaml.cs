using System;
using Microsoft.Maui.Controls;

namespace App.CMS.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnTeacherButtonClicked(object sender, EventArgs e)
        {
            // Placeholder for Instructor Dashboard navigation
            await DisplayAlert("Portal Selected", "Entering the Instructor Portal...", "OK");
        }

        private async void OnStudentButtonClicked(object sender, EventArgs e)
        {
           
            // Navigate to the StudentPortalPage
            await Navigation.PushAsync(new StudentPortalPage());
        }
    }
}