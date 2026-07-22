using LibraryApi.Application.DTOs.Member;
using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<MemberDto>> GetAllAsync()
    {
        var members = await _memberRepository.GetAllAsync();
        return members.Select(MapToDto);
    }

    public async Task<MemberDto?> GetByIdAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        return member == null ? null : MapToDto(member);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var emailExists = await _memberRepository.EmailExistsAsync(dto.Email);
        if (emailExists)
            throw new InvalidOperationException($"Member with email {dto.Email} already exists.");

        var member = new Member
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            MembershipDate = DateTime.UtcNow
        };
        var created = await _memberRepository.AddAsync(member);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null) return false;

        var emailExists = await _memberRepository.EmailExistsAsync(dto.Email, id);
        if (emailExists)
            throw new InvalidOperationException($"Member with email {dto.Email} already exists.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;

        await _memberRepository.UpdateAsync(member);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null) return false;

        await _memberRepository.DeleteAsync(member);
        return true;
    }

    private static MemberDto MapToDto(Member member) => new()
    {
        Id = member.Id,
        FirstName = member.FirstName,
        LastName = member.LastName,
        Email = member.Email,
        MembershipDate = member.MembershipDate
    };
}