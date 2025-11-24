using System.ComponentModel.DataAnnotations;

namespace UniversityTuitionAPI.Models
{
    public class Student
    {
        [Key]
        public string StudentNo { get; set; }
        public string FullName { get; set; }
    }
}