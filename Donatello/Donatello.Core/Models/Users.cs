using System;
using System.Collections.Generic;

namespace Donatello.Core.Models;
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string TCNumber { get; set; } = string.Empty; // Turkish ID
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}