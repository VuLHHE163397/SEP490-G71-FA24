using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMS_API.Models;
using RMS_Client.Services; // Thay đổi thành namespace thực tế của bạn

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<BuildingService>(); // Đảm bảo thêm dịch vụ BuildingService

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
builder.Services.AddDbContext<RMS_SEP490Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MyDB");
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// Cấu hình pipeline yêu cầu HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Sử dụng CORS
app.UseCors("AllowAllOrigins");
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Đảm bảo rằng bạn đã cấu hình đúng
});


app.UseAuthorization();

// Cấu hình route mặc định cho ứng dụng.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}"); // Đặt controller mặc định là Home và action là Index

app.Run();
