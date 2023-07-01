using HealthCare_Api_C_.Context;
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
using HealthCare_Api_C_.Helpers;

namespace HealthCare_Api_C_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly JwtauthDbcontext _authContext;

        public DoctorController (JwtauthDbcontext authContext)
        {
            _authContext = authContext;
        }

        

        [HttpPost("register")]
        public async Task<IActionResult> AddUser([FromBody] Doctor userObj)
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

            userObj.Role = "Doctor";
            userObj.Token = "";
            await _authContext.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Status = 200,
                Message = "Doctor id has been submitted to the admin!Please login after sometime...."
            });
        }
        private Task<bool> CheckEmailExistAsync(string? email)
            => _authContext.Doctors.AnyAsync(x => x.Email == email);

        private Task<bool> CheckUsernameExistAsync(string? username)
            => _authContext.Doctors.AnyAsync(x => x.Username == username);

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

        

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Doctor>> GetAllUsers()
        {
            return Ok(await _authContext.Doctors.ToListAsync());
        }

        
    }
}
