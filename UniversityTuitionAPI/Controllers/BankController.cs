using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityTuitionAPI.Data;

namespace UniversityTuitionAPI.Controllers
{
    [ApiController]
    [Route("api/v1/bank")]
    public class BankController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public BankController(UniversityDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Student")]
        [HttpGet("query-tuition")]
        public IActionResult QueryTuition([FromQuery] string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest("StudentNo is required");

            var student = _context.Student
                .FirstOrDefault(s => s.StudentNo == studentNo);

            if (student == null)
                return NotFound($"Student '{studentNo}' not found.");

            var tuition = _context.Tuition
                .Where(t => t.StudentNo == studentNo)
                .Select(t => new
                {
                    t.Term,
                    t.TotalAmount,
                    t.Balance
                })
                .ToList();

            if (!tuition.Any())
                return Ok($"Student '{studentNo}' has no tuition records.");

            return Ok(new
            {
                StudentNo = studentNo,
                Tuition = tuition
            });
        }

        [HttpPost("pay-tuition")]
        public IActionResult PayTuition([FromQuery] string studentNo, [FromQuery] string term, [FromQuery] decimal amount)
        {
            if (string.IsNullOrWhiteSpace(studentNo) || string.IsNullOrWhiteSpace(term))
                return BadRequest("StudentNo and Term are required.");

            if (amount <= 0)
                return BadRequest("Amount must be greater than 0.");

            var tuition = _context.Tuition
                .FirstOrDefault(t => t.StudentNo == studentNo && t.Term == term);

            if (tuition == null)
                return NotFound($"Tuition record not found for StudentNo '{studentNo}' and Term '{term}'.");

            if (tuition.Balance <= 0)
                return BadRequest("Tuition already fully paid.");

            if (amount > tuition.Balance)
                return BadRequest($"Payment exceeds remaining balance. Current balance: {tuition.Balance}");

            tuition.Balance -= amount;
            _context.SaveChanges();

            return Ok(new
            {
                Status = "Success",
                Message = $"Payment of {amount} applied successfully.",
                StudentNo = studentNo,
                Term = term,
                tuition.TotalAmount,
                RemainingBalance = tuition.Balance
            });
        }
    }
}
