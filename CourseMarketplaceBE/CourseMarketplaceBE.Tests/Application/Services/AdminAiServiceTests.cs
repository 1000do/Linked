using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class AdminAiServiceTests
    {
        private readonly IAiModelRepository _aiModelRepoMock;
        private readonly ISystemConfigRepository _configRepoMock;
        private readonly ICourseAiUsageLogRepository _courseLogRepoMock;
        private readonly ICourseReviewModerationLogRepository _courseReviewLogRepoMock;
        private readonly ILessonReviewModerationLogRepository _lessonReviewLogRepoMock;
        private readonly IMapper _mapperMock;
        private readonly AdminAiService _sut;

        public AdminAiServiceTests()
        {
            _aiModelRepoMock = Substitute.For<IAiModelRepository>();
            _configRepoMock = Substitute.For<ISystemConfigRepository>();
            _courseLogRepoMock = Substitute.For<ICourseAiUsageLogRepository>();
            _courseReviewLogRepoMock = Substitute.For<ICourseReviewModerationLogRepository>();
            _lessonReviewLogRepoMock = Substitute.For<ILessonReviewModerationLogRepository>();
            _mapperMock = Substitute.For<IMapper>();

            _sut = new AdminAiService(
                _aiModelRepoMock,
                _configRepoMock,
                _courseLogRepoMock,
                _courseReviewLogRepoMock,
                _lessonReviewLogRepoMock,
                _mapperMock
            );
        }

        // GetAllModelsAsync
        [Fact]
        public async Task GetAllModelsAsync_ShouldReturnMappedModels()
        {
            //Arrange 1
            var models = new List<AiModel> { new AiModel { ModelId = 1 } };
            var dtos = new List<AiModelAdminDto> { new AiModelAdminDto { ModelId = 1 } };

            //Arrange 2
            _aiModelRepoMock.GetAllAdminAsync().Returns(models);
            _mapperMock.Map<List<AiModelAdminDto>>(models).Returns(dtos);

            //Act
            var result = await _sut.GetAllModelsAsync();

            //Assert
            result.Should().BeEquivalentTo(dtos);
            
            await _aiModelRepoMock.Received(1).GetAllAdminAsync();
            _mapperMock.Received(1).Map<List<AiModelAdminDto>>(models);
        }

        [Fact]
        public async Task GetAllModelsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            var emptyModels = new List<AiModel>();

            //Arrange 2
            _aiModelRepoMock.GetAllAdminAsync().Returns(emptyModels);

            //Act
            Func<Task> act = async () => await _sut.GetAllModelsAsync();

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No AI models found.");
            
            await _aiModelRepoMock.Received(1).GetAllAdminAsync();
            _mapperMock.DidNotReceive().Map<List<AiModelAdminDto>>(Arg.Any<List<AiModel>>());
        }

        // GetPagedModelsAsync
        [Fact]
        public async Task GetPagedModelsAsync_ShouldReturnMappedModelsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var models = new List<AiModel> { new AiModel { ModelId = 1 } };
            int totalCount = 10;
            var dtos = new List<AiModelAdminDto> { new AiModelAdminDto { ModelId = 1 } };

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(page, pageSize).Returns((models, totalCount));
            _mapperMock.Map<List<AiModelAdminDto>>(models).Returns(dtos);

            //Act
            var result = await _sut.GetPagedModelsAsync(page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);
            
            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<AiModelAdminDto>>(models);
        }

        [Fact]
        public async Task GetPagedModelsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var emptyModels = new List<AiModel>();

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyModels, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetPagedModelsAsync(page, pageSize);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No AI models found.");
            
            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<AiModelAdminDto>>(Arg.Any<List<AiModel>>());
        }

        // GetModelByIdAsync
        [Fact]
        public async Task GetModelByIdAsync_WhenExists_ShouldReturnModel()
        {
            //Arrange 1
            int mockId = 1;
            var model = new AiModel { ModelId = mockId };
            var dto = new AiModelAdminDto { ModelId = mockId };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(model);
            _mapperMock.Map<AiModelAdminDto>(model).Returns(dto);

            //Act
            var result = await _sut.GetModelByIdAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);
            
            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            _mapperMock.Received(1).Map<AiModelAdminDto>(model);
        }

        [Fact]
        public async Task GetModelByIdAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns((AiModel?)null);

            //Act
            Func<Task> act = async () => await _sut.GetModelByIdAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("AI Model not found.");
            
            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<AiModelAdminDto>(Arg.Any<AiModel>());
        }

        // AddModelAsync
        [Fact]
        public async Task AddModelAsync_ShouldReturnAddedModel()
        {
            //Arrange 1
            var req = new CreateAiModelRequest { ModelName = "Test" };
            var addedModel = new AiModel {ModelId = 1, ModelName = req.ModelName};
            var dto = new AiModelAdminDto { ModelId = 1, ModelName = "Test"};

            //Arrange 2
            _aiModelRepoMock.Add(Arg.Any<AiModel>()).Returns(addedModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(1);
            _mapperMock.Map<AiModelAdminDto>(addedModel).Returns(dto);
            
            //Act
            var result = await _sut.AddModelAsync(req);

            //Assert
            result.ModelName.Should().Be(req.ModelName);
            result.Should().BeEquivalentTo(dto);

            _aiModelRepoMock.Received(1)
                            .Add(Arg.Is<AiModel>(m=>
                                m.ModelName == req.ModelName));
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
            _mapperMock.Received(1)
                        .Map<AiModelAdminDto>(addedModel);
        }

        [Fact]
        public async Task AddModelAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var req = new CreateAiModelRequest { ModelName = "Test" };
            var addedModel = new AiModel {ModelId = 1, ModelName = req.ModelName};

            //Arrange 2
            _aiModelRepoMock.Add(Arg.Any<AiModel>()).Returns(addedModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.AddModelAsync(req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No changes were saved to the database.");

            _aiModelRepoMock.Received(1).Add(Arg.Is<AiModel>(m => m.ModelName == req.ModelName));
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        // UpdateModelAsync
        [Fact]
        public async Task UpdateModelAsync_WhenExists_ShouldReturnUpdatedModel()
        {   
            //Arrange 1
            var mockId = 1;
            var req = new UpdateAiModelRequest { ModelProvider = "New" };
            var existingModel = new AiModel { ModelId = mockId, ModelProvider = "Old" };
            var updatedModel = new AiModel{ModelId = existingModel.ModelId, ModelProvider = req.ModelProvider};
            var dto = new AiModelAdminDto { ModelId = mockId, ModelProvider = "New" };
            
            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(existingModel.ModelId).Returns(existingModel);
            _aiModelRepoMock.Update(Arg.Any<AiModel>()).Returns(updatedModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(1);
            _mapperMock.Map<AiModelAdminDto>(updatedModel).Returns(dto);
            
            //Act
            var result = await _sut.UpdateModelAsync(mockId, req);

            //Assert
            result.ModelProvider.Should().Be(req.ModelProvider);
            result.Should().BeEquivalentTo(dto);

            await _aiModelRepoMock.Received(1)
                            .GetByIdAsync(mockId);
            _aiModelRepoMock.Received(1)
                            .Update(Arg.Is<AiModel>(m=>
                                    m.ModelId == existingModel.ModelId &&
                                    m.ModelProvider == req.ModelProvider));
            await _aiModelRepoMock.Received(1)
                                    .SaveChangesAsync();
            _mapperMock.Received(1)
                        .Map<AiModelAdminDto>(updatedModel);
        }

        [Fact]
        public async Task UpdateModelAsync_WhenNotFound_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            var mockId = 1;
            var req = new UpdateAiModelRequest { ModelProvider = "New" };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns((AiModel?)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateModelAsync(mockId, req);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("AI Model not found.");

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            _aiModelRepoMock.DidNotReceive().Update(Arg.Any<AiModel>());
        }

        [Fact]
        public async Task UpdateModelAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var mockId = 1;
            var req = new UpdateAiModelRequest { ModelProvider = "New" };
            var existingModel = new AiModel { ModelId = mockId };
            var updatedModel = new AiModel { ModelId = mockId, ModelProvider = req.ModelProvider };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(existingModel);
            _aiModelRepoMock.Update(Arg.Any<AiModel>()).Returns(updatedModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.UpdateModelAsync(mockId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No changes were saved to the database.");

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            _aiModelRepoMock.Received(1).Update(Arg.Is<AiModel>(m => m.ModelProvider == req.ModelProvider));
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        // ToggleModelStatusAsync
        [Fact]
        public async Task ToggleModelStatusAsync_WhenActive_ShouldSetInactive()
        {
            //Arrange 1
            var mockId = 1;
            var existingModel = new AiModel { ModelId = mockId, ModelStatus = AiModelConst.Active };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(existingModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ToggleModelStatusAsync(mockId);

            //Assert
            result.Should().BeTrue();
            existingModel.ModelStatus.Should().Be(AiModelConst.Inactive);

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ToggleModelStatusAsync_WhenInactive_ShouldSetActive()
        {
            //Arrange 1
            var mockId = 1;
            var existingModel = new AiModel { ModelId = mockId, ModelStatus = AiModelConst.Inactive };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(existingModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ToggleModelStatusAsync(mockId);

            //Assert
            result.Should().BeTrue();
            existingModel.ModelStatus.Should().Be(AiModelConst.Active);

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ToggleModelStatusAsync_WhenNotFound_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            var mockId = 1;

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns((AiModel?)null);

            //Act
            Func<Task> act = async () => await _sut.ToggleModelStatusAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("AI Model not found.");

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            await _aiModelRepoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task ToggleModelStatusAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var mockId = 1;
            var existingModel = new AiModel { ModelId = mockId, ModelStatus = AiModelConst.Active };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(existingModel);
            _aiModelRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.ToggleModelStatusAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No changes were saved to the database.");

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        // GetConfigurationsAsync
        [Fact]
        public async Task GetConfigurationsAsync_ShouldReturnConfigs()
        {
            //Arrange 1
            string jsonThreshold = "{\"spam\":0.8,\"toxic\":0.7,\"similarity\":0.9}";
            string path1 = "path1";
            string path2 = "path2";
            string path3 = "path3";
            string path4 = "path4";

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(jsonThreshold);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns(path1);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns(path2);
            _configRepoMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns(path3);
            _configRepoMock.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier).Returns(path4);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.SpamConfidenceThreshold.Should().Be(0.8f);
            result.ToxicityConfidenceThreshold.Should().Be(0.7f);
            result.SimilarityScoreThreshold.Should().Be(0.9f);
            result.CourseHarmfulTextClassifierPath.Should().Be(path1);
            result.CourseTextEmbeddingGeneratorPath.Should().Be(path2);
            result.CourseMediaEmbeddingGeneratorPath.Should().Be(path3);
            result.ReviewHarmfulTextClassifierPath.Should().Be(path4);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator);
            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier);
        }

        [Fact]
        public async Task GetConfigurationsAsync_WhenInvalidJson_ShouldUseDefaults()
        {
            //Arrange 1
            string invalidJson = "invalid json string";

            //Arrange 2
            _configRepoMock.GetValueAsync(SystemConfigKeys.ModerationThreshold).Returns(invalidJson);

            //Act
            var result = await _sut.GetConfigurationsAsync();

            //Assert
            result.SpamConfidenceThreshold.Should().Be(AiModelConst.DefaultSpamScoreThreshold);
            result.ToxicityConfidenceThreshold.Should().Be(AiModelConst.DefaultToxicScoreThreshold);
            result.SimilarityScoreThreshold.Should().Be(AiModelConst.DefaultSimilarityScoreThreshold);

            await _configRepoMock.Received(1).GetValueAsync(SystemConfigKeys.ModerationThreshold);
        }

        // UpdateThresholdsAsync
        [Fact]
        public async Task UpdateThresholdsAsync_ShouldReturnTrue()
        {
            //Arrange 1
            var req = new UpdateThresholdsRequest { SpamConfidenceThreshold = 0.5f, ToxicityConfidenceThreshold = 0.6f, SimilarityScoreThreshold = 0.7f };

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(SystemConfigKeys.ModerationThreshold, Arg.Is<string>(s => s.Contains("0.5") && s.Contains("0.6") && s.Contains("0.7")), Arg.Any<string>()).Returns(1);

            //Act
            var result = await _sut.UpdateThresholdsAsync(req);

            //Assert
            result.Should().BeTrue();

            await _configRepoMock.Received(1).UpsertConfigAsync(
                SystemConfigKeys.ModerationThreshold,
                Arg.Is<string>(s => s.Contains("0.5") && s.Contains("0.6") && s.Contains("0.7")),
                Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateThresholdsAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var req = new UpdateThresholdsRequest { SpamConfidenceThreshold = 0.5f, ToxicityConfidenceThreshold = 0.6f, SimilarityScoreThreshold = 0.7f };

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(SystemConfigKeys.ModerationThreshold, Arg.Any<string>(), Arg.Any<string>()).Returns(0);

            //Act
            Func<Task> act = async () => await _sut.UpdateThresholdsAsync(req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to update moderation thresholds: No changes were saved to the database.");

            await _configRepoMock.Received(1).UpsertConfigAsync(SystemConfigKeys.ModerationThreshold, Arg.Any<string>(), Arg.Any<string>());
        }

        // UpdateIntegrationAsync
        [Fact]
        public async Task UpdateIntegrationAsync_ShouldReturnTrue()
        {
            //Arrange 1
            var req = new UpdateIntegrationRequest { ConfigKey = "key", ConfigValue = "value" };

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(req.ConfigKey, req.ConfigValue, Arg.Any<string>()).Returns(1);

            //Act
            var result = await _sut.UpdateIntegrationAsync(req);

            //Assert
            result.Should().BeTrue();

            await _configRepoMock.Received(1).UpsertConfigAsync(req.ConfigKey, req.ConfigValue, Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateIntegrationAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var req = new UpdateIntegrationRequest { ConfigKey = "key", ConfigValue = "value" };

            //Arrange 2
            _configRepoMock.UpsertConfigAsync(req.ConfigKey, req.ConfigValue, Arg.Any<string>()).Returns(0);

            //Act
            Func<Task> act = async () => await _sut.UpdateIntegrationAsync(req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"Failed to update {req.ConfigKey}: No changes were saved to the database.");

            await _configRepoMock.Received(1).UpsertConfigAsync(req.ConfigKey, req.ConfigValue, Arg.Any<string>());
        }

        // GetCourseModerationLogsAsync
        [Fact]
        public async Task GetCourseModerationLogsAsync_ShouldReturnMappedLogsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var logs = new List<CourseAiUsageLog> { new CourseAiUsageLog { LogId = 1 } };
            int totalCount = 5;
            var dtos = new List<CourseModerationLogAdminDto> { new CourseModerationLogAdminDto { LogId = 1 } };

            //Arrange 2
            _courseLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((logs, totalCount));
            _mapperMock.Map<List<CourseModerationLogAdminDto>>(logs).Returns(dtos);

            //Act
            var result = await _sut.GetCourseModerationLogsAsync(page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _courseLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<CourseModerationLogAdminDto>>(logs);
        }

        [Fact]
        public async Task GetCourseModerationLogsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var emptyLogs = new List<CourseAiUsageLog>();

            //Arrange 2
            _courseLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyLogs, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetCourseModerationLogsAsync(page, pageSize);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No course moderation logs found.");

            await _courseLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<CourseModerationLogAdminDto>>(Arg.Any<List<CourseAiUsageLog>>());
        }

        // GetCourseModerationLogDetailAsync
        [Fact]
        public async Task GetCourseModerationLogDetailAsync_WhenExists_ShouldReturnMappedLog()
        {
            //Arrange 1
            int mockId = 1;
            var log = new CourseAiUsageLog { LogId = mockId };
            var dto = new CourseModerationLogAdminDto { LogId = mockId };

            //Arrange 2
            _courseLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns(log);
            _mapperMock.Map<CourseModerationLogAdminDto>(log).Returns(dto);

            //Act
            var result = await _sut.GetCourseModerationLogDetailAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);

            await _courseLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.Received(1).Map<CourseModerationLogAdminDto>(log);
        }

        [Fact]
        public async Task GetCourseModerationLogDetailAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _courseLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns((CourseAiUsageLog?)null);

            //Act
            Func<Task> act = async () => await _sut.GetCourseModerationLogDetailAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Log not found.");

            await _courseLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<CourseModerationLogAdminDto>(Arg.Any<CourseAiUsageLog>());
        }

        // GetCourseReviewModerationLogsAsync
        [Fact]
        public async Task GetCourseReviewModerationLogsAsync_ShouldReturnMappedLogsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var logs = new List<CourseReviewModerationLog> { new CourseReviewModerationLog { LogId = 1 } };
            int totalCount = 5;
            var dtos = new List<ReviewModerationLogAdminDto> { new ReviewModerationLogAdminDto { LogId = 1 } };

            //Arrange 2
            _courseReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((logs, totalCount));
            _mapperMock.Map<List<ReviewModerationLogAdminDto>>(logs).Returns(dtos);

            //Act
            var result = await _sut.GetCourseReviewModerationLogsAsync(page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<ReviewModerationLogAdminDto>>(logs);
        }

        [Fact]
        public async Task GetCourseReviewModerationLogsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var emptyLogs = new List<CourseReviewModerationLog>();

            //Arrange 2
            _courseReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyLogs, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetCourseReviewModerationLogsAsync(page, pageSize);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No course review moderation logs found.");

            await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<ReviewModerationLogAdminDto>>(Arg.Any<List<CourseReviewModerationLog>>());
        }

        // GetCourseReviewModerationLogDetailAsync
        [Fact]
        public async Task GetCourseReviewModerationLogDetailAsync_WhenExists_ShouldReturnMappedLog()
        {
            //Arrange 1
            int mockId = 1;
            var log = new CourseReviewModerationLog { LogId = mockId };
            var dto = new ReviewModerationLogAdminDto { LogId = mockId };

            //Arrange 2
            _courseReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns(log);
            _mapperMock.Map<ReviewModerationLogAdminDto>(log).Returns(dto);

            //Act
            var result = await _sut.GetCourseReviewModerationLogDetailAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);

            await _courseReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.Received(1).Map<ReviewModerationLogAdminDto>(log);
        }

        [Fact]
        public async Task GetCourseReviewModerationLogDetailAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _courseReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns((CourseReviewModerationLog?)null);

            //Act
            Func<Task> act = async () => await _sut.GetCourseReviewModerationLogDetailAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Log not found.");

            await _courseReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<ReviewModerationLogAdminDto>(Arg.Any<CourseReviewModerationLog>());
        }

        // GetLessonReviewModerationLogsAsync
        [Fact]
        public async Task GetLessonReviewModerationLogsAsync_ShouldReturnMappedLogsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var logs = new List<LessonReviewModerationLog> { new LessonReviewModerationLog { LogId = 1 } };
            int totalCount = 5;
            var dtos = new List<ReviewModerationLogAdminDto> { new ReviewModerationLogAdminDto { LogId = 1 } };

            //Arrange 2
            _lessonReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((logs, totalCount));
            _mapperMock.Map<List<ReviewModerationLogAdminDto>>(logs).Returns(dtos);

            //Act
            var result = await _sut.GetLessonReviewModerationLogsAsync(page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<ReviewModerationLogAdminDto>>(logs);
        }

        [Fact]
        public async Task GetLessonReviewModerationLogsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var emptyLogs = new List<LessonReviewModerationLog>();

            //Arrange 2
            _lessonReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyLogs, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetLessonReviewModerationLogsAsync(page, pageSize);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No lesson review moderation logs found.");

            await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<ReviewModerationLogAdminDto>>(Arg.Any<List<LessonReviewModerationLog>>());
        }

        // GetLessonReviewModerationLogDetailAsync
        [Fact]
        public async Task GetLessonReviewModerationLogDetailAsync_WhenExists_ShouldReturnMappedLog()
        {
            //Arrange 1
            int mockId = 1;
            var log = new LessonReviewModerationLog { LogId = mockId };
            var dto = new ReviewModerationLogAdminDto { LogId = mockId };

            //Arrange 2
            _lessonReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns(log);
            _mapperMock.Map<ReviewModerationLogAdminDto>(log).Returns(dto);

            //Act
            var result = await _sut.GetLessonReviewModerationLogDetailAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);

            await _lessonReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.Received(1).Map<ReviewModerationLogAdminDto>(log);
        }

        [Fact]
        public async Task GetLessonReviewModerationLogDetailAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _lessonReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns((LessonReviewModerationLog?)null);

            //Act
            Func<Task> act = async () => await _sut.GetLessonReviewModerationLogDetailAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Log not found.");

            await _lessonReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<ReviewModerationLogAdminDto>(Arg.Any<LessonReviewModerationLog>());
        }
    }
}
