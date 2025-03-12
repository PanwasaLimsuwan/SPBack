using Microsoft.EntityFrameworkCore;
using Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ลงทะเบียน ApplicationDbContext โดยใช้ SQL Server (หรือฐานข้อมูลที่คุณใช้งาน)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ตั้งค่า CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:8080")  // URL ของ Frontend
               .AllowAnyMethod()                    // อนุญาตทุก HTTP method
               .AllowAnyHeader();                   // อนุญาตทุก header
    });
});

// เพิ่มบริการที่จำเป็น
builder.Services.AddControllers();

// เพิ่ม Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// เปิดใช้งาน Swagger UI เมื่อใน Development Environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// เปิดใช้งาน CORS
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
