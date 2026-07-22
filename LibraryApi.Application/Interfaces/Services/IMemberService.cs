using LibraryApi.Application.DTOs.Member;

namespace LibraryApi.Application.Interfaces.Services;

public interface IMemberService
{
    Task<IEnumerable<MemberDto>> GetAllAsync();
    Task<MemberDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<bool> UpdateAsync(int id, UpdateMemberDto dto);
    Task<bool> DeleteAsync(int id);
}