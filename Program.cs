using Microsoft.EntityFrameworkCore;
using Api.Models;
using Api.Jobs;
using Api.Services;
using Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            // policy.WithOrigins("http://localhost:8080") // แหล่งที่มาที่จะอนุญาต
            policy.SetIsOriginAllowed(_ => true) 
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

// ลงทะเบียน ApplicationDbContext โดยใช้ SQL Server (หรือฐานข้อมูลที่คุณใช้งาน)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // สำหรับ BackgroundService ใช้เรียก API
// builder.Services.AddHostedService<GateToWorktimeJob>(); // ลงทะเบียน background job
// builder.Services.AddHostedService<WorktimeJob>();
// builder.Services.AddHostedService<CalculatedOTJob>();
// builder.Services.AddHostedService<Api.Services.CalculatedAttendanceJob>();
// builder.Services.AddHostedService<CalculatedAttendanceJob>();
// builder.Services.AddSingleton<AnalyticsRepository>();
builder.Services.AddScoped<AnalyticsRepository>(); // 👈 เพิ่มบรรทัดนี้
// builder.Services.AddScoped<DataMigrationService>();

builder.Services.AddSignalR();

builder.Services.AddScoped<WorktimeService>();
builder.Services.AddScoped<OTService>();
builder.Services.AddScoped<CleanroomEntryService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<ManpowerPlanService>();
builder.Services.AddScoped<ManpowerReqService>();
// builder.Services.AddScoped<DailySimulationService>();

builder.Services.AddSingleton<TransactionSimulatorService>();
builder.Services.AddHostedService(p => p.GetRequiredService<TransactionSimulatorService>());

builder.Services.AddHostedService<AttendanceJob>();
builder.Services.AddHostedService<WorktimeJob>();
// builder.Services.AddHostedService<OTJob>();
builder.Services.AddHostedService<CleanroomEntryJob>();
builder.Services.AddHostedService<ManpowerPlanJob>();
builder.Services.AddHostedService<ManpowerReqJob>();

var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var attendance = scope.ServiceProvider.GetRequiredService<AttendanceService>();
//     await attendance.ProcessAsync();
// }

// using (var scope = app.Services.CreateScope())
// {
//     var worktime = scope.ServiceProvider.GetRequiredService<WorktimeService>();
//     await worktime.CalculateAsync();
// }

// using (var scope = app.Services.CreateScope())
// {
//     var ot = scope.ServiceProvider.GetRequiredService<OTService>();
//     await ot.ProcessAsync();
// }

// using (var scope = app.Services.CreateScope())
// {
//     var cleanroom = scope.ServiceProvider.GetRequiredService<CleanroomEntryService>();
//     await cleanroom.ProcessAsync();
// }

// using (var scope = app.Services.CreateScope())
// {
//     var sim = scope.ServiceProvider
//         .GetRequiredService<DailySimulationService>();

//     await sim.RunNextDayAsync();
// }ฃ

// using (var scope = app.Services.CreateScope())
// {
//     var service = scope.ServiceProvider.GetRequiredService<ManpowerPlanService>();

//     await service.GeneratePlanAsync(
//         new DateTime(2025, 8, 15),
//         new DateTime(2026, 2, 15)
//     );
// }

// using (var scope = app.Services.CreateScope())
// {
//     var service = scope.ServiceProvider.GetRequiredService<ManpowerPlanService>();
//     await service.GeneratePlanAsync(
//         new DateTime(2025, 8, 15),
//         new DateTime(2026, 12, 31)  // 🔥 ขยายไปถึงสิ้นปี
//     );
// }

// 🔥 อัพเดต ActualHeadcount ก่อน
// using (var scope = app.Services.CreateScope())
// {
//     var manpowerPlan = scope.ServiceProvider.GetRequiredService<ManpowerPlanService>();
//     await manpowerPlan.UpdateActualHeadcountAsync();
// }

// using (var scope = app.Services.CreateScope())
// {
//     var manpower = scope.ServiceProvider.GetRequiredService<ManpowerReqService>();
//     await manpower.RecalculateAllAsync();
// }

// Enable Swagger middleware if in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Add this line to enable Swagger
    app.UseSwaggerUI(); // Add this line to enable Swagger UI
}

// ลบบรรทัดซ้ำออก และเรียงใหม่เป็น:
app.UseDefaultFiles();      // ← ต้องมาก่อน
app.UseStaticFiles();       // ← ต้องมาก่อน
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.MapHub<AttendanceHub>("/attendanceHub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllers();
app.MapFallbackToFile("index.html");  // ← ต้องมาหลังสุด
app.Run();