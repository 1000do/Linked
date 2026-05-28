using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    /// <summary>
    /// Service for computing MD5 hashes of content.
    /// Used during course creation to generate hashes for deduplication checking.
    /// </summary>
    public interface IContentHashService
    {
        /// <summary>
        /// Normalize text content for hashing (lowercase, trim, normalize whitespace).
        /// </summary>
        /// <param name="text">Text to normalize</param>
        /// <returns>Normalized text</returns>
        string NormalizeText(string text);

        /// <summary>
        /// Compute MD5 hash of text content.
        /// </summary>
        /// <param name="content">Text content to hash</param>
        /// <returns>32-character hex MD5 hash</returns>
        // Task<string> ComputeMD5HashAsync(string content);

        /// <summary>
        /// Compute MD5 hash of binary content (file bytes).
        /// </summary>
        /// <param name="fileBytes">Binary file content</param>
        /// <returns>32-character hex MD5 hash</returns>
        // Task<string> ComputeMD5HashAsync(byte[] fileBytes);

        /// <summary>
        /// Compute MD5 hash of a file from filesystem path.
        /// </summary>
        /// <param name="filePath">Full path to file</param>
        /// <returns>(hash, success) tuple</returns>
        // Task<(string hash, bool success)> ComputeFileHashAsync(string filePath);

        /// <summary>
        /// Compute MD5 hash of course text (title, description, etc).
        /// Normalizes text before hashing.
        /// </summary>
        /// <param name="text">Course text to hash</param>
        /// <returns>32-character hex MD5 hash</returns>
        Task<string> ComputeCourseHashAsync(string text);

        /// <summary>
        /// Compute MD5 hash of file bytes (for thumbnails, etc).
        /// </summary>
        /// <param name="fileBytes">File content as bytes</param>
        /// <returns>32-character hex MD5 hash</returns>
        Task<string> ComputeFileHashAsync(byte[] fileBytes);

        

        /// <summary>
        /// Get course hashes from database.
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <returns>CourseExtDto with hashes</returns>
        Task<CourseExtDto> GetCourseHashesAsync(int courseId);

        /// <summary>
        /// Get all course hashes from database.
        /// </summary>
        /// <returns>List of CourseExtDto</returns>
        Task<List<CourseExtDto>> GetAllCourseHashesAsync();
    }
}
