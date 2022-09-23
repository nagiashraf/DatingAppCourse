using API.DTOs;
using API.Entities;
using API.Helpers;
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
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto> GetMemberByUsernameAsync(string username);

}