using Microsoft.AspNetCore.Http;

namespace Pearl_Poetry.Helpers
{
    public static class FileUploadHelper
    {
        public static async Task<string?> UploadImageAsync(
            IFormFile? file,
            string webRootPath,
            string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Allowed image extensions
            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp",
                ".gif"
            };

            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Only JPG, JPEG, PNG, WEBP and GIF images are allowed."
                );
            }

            // Maximum file size: 5 MB
            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException(
                    "Image size cannot be larger than 5 MB."
                );
            }

            // Create folder
            string uploadFolder = Path.Combine(
                webRootPath,
                "images",
                folderName
            );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Generate unique filename
            string fileName =
                $"{Guid.NewGuid()}{extension}";

            string fullPath = Path.Combine(
                uploadFolder,
                fileName
            );

            // Save file
            using (var stream = new FileStream(
                fullPath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return browser-accessible path
            return $"/images/{folderName}/{fileName}";
        }


        public static void DeleteImage(
            string? imagePath,
            string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            // Convert:
            // /images/products/example.jpg
            //
            // into:
            // wwwroot/images/products/example.jpg

            string relativePath = imagePath
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar
                );

            string fullPath = Path.Combine(
                webRootPath,
                relativePath
            );

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}