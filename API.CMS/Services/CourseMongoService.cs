using API.CMS.Models;
using Library.CMS.Models; 
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.CMS.Services
{
    public class CourseMongoService
    {
        private readonly IMongoCollection<Course> _coursesCollection;

        public CourseMongoService(IOptions<MongoDbSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);

            
            _coursesCollection = mongoDatabase.GetCollection<Course>("Courses");
        }

        //get all
        public async Task<List<Course>> GetAsync() =>
            await _coursesCollection.Find(_ => true).ToListAsync();

        //get by id
        public async Task<Course?> GetAsync(int id) =>
            await _coursesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        //create
        public async Task CreateAsync(Course newCourse) =>
            await _coursesCollection.InsertOneAsync(newCourse);

        //update
        public async Task UpdateAsync(int id, Course updatedCourse) =>
            await _coursesCollection.ReplaceOneAsync(x => x.Id == id, updatedCourse);

        //deletion
        public async Task RemoveAsync(int id) =>
            await _coursesCollection.DeleteOneAsync(x => x.Id == id);
    }
}