using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums; 

namespace Donatello.Infrastructure;
public class EnrollmentRepository : BaseRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(DonatelloDbContext context, ILogger<EnrollmentRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _dbSet
                .Include(e => e.Course)
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollments by student id: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        try
        {
            return await _dbSet
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollments by course id: {CourseId}", courseId);
            throw;
        }
    }

    public async Task<Enrollment?> GetWithPaymentsAsync(Guid enrollmentId)
    {
        try
        {
            return await _dbSet
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Payments)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollment with payments: {EnrollmentId}", enrollmentId);
            throw;
        }
    }
}