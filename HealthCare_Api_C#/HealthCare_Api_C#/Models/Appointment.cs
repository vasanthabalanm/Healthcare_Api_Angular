using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare_Api_C_.Models
{
    public class Appointment
    {
        [Key]
        public int? Id { get; set; }

        [ForeignKey("Id")]
        public Doctor? DoctorId { get; set; }

        public Int64? Phone { get; set; }
        public string? Location { get; set; }
        public string? Gender { get; set; }

    }
}
