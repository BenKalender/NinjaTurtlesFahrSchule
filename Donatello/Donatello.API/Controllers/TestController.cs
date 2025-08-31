// Donatello.API/Controllers/TestController.cs
using Microsoft.AspNetCore.Mvc;
using Donatello.Core.Interfaces;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var courses = await _unitOfWork.Courses.GetAllAsync();
                return Ok(new 
                { 
                    Status = "Healthy",
                    CoursesCount = courses.Count(),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("create-test-course")]
        public async Task<IActionResult> CreateTestCourse()
        {
            try
            {
                var course = new Course
                {
                    Name = "Test B Sınıfı Kursu",
                    Description = "Test için oluşturulan kurs",
                    LicenseCategory = LicenseCategory.B,
                    Price = 3000m,
                    TheoryHours = 24,
                    PracticeHours = 16,
                    Duration = 45
                };

                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { Message = "Test course created!", CourseId = course.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}