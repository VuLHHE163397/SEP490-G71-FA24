using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMS_API.Models;


var builder = WebApplication.CreateBuilder(args);

// Register services in the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();



// Configure CORS to allow all origins, methods, and headers.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policyBuilder => policyBuilder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
});

// Configure DbContext with SQL Server connection.
builder.Services.AddDbContext<RMS_SEP490Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// Configure HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable CORS with configured policy.
app.UseCors("AllowAllOrigins");

app.UseRouting();
app.UseAuthorization();

// Configure default route for controllers.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Building}/{action=ListBuilding}/{id?}");

app.Run();