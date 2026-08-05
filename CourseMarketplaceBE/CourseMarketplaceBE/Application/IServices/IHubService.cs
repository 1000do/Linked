using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IHubService
    {
        Task SendInstructorApplicationUpdateAsync();
    }
}
