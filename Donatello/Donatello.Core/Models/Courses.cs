using System;
using System.Collections.Generic;
using Donatello.Core.Enums;

namespace Donatello.Core.Models;
public class Course : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LicenseCategory LicenseCategory { get; set; }
    public decimal Price { get; set; }
    public int TheoryHours { get; set; }
    public int PracticeHours { get; set; }
    public int Duration { get; set; } // Days
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}