namespace Application.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
