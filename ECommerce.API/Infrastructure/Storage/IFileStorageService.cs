using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] fileData, string directory, string fileName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath);
}
