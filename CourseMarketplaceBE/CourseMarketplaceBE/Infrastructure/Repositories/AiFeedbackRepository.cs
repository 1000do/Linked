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

        public async Task<System.Collections.Generic.List<CourseMarketplaceBE.Application.DTOs.AiFeedbackBubbleDto>> GetLatestFeedbacksByCourseAsync(int courseId)
        {
            var courseFeedbacksData = await _context.Set<CourseAiFeedback>()
                .Where(f => f.CourseId == courseId)
                .ToListAsync();

            var courseFeedbacks = courseFeedbacksData
                .GroupBy(f => f.FieldName)
                .Select(g => g.OrderByDescending(x => x.DateAdded).FirstOrDefault())
                .Select(f => new CourseMarketplaceBE.Application.DTOs.AiFeedbackBubbleDto
                {
                    FeedbackId = f!.FeedbackId,
                    FieldName = f.FieldName?.Replace(".", "_") ?? "",
                    FeedbackText = f.FeedbackText,
                    DateAdded = f.DateAdded,
                    ModerationStatus = f.ModerationStatus
                })
                .ToList();

            var lessonFeedbacksData = await _context.Set<LessonAiFeedback>()
                .Where(f => _context.Lessons.Any(l => l.LessonId == f.LessonId && l.CourseId == courseId))
                .ToListAsync();

            var lessonFeedbacks = lessonFeedbacksData
                .GroupBy(f => f.FieldName)
                .Select(g => g.OrderByDescending(x => x.DateAdded).FirstOrDefault())
                .Select(f => new CourseMarketplaceBE.Application.DTOs.AiFeedbackBubbleDto
                {
                    FeedbackId = f!.FeedbackId,
                    FieldName = f.FieldName?.Replace(".", "_") ?? "",
                    FeedbackText = f.FeedbackText,
                    DateAdded = f.DateAdded,
                    ModerationStatus = f.ModerationStatus
                })
                .ToList();

            var materialFeedbacksData = await _context.Set<LearningMaterialAiFeedback>()
                .Where(f => _context.LearningMaterials.Any(m => m.MaterialId == f.MaterialId && m.Lesson.CourseId == courseId))
                .ToListAsync();

            var materialFeedbacks = materialFeedbacksData
                .GroupBy(f => f.FieldName)
                .Select(g => g.OrderByDescending(x => x.DateAdded).FirstOrDefault())
                .Select(f => new CourseMarketplaceBE.Application.DTOs.AiFeedbackBubbleDto
                {
                    FeedbackId = f!.FeedbackId,
                    FieldName = f.FieldName?.Replace(".", "_") ?? "",
                    FeedbackText = f.FeedbackText,
                    DateAdded = f.DateAdded,
                    ModerationStatus = f.ModerationStatus
                })
                .ToList();

            var result = new System.Collections.Generic.List<CourseMarketplaceBE.Application.DTOs.AiFeedbackBubbleDto>();
            result.AddRange(courseFeedbacks);
            result.AddRange(lessonFeedbacks);
            result.AddRange(materialFeedbacks);

            return result;
        }
    }
}
