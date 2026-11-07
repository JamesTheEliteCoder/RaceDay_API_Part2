using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PROG_Part2_RaceDay_API.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "raceday-images";

        // Receive the Azure Blob Service client through dependency injection.
        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        // Upload an image to the specified folder inside our Azure container.
        public async Task<string> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            string folderName)
        {
            // Get a reference to our private Azure Blob container.
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            // Create a unique filename to prevent existing images from being overwritten.
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

            // Use the folder name as a virtual directory inside the container.
            var blobName = $"{folderName}/{uniqueFileName}";

            // Get a reference to the specific image blob.
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the image and specify its content type.
            await blobClient.UploadAsync(
                imageStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            // Return the blob's URL for storage in our database.
            return blobClient.Uri.ToString();
        }
    }
}



/*A few things to know:
Private container: We don't enable anonymous access.
Unique filenames: Guid.NewGuid() prevents accidental overwriting when two users upload files with the same name.
Virtual folders: event-banners/ and profile-pictures/ are prefixes in the blob name, not actual folders that need creating.
Content type: Azure receives the correct image MIME type, which will help when images are served later.
*/