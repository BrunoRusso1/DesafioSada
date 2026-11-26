using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Options;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly AdminOptions _admin;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IOptions<AdminOptions> admin, ITokenGenerator tokenGenerator, ILogger<AuthService> logger)
    {
        _admin = admin.Value;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public TokenResponseDto Login(LoginDto dto)
    {
        var validUser = AreEqual(dto.Username, _admin.Username);
        var validPassword = AreEqual(dto.Password, _admin.Password);

        if (!validUser || !validPassword)
        {
            _logger.LogWarning("Failed login attempt for user {Username}", dto.Username);
            throw new UnauthorizedException("Invalid username or password.");
        }

        _logger.LogInformation("User {Username} logged in", dto.Username);
        return _tokenGenerator.Generate(dto.Username);
    }

    private static bool AreEqual(string first, string second)
    {
        var firstHash = SHA256.HashData(Encoding.UTF8.GetBytes(first));
        var secondHash = SHA256.HashData(Encoding.UTF8.GetBytes(second));
        return CryptographicOperations.FixedTimeEquals(firstHash, secondHash);
    }
}
