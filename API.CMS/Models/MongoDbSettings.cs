namespace API.CMS.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string StudentsCollectionName { get; set; } = string.Empty;
    }
}