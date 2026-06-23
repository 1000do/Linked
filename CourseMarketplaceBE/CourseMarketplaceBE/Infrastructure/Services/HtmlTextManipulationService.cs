using Ganss.Xss;
using HtmlAgilityPack;
using CourseMarketplaceBE.Application.IServices;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class HtmlTextManipulationService : IHtmlTextManipulationService
    {
        private readonly IHtmlSanitizer _sanitizer;

        public HtmlTextManipulationService(IHtmlSanitizer sanitizer)
        {
            _sanitizer = sanitizer;
        }

        public string SanitizeHtml(string rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml)) return rawHtml;
            return _sanitizer.Sanitize(rawHtml);
        }

        public string ExtractPlainText(string rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml)) return rawHtml;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);
            var plainText = htmlDoc.DocumentNode.InnerText;
            Console.WriteLine($"Extracted HTML text: {plainText}");
            return plainText;
        }
    }
}
