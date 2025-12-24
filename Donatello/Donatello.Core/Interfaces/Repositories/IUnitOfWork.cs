namespace Donatello.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    IEnrollmentRepository Enrollments { get; }
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}