using CollegeSchedule.Data;
using CollegeSchedule.Middlewares;
using CollegeSchedule.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//загружаем настройки базы данных из файла .env
DotNetEnv.Env.Load();

//собираем строку подключения к PostgreSQL из переменных окружения
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

//настраиваем подключение к базе данных через Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

//регистрируем сервис расписания для dependency injection
builder.Services.AddScoped<IScheduleService, ScheduleService>();

//добавляем поддержку контроллеров (API endpoints)
builder.Services.AddControllers();

//добавляем Swagger для документации и тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//в режиме разработки включаем Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//добавляем middleware для обработки исключений (до маршрутизации)
app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

//настраиваем маршрутизацию к контроллерам
app.MapControllers();

app.Run();