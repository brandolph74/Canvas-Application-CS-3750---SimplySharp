using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int ClassId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        [DisplayName("Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }
        [DisplayName("Max Points")]
        public int MaxPoints{ get; set; }
        [DisplayName("Submission Type")]
        public string SubmissionType { get; set; }
    }
}
