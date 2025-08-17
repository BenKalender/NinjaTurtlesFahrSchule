using System;
using System.Collections.Generic;
using Donatello.Core.Enums;

namespace Donatello.Core.Models;
public class Payment : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Navigation
    public virtual Enrollment Enrollment { get; set; } = null!;
}