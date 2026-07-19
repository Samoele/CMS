using System;
using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public DateTime DueDate { get; set; }
        
        // Track completed work
        public List<Submission> Submissions { get; set; } = new List<Submission>();

        //creates a deep copy of this assignment with a completely clean submissions slate
        public Assignment Clone()
        {
        return new Assignment
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                TotalPoints = this.TotalPoints,
                DueDate = this.DueDate,
                Submissions = new List<Submission>()

                
                    
            };     
        }  

    }   

}