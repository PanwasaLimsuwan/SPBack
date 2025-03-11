using Microsoft.EntityFrameworkCore;
using Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ลงทะเบียน ApplicationDbContext โดยใช้ SQL Server (หรือฐานข้อมูลที่คุณใช้งาน)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger middleware if in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Add this line to enable Swagger
    app.UseSwaggerUI(); // Add this line to enable Swagger UI
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
