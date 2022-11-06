using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthProvider.Models;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace AuthProvider.Services;

public record TokenCredentials(string grant_type, string? client_id, string? client_secret, string? code,
    string? refresh_token);

public record AuthorizeCredentials(string response_type, string client_id, string redirect_uri, string? scope,
    string state, string code);

public class OAuth2Service
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly UserService _usersService;
    private static readonly Random Random = new Random();

    private static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public OAuth2Service(IConfiguration configuration, UserService userService)
    {
        _secret = configuration.GetSection("Configuration")["Secret"];
        _issuer = configuration.GetSection("Configuration")["Issuer"];
        _usersService = userService;
    }

    public async Task<User> GetUserById(string id)
    {
        return await _usersService.GetAsync(id) ??
               throw new Exception("No user found");
    }

    public async Task<User> CreateUser(User credentials)
    {
        credentials.password = BC.HashPassword(credentials.password);
        credentials.code = GenerateCode(24).ToLower();
        await _usersService.CreateAsync(credentials);

        return await _usersService.GetUser(credentials.email, credentials.username) ??
               throw new Exception("No user found");
    }

    public async Task<User> AuthorizeUser(User credentials)
    {
        var user = await _usersService.GetUser(credentials.email, credentials.username);
        if (user?.id is null) throw new Exception("No user found with this credentials");
        var checkPassword = BC.Verify(credentials.password, user!.password);

        if (!checkPassword) throw new Exception("Incorrect password");

        user!.code = GenerateCode(24).ToLower();
        await _usersService.UpdateAsync(user.id, user);

        return await _usersService.GetUser(credentials.email, credentials.username) ??
               throw new Exception("Something went wrong");
    }

    public IActionResult GetTokenInternal(TokenCredentials credentials)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, credentials!.code),
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

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var authorizationToken = new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = new JwtSecurityTokenHandler().ReadToken(accessToken).ValidTo
        };

        return new OkObjectResult(authorizationToken);
    }

    public async Task<User> GetUserInformation(string headers)
    {
        var headerToken = headers.Split(" ");
        var token = headerToken[1];

        if (!headers.Contains("Bearer")) throw new Exception("No bearer token found");

        var jwt = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);
        
        jwt.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var claims = jwtToken.Claims.First(x => x.Type == "sub").ToString();
        var code = claims.Split("sub: ")[1];
        var information = await _usersService.GetUserByCode(code) ?? throw new Exception("User not found with current sub code");
        await _usersService.RemoveCode(code);

        return new User()
        {
            username = information.username,
            id = information.id,
            email = information.email
        };
    }

    public async Task<User> GetUserByRecoverToken(string token)
    {
        return await _usersService.GetUserByRecoverToken(token);
    }

    public async Task<string> RecoverUser(string? username, string? email)
    {
        if (username is null && email is null)
        {
            throw new Exception("No user information");
        }
        
        var user = await _usersService.GetUser(email, username);
        if (user?.id is null) throw new Exception("No user found with this credentials");

        var recoverToken = GenerateCode(32).ToLower();
        return await _usersService.GetRecoverToken(user!.id, recoverToken);
    }

    public async Task<string> RecoverUserAndChangePassword(User user, string token)
    {
        user.password = BC.HashPassword(user.password);
        return await _usersService.ChangePasswordFromRecoverToken(user, token);
    }
}