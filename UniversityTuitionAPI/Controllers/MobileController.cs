using Microsoft.AspNetCore.Mvc;
using UniversityTuitionAPI.Data;

namespace UniversityTuitionAPI.Controllers
{
    [ApiController]
    [Route("api/v1/mobile")]
    public class MobileController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public MobileController(UniversityDbContext context)
        {
            _context = context;
        }

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
    }
}