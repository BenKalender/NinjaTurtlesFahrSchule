using Donatello.Core.Interfaces;
using Donatello.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Donatello.API.Grpc.Services;

namespace Donatello;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Database
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<DonatelloDbContext>(options =>
            options.UseNpgsql(connectionString, b => 
                b.MigrationsAssembly("Donatello.Infrastructure")));

        // Repository Pattern DI
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        // Base Repository for generic usage
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Business Services (will be created later)
        // services.AddScoped<IStudentService, StudentService>();
        // services.AddScoped<ICourseService, CourseService>();
        // services.AddScoped<IEnrollmentService, EnrollmentService>();
        // services.AddScoped<IPaymentService, PaymentService>();

        // gRPC Services Registration
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true; // Only for development
            options.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4MB
            options.MaxSendMessageSize = 4 * 1024 * 1024;    // 4MB
        });

        // Logging - Serilog
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()            
            .CreateLogger();
        /*
        .WriteTo.PostgreSQL(
            connectionString,
            tableName: "Logs",
            needAutoCreateTable: true)
        */
            
        // ⭐ BU SATIRI EKLE ⭐
        services.AddControllers();
        
        // gRPC Configuration
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true; // Only for development
        });
        services.AddGrpcReflection(); // For development/testing

        // Health Checks
        //services.AddHealthChecks().AddDbContextCheck<DonatelloDbContext>();

        // CORS (if needed for web clients)
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Exception Handling Middleware
        app.UseMiddleware<GlobalExceptionHandler>();
        
        // CORS
        app.UseCors();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            // gRPC services will be mapped here
            endpoints.MapGrpcService<StudentGrpcService>();
            endpoints.MapGrpcService<CourseGrpcService>();
            endpoints.MapGrpcService<EnrollmentGrpcService>();
            endpoints.MapGrpcService<PaymentGrpcService>();

            // Health Check endpoint
            //endpoints.MapHealthChecks("/health");
            
            // gRPC Reflection for development/testing
            if (env.IsDevelopment())
            {
                endpoints.MapGrpcReflectionService();
            }

            // ⭐ BU SATIRI EKLE ⭐
            endpoints.MapControllers();
        });

        // Auto-migrate database in development
        if (env.IsDevelopment())
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DonatelloDbContext>();
            context.Database.Migrate();
        }
    }
}