using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Capitec.FraudEngine.Infrastructure.Authentication
{
    public class JwtTokenGenerator(IConfiguration config) : IJwtTokenGenerator
    {
        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role), 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var scopes = user.Role switch
            {
                IdentityConstants.Roles.Admin => new[] { IdentityConstants.Scopes.FraudRead, IdentityConstants.Scopes.FraudWrite },
                IdentityConstants.Roles.Analyst => new[] { IdentityConstants.Scopes.FraudRead, IdentityConstants.Scopes.FraudWrite },
                IdentityConstants.Roles.System => new[] { IdentityConstants.Scopes.FraudRead },
                _ => new[] { IdentityConstants.Scopes.FraudRead }
            };

            foreach (var scope in scopes)
            {
                claims.Add(new Claim(IdentityConstants.Claims.Scope, scope));
            }
       
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
