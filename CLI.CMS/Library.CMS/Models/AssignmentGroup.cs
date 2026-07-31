using System.Collections.Generic;

namespace Library.CMS.Models
{
    public class AssignmentGroup
    {
        public int Id { get; set;}
        public string Name { get; set;} = string.Empty;
        public double Weight { get; set;} //represents percentage to be applied to gradeg group

        //reference to assignments belonging to group
        public List<Assignment> Assignments { get; set;} = new List<Assignment>();

        public AssignmentGroup Clone(List<Assignment> clonedAssignments)
        {
            var clonedGroup = new AssignmentGroup
            {
                Id = this.Id,
                Name = this.Name,
                Weight = this.Weight,
                Assignments = new List<Assignment>()
            };

            //finds newly cloned assignment references that match the original assignment IDs
            foreach (var origAssign in this.Assignments)
            {
                var matchedClone = clonedAssignments.FirstOrDefault(a => a.Id == origAssign.Id);
                if (matchedClone != null)
                {
                    clonedGroup.Assignments.Add(matchedClone);
                }
            }
            return clonedGroup;
        }
    }
}