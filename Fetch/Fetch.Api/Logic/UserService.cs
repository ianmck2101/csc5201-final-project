using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fetch.Api.DAL;
using Fetch.Models.Data;
using Microsoft.IdentityModel.Tokens;

namespace Fetch.Api.Logic
{
    public interface IUserService
    {
        User? Authenticate(string username, string password);
        string GenerateToken(User user);
        ClaimsPrincipal ValidateToken(string token);
        IEnumerable<User> LoadAllUsers();
        Provider? GetProviderByUsername(string username);
    }

    public class UserService : IUserService
    {
        private readonly IUserDAL _userDAL;
        private readonly string SecretKey = "SuperSecretKey12345678901234567890";
        private readonly string Issuer = "Fetch";
        private readonly string Audience = "FetchUsers";

        public UserService(IUserDAL userDAL)
        {
            _userDAL = userDAL ?? throw new ArgumentNullException(nameof(userDAL));
        }

        public User? Authenticate(string username, string password)
        {
            var user = _userDAL.Authenticate(username, password);

            return user;
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Provider? GetProviderByUsername(string username)
        {
            var user = _userDAL.LoadUserByUsername(username);

            if (user == null)
            {
                return null;
            }

            var provider = _userDAL.LoadProviderForUser(user);

            return provider;
        }

        public IEnumerable<User> LoadAllUsers()
        {
            return _userDAL.LoadAllUsers();
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                // You can further inspect the token if necessary
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                throw new UnauthorizedAccessException("Invalid token", ex);
            }
        }
    }
}
