using API.CMS.Models;
using Library.CMS.Models; 
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.CMS.Services
{
    public class StudentMongoService
    {
        private readonly IMongoCollection<Student> _studentsCollection;

        public StudentMongoService(IOptions<MongoDbSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);

            _studentsCollection = mongoDatabase.GetCollection<Student>(
                mongoSettings.Value.StudentsCollectionName
            );
        }

        //get all
        public async Task<List<Student>> GetAsync() =>
            await _studentsCollection.Find(_ => true).ToListAsync();

        //get by id
        public async Task<Student?> GetAsync(int id) =>
            await _studentsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // create
        public async Task CreateAsync(Student newStudent) =>
            await _studentsCollection.InsertOneAsync(newStudent);

        // update
        public async Task UpdateAsync(int id, Student updatedStudent) =>
            await _studentsCollection.ReplaceOneAsync(x => x.Id == id, updatedStudent);

        // delete
        public async Task RemoveAsync(int id) =>
            await _studentsCollection.DeleteOneAsync(x => x.Id == id);
    }
}