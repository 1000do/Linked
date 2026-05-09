// using System;
// using System.IO;
// using System.Security.Cryptography;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;

// namespace CourseMarketplaceBE.Application.Services
// {
//     /// <summary>
//     /// Implementation of content hash service using MD5.
//     /// </summary>
//     public class ContentHashService : IContentHashService
//     {
//         private readonly ILogger<ContentHashService> _logger;

//         public ContentHashService(ILogger<ContentHashService> logger)
//         {
//             _logger = logger;
//         }

//         /// <summary>
//         /// Compute MD5 hash of text content.
//         /// </summary>
//         public async Task<string> ComputeMD5HashAsync(string content)
//         {
//             if (string.IsNullOrEmpty(content))
//             {
//                 throw new ArgumentNullException(nameof(content), "Content cannot be null or empty");
//             }

//             try
//             {
//                 using (var md5 = MD5.Create())
//                 {
//                     byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
//                     byte[] hashBytes = md5.ComputeHash(contentBytes);
//                     string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

//                     _logger.LogDebug($"Computed MD5 hash for text content ({content.Length} chars): {hash.Substring(0, 8)}...");
//                     return hash;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error computing MD5 hash for text: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Compute MD5 hash of binary content (file bytes).
//         /// </summary>
//         public async Task<string> ComputeMD5HashAsync(byte[] fileBytes)
//         {
//             if (fileBytes == null || fileBytes.Length == 0)
//             {
//                 throw new ArgumentNullException(nameof(fileBytes), "File bytes cannot be null or empty");
//             }

//             try
//             {
//                 using (var md5 = MD5.Create())
//                 {
//                     byte[] hashBytes = md5.ComputeHash(fileBytes);
//                     string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

//                     _logger.LogDebug($"Computed MD5 hash for binary content ({fileBytes.Length} bytes): {hash.Substring(0, 8)}...");
//                     return hash;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error computing MD5 hash for binary: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Compute MD5 hash of a file from filesystem path.
//         /// </summary>
//         public async Task<(string hash, bool success)> ComputeFileHashAsync(string filePath)
//         {
//             if (string.IsNullOrEmpty(filePath))
//             {
//                 return (null, false);
//             }

//             try
//             {
//                 if (!File.Exists(filePath))
//                 {
//                     _logger.LogWarning($"File not found: {filePath}");
//                     return (null, false);
//                 }

//                 using (var md5 = MD5.Create())
//                 using (var stream = File.OpenRead(filePath))
//                 {
//                     byte[] hashBytes = md5.ComputeHash(stream);
//                     string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

//                     _logger.LogDebug($"Computed MD5 hash for file {Path.GetFileName(filePath)}: {hash.Substring(0, 8)}...");
//                     return (hash, true);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error computing file hash for {filePath}: {ex.Message}");
//                 return (null, false);
//             }
//         }
//     }
// }
