using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Library.CMS.Models;
using Library.CMS.Services;

namespace App.CMS.Views
{
    public partial class ManageStudentsPage : ContentPage
    {
        private Student? _editingStudent = null;

        public ManageStudentsPage()
        {
            InitializeComponent();
            RefreshStudentList();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Fetch live student data from MongoDB Atlas via Web API and Proxy
            await SiteServiceProxy.Current.RefreshStudentsFromDatabaseAsync();

            //refresh the view on teacher dashboard
            StudentsCollectionView.ItemsSource = SiteServiceProxy.Current.GetStudents();
        }

        private void RefreshStudentList()
        {
            var students = SiteServiceProxy.Current?.GetStudents() ?? new List<Student>();
            StudentsCollectionView.ItemsSource = null;
            StudentsCollectionView.ItemsSource = students;
        }

        private async void OnSaveStudentClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StudentNameEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a student name.", "OK");
                return;
            }

            // set classification or default to "Freshman"
            string classification = ClassificationPicker.SelectedItem?.ToString() ?? "Freshman";

            if (_editingStudent != null)
            {
                //update existing student fields
                _editingStudent.Name = StudentNameEntry.Text.Trim();
                _editingStudent.Classification = classification;

                //updates to database through ssproxy
                bool success = await SiteServiceProxy.Current.UpdateStudentAsync(_editingStudent);

                if (success)
                {
                    await DisplayAlert("Success", $"{_editingStudent.Name} updated successfully.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to update student in database.", "OK");
                }
            }
            else
            {
                //create new student object
                var newStudent = new Student 
                { 
                    Name = StudentNameEntry.Text.Trim(),
                    Classification = classification
                };

                // new student to MongoDB Atlas through ssproxy
                bool success = await SiteServiceProxy.Current.AddStudentAsync(newStudent);

                if (success)
                {
                    await DisplayAlert("Success", $"{newStudent.Name} added as {newStudent.Classification} (ID: {newStudent.Id}).", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to save student to database. Ensure API.CMS is running.", "OK");
                }
            }

            ResetForm();
            RefreshStudentList();
        }

        private void OnEditStudentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Student studentToEdit)
            {
                _editingStudent = studentToEdit;
                FormHeaderLabel.Text = $"Edit Student: {studentToEdit.Name}";
                StudentNameEntry.Text = studentToEdit.Name;
                ClassificationPicker.SelectedItem = studentToEdit.Classification;

                SaveStudentBtn.Text = "Update Student";
                CancelEditBtn.IsVisible = true;
            }
        }

        private async void OnDeleteStudentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Student studentToDelete)
            {
                bool confirm = await DisplayAlert(
                    "⚠️ Confirm Delete", 
                    $"Are you sure you want to remove {studentToDelete.Name} from the system?\n\nThis will automatically unenroll them from ALL courses and delete their grades and submissions.", 
                    "Delete Student", 
                    "Cancel");

                if (confirm)
                {
                    //cascading delete and removes student from DB 
                    bool success = await ExecuteCascadingStudentDeletionAsync(studentToDelete.Id);

                    if (success)
                    {
                        await DisplayAlert("Deleted", $"{studentToDelete.Name} has been erased from the system.", "OK");
                        RefreshStudentList();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to delete student from Database.", "OK");
                    }
                }
            }
        }

        private async Task<bool> ExecuteCascadingStudentDeletionAsync(int studentId)
        {
            //remove from all course rosters in local instance of ssproxy
            var courses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();
            foreach (var course in courses)
            {
                //unreroll students from course roster
                SiteServiceProxy.Current?.UnenrollStudent(course.Id, studentId);

                //also clear assignment submissions and grades associated with this student
                if (course.Assignments != null)
                {
                    foreach (var assignment in course.Assignments)
                    {
                        assignment.Submissions?.RemoveAll(sub => sub.StudentId == studentId);
                    }
                }
            }

            //removes student from global university directory in DB through proxy
            if (SiteServiceProxy.Current != null)
            {
                return await SiteServiceProxy.Current.DeleteStudentAsync(studentId);
            }

            return false;
        }

        private void OnCancelEditClicked(object sender, EventArgs e) => ResetForm();

        private void ResetForm()
        {
            _editingStudent = null;
            FormHeaderLabel.Text = "Add New Student";
            StudentNameEntry.Text = string.Empty;
            ClassificationPicker.SelectedItem = null;
            SaveStudentBtn.Text = "Save Student";
            CancelEditBtn.IsVisible = false;
        }

        private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    }
}