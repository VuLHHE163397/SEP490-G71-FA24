
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMS_API.Models;


var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

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
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
app.UseAuthorization();
app.UseStaticFiles();


// Sử dụng CORS
app.UseCors("AllowAllOrigins");
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Đảm bảo rằng bạn đã cấu hình đúng
});


// Cấu hình route mặc định cho ứng dụng.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}"); 

app.Run();