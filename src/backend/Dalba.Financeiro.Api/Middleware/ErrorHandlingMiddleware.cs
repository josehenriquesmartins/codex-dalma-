using System.Net;
using Dalba.Financeiro.Application.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Dalba.Financeiro.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (AppException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Erro de persistência no banco de dados.");
            await WriteErrorAsync(context, (int)HttpStatusCode.Conflict, GetDatabaseMessage(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado.");
            await WriteErrorAsync(context, (int)HttpStatusCode.InternalServerError, "Não foi possível concluir a operação. Verifique os dados informados e tente novamente.");
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { message });
    }

    private static string GetDatabaseMessage(DbUpdateException exception)
    {
        if (exception.InnerException is PostgresException postgresException)
        {
            if (postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return postgresException.ConstraintName switch
                {
                    "uq_documentos_enviados_ref" => "Já existe competência aberta para este contrato, mês e ano. Abra a competência existente para conferir os documentos enviados e pendentes.",
                    "uq_documentos_registrados_item" => "Este documento já foi enviado para esta competência. Confira a lista para ver o que já está enviado e o que ainda está pendente.",
                    _ => "Já existe um registro cadastrado com estas informações. Revise os campos únicos e tente novamente."
                };
            }

            return postgresException.SqlState switch
            {
                PostgresErrorCodes.ForeignKeyViolation => "O registro selecionado está vinculado a outro cadastro ou referência inválida.",
                PostgresErrorCodes.NotNullViolation => "Existem campos obrigatórios sem preenchimento. Revise o formulário e tente novamente.",
                PostgresErrorCodes.CheckViolation => "Os dados informados não atendem às regras de validação. Revise o formulário e tente novamente.",
                _ => "Erro ao gravar no banco de dados. Revise os dados informados e tente novamente."
            };
        }

        return "Erro ao gravar no banco de dados. Revise os dados informados e tente novamente.";
    }
}
