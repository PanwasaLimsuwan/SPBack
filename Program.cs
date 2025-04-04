using Microsoft.EntityFrameworkCore;
using Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080") // แหล่งที่มาที่จะอนุญาต
                  .AllowAnyMethod()
                  .AllowAnyHeader();
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
// builder.Services.AddHostedService<CalculatedOTJob>();


var app = builder.Build();

// ใช้ CORS ที่กำหนด
app.UseCors("AllowLocalhost");

// Enable Swagger middleware if in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Add this line to enable Swagger
    app.UseSwaggerUI(); // Add this line to enable Swagger UI
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
