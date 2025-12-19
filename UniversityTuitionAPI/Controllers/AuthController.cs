using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniversityTuitionAPI.Data;

namespace UniversityTuitionAPI.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UniversityDbContext _context;

        public AuthController(IConfiguration config, UniversityDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("token")]
        public IActionResult Token([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username))
                return BadRequest("Username is required");

            string role;
            string subject;
            List<Claim> claims = new();

            if (req.Username == "admin")
            {
                if (req.Password != "12345")
                    return Unauthorized("Invalid admin credentials");

                role = "Admin";
                subject = "admin";

                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            else
            {
                var student = _context.Student.FirstOrDefault(s => s.StudentNo == req.Username);

                if (student == null)
                    return Unauthorized("Student not found");

                var firstName = student.FullName.Split(' ')[0];

                if (req.Password != firstName)
                    return Unauthorized("Invalid student credentials");

                role = "Student";
                subject = student.StudentNo;

                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("StudentNo", student.StudentNo));
                claims.Add(new Claim("FullName", student.FullName));
            }

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            );

            if (role == "Student")
            {
                var student = _context.Student.FirstOrDefault(s => s.StudentNo == req.Username);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expires = token.ValidTo,
                    studentNo = student.StudentNo,
                    fullName = student.FullName
                });
            }

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires = token.ValidTo
            });
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string? Password { get; set; }
        }
    }
}
