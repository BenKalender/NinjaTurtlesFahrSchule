using Grpc.Core;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.API.Helpers;

namespace Donatello.API.Grpc.Services;

public class StudentGrpcService : StudentService.StudentServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentGrpcService> _logger;

    public StudentGrpcService(IUnitOfWork unitOfWork, ILogger<StudentGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<StudentResponse> CreateStudent(CreateStudentRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating new student: {Email}", request.Email);

            var dateOfBirthUTC = DateHelper.ParseToUtcDateTime(request.DateOfBirth);

            if (dateOfBirthUTC == null)
            {
                // Dönüştürme başarısız oldu, uygun bir hata döndürün.
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid date of birth format."));
            }
            
            await _unitOfWork.BeginTransactionAsync();

            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email already exists"));
            }

            if (await _unitOfWork.Users.TCNumberExistsAsync(request.TcNumber))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "TC Number already exists"));
            }


            // Create User first
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                TCNumber = request.TcNumber,
                DateOfBirth = dateOfBirthUTC.Value,
                PasswordHash = "temp_hash"  + Guid.NewGuid().ToString(), // TODO: Implement proper password hashing
                IsActive = true
            };

            // Note: You'll need a User repository too
            var createdUser = await _unitOfWork.Users.AddAsync(user);

            // Create Student
            var student = new Student
            {
                UserId = user.Id,
                StudentNumber = GenerateStudentNumber(),
                Address = request.Address,
                EmergencyContact = request.EmergencyContact,
                EmergencyPhone = request.EmergencyPhone
            };

            var createdStudent = await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Student created successfully: {StudentId}", createdStudent.Id);

            // ✅ User bilgilerini dahil etmek için tekrar yükle
            var studentWithUser = await _unitOfWork.Students.GetByIdWithUserAsync(createdStudent.Id);
            return MapToStudentResponse(createdStudent);
        }
        catch (RpcException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating student");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<StudentResponse> GetStudent(GetStudentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.StudentId, out var studentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid student ID format"));
            }

            var student = await _unitOfWork.Students.GetByIdWithUserAsync(studentId);
            
            if (student == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Student not found"));
            }

            return MapToStudentResponse(student);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student: {StudentId}", request.StudentId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<StudentResponse> GetStudentByNumber(GetStudentByNumberRequest request, ServerCallContext context)
    {
        try
        {
            var student = await _unitOfWork.Students.GetByStudentNumberAsync(request.StudentNumber);
            
            if (student == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Student not found"));
            }

            return MapToStudentResponse(student);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student by number: {StudentNumber}", request.StudentNumber);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetAllStudentsResponse> GetAllStudents(GetAllStudentsRequest request, ServerCallContext context)
    {
        try
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            
            var response = new GetAllStudentsResponse
            {
                TotalCount = students.Count(),
                Page = request.Page,
                PageSize = request.PageSize
            };

            // Apply pagination
            var pagedStudents = students
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            foreach (var student in pagedStudents)
            {
                response.Students.Add(MapToStudentResponse(student));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all students");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<DeleteStudentResponse> DeleteStudent(DeleteStudentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.StudentId, out var studentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid student ID format"));
            }

            var success = await _unitOfWork.Students.SoftDeleteAsync(studentId);
            await _unitOfWork.SaveChangesAsync();

            return new DeleteStudentResponse
            {
                Success = success,
                Message = success ? "Student deleted successfully" : "Student not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student: {StudentId}", request.StudentId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    // Helper Methods
    private StudentResponse MapToStudentResponse(Student student)
    {
        return new StudentResponse
        {
            StudentId = student.Id.ToString(),
            UserId = student.UserId.ToString(),
            StudentNumber = student.StudentNumber,
            Email = student.User?.Email ?? "",
            FirstName = student.User?.FirstName ?? "",
            LastName = student.User?.LastName ?? "",
            PhoneNumber = student.User?.PhoneNumber ?? "",
            TcNumber = student.User?.TCNumber ?? "",
            DateOfBirth = student.User?.DateOfBirth.ToString("yyyy-MM-dd") ?? "",
            Address = student.Address,
            EmergencyContact = student.EmergencyContact,
            EmergencyPhone = student.EmergencyPhone,
            CreatedAt = student.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsActive = student.User?.IsActive ?? false
        };
    }

    private string GenerateStudentNumber()
    {
        // Simple student number generation
        var year = DateTime.Now.Year;
        var random = new Random().Next(1000, 9999);
        return $"STD{year}{random}";
    }
}
