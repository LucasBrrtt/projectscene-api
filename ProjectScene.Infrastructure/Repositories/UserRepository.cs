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
        // Salva a entidade e confirma a alteracao no banco.
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        // Usa AnyAsync para consultar apenas existencia.
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        // Usa AnyAsync para consultar apenas existencia.
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task UpdateAsync(User user)
    {
        // Reaproveita a entidade rastreada quando disponivel para evitar regravar campos nao alterados.
        var entry = _context.Entry(user);

        if (entry.State == EntityState.Detached)
        {
            _context.Users.Attach(user);
        }

        await _context.SaveChangesAsync();
    }
    
    public bool VerifyPassword(User user, string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        // So aceita senha validada com sucesso pelo hasher.
        return result == PasswordVerificationResult.Success;
    }
}
