using System;
using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double TotalPoints { get; set; }
        public DateTime DueDate { get; set; }

        // Weight percentage for this assignment (10 = 10% and so on)
        public double Weight { get; set; } = 10.0;
        
        // Track completed work
        public List<Submission> Submissions { get; set; } = new List<Submission>();

        //Properties for Quizzes as a form of assignment
        public bool IsQuiz { get; set; } = false;
        public string QuizQuestion { get; set; } = string.Empty; //Free form question 








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
                Submissions = new List<Submission>(),
                IsQuiz = this.IsQuiz,
                QuizQuestion = this.QuizQuestion,

                
                    
            };     
        }  

    }   

}