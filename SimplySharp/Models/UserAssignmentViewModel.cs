using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    public class UserAssignmentViewModel
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public string UserId { get; set; }

        [DisplayName("Submission Date")]
        [DataType(DataType.DateTime)]
        public DateTime SubmissionDate { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public int? Score { get; set; }
       
    }
}
