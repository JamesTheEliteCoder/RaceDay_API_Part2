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
        } //end of UploadImageAsync 






        // Retrieve an image from private Azure Blob Storage.
        public async Task<(Stream Content, string ContentType)?> GetImageAsync(
            string imageUrl)
        {
            // Make sure the supplied URL is valid.
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri))
            {
                return null;
            }

            // Get a reference to our private Azure Blob container.
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            // Get the container's configured Azure URL.
            var containerUri = containerClient.Uri;

            // Ensure the URL belongs to our own Azure Storage account.
            if (!string.Equals(
                imageUri.Host,
                containerUri.Host,
                StringComparison.OrdinalIgnoreCase) ||
                imageUri.Scheme != Uri.UriSchemeHttps)
            {
                return null;
            }

            // Ensure the image URL points to our expected container.
            var containerPath = containerUri.AbsolutePath.TrimEnd('/') + "/";

            if (!imageUri.AbsolutePath.StartsWith(
                containerPath,
                StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Extract the image's name from its URL.
            var blobName = Uri.UnescapeDataString(
                imageUri.AbsolutePath[containerPath.Length..]);

            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            // Get a reference to the requested image.
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check whether the image exists.
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            // Download the image as a stream.
            var download = await blobClient.DownloadStreamingAsync();

            // Retrieve the image's content type.
            var contentType = download.Value.Details.ContentType
                ?? "application/octet-stream";

            // Return the image stream and its content type.
            return (download.Value.Content, contentType);
        }











    } //end of BlobStorageService class
} //end of namespace



/*A few things to know:
Private container: We don't enable anonymous access.
Unique filenames: Guid.NewGuid() prevents accidental overwriting when two users upload files with the same name.
Virtual folders: event-banners/ and profile-pictures/ are prefixes in the blob name, not actual folders that need creating.
Content type: Azure receives the correct image MIME type, which will help when images are served later.
*/