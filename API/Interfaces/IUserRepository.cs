using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    public Task UpdateAsync(MemberUpdateDto memberUpdateDto, AppUser user);
    public Task<IEnumerable<AppUser>> GetUsersAsync();
    public Task<AppUser> GetUserByIdAsync(int id);
    public Task<AppUser> GetUserByUsernameAsync(string username);
    public Task DeleteAsync(int id);
    public Task<IEnumerable<AppUser>> SearchAsync(string username, string gender);

    public Task<IEnumerable<MemberDto>> GetMembersAsync();
    public Task<MemberDto> GetMemberByUsernameAsync(string username);

}