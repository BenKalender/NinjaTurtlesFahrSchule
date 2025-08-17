using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.Infrastructure;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(DonatelloDbContext context, ILogger<PaymentRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<Payment>> GetByEnrollmentIdAsync(Guid enrollmentId)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Enrollment)
                .ThenInclude(e => e.Student)
                .ThenInclude(s => s.User)
                .Where(p => p.EnrollmentId == enrollmentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments by enrollment id: {EnrollmentId}", enrollmentId);
            throw;
        }
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
    {
        try
        {
            return await _dbSet
                .Include(p => p.Enrollment)
                .ThenInclude(e => e.Student)
                .ThenInclude(s => s.User)
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            throw;
        }
    }
}