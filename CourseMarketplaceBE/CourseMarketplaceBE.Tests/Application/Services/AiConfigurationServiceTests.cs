using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class AiConfigurationServiceTests
    {
        private readonly ISystemConfigRepository _configRepoMock;
        private readonly AiConfigurationService _sut;

        public AiConfigurationServiceTests()
        {
            _configRepoMock = Substitute.For<ISystemConfigRepository>();
            _sut = new AiConfigurationService(_configRepoMock);
        }

        // ── GetConfigurationsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetConfigurationsAsync_NullModerationThreshold_ReturnsDefaultThresholds()
        {
            //Arrange 1
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = AiModelConst.DefaultSimilarityScoreThreshold,
                SpamConfidenceThreshold = AiModelConst.DefaultSpamScoreThreshold,
                ToxicityConfidenceThreshold = AiModelConst.DefaultToxicScoreThreshold,
                CourseHarmfulTextClassifierPath = "course-harmful",
                CourseTextEmbeddingGeneratorPath = "course-embed",
                CourseMediaEmbeddingGeneratorPath = "course-media",
                ReviewHarmfulTextClassifierPath = "review-harmful"
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns(expectedDto.CourseHarmfulTextClassifierPath);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns(expectedDto.CourseTextEmbeddingGeneratorPath);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns(expectedDto.CourseMediaEmbeddingGeneratorPath);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns(expectedDto.ReviewHarmfulTextClassifierPath);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier);
        }

        [Fact]
        public async Task GetConfigurationsAsync_EmptyModerationThreshold_ReturnsDefaultThresholds()
        {
            //Arrange 1
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = AiModelConst.DefaultSimilarityScoreThreshold,
                SpamConfidenceThreshold = AiModelConst.DefaultSpamScoreThreshold,
                ToxicityConfidenceThreshold = AiModelConst.DefaultToxicScoreThreshold
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns("");
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns((string?)null);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier);
        }

        [Fact]
        public async Task GetConfigurationsAsync_ValidModerationThresholdJson_ReturnsParsedThresholds()
        {
            //Arrange 1
            var jsonString = "{\"spam\": 0.85, \"toxic\": 0.90, \"similarity\": 0.95}";
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = 0.95f,
                SpamConfidenceThreshold = 0.85f,
                ToxicityConfidenceThreshold = 0.90f
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(jsonString);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns((string?)null);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
        }

        [Fact]
        public async Task GetConfigurationsAsync_PartialPropertiesInJson_ReturnsMixedThresholds()
        {
            //Arrange 1
            var jsonString = "{\"spam\": 0.88}";
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = AiModelConst.DefaultSimilarityScoreThreshold,
                SpamConfidenceThreshold = 0.88f,
                ToxicityConfidenceThreshold = AiModelConst.DefaultToxicScoreThreshold
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(jsonString);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns((string?)null);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
        }

        [Fact]
        public async Task GetConfigurationsAsync_InvalidTypesInJson_CatchesExceptionAndReturnsDefaults()
        {
            //Arrange 1
            var jsonString = "{\"spam\": \"not-a-number\", \"toxic\": true, \"similarity\": {}}";
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = AiModelConst.DefaultSimilarityScoreThreshold,
                SpamConfidenceThreshold = AiModelConst.DefaultSpamScoreThreshold,
                ToxicityConfidenceThreshold = AiModelConst.DefaultToxicScoreThreshold
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(jsonString);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns((string?)null);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
        }

        [Fact]
        public async Task GetConfigurationsAsync_MissingSpamInJson_ReturnsDefaultSpamAndParsedOthers()
        {
            //Arrange 1
            var jsonString = "{\"toxic\": 0.92, \"similarity\": 0.94}";
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = 0.94f,
                SpamConfidenceThreshold = AiModelConst.DefaultSpamScoreThreshold,
                ToxicityConfidenceThreshold = 0.92f
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(jsonString);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns((string?)null);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
        }

        [Fact]
        public async Task GetConfigurationsAsync_InvalidJson_CatchesExceptionAndReturnsDefaultThresholds()
        {
            //Arrange 1
            var jsonString = "invalid-json-content";
            var expectedDto = new AiConfigurationDto
            {
                SimilarityScoreThreshold = AiModelConst.DefaultSimilarityScoreThreshold,
                SpamConfidenceThreshold = AiModelConst.DefaultSpamScoreThreshold,
                ToxicityConfidenceThreshold = AiModelConst.DefaultToxicScoreThreshold
            };

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(jsonString);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns((string?)null);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns((string?)null);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.Should().BeEquivalentTo(expectedDto);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
        }

        // ── UpdateThresholdsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task UpdateThresholdsAsync_ValidRequest_ReturnsTrue()
        {
            //Arrange 1
            var req = new UpdateThresholdsRequest
            {
                SimilarityScoreThreshold = 0.95f,
                SpamConfidenceThreshold = 0.85f,
                ToxicityConfidenceThreshold = 0.90f
            };
            var expectedJsonString = System.Text.Json.JsonSerializer.Serialize(new
            {
                similarity = req.SimilarityScoreThreshold,
                spam = req.SpamConfidenceThreshold,
                toxic = req.ToxicityConfidenceThreshold
            });

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(SystemConfigKeys.ModerationThreshold, expectedJsonString, Arg.Any<string>()).Returns(1);

            //Act
            var result = await _sut.UpdateThresholdsAsync(req);

            //Assert
            result.Should().BeTrue();

            await _configRepoMock.Received(1).UpsertConfigAsync(
                SystemConfigKeys.ModerationThreshold,
                expectedJsonString,
                "system config of AI moderation threshold"
            );
        }

        [Fact]
        public async Task UpdateThresholdsAsync_SystemConfigException_ThrowsBadRequestException()
        {
            //Arrange 1
            var req = new UpdateThresholdsRequest();
            var expectedJsonString = System.Text.Json.JsonSerializer.Serialize(new
            {
                similarity = req.SimilarityScoreThreshold,
                spam = req.SpamConfidenceThreshold,
                toxic = req.ToxicityConfidenceThreshold
            });

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(SystemConfigKeys.ModerationThreshold, expectedJsonString, Arg.Any<string>())
                .Throws(new SystemConfigException("DB Update Failed"));

            //Act
            Func<Task> act = async () => await _sut.UpdateThresholdsAsync(req);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Update Failed");

            await _configRepoMock.Received(1).UpsertConfigAsync(
                SystemConfigKeys.ModerationThreshold,
                expectedJsonString,
                "system config of AI moderation threshold"
            );
        }

        // ── UpdateIntegrationAsync ───────────────────────────────────────────────

        [Fact]
        public async Task UpdateIntegrationAsync_ValidRequest_ReturnsTrue()
        {
            //Arrange 1
            var req = new UpdateIntegrationRequest
            {
                ConfigKey = "TEST_KEY",
                ConfigValue = "TEST_VALUE"
            };

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(req.ConfigKey, req.ConfigValue, $"system config of {req.ConfigKey}").Returns(1);

            //Act
            var result = await _sut.UpdateIntegrationAsync(req);

            //Assert
            result.Should().BeTrue();

            await _configRepoMock.Received(1).UpsertConfigAsync(
                req.ConfigKey,
                req.ConfigValue,
                $"system config of {req.ConfigKey}"
            );
        }

        [Fact]
        public async Task UpdateIntegrationAsync_SystemConfigException_ThrowsBadRequestException()
        {
            //Arrange 1
            var req = new UpdateIntegrationRequest
            {
                ConfigKey = "TEST_KEY",
                ConfigValue = "TEST_VALUE"
            };

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(req.ConfigKey, req.ConfigValue, Arg.Any<string>())
                .Throws(new SystemConfigException("DB Update Failed"));

            //Act
            Func<Task> act = async () => await _sut.UpdateIntegrationAsync(req);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Update Failed");

            await _configRepoMock.Received(1).UpsertConfigAsync(
                req.ConfigKey,
                req.ConfigValue,
                $"system config of {req.ConfigKey}"
            );
        }
    }
}
