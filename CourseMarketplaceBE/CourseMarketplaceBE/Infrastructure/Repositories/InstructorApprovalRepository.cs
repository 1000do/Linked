using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class InstructorRepository : IInstructorRepository
    {
        private readonly AppDbContext _context;
        public InstructorRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Instructor>> GetPendingInstructorsAsync()
        {
            return await _context.Instructors
                .Include(i => i.InstructorNavigation) // Link tới bảng User
                    .ThenInclude(u => u.UserNavigation) // ĐỔI TÊN Ở ĐÂY: User -> Account thông qua UserNavigation
                .Where(i => i.ApprovalStatus == "Pending")
                .ToListAsync();
        }

        public async Task<Instructor?> GetByIdAsync(int id) =>
            await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u.UserNavigation) // Thêm cả ở đây để khi Approve/Reject có dữ liệu Account
                .FirstOrDefaultAsync(i => i.InstructorId == id);
        public void Update(Instructor instructor) => _context.Instructors.Update(instructor);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
