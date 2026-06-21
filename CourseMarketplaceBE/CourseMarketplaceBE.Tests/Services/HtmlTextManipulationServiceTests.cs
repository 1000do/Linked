using Xunit;
using FluentAssertions;
using CourseMarketplaceBE.Infrastructure.Services;

namespace CourseMarketplaceBE.Tests.Services
{
    public class HtmlTextManipulationServiceTests
    {
        private readonly HtmlTextManipulationService _service;

        public HtmlTextManipulationServiceTests()
        {
            var sanitizer = new Ganss.Xss.HtmlSanitizer();
            _service = new HtmlTextManipulationService(sanitizer);
        }

        [Fact]
        public void SanitizeHtml_ShouldRemoveScriptTags()
        {
            // Arrange
            string maliciousHtml = "<p>This is a test</p><script>alert('xss');</script>";

            // Act
            var result = _service.SanitizeHtml(maliciousHtml);

            // Assert
            result.Should().NotContain("<script>");
            result.Should().NotContain("alert('xss')");
            result.Should().Contain("<p>This is a test</p>");
        }

        [Fact]
        public void SanitizeHtml_ShouldPreserveSafeTags()
        {
            // Arrange
            string safeHtml = "<b>Bold text</b> and <i>italic text</i> with a <a href=\"https://google.com\">link</a>.";

            // Act
            var result = _service.SanitizeHtml(safeHtml);

            // Assert
            result.Should().Contain("<b>Bold text</b>");
            result.Should().Contain("<i>italic text</i>");
            result.Should().Contain("<a href=\"https://google.com\">link</a>");
        }

        [Fact]
        public void ExtractPlainText_ShouldRemoveAllHtmlTags()
        {
            // Arrange
            string html = "<h1>Heading</h1><p>Some <b>bold</b> and <span style=\"color: red\">red</span> text.</p><script>alert('xss');</script>";

            // Act
            var result = _service.ExtractPlainText(html);

            // Assert
            result.Should().NotContain("<");
            result.Should().NotContain(">");
            // InnerText correctly ignores script contents
            result.Should().Be("HeadingSome bold and red text.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void SanitizeHtml_ShouldHandleNullOrWhitespace(string? input)
        {
            // Act
            var result = _service.SanitizeHtml(input);

            // Assert
            result.Should().Be(input);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ExtractPlainText_ShouldHandleNullOrWhitespace(string? input)
        {
            // Act
            var result = _service.ExtractPlainText(input);

            // Assert
            result.Should().Be(input);
        }
    }
}
