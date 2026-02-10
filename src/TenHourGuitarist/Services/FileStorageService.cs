using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class FileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private static readonly HashSet<string> AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private static readonly HashSet<string> AllowedAudioExtensions = [".mp3", ".wav", ".ogg", ".m4a"];

    private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB
    private const long MaxAudioSize = 50 * 1024 * 1024; // 50 MB
    private const int MaxImageWidth = 1200;

    public async Task<string> SaveImageAsync(Stream stream, string fileName, string subfolder)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Invalid image file extension '{extension}'. Allowed: {string.Join(", ", AllowedImageExtensions)}");
        }

        if (stream.Length > MaxImageSize)
        {
            throw new InvalidOperationException(
                $"Image file size exceeds the maximum allowed size of {MaxImageSize / (1024 * 1024)} MB.");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Load image and resize if necessary
        using var image = await Image.LoadAsync(stream);
        if (image.Width > MaxImageWidth)
        {
            var ratio = (double)MaxImageWidth / image.Width;
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(MaxImageWidth, newHeight));
        }

        await image.SaveAsync(filePath);

        return $"/uploads/{subfolder}/{uniqueFileName}";
    }

    public async Task<string> SaveAudioAsync(Stream stream, string fileName, string subfolder)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedAudioExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Invalid audio file extension '{extension}'. Allowed: {string.Join(", ", AllowedAudioExtensions)}");
        }

        if (stream.Length > MaxAudioSize)
        {
            throw new InvalidOperationException(
                $"Audio file size exceeds the maximum allowed size of {MaxAudioSize / (1024 * 1024)} MB.");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return $"/uploads/{subfolder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var filePath = Path.Combine(environment.WebRootPath, relativePath.TrimStart('/'));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
