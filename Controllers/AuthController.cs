using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text;
using System;

using UserApi.DTOs;
using UserApi.Services;
using UserApi.Models;

namespace UserApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserCredentialsRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetActiveUserByCredentialsAsync(loginRequest.Login, loginRequest.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid login or password, or user is inactive" });
            }

            var token = GenerateJwtToken(user);
            var tokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:LifetimeMinutes"]));

            return Ok(new TokenResponseDto
            {
                Token = token,
                Expiration = tokenExpiration
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }

            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Login),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Login)
            };

            if (user.Admin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            var lifetimeMinuteConfig = _configuration["Jwt:LifetimeMinutes"];
            if (string.IsNullOrEmpty(lifetimeMinuteConfig) || !double.TryParse(lifetimeMinuteConfig, out double lifetimeMinutes))
            {
                lifetimeMinutes = 60;
            }
            var expiresTime = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

            var tokenToSign = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresTime,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenToSign);
        }
    }
}