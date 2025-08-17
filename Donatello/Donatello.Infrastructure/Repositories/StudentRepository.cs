using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;

namespace Donatello.Infrastructure;

public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    public StudentRepository(DonatelloDbContext context, ILogger<StudentRepository> logger) 
        : base(context, logger) { }

    public async Task<Student?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
    }

    public async Task<IEnumerable<Student>> GetStudentsWithEnrollmentsAsync()
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .ToListAsync();
    }
}