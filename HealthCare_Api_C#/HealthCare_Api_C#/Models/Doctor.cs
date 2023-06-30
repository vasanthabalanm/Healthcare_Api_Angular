using System.ComponentModel.DataAnnotations;

namespace HealthCare_Api_C_.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
        public Int64? Phone { get; set; }
        public string? Specialization { get; set; }
        public int? Experience { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }


    }
}
