using HealthCare_Api_C_.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCare_Api_C_.Context
{
    public class JwtauthDbcontext:DbContext
    {
        public JwtauthDbcontext(DbContextOptions<JwtauthDbcontext> options) : base(options) { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
