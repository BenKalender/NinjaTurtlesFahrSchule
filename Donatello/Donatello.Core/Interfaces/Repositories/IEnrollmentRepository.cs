using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Donatello.Core.Models;

namespace Donatello.Core.Interfaces;

public interface IEnrollmentRepository : IBaseRepository<Enrollment>
{
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
    Task<Enrollment?> GetWithPaymentsAsync(Guid enrollmentId);
}