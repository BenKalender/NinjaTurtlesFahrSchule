using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Donatello.Core.Interfaces;


namespace Donatello.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly DonatelloDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        DonatelloDbContext context, 
        ILogger<UnitOfWork> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    // Lazy-loaded repositories using DI
    private IUserRepository? _users;
    private IStudentRepository? _students;
    private ICourseRepository? _courses;
    private IEnrollmentRepository? _enrollments;
    private IPaymentRepository? _payments;

    public IUserRepository Users => 
        _users ??= _serviceProvider.GetRequiredService<IUserRepository>();
        
    public IStudentRepository Students =>
        _students ??= _serviceProvider.GetRequiredService<IStudentRepository>();
    
    public ICourseRepository Courses => 
        _courses ??= _serviceProvider.GetRequiredService<ICourseRepository>();
    
    public IEnrollmentRepository Enrollments => 
        _enrollments ??= _serviceProvider.GetRequiredService<IEnrollmentRepository>();
    
    public IPaymentRepository Payments => 
        _payments ??= _serviceProvider.GetRequiredService<IPaymentRepository>();

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Changes saved to database: {Count} entities", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        _logger.LogInformation("Database transaction started");
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            _logger.LogInformation("Database transaction committed");
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _logger.LogInformation("Database transaction rolled back");
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}