namespace UniversityTuitionAPI.Models
{
    public class Tuition
    {
        public int TuitionId { get; set; }
        public string StudentNo { get; set; }
        public string Term { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }
    }
}