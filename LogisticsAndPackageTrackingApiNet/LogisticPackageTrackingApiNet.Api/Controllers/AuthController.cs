using LogisticPackageTrackingApiNet.Api.Helpers;
using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Domain.Common;
using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogisticPackageTrackingApiNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDTO>>> Login(AuthDTO dto)
    {
        var user = await _unitOfWork.Users.GetByMailAsync(dto.Mail);
        if (user == null || !AuthHelpers.VerifyPassword(dto.Password, user.PasswordHash ?? string.Empty))
            return Unauthorized(ApiResponse<AuthResponseDTO>.FailureResponse("Invalid credentials"));

        var token = GenerateJwtToken(user);
        return Ok(ApiResponse<AuthResponseDTO>.SuccessResponse(new AuthResponseDTO
        {
            Token = token,
            FullName = $"{user.FirstName} {user.LastName}",
            Mail = user.Mail
        }));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserResponseDTO>>>> GetUsers()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var response = users.Select(u => new UserResponseDTO
        {
            FullName = $"{u.FirstName} {u.LastName}",
            Mail = u.Mail,
            Address = u.Address,
            Role = u.Role
        }).ToList();

        return Ok(ApiResponse<List<UserResponseDTO>>.SuccessResponse(response));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register(RegisterDTO dto)
    {
        var user = new User
        {
            Mail = dto.Mail,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
            Password = dto.Password,
            PasswordHash = AuthHelpers.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("User registered successfully"));
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Mail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}