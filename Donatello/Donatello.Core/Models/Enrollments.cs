using System;
using System.Collections.Generic;
using Donatello.Core.Enums;

namespace Donatello.Core.Models;
public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.PreRegistered;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? CompletionDate { get; set; }
    
    // Navigation
    public virtual Student Student { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}