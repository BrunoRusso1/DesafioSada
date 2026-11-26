using TaskManager.Application.Dtos;

namespace TaskManager.Application.Interfaces;

public interface ITokenGenerator
{
    TokenResponseDto Generate(string username);
}
