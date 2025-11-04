using Application.Dtos;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AuthService(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task<Response<AuthResultDto>> RegisterAsync(RegisterRequest dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Response<AuthResultDto>.Fail("Email already registered", "EmailExists");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Response<AuthResultDto>.Fail($"Registration failed: {errors}");
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole<int>(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            var token = GenerateJwtToken(user);

            var authResult = new AuthResultDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(2),
                UserId = user.Id.ToString(),
                Email = user.Email,
                UserName = user.UserName
            };

            return Response<AuthResultDto>.Success(authResult, "User registered successfully");
        }
        public async Task<Response<AuthResultDto>> LoginAsync(LoginRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Response<AuthResultDto>.Fail("Invalid credentials");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return Response<AuthResultDto>.Fail("Invalid credentials");

            var token = GenerateJwtToken(user);

            var result = new AuthResultDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(2),
                UserId = user.Id.ToString(),
                Email = user.Email,
                UserName = user.UserName
            };

            return Response<AuthResultDto>.Success(result, "Login successful");
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
