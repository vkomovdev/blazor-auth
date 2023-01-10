using BlazorAuthApp.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlazorAuthApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignInController : ControllerBase
    {

        private readonly ILogger<SignInController> _logger;

        public SignInController(ILogger<SignInController> logger)
        {
            _logger = logger;
        }
       
        [HttpGet]
        public IEnumerable<ClaimRecord> Get()
        {
            var claims = HttpContext.User.Claims;

            var res = new List<ClaimRecord>();
            foreach (var item in claims)
            {
                res.Add(new ClaimRecord() { Type = item.Type, Value = item.Value });
            }

            return res;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            var Email = user.UserName;
            var Password = user.Password;
            if (!(Email == "admin@gmail.com" && Password == "password"))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Email, Email),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
               new ClaimsPrincipal(claimsIdentity));

            return Ok();
        }
    }  
}