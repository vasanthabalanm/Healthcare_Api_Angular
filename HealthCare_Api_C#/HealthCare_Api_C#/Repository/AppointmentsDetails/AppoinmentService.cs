using HealthCare_Api_C_.Context;
using HealthCare_Api_C_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCare_Api_C_.Repository.AppointmentsDetails
{
    public class AppoinmentService : IAppoinments
    {
        private readonly JwtauthDbcontext _dbcontext;

        public AppoinmentService(JwtauthDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        //to view doctors list
        public async Task<ActionResult<IEnumerable<Admin>>> GetDoctorDetails()
        {
            var approveddoctor = await _dbcontext.Admins.Where(p => p.Role == "Doctors").ToListAsync();
            return approveddoctor;
        }

        //to view appointment details
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentDetails()
        {
            var getdt = await _dbcontext.Appointments.ToListAsync();
            return getdt;
        }

        //to fillter by doctorSpecialization
        public async Task<List<Admin>> FilterDoctor(string Specialization)
        {
            var query = _dbcontext.Admins.AsQueryable();

            // Apply filters based on criteria
            if (!string.IsNullOrEmpty(Specialization))
                query = query.Where(h => h.Specialization == Specialization && h.Role == "Doctors");

            // Execute the query asynchronously and return the filtered hotels
            return await query.ToListAsync();
        }

        //to post appintment based on preference
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            _dbcontext.Add(appointment);
            await _dbcontext.SaveChangesAsync();
            return appointment;

        }

        //to update appointment
        public async Task<Appointment> UpdateAppointment(int id, Appointment appointment)
        {
            var getdt = await _dbcontext.Appointments.FindAsync(id);
            if (getdt == null)
            {
                throw new ArithmeticException("The given appooitmentId is Not available! Try again");
            }
            getdt.Id = appointment.Id;
            getdt.Date = appointment.Date;
            getdt.Problem = appointment.Problem;
            getdt.Phone = appointment.Phone;
            getdt.Location = appointment.Location;
            getdt.Gender = appointment.Gender;

            await _dbcontext.SaveChangesAsync();
            return getdt;
        }

        //to delete 
        public async Task<string> DeleteAppointment(int id)
        {
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    var Apointmetns = await _dbcontext.Appointments.FindAsync(id);
                    if (Apointmetns == null)
                    {
                        return "Appointmnet details not found";
                    }

                    var doctorappoint = await _dbcontext.Appointments.Where(appoint => appoint.AppointmentId == id).ToListAsync();

                    foreach (var doctorApointment in doctorappoint)
                    {
                        _dbcontext.Appointments.Remove(doctorApointment);
                    }

                    _dbcontext.Appointments.Remove(Apointmetns);
                    await _dbcontext.SaveChangesAsync();

                    transaction.Commit();

                    return "Appointment Deleted,Thanks for Visiting us!..";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return $"Error deleting Appointmnet details: {ex.Message}";
                }
            }
        }

    }
}
