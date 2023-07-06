using HealthCare_Api_C_.Context;
using HealthCare_Api_C_.Helpers;
using HealthCare_Api_C_.Models;
using HealthCare_Api_C_.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HealthCare_Api_C_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly JwtauthDbcontext _authContext;

        public AdminController (JwtauthDbcontext authContext)
        {
            _authContext = authContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] Admin userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Admins
                .FirstOrDefaultAsync(x => x.Username == userObj.Username);

            if (user == null)
                return NotFound(new { Message = "User not found!" });

            if (!PasswordHash.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Password is Incorrect" });
            }

            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddSeconds(300);
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        private string CreateJwt(Admin user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Admins
                .Any(a => a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }
        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is Invalid Token");
            return principal;

        }

        [Authorize]
        [HttpGet("pendingdoctors")]
        public async Task<ActionResult<Doctor>> GetAllUsers()
        {
            try
            {
                return Ok(await _authContext.Doctors.ToListAsync());
            }
            catch
            {
                return NotFound("there is no pending Doctors");
            }
        }

        [Authorize]
        [HttpGet("Approveddoctorsandadmin")]
        public async Task<ActionResult<Admin>> GetAlladmin()
        {
            try
            {
                return Ok(await _authContext.Admins.ToListAsync());
            }
            catch
            {
                return NotFound("there is no pending Doctors");
            }
        }

        [Authorize]
        [HttpGet("allpatients")]
        public async Task<ActionResult<IEnumerable<Patient>>> GetallPatient()
        {
            try
            {
                return Ok(await _authContext.Patients.ToListAsync());
            }
            catch
            {
                return NotFound("There is no patient");
            }
        }

        [Authorize]
        [HttpPost("doctor_Approved")]
        public async Task<IActionResult> AddUser([FromBody] Admin userObj)
        {
            if (userObj == null)
                return BadRequest();

            // check email
            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email Already Exist" });

            //check username
            if (await CheckUsernameExistAsync(userObj.Username))
                return BadRequest(new { Message = "Username Already Exist" });


            userObj.Password = PasswordHash.HashPassword(userObj.Password);
            userObj.Role = "Doctors";
            userObj.Token = "";
            await _authContext.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Status = 200,
                Message = "Doctor has been approved"
            });
        }

        private Task<bool> CheckEmailExistAsync(string? email)
            => _authContext.Doctors.AnyAsync(x => x.Email == email);

        private Task<bool> CheckUsernameExistAsync(string? username)
            => _authContext.Doctors.AnyAsync(x => x.Username == username);

        [Authorize]
        [HttpDelete("delete_approved_Doctor/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _authContext.FindAsync<Admin>(id);
                if (user == null)
                    return NotFound();

                _authContext.Remove(user);
                await _authContext.SaveChangesAsync();

                return Ok(new
                {
                    Status = 200,
                    Message = "Doctor has been deleted"
                });
            }
            catch
            {
                return NotFound("there is no doctor based on your Id");
            }
        
            
        }

        [Authorize]
        [HttpDelete("delete_Pending_doctor/{id}")]
        public async Task<IActionResult> DeletependingUser(int id)
        {
            try
            {
                var user = await _authContext.FindAsync<Doctor>(id);
                if (user == null)
                    return NotFound();

                _authContext.Remove(user);
                await _authContext.SaveChangesAsync();

                return Ok(new
                {
                    Status = 200,
                    Message = "Doctor has been deleted"
                });
            }
            catch
            {
                return NotFound("there is no pending doctor based on your ID");
            }
           
        }

        [Authorize (Roles ="Admin")]
        [HttpGet ("GetRolesOfDoctors")]
        public async Task<ActionResult<IEnumerable<Admin>>> GetRoleDoctor()
        {
            try
            {
                var roledoctor = await _authContext.Admins.Where(p => p.Role == "Doctors").ToListAsync();
                return Ok(roledoctor);
            }
            catch
            {
                return NotFound("There is no roles in Doctors, \n please ask admin if any mistake");
            }
            
        }

        [Authorize]
        [HttpPut("Doctorareupdate/{id}")]
        public async Task<ActionResult<Admin>> updatedoctors(int id, Admin admin)
        {
            var roledoctor = await _authContext.Admins.FirstOrDefaultAsync(p => p.Id == id && p.Role == "Doctors");
            if (roledoctor == null)
            {
                throw new ArgumentException("The given doctor ID is not available! Please try again.");
            }

            roledoctor.Experience = admin.Experience;
            roledoctor.Phone = admin.Phone;

            await _authContext.SaveChangesAsync();
            return roledoctor;
        }



        [Authorize]
        [HttpGet("appointments_Doctors")]
        public async Task<IActionResult> GetDoctorAppointments(string doctorName, string doctorEmail)
        {
            try
            {
                // Find the admin (doctor) based on unique name and email
                var doctor = await _authContext.Admins.FirstOrDefaultAsync(a => a.Username == doctorName && a.Email == doctorEmail);

                if (doctor == null)
                    return NotFound("Doctor not found.");

                // Get appointments associated with the doctor
                var appointments = await _authContext.Appointments.Where(a => a.Id == doctor.Id).ToListAsync();

                return Ok(appointments);
            }
            catch
            {
                return NotFound("there is no Active patient based on your details");
            }
           
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest("Invalid Client Request");
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _authContext.Admins.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid Request");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }

    }
}
