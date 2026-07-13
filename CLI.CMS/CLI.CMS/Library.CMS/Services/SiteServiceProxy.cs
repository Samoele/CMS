using System;
using System.Collections.Generic;
using System.Linq;
using Library.CMS.Models;

namespace Library.CMS.Services
{
    public class SiteServiceProxy
    {   
        //Creates instance of Site Service Proxy
        private static SiteServiceProxy? _instance;
        public static SiteServiceProxy Instance => _instance ??= new SiteServiceProxy();

        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set;} = new List<Student>();

        //track who is currently logged in 
        public User? CurrentUser { get; set;} 

        private SiteServiceProxy()
        {
            Courses = new List<Course>();
            Users = new List<User>();
            //data for testing
            SeedData();
        }

        private void SeedData()
        {
            Users.Add(new Student { Id = 1, Name = "Alice", Code = "S001", Classification = "Freshman"});

        }

    }
}