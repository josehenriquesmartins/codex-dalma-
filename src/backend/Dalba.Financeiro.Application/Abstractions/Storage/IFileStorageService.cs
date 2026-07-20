using Microsoft.AspNetCore.Http;

namespace Dalba.Financeiro.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveAsync(string fornecedorCodigo, int ano, int mes, IFormFile file, CancellationToken cancellationToken);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}

public sealed record FileStorageResult(
    string RelativePath,
    string FileName,
    string OriginalFileName,
    string Extension,
    long SizeBytes);
