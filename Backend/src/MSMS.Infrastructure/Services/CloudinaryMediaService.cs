using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using MSMS.Application.Abstractions;
using MSMS.Application.Configuration;

namespace MSMS.Infrastructure.Services;

public sealed class CloudinaryMediaService(IOptions<CloudinarySettings> options) : IMediaStorageService
{
    private readonly CloudinarySettings _settings = options.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.CloudName)
        && !string.IsNullOrWhiteSpace(_settings.ApiKey)
        && !string.IsNullOrWhiteSpace(_settings.ApiSecret);

    public async Task<string> UploadStudentPhotoAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "Cloudinary is not configured. Add Cloudinary:CloudName, ApiKey, and ApiSecret to appsettings.json.");
        }

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        var cloudinary = new Cloudinary(account);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = _settings.Folder,
            Overwrite = true,
            Transformation = new Transformation().Width(400).Height(400).Crop("fill").Gravity("face")
        };

        var result = await cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.Error is not null)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        return result.SecureUrl.ToString();
    }
}
