using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.Infrastructure;
public class CourseRepository : BaseRepository<Course>, ICourseRepository
{
    public CourseRepository(DonatelloDbContext context, ILogger<CourseRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<Course>> GetByLicenseCategoryAsync(LicenseCategory category)
    {
        try
        {
            return await _dbSet
                .Where(c => c.LicenseCategory == category && c.IsActive)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses by license category: {Category}", category);
            throw;
        }
    }

    public async Task<IEnumerable<Course>> GetActiveCourses()
    {
        try
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active courses");
            throw;
        }
    }

    public async Task<IEnumerable<Course>> GetCoursesByNameAsync(string name)
    {
        return await _context.Courses
            .Where(c => c.Name.Contains(name))
            .ToListAsync();
    }
}