using Grpc.Core;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.API.Grpc.Services;

public class EnrollmentGrpcService : EnrollmentService.EnrollmentServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnrollmentGrpcService> _logger;

    public EnrollmentGrpcService(IUnitOfWork unitOfWork, ILogger<EnrollmentGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<EnrollmentResponse> CreateEnrollment(CreateEnrollmentRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating enrollment for student: {StudentId}, course: {CourseId}", 
                request.StudentId, request.CourseId);

            if (!Guid.TryParse(request.StudentId, out var studentId) || 
                !Guid.TryParse(request.CourseId, out var courseId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));
            }

            await _unitOfWork.BeginTransactionAsync();

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                TotalAmount = (decimal)request.TotalAmount,
                Status = EnrollmentStatus.PreRegistered
            };

            var createdEnrollment = await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Load related data for response
            var enrollmentWithData = await _unitOfWork.Enrollments.GetWithPaymentsAsync(createdEnrollment.Id);

            return MapToEnrollmentResponse(enrollmentWithData!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating enrollment");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetEnrollmentsByStudentResponse> GetEnrollmentsByStudent(GetEnrollmentsByStudentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.StudentId, out var studentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid student ID format"));
            }

            var enrollments = await _unitOfWork.Enrollments.GetByStudentIdAsync(studentId);
            
            var response = new GetEnrollmentsByStudentResponse
            {
                TotalCount = enrollments.Count(),
                Page = request.Page,
                PageSize = request.PageSize
            };

            var pagedEnrollments = enrollments
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            foreach (var enrollment in pagedEnrollments)
            {
                response.Enrollments.Add(MapToEnrollmentResponse(enrollment));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollments for student: {StudentId}", request.StudentId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<EnrollmentWithPaymentsResponse> GetEnrollmentWithPayments(GetEnrollmentWithPaymentsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.EnrollmentId, out var enrollmentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid enrollment ID format"));
            }

            var enrollment = await _unitOfWork.Enrollments.GetWithPaymentsAsync(enrollmentId);
            
            if (enrollment == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Enrollment not found"));
            }

            var response = new EnrollmentWithPaymentsResponse
            {
                Enrollment = MapToEnrollmentResponse(enrollment)
            };

            foreach (var payment in enrollment.Payments)
            {
                response.Payments.Add(MapToPaymentResponse(payment));
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollment with payments: {EnrollmentId}", request.EnrollmentId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    // Helper Methods
    private EnrollmentResponse MapToEnrollmentResponse(Enrollment enrollment)
    {
        return new EnrollmentResponse
        {
            EnrollmentId = enrollment.Id.ToString(),
            StudentId = enrollment.StudentId.ToString(),
            CourseId = enrollment.CourseId.ToString(),
            StudentName = $"{enrollment.Student?.User?.FirstName} {enrollment.Student?.User?.LastName}".Trim(),
            CourseName = enrollment.Course?.Name ?? "",
            LicenseCategory = (int)(enrollment.Course?.LicenseCategory ?? LicenseCategory.B),
            EnrollmentDate = enrollment.EnrollmentDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Status = (int)enrollment.Status,
            TotalAmount = (double)enrollment.TotalAmount,
            PaidAmount = (double)enrollment.PaidAmount,
            RemainingAmount = (double)(enrollment.TotalAmount - enrollment.PaidAmount),
            CompletionDate = enrollment.CompletionDate?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "",
            CreatedAt = enrollment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private PaymentResponse MapToPaymentResponse(Payment payment)
    {
        return new PaymentResponse
        {
            PaymentId = payment.Id.ToString(),
            EnrollmentId = payment.EnrollmentId.ToString(),
            StudentName = $"{payment.Enrollment?.Student?.User?.FirstName} {payment.Enrollment?.Student?.User?.LastName}".Trim(),
            CourseName = payment.Enrollment?.Course?.Name ?? "",
            Amount = (double)payment.Amount,
            PaymentType = (int)payment.PaymentType,
            Status = (int)payment.Status,
            TransactionId = payment.TransactionId ?? "",
            PaymentGateway = payment.PaymentGateway ?? "",
            PaidAt = payment.PaidAt?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "",
            CreatedAt = payment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}