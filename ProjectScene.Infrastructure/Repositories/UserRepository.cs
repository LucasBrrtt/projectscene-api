using Microsoft.AspNetCore.Identity;
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
        var passwordHasher = new PasswordHasher<User>();

        // gera hash a partir da senha digitada
        user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public bool VerifyPassword(User user, string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success;
    }
}
