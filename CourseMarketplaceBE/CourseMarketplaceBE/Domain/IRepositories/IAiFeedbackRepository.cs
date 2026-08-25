using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IAiFeedbackRepository
    {
        Task<int> SaveChangesAsync();
        void AddCourseFeedback(CourseAiFeedback feedback);
        void AddLessonFeedback(LessonAiFeedback feedback);
        void AddMaterialFeedback(LearningMaterialAiFeedback feedback);
        Task<System.Collections.Generic.List<CourseMarketplaceBE.Application.DTOs.AiFeedbackBubbleDto>> GetLatestFeedbacksByCourseAsync(int courseId);
    }
}
