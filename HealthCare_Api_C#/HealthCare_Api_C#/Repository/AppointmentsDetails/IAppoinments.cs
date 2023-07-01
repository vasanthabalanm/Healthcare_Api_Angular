using HealthCare_Api_C_.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Api_C_.Repository.AppointmentsDetails
{
    public interface IAppoinments
    {
        Task<ActionResult<IEnumerable<Admin>>> GetDoctorDetails();
        Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentDetails();
        Task<List<Admin>> FilterDoctor(string Specialization);
        Task<ActionResult<Appointment>> PostAppointment(Appointment appointment);
        Task<Appointment> UpdateAppointment(int id, Appointment appointment);
        Task<string> DeleteAppointment(int id);



    }
}
