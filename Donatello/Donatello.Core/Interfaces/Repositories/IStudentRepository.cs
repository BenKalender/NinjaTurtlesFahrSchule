using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Donatello.Core.Models;

namespace Donatello.Core.Interfaces;

public interface IStudentRepository : IBaseRepository<Student>
{
    Task<Student?> GetByUserIdAsync(Guid userId);
    Task<Student?> GetByStudentNumberAsync(string studentNumber);
    Task<IEnumerable<Student>> GetStudentsWithEnrollmentsAsync();
}