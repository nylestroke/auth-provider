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

        // [HttpGet]
        // public IActionResult Authorize(
        //     string response_type,
        //     string client_id,
        //     string redirect_uri,
        //     string scope,
        //     string state
        // )
        // {
        //     var query = new QueryBuilder();
        //     query.Add("client_id", client_id);
        //     query.Add("response_type", response_type);
        //     query.Add("redirect_uri", redirect_uri);
        //     query.Add("state", state);
        //     query.Add("scope", scope);
        //     return Ok(query.ToString());
        // }

        [HttpPost("code")]
        [HttpGet("code")]
        public async Task<IActionResult> Code(
            string redirect_uri,
            string state,
            User user
        )
        {
            var query = new QueryBuilder();
            query.Add("state", state);
            
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
            if (checkPassword)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userModel!.id),
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
                    expires: DateTime.Now.AddMinutes(1),
                    signingCredentials);

                var authorization_token = new JwtSecurityTokenHandler().WriteToken(token);

                query.Add("code", authorization_token);

                return Ok(redirect_uri + query.ToString());
            }

            return Conflict("Incorrect credentials");
        }

        // [HttpPost]
        // [HttpGet]
        // public IActionResult Authorize(
        //     string redirect_uri,
        //     string state
        // )
        // {
        //     var query = new QueryBuilder();
        //     
        //     var user = PostUser()
        //     
        //     query.Add("code", "authorized");
        //     query.Add("state", state);
        //
        //     return Ok($"{redirect_uri}{query.ToString()}");
        // }

        [HttpPost("token")]
        public async Task<IActionResult> Token(
            string grant_type,
            string code
        )
        {

            return new OkObjectResult(code);
        }

        [HttpGet("userinfo")]
        public async Task<IActionResult> UserInfo()
        {
            string headers = Request.Headers.Authorization;
            var headerToken = headers.Split(" ");
            var token = headerToken[1];

            if (!headers.Contains("Bearer"))
            {
                return BadRequest("No bearer token found");
            }

            var jwt = new JwtSecurityTokenHandler();
            var parsedToken = jwt.ReadToken(token);
            return Ok(parsedToken);
        }
    }
}