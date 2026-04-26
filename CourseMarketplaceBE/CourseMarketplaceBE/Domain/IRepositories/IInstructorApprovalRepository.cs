using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IInstructorRepository
    {
        Task<IEnumerable<Instructor>> GetPendingInstructorsAsync();
        Task<Instructor?> GetByIdAsync(int id);
        void Update(Instructor instructor);
        Task SaveChangesAsync();
    }
}
