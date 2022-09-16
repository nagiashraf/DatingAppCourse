using API.DTOs;
using API.Entities;
using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IUserRepository
{
    Task UpdateAsync(AppUser user);
    Task<Photo> AddPhotoAsync(AppUser user, ImageUploadResult imageUploadResult);
    Task<Photo> SetMainPhotoAsync(AppUser user, int photoId);
    Task DeletePhotoAsync(AppUser user, Photo photo);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser> GetUserByIdAsync(int id);
    Task<AppUser> GetUserByUsernameAsync(string username);
    Task DeleteAsync(int id);
    Task<IEnumerable<AppUser>> SearchAsync(string username, string gender);
    Task<IEnumerable<MemberDto>> GetMembersAsync();
    Task<MemberDto> GetMemberByUsernameAsync(string username);

}