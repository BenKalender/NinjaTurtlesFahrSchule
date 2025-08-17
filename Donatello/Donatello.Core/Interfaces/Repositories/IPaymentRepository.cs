using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Donatello.Core.Models;

namespace Donatello.Core.Interfaces;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByEnrollmentIdAsync(Guid enrollmentId);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
}