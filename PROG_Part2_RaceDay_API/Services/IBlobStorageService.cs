namespace PROG_Part2_RaceDay_API.Services
{
    public interface IBlobStorageService
    {
        // Uploads an image to the specified Azure Blob Storage folder
        // and returns its stored URL.
        Task<string> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            string folderName);


        // Retrieves an image from Azure Blob Storage
        // and returns its content stream and content type.
        Task<(Stream Content, string ContentType)?> GetImageAsync(
            string imageUrl);
    }



}