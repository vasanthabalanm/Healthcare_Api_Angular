using HealthCare_Api_C_.Models;
using HealthCare_Api_C_.Repository.AppointmentsDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCare_Api_C_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppoinments _appointment;
        public AppointmentController(IAppoinments appointment)
        {
            _appointment = appointment;
        }

        [HttpGet("Approved")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorDetails()
        {
            return await _appointment.GetDoctorDetails();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentDetails()
        {
            
            var res =  await _appointment.GetAppointmentDetails();
            if (res != null)
            {
                return Ok(res);
            }
            else
            {
                return NotFound("there is no appointmnets");
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterDoctor([FromQuery] string Specialization)
        {
            var filterdoctor = await _appointment.FilterDoctor(Specialization);
            if (filterdoctor != null)
            {
                return Ok(filterdoctor);
            }
            else
            {
                return NotFound("Doctor is not found based on your preference");
            }

        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            try
            {
                var getdt = await _appointment.PostAppointment(appointment);
                return Ok(getdt);
            }
            catch (ArithmeticException ex)
            {
                return NotFound(ex.Message);

            }
        }

        [HttpDelete("{appointment}")]
        public async Task<ActionResult<string>> DeleteAppointment(int appointment)
        {
            try
            {
                var getdt = await _appointment.DeleteAppointment(appointment);


                return Ok(getdt);
            }
            catch (ArithmeticException ex)
            {
                return NotFound(ex.Message);

            }
        }
    }
}
