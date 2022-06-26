using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    public class AssignmentEvent
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string AssignmentTitle { get; set; }
        public string DueDate { get; set; }
        public string DueDateOffset { get; set; }
    }
}
