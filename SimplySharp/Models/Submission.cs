using Castle.MicroKernel.SubSystems.Conversion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
       
        public string UserId { get; set; }
      
        public string Text { get; set; }

        public string File { get; set; }

        [DisplayName("Submission Date")]
        [DataType(DataType.DateTime)]
        public DateTime SubmissionDate { get; set; }

        public int? Score { get; set; }

        [DisplayName("Instructor Feedback")]
        public string InstructorFeedback { get; set; }
        


        
    }
}
