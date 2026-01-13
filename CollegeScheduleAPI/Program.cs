using CollegeSchedule.Data;
using CollegeSchedule.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
DotNetEnv.Env.Load();
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
 $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
 $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
 $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +

$"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
builder.Services.AddDbContext<AppDbContext>(options =>
 options.UseNpgsql(connectionString));

builder.Services.AddScoped<IScheduleService, ScheduleService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.MapControllers();

app.Run();
