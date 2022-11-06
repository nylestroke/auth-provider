using Microsoft.AspNetCore.Mvc;
using AuthProvider.Models;
using AuthProvider.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using BC = BCrypt.Net.BCrypt;

namespace AuthProvider.Controllers
{
    [ApiController]
    [Route("api/v2")]
    public class OAuth2Controller : ControllerBase
    {
        private readonly OAuth2Service _oauthService;

        public OAuth2Controller(OAuth2Service oauthService)
        {
            _oauthService = oauthService;
        }

        [AllowAnonymous]
        [HttpGet("authorize")]
        [HttpPost("authorize")]
        public IActionResult Authorize([FromQuery] AuthorizeCredentials credentials)
        {
            var query = new QueryBuilder
            {
                { "client_id", credentials.client_id },
                { "response_type", credentials.response_type },
                { "state", credentials.state },
                { "scope", credentials.scope },
                { "code", credentials.code }
            };
            return Ok($"{credentials.redirect_uri}{query.ToString()}");
        }

        [AllowAnonymous]
        [HttpPost("credentials")]
        [Consumes("application/json")]
        public async Task<User> Code([FromBody] User credentials)
        {
            if (credentials.username is not null && credentials.email is not null)
            {
                return await _oauthService.CreateUser(credentials);
            }

            return await _oauthService.AuthorizeUser(credentials);
        }

        [AllowAnonymous]
        [HttpPost("token")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult GetTokenFromForm([FromForm] TokenCredentials credentials)
        {
            return _oauthService.GetTokenInternal(credentials);
        }

        [AllowAnonymous]
        [HttpPost("token")]
        [Consumes("application/json")]
        public IActionResult GetTokenFromBody([FromBody] TokenCredentials credentials)
        {
            return _oauthService.GetTokenInternal(credentials);
        }

        [AllowAnonymous]
        [HttpGet("info")]
        public Task<User> UserInfo()
        {
            string headers = Request.Headers.Authorization;
            return _oauthService.GetUserInformation(headers);
        }

        [AllowAnonymous]
        [HttpPost("recover/token")]
        [Consumes("application/json")]
        public async Task<string> Recover([FromBody] Recover credentials)
        {
            return await _oauthService.RecoverUser(credentials.username, credentials.email);
        }

        [AllowAnonymous]
        [HttpPost("recover/password")]
        [Consumes("application/json")]
        public async Task<string> RecoverUserAndChangePassword([FromBody] Recover credentials)
        {
            var user = await _oauthService.GetUserByRecoverToken(credentials.token);
            user.password = credentials!.password;
            return await _oauthService.RecoverUserAndChangePassword(user, credentials.token);
        }
    }
}