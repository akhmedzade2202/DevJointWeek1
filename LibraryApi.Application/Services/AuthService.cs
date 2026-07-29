using LibraryApi.Application.DTOs.Auth;
using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Domain.Entities;
using LibraryApi.Domain.Enums;

namespace LibraryApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.UsernameExistsAsync(dto.Username))
            throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");

        if (await _userRepository.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = Role.User, // default rol
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.AddAsync(user);
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(created);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = created.Username,
            Role = created.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username);
        if (user == null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }
}