using System.Net;
using System.Text.Json;

namespace CollegeSchedule.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        //конструктор получает следующий middleware в цепочке и логгер
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        //метод, который вызывается для каждого HTTP-запроса
        public async Task Invoke(HttpContext context)
        {
            try
            {
                //пробуем выполнить следующий middleware в цепочке
                await _next(context);
            }
            catch (Exception ex)
            {
                //логируем ошибку
                _logger.LogError(ex, ex.Message);
                //обрабатываем исключение
                await HandleException(context, ex);
            }
        }

        //метод для обработки исключения и формирования ответа клиенту
        private static Task HandleException(HttpContext context, Exception ex)
        {
            //определяем HTTP-статус в зависимости от типа исключения
            var status = ex switch
            {
                ArgumentOutOfRangeException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            //создаем объект ошибки для отправки клиенту
            var response = new
            {
                error = ex.Message
            };

            //устанавливаем заголовки ответа
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            //сериализуем и отправляем ответ
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}