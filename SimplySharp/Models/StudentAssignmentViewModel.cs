using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StudentAssignmentViewModel
    {
            
        public IEnumerable<SimplySharp.Models.Class> Classes { get; set; }
        public IEnumerable<SimplySharp.Models.Assignment> Assignment { get; set; } 
        
        public IEnumerable<SimplySharp.Models.Submission> Submission { get; set; }

    }
}

