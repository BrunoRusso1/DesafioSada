using TaskManager.Application.Dtos;

namespace TaskManager.Application.Interfaces;

public interface IAuthService
{
    TokenResponseDto Login(LoginDto dto);
}
