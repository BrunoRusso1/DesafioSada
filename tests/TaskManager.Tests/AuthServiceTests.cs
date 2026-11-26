using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Options;
using TaskManager.Application.Services;

namespace TaskManager.Tests;

public class AuthServiceTests
{
    private readonly Mock<ITokenGenerator> _tokenGenerator = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var admin = Options.Create(new AdminOptions { Username = "admin", Password = "secret" });
        _service = new AuthService(admin, _tokenGenerator.Object, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsToken()
    {
        _tokenGenerator
            .Setup(generator => generator.Generate("admin"))
            .Returns(new TokenResponseDto { AccessToken = "token" });

        var result = _service.Login(new LoginDto { Username = "admin", Password = "secret" });

        Assert.Equal("token", result.AccessToken);
    }

    [Theory]
    [InlineData("admin", "wrong")]
    [InlineData("other", "secret")]
    public void Login_InvalidCredentials_ThrowsUnauthorizedException(string username, string password)
    {
        Assert.Throws<UnauthorizedException>(() => _service.Login(new LoginDto { Username = username, Password = password }));
    }
}
