using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog.Configuration;
using System.Net;


namespace ChatServer.Services
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
            )
        {


            _logger.LogError(exception, "Глобальный перехват исключения: {Message}", exception.Message);

            var (statuseCode, title) = exception switch
            {
                ArgumentException or BadHttpRequestException =>
                ((int)HttpStatusCode.BadRequest, "Неверный запрос (Bad Request)"),

                KeyNotFoundException => 
                ((int)HttpStatusCode.NotFound, "Ресурс не найден (Not Found)"),

                UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized, "Доступ не авторизован (Unauthorized)"),

                _ =>
                ((int)HttpStatusCode.InternalServerError, "Глобальная ошибка сервера")

            };



            var problemDetails = new ProblemDetails
            {
                Status = statuseCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
        




















    }
}
