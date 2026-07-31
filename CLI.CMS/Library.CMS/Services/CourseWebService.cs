using System.Net.Http.Json;
using Library.CMS.Models;

namespace Library.CMS.Services
{
    public class CourseWebService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5286/";

        public CourseWebService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<List<Course>> FetchAllCoursesAsync()
        {
            try
            {
                var courses = await _client.GetFromJsonAsync<List<Course>>("api/course");
                return courses ?? new List<Course>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CourseWebService Error]: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<bool> CreateCourseAsync(Course course)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/course", course);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CourseWebService Error]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"api/course/{course.Id}", course);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CourseWebService Error]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/course/{courseId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CourseWebService Error]: {ex.Message}");
                return false;
            }
        }
    }
}