namespace CourseMarketplaceBE.Application.IServices
{
    public interface IHtmlTextManipulationService
    {
        // Sanitizes HTML to prevent XSS while keeping safe formatting tags
        string SanitizeHtml(string rawHtml);
        
        // Strips all HTML tags to return only plain text
        string ExtractPlainText(string rawHtml);
    }
}
