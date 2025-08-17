using System.Linq.Expressions;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Donatello.Infrastructure;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly DonatelloDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<BaseRepository<T>> _logger;

    public BaseRepository(DonatelloDbContext context, ILogger<BaseRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by id: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities");
            throw;
        }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities with predicate");
            throw;
        }
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await _dbSet.AddAsync(entity);
            _logger.LogInformation("Entity added: {EntityType} - {Id}", typeof(T).Name, entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity: {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public async Task<T> UpdateAsync(T entity)
    {
        try
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Entity updated: {EntityType} - {Id}", typeof(T).Name, entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity: {EntityType} - {Id}", typeof(T).Name, entity.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            _dbSet.Remove(entity);
            _logger.LogInformation("Entity deleted: {EntityType} - {Id}", typeof(T).Name, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity: {EntityType} - {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(entity);
            
            _logger.LogInformation("Entity soft deleted: {EntityType} - {Id}", typeof(T).Name, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting entity: {EntityType} - {Id}", typeof(T).Name, id);
            throw;
        }
    }
}