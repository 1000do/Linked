using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository interface for CourseExt entity (course hashes).
    /// </summary>
    public interface ICourseExtRepository
    {
        /// <summary>
        /// Add a new course hash record.
        /// </summary>
        Task AddAsync(CourseExt courseExt);

        /// <summary>
        /// Update an existing course hash record.
        /// </summary>
        void Update(CourseExt courseExt);

        /// <summary>
        /// Delete a course hash record.
        /// </summary>
        void Delete(CourseExt courseExt);

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Get course hash by course ID.
        /// </summary>
        Task<CourseExt?> GetByIdAsync(int courseId);

        /// <summary>
        /// Get all course hashes.
        /// </summary>
        Task<List<CourseExt>> GetAllAsync();
    }
}
