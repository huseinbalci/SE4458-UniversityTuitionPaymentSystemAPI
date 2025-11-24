using Microsoft.EntityFrameworkCore;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Data
{
    public class UniversityDbContext : DbContext
    {
        public UniversityDbContext(DbContextOptions<UniversityDbContext> options) : base(options) { }

        public DbSet<Student> Student { get; set; }
        public DbSet<Tuition> Tuition { get; set; }
    }
}