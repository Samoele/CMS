using System.Net.Http.Json;
using Library.CMS.Models;


//handles HTTP requests and connects to ssproxy instead of rewriting ssproxy
//less risk of breaking app
namespace Library.CMS.Services
{
    public class StudentWebService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5286/"; // base URL 5286

        public StudentWebService()
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

        public async Task<List<Student>> FetchAllStudentsAsync()
        {
            try
            {
                var students = await _client.GetFromJsonAsync<List<Student>>("api/student");
                return students ?? new List<Student>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StudentWebService Error]: {ex.Message}");
                return new List<Student>();
            }
        }

        public async Task<bool> CreateStudentAsync(Student student)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/student", student);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StudentWebService Error]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"api/student/{student.Id}", student);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StudentWebService Error]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/student/{studentId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StudentWebService Error]: {ex.Message}");
                return false;
            }
        }
    }
}