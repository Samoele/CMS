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

            //selected classification or default to "Freshman"
            string classification = ClassificationPicker.SelectedItem?.ToString() ?? "Freshman";

            if (_editingStudent != null)
            {
                //update existing student
                _editingStudent.Name = StudentNameEntry.Text.Trim();
                _editingStudent.Classification = classification;

                await DisplayAlert("Success", $"{_editingStudent.Name} updated successfully.", "OK");
            }
            else
            {
                //create new student
                var newStudent = new Student 
                { 
                    Name = StudentNameEntry.Text.Trim(),
                    Classification = classification
                };

                // SiteServiceProxy handles auto generating unique ID and appending to roster (AddStudentMethod)
                SiteServiceProxy.Current?.AddStudent(newStudent);
                await DisplayAlert("Success", $"{newStudent.Name} added as {newStudent.Classification} (ID: {newStudent.Id}).", "OK");
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
                    //executes cascading delete
                    ExecuteCascadingStudentDeletion(studentToDelete.Id);
                    
                    await DisplayAlert("Deleted", $"{studentToDelete.Name} has been erased from the system.", "OK");
                    RefreshStudentList();
                }
            }
        }

        private void ExecuteCascadingStudentDeletion(int studentId)
        {
            //Remove from all course rosters
            var courses = SiteServiceProxy.Current?.GetCourses() ?? new List<Course>();
            foreach (var course in courses)
            {
                //unreroll student from course roster
                SiteServiceProxy.Current?.UnenrollStudent(course.Id, studentId);

                //clear assignments submissions and grades associated with this student
                if (course.Assignments != null)
                {
                    foreach (var assignment in course.Assignments)
                    {
                        assignment.Submissions?.RemoveAll(sub => sub.StudentId == studentId);
                    }
                }
            }

            //remove student from global university directory
            SiteServiceProxy.Current?.DeleteStudent(studentId);
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