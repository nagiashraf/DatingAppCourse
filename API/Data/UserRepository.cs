using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.Id == id);
        return user;
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == username);
        return user;
    }

    public async Task<IEnumerable<AppUser>> SearchAsync(string username, string gender)
    {
        IQueryable<AppUser> query = _context.Users;

        if (!string.IsNullOrEmpty(username))
        {
            query = query.Where(u => u.UserName.Contains(username));
        }

        if(!string.IsNullOrEmpty(gender))
        {
            query = query.Where(u => u.Gender.Equals(gender));
        }

        return await query.ToListAsync();
    }
    public async Task UpdateAsync(MemberUpdateDto memberUpdateDto, AppUser user)
    {
        _mapper.Map(memberUpdateDto, user);

        var entityEntry = _context.Users.Attach(user);
        entityEntry.State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MemberDto>> GetMembersAsync()
    {
        var users = await _context.Users
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return users;
    }

    public async Task<MemberDto> GetMemberByUsernameAsync(string username)
    {
        var user = await _context.Users
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(u => u.Username == username);
        return user;
    }
}