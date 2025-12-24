using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Donatello.Infrastructure;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(DonatelloDbContext context, ILogger<UserRepository> logger) 
        : base(context, logger) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Getting user by email: {Email}", email);
            
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetByTCNumberAsync(string tcNumber)
    {
        try
        {
            _logger.LogInformation("Getting user by TC number: {TCNumber}", tcNumber);
            
            return await _dbSet
                .FirstOrDefaultAsync(u => u.TCNumber == tcNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by TC number: {TCNumber}", tcNumber);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            throw;
        }
    }

    public async Task<bool> TCNumberExistsAsync(string tcNumber)
    {
        try
        {
            return await _dbSet.AnyAsync(u => u.TCNumber == tcNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking TC number existence: {TCNumber}", tcNumber);
            throw;
        }
    }

    public async Task<User?> AuthenticateAsync(string email, string passwordHash)
    {
        try
        {
            _logger.LogInformation("Authenticating user: {Email}", email);
            
            return await _dbSet
                .FirstOrDefaultAsync(u => 
                    u.Email == email && 
                    u.PasswordHash == passwordHash && 
                    u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        try
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            throw;
        }
    }
}