using HRManagement.API.Middleware;
using HRManagement.BLL.Interfaces;
using HRManagement.BLL.Services;
using HRManagement.DAL;
using HRManagement.DAL.Interfaces;
using HRManagement.DAL.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models; // for X-Role appearance in Swagger.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
// Adding X-Role to Swagger.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("X-Role", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Role",
        Type = SecuritySchemeType.ApiKey,
        Description = "Nhập role: Admin, IT, hoặc KeToan"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-Role"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DAL
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<ILuongRepository, LuongRepository>();

// BLL
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<ILuongService, LuongService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5075")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");

// ── Endpoint Login: thử kết nối SQL bằng credentials người dùng nhập ──
app.MapPost("/api/auth/login", async (LoginRequest req, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu." });

    var server = config["DbServer"] ?? "localhost\\SQLEXPRESS";

    // Chọn DB để test kết nối dựa vào tên login
    string testDb = req.Username.ToLower().Contains("ketoan") ? "DB_Payroll" : "DB_Personnel";

    var connStr = new SqlConnectionStringBuilder
    {
        DataSource = server,
        InitialCatalog = testDb,
        UserID = req.Username,
        Password = req.Password,
        TrustServerCertificate = true,
        ConnectTimeout = 5
    }.ConnectionString;

    try
    {
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(); // Sai credentials → throw SqlException
    }
    catch (SqlException ex)
    {
        return Results.Json(new
        {
            success = false,
            message = ex.Message
        }, statusCode: 500);
    }
    catch (Exception ex)
    {
        return Results.Json(new { success = false, message = "Lỗi kết nối: " + ex.Message },
            statusCode: 500);
    }

    // Xác định role từ tên login
    string role = req.Username.ToLower() switch
    {
        var u when u.Contains("admin") => "Admin",
        var u when u.Contains("ketoan") => "KeToan",
        var u when u.Contains("it") => "IT",
        _ => "Unknown"
    };

    if (role == "Unknown")
        return Results.Json(new { success = false, message = "Tài khoản không được phép truy cập hệ thống." },
            statusCode: 403);

    return Results.Ok(new { success = true, role });
});

// ── Middleware phân quyền: đọc X-Role header cho các API còn lại ──
app.UseMiddleware<RoleAuthMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public record LoginRequest(string Username, string Password);