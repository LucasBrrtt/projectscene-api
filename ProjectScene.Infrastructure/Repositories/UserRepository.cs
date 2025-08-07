using Microsoft.EntityFrameworkCore;
using ProjectScene.Domain.Entities;
using ProjectScene.Domain.Interfaces;
using ProjectScene.Infrastructure.Data;

namespace ProjectScene.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}
