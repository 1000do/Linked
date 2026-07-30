using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class AiFeedbackRepository : IAiFeedbackRepository
    {
        private readonly AppDbContext _context;

        public AiFeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddCourseFeedback(CourseAiFeedback feedback)
        {
            _context.Set<CourseAiFeedback>().Add(feedback);
        }

        public void AddLessonFeedback(LessonAiFeedback feedback)
        {
            _context.Set<LessonAiFeedback>().Add(feedback);
        }

        public void AddMaterialFeedback(LearningMaterialAiFeedback feedback)
        {
            _context.Set<LearningMaterialAiFeedback>().Add(feedback);
        }

        public async Task<int> SaveChangesAsync()
        {
            try 
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new CourseException("Database operation failed due to a constraint violation or data issue while saving AI Feedback.");
            }
        }
    }
}
