using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddOData(option =>
{
    option.Filter().Select().OrderBy().Expand().Count();
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddScoped<RMS_SEP490Context>();
builder.Services.AddDbContext<RMS_SEP490Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();

