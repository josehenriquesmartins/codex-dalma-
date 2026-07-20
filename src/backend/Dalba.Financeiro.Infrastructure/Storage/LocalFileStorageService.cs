using System.Text.RegularExpressions;
using Dalba.Financeiro.Application.Abstractions.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Dalba.Financeiro.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _basePath = Path.Combine(environment.ContentRootPath, "storage", "uploads");
    }

    public async Task<FileStorageResult> SaveAsync(string fornecedorCodigo, int ano, int mes, IFormFile file, CancellationToken cancellationToken)
    {
        var sanitizedName = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), "[^a-zA-Z0-9_-]", "_");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var physicalFolder = Path.Combine(_basePath, fornecedorCodigo, ano.ToString("0000"), mes.ToString("00"));
        Directory.CreateDirectory(physicalFolder);

        var fileName = $"{sanitizedName}_{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(physicalFolder, fileName);
        await using var stream = File.Create(physicalPath);
        await file.CopyToAsync(stream, cancellationToken);

        var relativePath = Path.Combine(fornecedorCodigo, ano.ToString("0000"), mes.ToString("00"), fileName);
        return new FileStorageResult(relativePath, fileName, file.FileName, extension, file.Length);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_basePath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        Stream? stream = File.Exists(fullPath) ? File.OpenRead(fullPath) : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = Path.Combine(_basePath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (!File.Exists(fullPath))
        {
            return Task.CompletedTask;
        }

        File.Delete(fullPath);

        var currentDirectory = Path.GetDirectoryName(fullPath);
        while (!string.IsNullOrWhiteSpace(currentDirectory) &&
               currentDirectory.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(currentDirectory, _basePath, StringComparison.OrdinalIgnoreCase) &&
               Directory.Exists(currentDirectory) &&
               !Directory.EnumerateFileSystemEntries(currentDirectory).Any())
        {
            Directory.Delete(currentDirectory);
            currentDirectory = Path.GetDirectoryName(currentDirectory);
        }

        return Task.CompletedTask;
    }
}
