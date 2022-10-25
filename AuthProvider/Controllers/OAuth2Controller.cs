using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace AuthProvider.Controllers
{
    [AllowAnonymous]
    public class OAuth2Controller : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string secret;
        private readonly string issuer;

        public OAuth2Controller(IConfiguration configuration)
        {
            _configuration = configuration;
            secret = _configuration.GetSection("Configuration")["Secret"];
            issuer = _configuration.GetSection("Configuration")["Issuer"];
        }

        [HttpGet]
        public IActionResult Authorize(
            string response_type,
            string client_id,
            string redirect_uri,
            string scope,
            string state
        )
        {
            var query = new QueryBuilder();
            query.Add("client_id", client_id);
            query.Add("response_type", response_type);
            query.Add("redirect_uri", redirect_uri);
            query.Add("state", state);

            return Ok(query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string username,
            string redirect_uri,
            string state
        )
        {
            var code = "authorized";
            
            var query = new QueryBuilder();
            query.Add("code", code);
            query.Add("state", state);

            return Ok($"{redirect_uri}{query.ToString()}");
        }

        public async Task<IActionResult> Token(
            string grant_type,
            string code,
            string redirect_uri,
            string client_id
        )
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "mainWeb"),
                new Claim(".Authorization", "cookie"),
            };
            
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                issuer,
                issuer,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(30),
                signingCredentials);

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthClient",
                email = "test@email.com",
                sub = "1",
                name = "Vladyslav",
            };
            
            return new OkObjectResult(responseObject);
        }
    }
}