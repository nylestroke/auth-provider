using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthProvider.Models;
using AuthProvider.Services;
using Microsoft.AspNetCore.Authorization;
using BC = BCrypt.Net.BCrypt;

namespace AuthProvider.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class OAuth2Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly UserService _usersService;

        public OAuth2Controller(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _secret = _configuration.GetSection("Configuration")["Secret"];
            _issuer = _configuration.GetSection("Configuration")["Issuer"];
            _usersService = userService;
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _usersService.GetAsync(id);
            if (user is null) return NotFound();
            return user;
        }

        [HttpPost("user/register")]
        public async Task<IActionResult> RegUser(User user)
        {
            user.password = BC.HashPassword(user.password);
            await _usersService.CreateAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.id }, user);
        }

        [HttpPost("user/login")]
        public async Task<IActionResult> PostUser(User user)
        {
            if (user.email == null && user.username == null)
            {
                return NotFound("Please enter email or username. User not found");
            }

            var userModel = await _usersService.GetUser(user.email, user.username);
            if (userModel == null)
            {
                return NotFound("User not found with current credentials");
            }

            var checkPassword = BC.Verify(user.password, userModel?.password);
            return checkPassword switch
            {
                true => Ok(userModel),
                false => Conflict("Incorrect credentials")
            };
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
            // query.Add("client_id", client_id);
            // query.Add("response_type", response_type);
            query.Add("redirect_uri", redirect_uri);
            query.Add("state", state);
            // query.Add("scope", scope);
            return Ok(query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string redirect_uri,
            string state
        )
        {
            var query = new QueryBuilder();
            query.Add("code", "authorized");
            query.Add("state", state);

            return Ok($"{redirect_uri}{query.ToString()}");
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token(
            string grant_type,
            string code
        )
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "mainWeb"),
            };

            var secretBytes = Encoding.UTF8.GetBytes(_secret);
            var key = new SymmetricSecurityKey(secretBytes);
            const string algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                _issuer,
                _issuer,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(30),
                signingCredentials);

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthClient"
            };

            return new OkObjectResult(responseObject);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyUser()
        {
            string token = Request.Headers.Authorization;

            if (!token.Contains("Bearer"))
            {
                return Conflict("No token provided");
            }

            string[] parsedToken = token.Split(" ");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var decodedToken = tokenHandler.ReadToken(parsedToken[1]);
            return Ok(parsedToken[1]);
        }
    }
}