# ขั้นที่ 1: ใช้ base image ของ .NET SDK ที่รองรับการ build แอปพลิเคชัน
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# ขั้นที่ 2: ตั้ง working directory ภายใน container เป็น /src
WORKDIR /src

# ขั้นที่ 3: คัดลอกไฟล์ .csproj (ไฟล์โปรเจคของ C#) ไปยัง container
COPY ["Api/Api.csproj", "Api/"]

# ขั้นที่ 4: ติดตั้ง dependencies ที่ระบุใน .csproj
RUN dotnet restore "Api/Api.csproj"

# ขั้นที่ 5: คัดลอกไฟล์อื่นๆ ในโปรเจคไปยัง container
COPY . .

# ขั้นที่ 6: ตั้ง working directory เป็น /src/Api และ build โปรเจค
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# ขั้นที่ 7: Publish แอปพลิเคชันในโหมด Release
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

# ขั้นที่ 8: ใช้ base image ของ ASP.NET สำหรับรันแอปพลิเคชันใน production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# ขั้นที่ 9: ตั้ง working directory เป็น /app
WORKDIR /app

# ขั้นที่ 10: คัดลอกไฟล์ที่ถูก publish จากขั้นตอนก่อนหน้าไปยัง /app
COPY --from=publish /app/publish .

# ขั้นที่ 11: กำหนดคำสั่งที่ใช้รันแอปพลิเคชันเมื่อ container เริ่มทำงาน
ENTRYPOINT ["dotnet", "Api.dll"]
