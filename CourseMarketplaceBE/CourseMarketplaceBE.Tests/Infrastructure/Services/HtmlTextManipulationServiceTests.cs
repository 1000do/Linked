using System;
using CourseMarketplaceBE.Infrastructure.Services;
using FluentAssertions;
using Ganss.Xss;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Infrastructure.Services
{
    public class HtmlTextManipulationServiceTests
    {
        private readonly IHtmlSanitizer _sanitizerMock;
        private readonly HtmlTextManipulationService _sut;

        public HtmlTextManipulationServiceTests()
        {
            _sanitizerMock = Substitute.For<IHtmlSanitizer>();
            _sut = new HtmlTextManipulationService(_sanitizerMock);
        }

        // ── SanitizeHtml ────────────────────────────────────────────────────────

        [Fact]
        public void SanitizeHtml_ShouldReturnSanitizedString()
        {
            //Arrange 1
            string maliciousHtml = "<p>This is a test</p><script>alert('xss');</script>";
            string safeHtml = "<p>This is a test</p>";

            //Arrange 2
            _sanitizerMock.Sanitize(maliciousHtml).Returns(safeHtml);

            //Act
            var result = _sut.SanitizeHtml(maliciousHtml);

            //Assert
            result.Should().Be(safeHtml);
            _sanitizerMock.Received(1).Sanitize(maliciousHtml);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void SanitizeHtml_ShouldHandleNullOrWhitespace(string? input)
        {
            //Arrange 1
            // No additional setup

            //Arrange 2
            // No mock setup needed, it should return early

            //Act
            var result = _sut.SanitizeHtml(input!);

            //Assert
            result.Should().Be(input);
            _sanitizerMock.DidNotReceive().Sanitize(Arg.Any<string>());
        }

        // ── ExtractPlainText ──────────────────────────────────────────────────────

        [Fact]
        public void ExtractPlainText_ShouldRemoveAllHtmlTags()
        {
            //Arrange 1
            string html = "<h1>Heading</h1><p>Some <b>bold</b> and <span style=\"color: red\">red</span> text.</p><script>alert('xss');</script>";
            string expectedText = "HeadingSome bold and red text.";

            //Arrange 2
            // No mocks used for HTML parsing as it uses concrete HtmlAgilityPack

            //Act
            var result = _sut.ExtractPlainText(html);

            //Assert
            result.Should().Be(expectedText);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ExtractPlainText_ShouldHandleNullOrWhitespace(string? input)
        {
            //Arrange 1
            // No additional setup

            //Arrange 2
            // No mock setup needed, it should return early

            //Act
            var result = _sut.ExtractPlainText(input!);

            //Assert
            result.Should().Be(input);
        }
    }
}
