using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Donatello.Core.Enums;
using Donatello.Core.Models;

namespace Donatello.Core.Interfaces;

public interface ICourseRepository : IBaseRepository<Course>
{
    Task<IEnumerable<Course>> GetByLicenseCategoryAsync(LicenseCategory category);
    Task<IEnumerable<Course>> GetActiveCourses();
}