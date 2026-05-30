using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly AppDbContext _context;

        public EnrollmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Enrollment?> GetEnrollmentAsync(int userId, int courseId)
            => await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.EnrollmentStatus != "revoked");

        public async Task<Enrollment?> GetEnrollmentWithProgressAsync(int userId, int courseId)
            => await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.EnrollmentStatus != "revoked");

        public async Task<Enrollment?> GetEnrollmentIncludingRefundedAsync(int userId, int courseId)
            => await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        public async Task ClearMaterialCompletionsAsync(int enrollmentId)
        {
            var completions = await _context.MaterialCompletions
                .Where(mc => mc.EnrollmentId == enrollmentId)
                .ToListAsync();
            _context.MaterialCompletions.RemoveRange(completions);
        }

        public async Task<List<Enrollment>> GetMyEnrolledCoursesAsync(int userId)
        {
            return await _context.Enrollments
                .Where(e => e.UserId == userId && e.EnrollmentStatus != "revoked")
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Instructor)
                        .ThenInclude(i => i!.InstructorNavigation)
                .OrderByDescending(e => e.LastAccessedAt)
                .ToListAsync();
        }

        public async Task<bool> IsMaterialCompletedAsync(int enrollmentId, int materialId)
            => await _context.MaterialCompletions.AnyAsync(mc => mc.EnrollmentId == enrollmentId && mc.MaterialId == materialId);

        public async Task<int> GetCompletedMaterialCountAsync(int enrollmentId)
            => await _context.MaterialCompletions.CountAsync(mc => mc.EnrollmentId == enrollmentId);

        public async Task<List<int>> GetCompletedMaterialIdsAsync(int enrollmentId)
            => await _context.MaterialCompletions
                .Where(mc => mc.EnrollmentId == enrollmentId)
                .Select(mc => mc.MaterialId)
                .ToListAsync();

        public async Task<List<int>> GetEnrolledUserIdsAsync(int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.UserId ?? 0)
                .Where(id => id != 0)
                .Distinct()
                .ToListAsync();
        }

        public async Task AddEnrollmentAsync(Enrollment enrollment)
            => await _context.Enrollments.AddAsync(enrollment);



        public async Task AddMaterialCompletionAsync(MaterialCompletion completion)
            => await _context.MaterialCompletions.AddAsync(completion);

        public async Task<IDbContextTransaction> BeginTransactionAsync()
            => await _context.Database.BeginTransactionAsync();

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
