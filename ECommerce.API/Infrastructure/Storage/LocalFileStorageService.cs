using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ECommerce.API.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(byte[] fileData, string directory, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), directory);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fullPath = Path.Combine(uploadPath, fileName);
        await File.WriteAllBytesAsync(fullPath, fileData, cancellationToken);

        // Return relative path for HTTP access
        return $"/{directory}/{fileName}".Replace("\\", "/");
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return Task.CompletedTask;

        // filePath is like "/uploads/products/xxx.webp"
        var relativePath = filePath.TrimStart('/');
        var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
