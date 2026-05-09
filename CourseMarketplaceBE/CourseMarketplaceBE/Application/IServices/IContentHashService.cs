using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    /// <summary>
    /// Service for computing MD5 hashes of content.
    /// Used during course creation to generate hashes for deduplication checking.
    /// </summary>
    public interface IContentHashService
    {
        /// <summary>
        /// Compute MD5 hash of text content.
        /// </summary>
        /// <param name="content">Text content to hash</param>
        /// <returns>32-character hex MD5 hash</returns>
        Task<string> ComputeMD5HashAsync(string content);

        /// <summary>
        /// Compute MD5 hash of binary content (file bytes).
        /// </summary>
        /// <param name="fileBytes">Binary file content</param>
        /// <returns>32-character hex MD5 hash</returns>
        Task<string> ComputeMD5HashAsync(byte[] fileBytes);

        /// <summary>
        /// Compute MD5 hash of a file from filesystem path.
        /// </summary>
        /// <param name="filePath">Full path to file</param>
        /// <returns>(hash, success) tuple</returns>
        Task<(string hash, bool success)> ComputeFileHashAsync(string filePath);
    }
}
