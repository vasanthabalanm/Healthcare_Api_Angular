using HealthCare_Api_C_.Models;
using HealthCare_Api_C_.Repository.AppointmentsDetails;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet("Approved")]
        public async Task<ActionResult<IEnumerable<Admin>>> GetDoctorDetails()
        {
            try
            {

                return await _appointment.GetDoctorDetails();
            }
            catch
            {
                return NotFound("there is no doctor details found");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentDetails()
        {
              var res = await _appointment.GetAppointmentDetails();
                if (res != null)
                {
                    return Ok(res);
                }
                else
                {
                    return NotFound("there is no appointmnets");
                }
               
        }

        [Authorize]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterDoctor([FromQuery] string Specialization)
        {
              var filterdoctor = await _appointment.FilterDoctor(Specialization);
                if (filterdoctor != null )
                {
                    return Ok(filterdoctor);
                }
                else
                {
                    return NotFound("Doctor is not found based on your preference");
                }
            
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            try
            {
                var getdt = await _appointment.PostAppointment(appointment);
                return Ok(getdt);
            }
            catch
            {
                return NotFound("You could allow to post your apppointment,\n please again fill the form to make appointments");

            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, Appointment appointment)
        {
            try
            {
                var getdt = await _appointment.UpdateAppointment(id, appointment);
                return Ok(getdt);
            }
            catch
            {
                return NotFound("you could not update based on appointment Id");

            }
        }

        [Authorize]
        [HttpDelete("{appointment}")]
        public async Task<ActionResult<string>> DeleteAppointment(int appointment)
        {
            try
            {
                var getdt = await _appointment.DeleteAppointment(appointment);
                return Ok(getdt);
            }
            catch
            {
                return NotFound("There is no appointment to delete by the given Id");

            }
        }
    }
}
