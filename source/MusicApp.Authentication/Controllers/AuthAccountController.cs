using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;

namespace MusicApp.Authentication.Controllers
{
    [ApiController]
    public class AuthAccountController : ControllerBase
    {
        [HttpGet("~/authorize")]
        [HttpPost("~/authorize")]
        [IgnoreAntiforgeryToken]
        public Task AuthorizeUser()
        {
            
        }

        [HttpPost("~/token")]
        public Task GenerateToken()
        {
            
        }

        [HttpPost("~/revoke")]
        public Task RevokeToken()
        {
            
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("~/user-info")]
        public Task GetUserInfo()
        {
            
        }

        [HttpPost("~/introspect")]
        public Task IntrospectToken()
        {
            
        }
    }
}