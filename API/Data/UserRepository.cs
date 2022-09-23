using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public UserRepository(DataContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
        
    }
    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        var users = await _context.Users
        .Include(u => u.Photos)
        .ToListAsync();
        return users;
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
        return user;
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == username);
        return user;
    }
    
    public async Task UpdateAsync(AppUser user)
    {
        var entityEntry = _context.Users.Attach(user);
        entityEntry.State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }
    
    public async Task<Photo> AddPhotoAsync(AppUser user, ImageUploadResult imageUploadResult)
    {
        var photo = new Photo {
            PublicId = imageUploadResult.PublicId,
            Url = imageUploadResult.SecureUrl.AbsoluteUri
        };

        if(user.Photos.Count ==0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        await _context.SaveChangesAsync();

        return photo;
    }

    public async Task<Photo> SetMainPhotoAsync(AppUser user, int photoId)
    {
        var newMain = user.Photos.FirstOrDefault(ph => ph.Id == photoId);
        if(newMain == null) return null;
        
        
        var currentMain = user.Photos.FirstOrDefault(ph => ph.IsMain);
        if(currentMain == null) return null;

        currentMain.IsMain = false;
        newMain.IsMain = true;

        await _context.SaveChangesAsync();

        return newMain;
    }

    public async Task DeletePhotoAsync(AppUser user, Photo photo)
    {
        user.Photos.Remove(photo);

        await _context.SaveChangesAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var dobMin = DateTime.Today.AddYears(-userParams.MaxAge - 1).AddDays(1);
        var dobMax = DateTime.Today.AddYears(-userParams.MinAge);

        IQueryable<MemberDto> users = _context.Users
            .Where(u => u.UserName != userParams.CurrentUsername)
            .Where(u => u.Gender == userParams.Gender)
            .Where(u => u.DateOfBirth >= dobMin && u.DateOfBirth <= dobMax)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .AsNoTracking();

        users = userParams.OrderBy switch
        {
            "created" => users.OrderByDescending(u => u.Created),
            _ => users.OrderByDescending(u => u.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(users, userParams.PageIndex, userParams.PageSize);
    }

    public async Task<MemberDto> GetMemberByUsernameAsync(string username)
    {
        var user = await _context.Users
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(u => u.Username == username);
        return user;
    }
}