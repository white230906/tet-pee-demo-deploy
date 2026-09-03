
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TetPee.Service.MediaService;

namespace TetPee.Service.CloudinaryService;

public class Service: IService   
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _cloudinaryOptions = new();

    public Service(IConfiguration configuration)
    {
        configuration.GetSection(nameof(CloudinaryOptions)).Bind(_cloudinaryOptions);
        _cloudinary = new Cloudinary(new Account(
            _cloudinaryOptions.CloudName,
            _cloudinaryOptions.ApiKey,
            _cloudinaryOptions.ApiSecret
            )
        );
    }
    
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        if (!IsImageFile(file))
        {
            throw new ArgumentException("File is not an image", nameof(file));
        }
        //đọc ảnh
        await using var stream = file.OpenReadStream();
        //upload ảnh lên server: tên và nôi dung (stream)
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, stream),
        };
        //tiến hành upload
        var uploadResult = await _cloudinary.UploadAsync( uploadParams);
        //trả về string
        return uploadResult.SecureUrl.ToString();
    }
    //check đuôi file
    private bool IsImageFile(IFormFile file)
    {
        //This is a basic check. For more robust validation, consider using a libarary like MimeDetective
        var allowsExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        return allowsExtensions.Contains(fileExtension);
    }
    /*
       public async Task<string> UploadImageAsync(
      IFormFile file,
      Guid outputId)
  {
      if (file == null || file.Length == 0)
          throw new ArgumentException("File is empty or null", nameof(file));

      if (!IsImageFile(file))
          throw new ArgumentException("File is not an image", nameof(file));

      await using var stream = file.OpenReadStream();

      var publicId = $"generated-flowers/{outputId:N}";

      var uploadParams = new ImageUploadParams
      {
          File = new FileDescription(file.FileName, stream),
          PublicId = publicId,
          Type = "authenticated",
          Format = "png"
      };

      var uploadResult = await _cloudinary.UploadAsync(uploadParams);

      return uploadResult.SecureUrl.ToString();
  }
  
  
  //
  public string CreateImageDownloadUrl(
      Guid outputId,
      string fileName)
  {
      var publicId = $"generated-flowers/{outputId:N}";

      var expiresAt = DateTimeOffset.UtcNow
          .AddMinutes(5)
          .ToUnixTimeSeconds();

      return _cloudinary.DownloadPrivate(
          publicId: publicId,
          attachment: true,
          format: "png",
          type: "authenticated",
          expiresAt: expiresAt,
          resourceType: "image",
          targetFilename: fileName);
  }
     */
    
    
}