using System;
using Microsoft.Maui.Controls;

namespace App.CMS.Views
{
    public partial class TeacherPortalPage : ContentPage
    {
        public TeacherPortalPage()
        {
            InitializeComponent();
        }

        private async void OnReturnToMainMenuClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}