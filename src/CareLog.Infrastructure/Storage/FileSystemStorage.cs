using CareLog.Application.Abstractions;

namespace CareLog.Infrastructure.Storage;

public sealed class FileSystemStorage(IWebHostEnvironment env) : IFileStorage
{
    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct)
    {
        var root = Path.Combine(env.ContentRootPath, "attachments");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, $"{Guid.NewGuid()}_{fileName}");
        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, ct);
        return path;
    }
}
