using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SimplySharp.Models
{
    public class ClassRegistration
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public string StudentId { get; set; }
        public string LetterGrade { get; set; }

    }    
}
