using System;
using System.Collections.Generic;

namespace Donatello.Core.Models;
public class Student : BaseEntity
{
    public Guid UserId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}