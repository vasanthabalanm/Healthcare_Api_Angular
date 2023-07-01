using HealthCare_Api_C_.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Api_C_.Repository.AppointmentsDetails
{
    public interface IAppoinments
    {
        Task<ActionResult<IEnumerable<Doctor>>> GetDoctorDetails();
        Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentDetails();
        Task<List<Appointment>> FilterDoctor(string Specialization);
        Task<ActionResult<Appointment>> PostAppointment(Appointment appointment);
        Task<string> DeleteAppointment(int id);



    }
}
