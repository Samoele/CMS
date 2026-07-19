using System;

namespace Library.CMS.Models
{
    public class AssignmentItem : ContentItem
    {
        // links module item directly to an existing Assignment object
        public Assignment LinkedAssignment { get; set; }

        public AssignmentItem(Assignment assignment)
        {
            LinkedAssignment = assignment;
            Name = assignment.Name; //set names
        }

        public override void Open()
        {
            Console.Clear();
            Console.WriteLine($"======================================");
            Console.WriteLine($" MODULE ASSIGNMENT: {LinkedAssignment.Name}");
            Console.WriteLine($"======================================");
            Console.WriteLine($"Description: {LinkedAssignment.Description}");
            Console.WriteLine($"Due Date:    {LinkedAssignment.DueDate:MM/dd/yyyy}");
            Console.WriteLine($"Max Points:  {LinkedAssignment.TotalPoints}");
            Console.WriteLine($"======================================");
            Console.WriteLine("\nPress any key to close assignment overview...");
            Console.ReadKey();
        }


        public override ContentItem Clone()
        {
            //when deep copy is performed, this item is linked to a newly cloned copy of the assignment
            return new AssignmentItem(this.LinkedAssignment.Clone()) { Id = this.Id };
        }
    }
}