using BlazorAuthApp.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlazorAuthApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignOutController : ControllerBase
    {
        private readonly ILogger<SignOutController> _logger;

        public SignOutController(ILogger<SignOutController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            await HttpContext
                .SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect(Url.Content("~/"));
        }        
    }
}