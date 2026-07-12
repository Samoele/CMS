namespace Library.CMS.Models
{
    public class User {
        public int Id { get ; set; }
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public enum UserRole{
        Student,
        Teacher

    }
}

