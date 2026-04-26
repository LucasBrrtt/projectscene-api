using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectScene.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace ProjectScene.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Registra o erro no log antes de montar a resposta HTTP.
                _logger.LogError(ex, "Unhandled exception");

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ocorreu um erro inesperado.";

            // Traduz excecoes internas para respostas HTTP previsiveis.
            switch (ex)
            {
                case DuplicateResourceException duplicateResourceEx:
                    statusCode = HttpStatusCode.Conflict;
                    message = duplicateResourceEx.Message;
                    break;

                case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == "23505":
                    statusCode = HttpStatusCode.Conflict;
                    message = "Ja existe um registro com este valor unico.";
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    break;

                case KeyNotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundEx.Message;
                    break;
            }

            var response = new
            {
                error = message,
                status = (int)statusCode
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Mantem o json de erro no mesmo padrao camelCase da API.
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
