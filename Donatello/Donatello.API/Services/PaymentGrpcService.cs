using Grpc.Core;
using Microsoft.Extensions.Logging;
using Donatello.API.Grpc;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.API.Grpc.Services;

public class PaymentGrpcService : PaymentService.PaymentServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentGrpcService> _logger;

    public PaymentGrpcService(IUnitOfWork unitOfWork, ILogger<PaymentGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<PaymentResponse> CreatePayment(CreatePaymentRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating payment for enrollment: {EnrollmentId}", request.EnrollmentId);

            if (!Guid.TryParse(request.EnrollmentId, out var enrollmentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid enrollment ID format"));
            }

            await _unitOfWork.BeginTransactionAsync();

            var payment = new Payment
            {
                EnrollmentId = enrollmentId,
                Amount = (decimal)request.Amount,
                PaymentType = (PaymentType)request.PaymentType,
                TransactionId = request.TransactionId,
                PaymentGateway = request.PaymentGateway,
                Status = PaymentStatus.Pending
            };

            var createdPayment = await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Load with enrollment data
            var paymentWithData = (await _unitOfWork.Payments.GetByEnrollmentIdAsync(enrollmentId))
                .FirstOrDefault(p => p.Id == createdPayment.Id);

            return MapToPaymentResponse(paymentWithData!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating payment");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<PaymentResponse> ProcessPayment(ProcessPaymentRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.PaymentId, out var paymentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid payment ID format"));
            }

            await _unitOfWork.BeginTransactionAsync();

            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Payment not found"));
            }

            // Update payment status
            payment.Status = PaymentStatus.Completed;
            payment.TransactionId = request.TransactionId;
            payment.PaymentGateway = request.PaymentGateway;
            payment.PaidAt = DateTime.UtcNow;

            await _unitOfWork.Payments.UpdateAsync(payment);

            // Update enrollment paid amount
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(payment.EnrollmentId);
            if (enrollment != null)
            {
                enrollment.PaidAmount += payment.Amount;
                
                // Check if fully paid
                if (enrollment.PaidAmount >= enrollment.TotalAmount)
                {
                    enrollment.Status = EnrollmentStatus.Active;
                }
                
                await _unitOfWork.Enrollments.UpdateAsync(enrollment);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Return updated payment
            var updatedPayment = (await _unitOfWork.Payments.GetByEnrollmentIdAsync(payment.EnrollmentId))
                .FirstOrDefault(p => p.Id == paymentId);

            return MapToPaymentResponse(updatedPayment!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing payment: {PaymentId}", request.PaymentId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetPendingPaymentsResponse> GetPendingPayments(GetPendingPaymentsRequest request, ServerCallContext context)
    {
        try
        {
            var pendingPayments = await _unitOfWork.Payments.GetPendingPaymentsAsync();
            
            var response = new GetPendingPaymentsResponse
            {
                TotalCount = pendingPayments.Count(),
                Page = request.Page,
                PageSize = request.PageSize
            };

            var pagedPayments = pendingPayments
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            foreach (var payment in pagedPayments)
            {
                response.Payments.Add(MapToPaymentResponse(payment));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    // Helper Method
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