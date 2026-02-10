namespace TenHourGuitarist.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(Stream stream, string fileName, string subfolder);
    Task<string> SaveAudioAsync(Stream stream, string fileName, string subfolder);
    Task DeleteFileAsync(string relativePath);
}
