using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using jwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace jwt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JwtController : ControllerBase
    {
        private readonly IConfiguration _config;
        public JwtController(IConfiguration config)
        {
            _config = config;

        }
        private Login AuthenticateUser(Login login)    
        {    
            Login user = null;    
    
            if (login.Username == "freecode")    
            {    
                user = new Login { Username = "freecode", EmailAddress = "freecode@gmail.com" };    
            }    
            return user;    
        }

        private string GenerateJWT(Login userInfo)    
        {    
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtAuth:Key"]));    
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);    
    

            //claim is used to add identity to JWT token
            var claims = new[] 
            {    
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username),    
                new Claim(JwtRegisteredClaimNames.Email, userInfo.EmailAddress),    
                new Claim("Date", DateTime.Now.ToString()),    
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())    
            };    
            

            var token = new JwtSecurityToken(_config["JwtAuth:Issuer"],    
              _config["JwtAuth:Issuer"],    
              claims,    
              expires: DateTime.Now.AddMinutes(120),    
              signingCredentials: credentials);    
    
            return new JwtSecurityTokenHandler().WriteToken(token);    
        }    

     
        [HttpPost("Login")]  

        public IActionResult Login([FromBody]Login login)    
        {    
            
            var user = AuthenticateUser(login);    
    
            if (user != null)    
            {    
                var tokenString = GenerateJWT(user);    
               return Ok(new { token = tokenString });    
            }    else
            {
                return Unauthorized();
            }
    
            
        } 


        [HttpGet]    
        [Authorize]  
        public ActionResult<IEnumerable<string>> Gets()    
        {    

        var currentUser = HttpContext.User;  
        Console.WriteLine(currentUser);  
         DateTime TokenDate = new DateTime();    
                    
            if (currentUser.HasClaim(c => c.Type == "Date"))    
            {    
                TokenDate = DateTime.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == "Date").Value);    
                            
            }    
                    
            return Ok("Custom Claims(date): " + TokenDate);
                
        }
    }
}