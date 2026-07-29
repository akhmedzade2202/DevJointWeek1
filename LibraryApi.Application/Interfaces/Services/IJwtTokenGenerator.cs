using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
