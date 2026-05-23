namespace MSMS.Application.Abstractions;

public interface IMediaStorageService
{
    Task<string> UploadStudentPhotoAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    bool IsConfigured { get; }
}
