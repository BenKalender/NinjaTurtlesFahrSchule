using Donatello.Core.Models;

namespace Donatello.Core.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByTCNumberAsync(string tcNumber);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> TCNumberExistsAsync(string tcNumber);
    Task<User?> AuthenticateAsync(string email, string passwordHash);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}