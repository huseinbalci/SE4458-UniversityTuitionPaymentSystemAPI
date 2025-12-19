using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityTuitionAPI.Data;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public AdminController(UniversityDbContext context)
        {
            _context = context;
        }
        
        [HttpPost("add-student")]
        public IActionResult AddStudent([FromQuery] string studentNo, [FromQuery] string fullName)
        {
            if (string.IsNullOrWhiteSpace(studentNo) || string.IsNullOrWhiteSpace(fullName))
                return BadRequest("StudentNo and FullName are required.");

            var exists = _context.Student.Any(s => s.StudentNo == studentNo);
            if (exists)
                return Conflict($"Student with StudentNo '{studentNo}' already exists.");

            _context.Student.Add(new Student
            {
                StudentNo = studentNo,
                FullName = fullName
            });

            _context.SaveChanges();

            return Ok(new
            {
                Status = "Success",
                Message = $"Student '{studentNo}' added successfully."
            });
        }
        
        [HttpPost("add-tuition")]
        public IActionResult AddTuition([FromQuery] string studentNo, [FromQuery] string term, [FromQuery] decimal totalAmount = 0)
        {
            if (string.IsNullOrWhiteSpace(studentNo) || string.IsNullOrWhiteSpace(term))
                return BadRequest("StudentNo and Term are required.");

            var student = _context.Student.FirstOrDefault(s => s.StudentNo == studentNo);
            if (student == null)
                return NotFound($"Student with StudentNo '{studentNo}' not found.");

            _context.Database.ExecuteSqlRaw(
                "INSERT INTO Tuition (StudentNo, Term, TotalAmount, Balance) VALUES ({0}, {1}, {2}, {3})",
                studentNo, term, totalAmount, totalAmount
            );

            return Ok(new
            {
                Status = "Success",
                Message = $"Tuition for student '{studentNo}' for term '{term}' added successfully."
            });
        }

        [HttpGet("unpaid")]
        public IActionResult GetUnpaidTuition([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var unpaidQuery = _context.Tuition
                .Where(t => t.Balance > 0)
                .Select(t => new
                {
                    t.StudentNo,
                    t.Term,
                    t.TotalAmount,
                    t.Balance
                });

            var totalRecords = unpaidQuery.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var results = unpaidQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Data = results
            };

            return Ok(response);
        }
        
        [HttpPost("add-tuition-batch")]
        public IActionResult AddTuitionBatch(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("CSV file is required.");

            var results = new List<object>();

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                int lineNumber = 0;
                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');
                    if (columns.Length < 3)
                    {
                        results.Add(new { Line = lineNumber, Status = "Failed", Reason = "Invalid format" });
                        continue;
                    }

                    var studentNo = columns[0].Trim();
                    var term = columns[1].Trim();
                    if (!decimal.TryParse(columns[2].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var totalAmount))
                    {
                        results.Add(new { Line = lineNumber, Status = "Failed", Reason = "Invalid amount" });
                        continue;
                    }

                    var student = _context.Student.FirstOrDefault(s => s.StudentNo == studentNo);
                    if (student == null)
                    {
                        results.Add(new { Line = lineNumber, Status = "Failed", Reason = "Student not found" });
                        continue;
                    }

                    _context.Database.ExecuteSqlRaw(
                        "INSERT INTO Tuition (StudentNo, Term, TotalAmount, Balance) VALUES ({0}, {1}, {2}, {3})",
                        studentNo, term, totalAmount, totalAmount
                    );

                    results.Add(new { Line = lineNumber, Status = "Success" });
                }
            }
            return Ok(results);
        }
    }
}
