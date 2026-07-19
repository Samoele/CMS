using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class AssignmentGroup
    {
        public int Id { get; set;}
        public string Name { get; set;} = string.Empty;
        public double Weight { get; set;} //Represents percentage to be applied to gradeg group

        //Reference to assignments belonging to this group
        public List<Assignment> Assignments { get; set;} = new List<Assignment>();

    }
}