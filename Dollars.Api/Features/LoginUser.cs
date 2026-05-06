using System.Security.Claims;
using System.Text;
using Dollars.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public static class LoginUser
{
    public record Request(string Email, string Password);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("login", async (
            IConfiguration config, // todo: use IOptions
            Request req, 
            UserManager<AppUser> um) =>
        {
            var user = await um.FindByNameAsync(req.Email);
            if(user == null || !await um.CheckPasswordAsync(user, req.Password))
            {
                return Results.Unauthorized();
            }

            var roles = await um.GetRolesAsync(user);

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"] ?? throw new ArgumentException("Jwt:SecretKey not found")));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = [
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.UserName!),
                ..roles.Select(r => new Claim(ClaimTypes.Role, r))
            ];
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:ExpirationInMinutes")),
                SigningCredentials = creds,
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
            };

            var tokenHandler = new JsonWebTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Results.Ok(new { token });
        });
    }
}