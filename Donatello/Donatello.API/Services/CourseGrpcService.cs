using Grpc.Core;
using Microsoft.Extensions.Logging;
using Donatello.API.Grpc;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.API.Grpc.Services;
    
public class CourseGrpcService : CourseService.CourseServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseGrpcService> _logger;

    public CourseGrpcService(IUnitOfWork unitOfWork, ILogger<CourseGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<CourseResponse> CreateCourse(CreateCourseRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating new course: {Name}", request.Name);

            var course = new Course
            {
                Name = request.Name,
                Description = request.Description,
                LicenseCategory = (LicenseCategory)request.LicenseCategory,
                Price = (decimal)request.Price,
                TheoryHours = request.TheoryHours,
                PracticeHours = request.PracticeHours,
                Duration = request.Duration
            };

            var createdCourse = await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Course created successfully: {CourseId}", createdCourse.Id);

            return MapToCourseResponse(createdCourse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<CourseResponse> GetCourse(GetCourseRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.CourseId, out var courseId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid course ID format"));
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            
            if (course == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Course not found"));
            }

            return MapToCourseResponse(course);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course: {CourseId}", request.CourseId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetAllCoursesResponse> GetActiveCourses(GetActiveCoursesRequest request, ServerCallContext context)
    {
        try
        {
            var courses = await _unitOfWork.Courses.GetActiveCourses();
            
            var response = new GetAllCoursesResponse
            {
                TotalCount = courses.Count(),
                Page = request.Page,
                PageSize = request.PageSize
            };

            // Apply pagination
            var pagedCourses = courses
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            foreach (var course in pagedCourses)
            {
                response.Courses.Add(MapToCourseResponse(course));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active courses");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetAllCoursesResponse> GetCoursesByCategory(GetCoursesByCategoryRequest request, ServerCallContext context)
    {
        try
        {
            var category = (LicenseCategory)request.LicenseCategory;
            var courses = await _unitOfWork.Courses.GetByLicenseCategoryAsync(category);
            
            var response = new GetAllCoursesResponse
            {
                TotalCount = courses.Count()
            };

            foreach (var course in courses)
            {
                response.Courses.Add(MapToCourseResponse(course));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses by category: {Category}", request.LicenseCategory);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    // Helper Method
    private CourseResponse MapToCourseResponse(Course course)
    {
        return new CourseResponse
        {
            CourseId = course.Id.ToString(),
            Name = course.Name,
            Description = course.Description,
            LicenseCategory = (int)course.LicenseCategory,
            Price = (double)course.Price,
            TheoryHours = course.TheoryHours,
            PracticeHours = course.PracticeHours,
            Duration = course.Duration,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}
