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
            //navigate to Teacher page
            await Navigation.PushAsync(new TeacherPortalPage());
        }

        private async void OnStudentButtonClicked(object sender, EventArgs e)
        {
           
            //navigate to the StudentPortalPage
            await Navigation.PushAsync(new StudentPortalPage());
        }

    }
}