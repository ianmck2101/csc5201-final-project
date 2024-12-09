using System.Security.Claims;
using Fetch.Api.Logic;
using Fetch.Models.Request;
using Microsoft.AspNetCore.Mvc;

[Route("")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _userService.Authenticate(request.Username, request.Password);
        if (user == null) return Unauthorized();

        var token = _userService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    [HttpGet("verify")]
    public IActionResult VerifyUser()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized();
        }

        try
        {
            var claims = _userService.ValidateToken(token);
            var roleClaim = claims.FindFirst(ClaimTypes.Role); 
            var role = roleClaim?.Value; 
            return Ok(new { role });
        }
        catch (Exception)
        {
            return Unauthorized(); // Token is invalid or expired
        }
    }

    [HttpGet]
    public IActionResult LoadUsers()
    {
        var result =  _userService.LoadAllUsers();

        return Ok(result);
    }
}
