// filepath: /home/ubu/Projects/NinjaTurtlesFahrSchule/Donatello/Donatello.Tests/UnitTests/API/Services/StudentGrpcServiceTests.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Donatello.API.Grpc.Services;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using FluentAssertions;

namespace Donatello.Tests.UnitTests.API.Services;

public class StudentGrpcServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentGrpcService> _logger;
    private readonly StudentGrpcService _studentGrpcService;

    public StudentGrpcServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<StudentGrpcService>>();
        _studentGrpcService = new StudentGrpcService(_unitOfWork, _logger);
    }

    [Fact]
    public async Task CreateStudent_ValidRequest_ReturnsStudentResponse()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "123-456-7890",
            TcNumber = "12345678901",
            DateOfBirth = "2000-01-01",
            Address = "123 Main St",
            EmergencyContact = "Jane Doe",
            EmergencyPhone = "987-654-3210"
        };

        var student = new Student { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StudentNumber = "STD20241234" };

        _unitOfWork.Students.AddAsync(Arg.Any<Student>()).Returns(student);
        _unitOfWork.SaveChangesAsync().Returns(1);
        _unitOfWork.BeginTransactionAsync().Returns(Task.CompletedTask);
        _unitOfWork.CommitTransactionAsync().Returns(Task.CompletedTask);

        // Act
        var response = await _studentGrpcService.CreateStudent(request, TestServerCallContext.Default);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(student.Id.ToString(), response.StudentId);
        _unitOfWork.Students.Received(1).AddAsync(Arg.Any<Student>());
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _unitOfWork.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task CreateStudent_InvalidDateOfBirth_ThrowsRpcException()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "123-456-7890",
            TcNumber = "12345678901",
            DateOfBirth = "invalid-date",
            Address = "123 Main St",
            EmergencyContact = "Jane Doe",
            EmergencyPhone = "987-654-3210"
        };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _studentGrpcService.CreateStudent(request, TestServerCallContext.Default));
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
        await _unitOfWork.DidNotReceive().CommitTransactionAsync();
        await _unitOfWork.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task GetStudent_ValidStudentId_ReturnsStudentResponse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student { Id = studentId, UserId = Guid.NewGuid(), StudentNumber = "STD20241234", User = new User { Email = "test@example.com", FirstName = "Test", LastName = "User", PhoneNumber = "123-456-7890", TCNumber = "12345678901", DateOfBirth = DateTime.Now } };

        _unitOfWork.Students.GetByIdAsync(studentId).Returns(student);

        var request = new GetStudentRequest { StudentId = studentId.ToString() };

        // Act
        var response = await _studentGrpcService.GetStudent(request, TestServerCallContext.Default);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(studentId.ToString(), response.StudentId);
    }

    [Fact]
    public async Task GetStudent_InvalidStudentId_ThrowsRpcException()
    {
        // Arrange
        var request = new GetStudentRequest { StudentId = "invalid-guid" };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _studentGrpcService.GetStudent(request, TestServerCallContext.Default));
    }

    [Fact]
    public async Task GetStudent_StudentNotFound_ThrowsRpcException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _unitOfWork.Students.GetByIdAsync(studentId).Returns((Student)null);

        var request = new GetStudentRequest { StudentId = studentId.ToString() };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _studentGrpcService.GetStudent(request, TestServerCallContext.Default));
    }

    [Fact]
    public async Task GetStudentByNumber_ValidStudentNumber_ReturnsStudentResponse()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StudentNumber = "STD20241234", User = new User { Email = "test@example.com", FirstName = "Test", LastName = "User", PhoneNumber = "123-456-7890", TCNumber = "12345678901", DateOfBirth = DateTime.Now } };
        _unitOfWork.Students.GetByStudentNumberAsync("STD20241234").Returns(student);

        var request = new GetStudentByNumberRequest { StudentNumber = "STD20241234" };

        // Act
        var response = await _studentGrpcService.GetStudentByNumber(request, TestServerCallContext.Default);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(student.StudentNumber, response.StudentNumber);
    }

    [Fact]
    public async Task GetStudentByNumber_StudentNotFound_ThrowsRpcException()
    {
        // Arrange
        _unitOfWork.Students.GetByStudentNumberAsync("STD20241234").Returns((Student)null);
        var request = new GetStudentByNumberRequest { StudentNumber = "STD20241234" };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _studentGrpcService.GetStudentByNumber(request, TestServerCallContext.Default));
    }

    [Fact]
    public async Task GetAllStudents_ReturnsAllStudentsResponse()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StudentNumber = "STD20241234", User = new User { Email = "test1@example.com" } },
            new Student { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StudentNumber = "STD20245678", User = new User { Email = "test2@example.com" } }
        };

        _unitOfWork.Students.GetAllAsync().Returns(students);

        var request = new GetAllStudentsRequest { Page = 1, PageSize = 10 };

        // Act
        var response = await _studentGrpcService.GetAllStudents(request, TestServerCallContext.Default);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Students.Count);
    }

    [Fact]
    public async Task DeleteStudent_ValidStudentId_ReturnsSuccess()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _unitOfWork.Students.SoftDeleteAsync(studentId).Returns(true);
        _unitOfWork.SaveChangesAsync().Returns(1);

        var request = new DeleteStudentRequest { StudentId = studentId.ToString() };

        // Act
        var response = await _studentGrpcService.DeleteStudent(request, TestServerCallContext.Default);

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Student deleted successfully", response.Message);
    }

    [Fact]
    public async Task DeleteStudent_InvalidStudentId_ThrowsRpcException()
    {
        // Arrange
        var request = new DeleteStudentRequest { StudentId = "invalid-guid" };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _studentGrpcService.DeleteStudent(request, TestServerCallContext.Default));
    }

    [Fact]
    public async Task DeleteStudent_StudentNotFound_ReturnsFailure()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _unitOfWork.Students.SoftDeleteAsync(studentId).Returns(false);

        var request = new DeleteStudentRequest { StudentId = studentId.ToString() };

        // Act
        var response = await _studentGrpcService.DeleteStudent(request, TestServerCallContext.Default);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Student not found", response.Message);
    }
}

// Helper class to mock ServerCallContext
public class TestServerCallContext : ServerCallContext
{
    private readonly Metadata _requestHeaders = new Metadata();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly string _peer = "test";
    private readonly AuthContext _authContext = null; // You might need to create a mock AuthContext if your tests require it

    public static TestServerCallContext Default { get; } = new TestServerCallContext();

    protected override Task WriteResponseHeadersAsync(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }

    protected override Metadata RequestHeadersCore => _requestHeaders;
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override string PeerCore => _peer;
    protected override AuthContext AuthContextCore => _authContext; // Return the mock AuthContext
    protected override IDictionary<object, object> UserStateCore { get; } = new Dictionary<object, object>();
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override CompressionLevel? ResponseCompressionLevelCore { get; set; }
}
