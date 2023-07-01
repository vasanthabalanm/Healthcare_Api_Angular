using HealthCare_Api_C_.Context;
using HealthCare_Api_C_.Helpers;
using HealthCare_Api_C_.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using HealthCare_Api_C_.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCare_Api_C_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly JwtauthDbcontext _authContext;

        public PatientController(JwtauthDbcontext authContext)
        {
            _authContext = authContext;
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] Patient userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Patients
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
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(1);
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddUser([FromBody] Patient userObj)
        {
            if (userObj == null)
                return BadRequest();

            // check email
            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email Already Exist" });

            //check username
            if (await CheckUsernameExistAsync(userObj.Username))
                return BadRequest(new { Message = "Username Already Exist" });

            var passMessage = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrEmpty(passMessage))
                return BadRequest(new { Message = passMessage.ToString() });

            userObj.Password = PasswordHash.HashPassword(userObj.Password);
            userObj.Role = "Patient";
            userObj.Token = "";
            await _authContext.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Status = 200,
                Message = "Patient Account Created! Now Login"
            });
        }
        private Task<bool> CheckEmailExistAsync(string? email)
            => _authContext.Patients.AnyAsync(x => x.Email == email);

        private Task<bool> CheckUsernameExistAsync(string? username)
            => _authContext.Patients.AnyAsync(x => x.Username == username);

        private static string CheckPasswordStrength(string pass)
        {
            StringBuilder sb = new StringBuilder();
            if (pass.Length < 9)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(pass, "[a-z]") && Regex.IsMatch(pass, "[A-Z]") && Regex.IsMatch(pass, "[0-9]")))
                sb.Append("Password should be AlphaNumeric" + Environment.NewLine);
            if (!Regex.IsMatch(pass, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special charcter" + Environment.NewLine);
            return sb.ToString();
        }

        private string CreateJwt(Patient user)
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
                Expires = DateTime.Now.AddSeconds(10),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Patients
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

        
        [HttpGet("Approved")]
        public async Task<ActionResult<Admin>> GetAllDoctors()
        {
            return Ok(await _authContext.Admins.ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetAllUsers()
        {
            return Ok(await _authContext.Patients.ToListAsync());
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _authContext.Patients.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { Message = "User not found!" });

            return Ok(user);
        }



        /*[HttpPut("{email}")]
        public async Task<IActionResult> UpdateUser(string email, [FromBody] Patient userObj)
        {
            if (userObj == null || email != userObj.Email)
                return BadRequest();

            var existingUser = await _authContext.Patients.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser == null)
                return NotFound(new { Message = "User not found!" });

            // Update the properties
            existingUser.Gender = userObj.Gender;
            // Add other properties that you want to update here.

            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                Status = 200,
                Message = "User updated successfully"
            });
        }*/

        [HttpPut("{email}")]
        public async Task<IActionResult> updateemail(string email, Patient patient)
        {
            try
            {
                var getdt = await _authContext.Patients.FirstOrDefaultAsync(h => h.Email == email);
                if (getdt == null)
                {
                    throw new ArithmeticException("The given email is not available! Try again");
                }

                getdt.FirstName=patient.FirstName; 
                getdt.LastName=patient.LastName;
                getdt.Gender = patient.Gender;

                await _authContext.SaveChangesAsync();
                return Ok(getdt);
            }
            catch (ArithmeticException ex)
            {
                return NotFound(ex.Message);
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
            var user = await _authContext.Patients.FirstOrDefaultAsync(u => u.Username == username);
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
