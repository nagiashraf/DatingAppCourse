using API.Interfaces;
using CloudinaryDotNet.Actions;

namespace API.Extensions;

public class PhotoService : IPhotoService
{
    public Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        throw new NotImplementedException();
    }

    public Task<DeletionResult> DeletePhotoAsync(string PublicId)
    {
        throw new NotImplementedException();
    }
}