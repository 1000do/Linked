using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Enums;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class CourseAiModerationServiceTests
    {
        private readonly IAiModerationService _aiModerationServiceMock;
        private readonly IAiModelManagementService _aiModelManagementServiceMock;
        private readonly IAiModerationLogService _aiModerationLogServiceMock;
        private readonly IAiConfigurationService _aiConfigurationServiceMock;
        private readonly ICourseQueryService _courseQueryServiceMock;
        private readonly ICourseCommandService _courseCommandServiceMock;
        private readonly IRedisService _redisServiceMock;
        private readonly ICourseAiIntegrationRepository _aiIntegrationRepositoryMock;
        private readonly IAiModelRepository _aiModelRepositoryMock;
        private readonly ISystemConfigRepository _systemConfigRepositoryMock;
        private readonly ILogger<CourseAiModerationService> _loggerMock;
        private readonly ICourseRepository _courseRepositoryMock;
        private readonly IMapper _mapperMock;
        private readonly IHtmlTextManipulationService _htmlTextManipulationServiceMock;
        private readonly IEmbeddingService _embeddingServiceMock;
        private readonly IBackgroundTaskQueue _taskQueueMock;
        private readonly IUserRepository _userRepoMock;
        private readonly INotificationService _notificationServiceMock;
        
        private readonly CourseAiModerationService _sut;

        public CourseAiModerationServiceTests()
        {
            _aiModerationServiceMock = Substitute.For<IAiModerationService>();
            _aiModelManagementServiceMock = Substitute.For<IAiModelManagementService>();
            _aiModerationLogServiceMock = Substitute.For<IAiModerationLogService>();
            _aiConfigurationServiceMock = Substitute.For<IAiConfigurationService>();
            _courseQueryServiceMock = Substitute.For<ICourseQueryService>();
            _courseCommandServiceMock = Substitute.For<ICourseCommandService>();
            _redisServiceMock = Substitute.For<IRedisService>();
            _aiIntegrationRepositoryMock = Substitute.For<ICourseAiIntegrationRepository>();
            _aiModelRepositoryMock = Substitute.For<IAiModelRepository>();
            _systemConfigRepositoryMock = Substitute.For<ISystemConfigRepository>();
            _loggerMock = Substitute.For<ILogger<CourseAiModerationService>>();
            _courseRepositoryMock = Substitute.For<ICourseRepository>();
            _mapperMock = Substitute.For<IMapper>();
            _htmlTextManipulationServiceMock = Substitute.For<IHtmlTextManipulationService>();
            _embeddingServiceMock = Substitute.For<IEmbeddingService>();
            _taskQueueMock = Substitute.For<IBackgroundTaskQueue>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _notificationServiceMock = Substitute.For<INotificationService>();

            _sut = new CourseAiModerationService(
                _aiModerationServiceMock,
                _aiModelManagementServiceMock,
                _aiModerationLogServiceMock,
                _aiConfigurationServiceMock,
                _courseQueryServiceMock,
                _courseCommandServiceMock,
                _redisServiceMock,
                _aiIntegrationRepositoryMock,
                _aiModelRepositoryMock,
                _systemConfigRepositoryMock,
                _loggerMock,
                _courseRepositoryMock,
                _mapperMock,
                _htmlTextManipulationServiceMock,
                _embeddingServiceMock,
                _taskQueueMock,
                _userRepoMock,
                _notificationServiceMock
            );
        }

        private async Task<T> InvokePrivateMethodAsync<T>(string methodName, params object?[] parameters)
        {
            var method = typeof(CourseAiModerationService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Should().NotBeNull($"Method {methodName} should exist");
            var task = (Task<T>)method!.Invoke(_sut, parameters)!;
            return await task;
        }

        private async Task InvokePrivateMethodAsync(string methodName, params object?[] parameters)
        {
            var method = typeof(CourseAiModerationService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Should().NotBeNull($"Method {methodName} should exist");
            var task = (Task)method!.Invoke(_sut, parameters)!;
            await task;
        }

        private T InvokePrivateMethod<T>(string methodName, params object?[] parameters)
        {
            var method = typeof(CourseAiModerationService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Should().NotBeNull($"Method {methodName} should exist");
            return (T)method!.Invoke(_sut, parameters)!;
        }

        private void InvokePrivateMethod(string methodName, params object?[] parameters)
        {
            var method = typeof(CourseAiModerationService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Should().NotBeNull($"Method {methodName} should exist");
            try {
                method!.Invoke(_sut, parameters);
            } catch (TargetInvocationException ex) {
                throw ex.InnerException!;
            }
        }

        // --- PUBLIC METHODS ---

        [Fact]
        public async Task StartCourseModerationAsync_ValidRequest_UpdatesStatusQueuesWorkAndReturnsTrue()
        {
            //Arrange 1
            var request = new CourseModerationRequest { CourseId = 1 };
            int instructorId = 2;
            Func<ICourseAiModerationService, CancellationToken, ValueTask>? capturedDelegate = null;

            //Arrange 2
            _courseCommandServiceMock.UpdateCourseStatusAsync(request.CourseId, CourseStatus.Pending.ToValue(), instructorId).Returns(Task.CompletedTask);
            _redisServiceMock.RemoveCacheAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
            _taskQueueMock.When(x => x.QueueBackgroundWorkItemAsync(Arg.Any<Func<ICourseAiModerationService, CancellationToken, ValueTask>>()))
                .Do(callInfo => capturedDelegate = callInfo.Arg<Func<ICourseAiModerationService, CancellationToken, ValueTask>>());

            //Act
            var result = await _sut.StartCourseModerationAsync(request, instructorId);

            //Assert
            result.Should().BeTrue();
            await _courseCommandServiceMock.Received(1).UpdateCourseStatusAsync(request.CourseId, CourseStatus.Pending.ToValue(), instructorId);
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseModerationDetail.GetKey(request.CourseId));
            
            capturedDelegate.Should().NotBeNull();
            var serviceMock = Substitute.For<ICourseAiModerationService>();
            serviceMock.HandleCourseModerationWithAIAsync(request).Returns(new CourseModerationResult());
            await capturedDelegate!(serviceMock, CancellationToken.None);
            await serviceMock.Received(1).HandleCourseModerationWithAIAsync(request);
        }

        [Fact]
        public async Task StartCourseModerationAsync_DelegateThrowsException_ExceptionIsSwallowed()
        {
            //Arrange 1
            var request = new CourseModerationRequest { CourseId = 1 };
            int instructorId = 2;
            Func<ICourseAiModerationService, CancellationToken, ValueTask>? capturedDelegate = null;

            //Arrange 2
            _courseCommandServiceMock.UpdateCourseStatusAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.CompletedTask);
            _redisServiceMock.RemoveCacheAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
            _taskQueueMock.When(x => x.QueueBackgroundWorkItemAsync(Arg.Any<Func<ICourseAiModerationService, CancellationToken, ValueTask>>()))
                .Do(callInfo => capturedDelegate = callInfo.Arg<Func<ICourseAiModerationService, CancellationToken, ValueTask>>());

            //Act
            var result = await _sut.StartCourseModerationAsync(request, instructorId);

            //Assert
            result.Should().BeTrue();
            capturedDelegate.Should().NotBeNull();
            var serviceMock = Substitute.For<ICourseAiModerationService>();
            serviceMock.When(x => x.HandleCourseModerationWithAIAsync(request)).Do(x => throw new Exception("Test error"));
            
            Func<Task> actDelegate = async () => await capturedDelegate!(serviceMock, CancellationToken.None);
            await actDelegate.Should().NotThrowAsync();
        }
        
        [Fact]
        public async Task HandleCourseModerationWithAIAsync_AiServiceUnhealthy_ReturnsManualAuditAndNotifiesManagers()
        {
            //Arrange 1
            var request = new CourseModerationRequest { CourseId = 1 };
            var expectedStatus = ModerationStatus.ManualAudit.ToValue();

            //Arrange 2
            _aiModerationServiceMock.HealthCheckAsync().Returns(false);
            _userRepoMock.GetAllManagerIdsAsync().Returns(new List<int> { 101, 102 });
            _notificationServiceMock.SendBulkNotificationsAsync(Arg.Any<List<NotificationBulkDto>>()).Returns(true);

            //Act
            var result = await _sut.HandleCourseModerationWithAIAsync(request);

            //Assert
            result.CourseId.Should().Be(request.CourseId);
            result.ModerationStatus.Should().Be(expectedStatus);
            await _aiModerationServiceMock.Received(1).HealthCheckAsync();
            await _userRepoMock.Received(1).GetAllManagerIdsAsync();
            await _notificationServiceMock.Received(1).SendBulkNotificationsAsync(Arg.Is<List<NotificationBulkDto>>(l => l.Count == 2 && l[0].Title == "AI Service Unhealthy"));
        }

        [Fact]
        public async Task HandleCourseModerationWithAIAsync_PipelineThrowsException_CatchesLogsAndReturnsManualAudit()
        {
            //Arrange 1
            var request = new CourseModerationRequest { CourseId = 1 };
            var expectedStatus = ModerationStatus.ManualAudit.ToValue();

            //Arrange 2
            _aiModerationServiceMock.HealthCheckAsync().Returns(true);
            _systemConfigRepositoryMock.GetValueAsync(Arg.Any<string>()).Throws(new Exception("Mock DB Failure"));
            _userRepoMock.GetAllManagerIdsAsync().Returns(new List<int> { 101 });

            //Act
            var result = await _sut.HandleCourseModerationWithAIAsync(request);

            //Assert
            result.CourseId.Should().Be(request.CourseId);
            result.ModerationStatus.Should().Be(expectedStatus);
            await _notificationServiceMock.Received(1).SendBulkNotificationsAsync(Arg.Is<List<NotificationBulkDto>>(l => l[0].Title == "Moderation Process Exception"));
        }

        [Fact]
        public async Task HandleCourseModerationWithAIAsync_PipelineSuccess_ReturnsModerationResult()
        {
            //Arrange 1
            var request = new CourseModerationRequest { CourseId = 1 };
            var mockPrepResult = new CourseModerationResult { CourseId = 1, ModerationStatus = ModerationStatus.Approved.ToValue(), StageLogs = new List<StageLog>() };
            
            //Arrange 2
            _aiModerationServiceMock.HealthCheckAsync().Returns(true);
            
            _systemConfigRepositoryMock.GetValueAsync(Arg.Any<string>()).Returns("path");
            _aiModelRepositoryMock.GetByModelPathAsync(Arg.Any<string>()).Returns(new AiModelDto { ModelId = 1, ProcessType = "classifier", ModelType = "text" });
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(new AiConfigurationDto { SimilarityScoreThreshold = 0.9f });
            _aiIntegrationRepositoryMock.GetByCourseIdAsync(request.CourseId).Returns(new List<CourseAiIntegration>());
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns(new CourseModerationDetailResponse { Lessons = new List<LessonResponse>() });
            _embeddingServiceMock.PrepareMaterialEmbeddingsAsync().Returns(true);
            
            _aiModerationServiceMock.ModerateCourseFullPipelineAsync(Arg.Any<SemanticDuplicationRequest>(), Arg.Any<CourseHarmfulRequest>()).Returns(mockPrepResult);
            _courseCommandServiceMock.UpdateCourseStatusAndFeedbackAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<AiThreatLevel>()).Returns(Task.CompletedTask);
            _userRepoMock.GetAllManagerIdsAsync().Returns(new List<int>());

            //Act
            var result = await _sut.HandleCourseModerationWithAIAsync(request);

            //Assert
            result.CourseId.Should().Be(request.CourseId);
            result.ModerationStatus.Should().Be(ModerationStatus.Approved.ToValue());
            await _aiModerationServiceMock.Received(1).ModerateCourseFullPipelineAsync(Arg.Any<SemanticDuplicationRequest>(), Arg.Any<CourseHarmfulRequest>());
        }

        // --- PRIVATE METHODS ---

        [Fact]
        public async Task GetCourseForModerationAsync_CacheHit_ReturnsCachedResponse()
        {
            //Arrange 1
            int courseId = 1;
            var expectedResponse = new CourseModerationDetailResponse { CourseId = courseId };
            
            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns(expectedResponse);

            //Act
            var result = await InvokePrivateMethodAsync<CourseModerationDetailResponse?>("GetCourseForModerationAsync", courseId);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _redisServiceMock.Received(1).GetCacheAsync<CourseModerationDetailResponse>(CacheKeys.CourseModerationDetail.GetKey(courseId));
            await _courseRepositoryMock.DidNotReceive().GetCourseWithDetailsAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetCourseForModerationAsync_CacheMissAndCourseNotFound_ReturnsNull()
        {
            //Arrange 1
            int courseId = 1;
            
            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns((CourseModerationDetailResponse?)null);
            _courseRepositoryMock.GetCourseWithDetailsAsync(courseId).Returns((Course?)null);

            //Act
            var result = await InvokePrivateMethodAsync<CourseModerationDetailResponse?>("GetCourseForModerationAsync", courseId);

            //Assert
            result.Should().BeNull();
            await _redisServiceMock.Received(1).GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>());
            await _courseRepositoryMock.Received(1).GetCourseWithDetailsAsync(courseId);
        }

        [Fact]
        public async Task GetCourseForModerationAsync_CacheMissAndCourseExists_CachesAndReturnsResponse()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId };
            var mappedResponse = new CourseModerationDetailResponse { CourseId = courseId, Description = "desc" };
            
            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns((CourseModerationDetailResponse?)null);
            _courseRepositoryMock.GetCourseWithDetailsAsync(courseId).Returns(course);
            _mapperMock.Map<CourseModerationDetailResponse>(course).Returns(mappedResponse);
            _htmlTextManipulationServiceMock.ExtractPlainText(Arg.Any<string>()).Returns("plain");
            _redisServiceMock.SetCacheAsync(Arg.Any<string>(), Arg.Any<CourseModerationDetailResponse>(), Arg.Any<TimeSpan>()).Returns(Task.CompletedTask);

            //Act
            var result = await InvokePrivateMethodAsync<CourseModerationDetailResponse?>("GetCourseForModerationAsync", courseId);

            //Assert
            result.Should().NotBeNull();
            result!.Description.Should().Be("plain");
            await _courseRepositoryMock.Received(1).GetCourseWithDetailsAsync(courseId);
            await _redisServiceMock.Received(1).SetCacheAsync(Arg.Any<string>(), mappedResponse, Arg.Any<TimeSpan>());
        }

        [Fact]
        public async Task UpdateCourseStatusAndClearCacheAsync_ValidInputs_ExecutesCommands()
        {
            //Arrange 1
            int courseId = 1;
            string status = "status";
            int instructorId = 2;

            //Arrange 2
            _courseCommandServiceMock.UpdateCourseStatusAsync(courseId, status, instructorId).Returns(Task.CompletedTask);
            _redisServiceMock.RemoveCacheAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

            //Act
            await InvokePrivateMethodAsync("UpdateCourseStatusAndClearCacheAsync", courseId, status, instructorId);

            //Assert
            await _courseCommandServiceMock.Received(1).UpdateCourseStatusAsync(courseId, status, instructorId);
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseModerationDetail.GetKey(courseId));
        }

        [Fact]
        public void ExtractPlainTextForModerationResponse_NullProperties_HandlesSafely()
        {
            //Arrange 1
            var response = new CourseModerationDetailResponse { Description = null, WhatYouWillLearn = null, Requirements = null, Lessons = null };

            //Arrange 2
            _htmlTextManipulationServiceMock.ExtractPlainText("").Returns("empty");

            //Act
            InvokePrivateMethod("ExtractPlainTextForModerationResponse", response);

            //Assert
            response.Description.Should().Be("empty");
            response.WhatYouWillLearn.Should().Be("empty");
            response.Requirements.Should().Be("empty");
            _htmlTextManipulationServiceMock.Received(3).ExtractPlainText("");
        }
        
        [Fact]
        public void ExtractPlainTextForModerationResponse_EmptyLessons_BypassesLoop()
        {
            //Arrange 1
            var response = new CourseModerationDetailResponse { Lessons = new List<LessonResponse>() };

            //Arrange 2
            _htmlTextManipulationServiceMock.ExtractPlainText(Arg.Any<string>()).Returns("empty");

            //Act
            InvokePrivateMethod("ExtractPlainTextForModerationResponse", response);

            //Assert
            _htmlTextManipulationServiceMock.Received(3).ExtractPlainText(Arg.Any<string>());
        }

        [Fact]
        public void ExtractPlainTextForModerationResponse_ValidLessonsAndMaterials_ExtractsTextForMaterials()
        {
            //Arrange 1
            var material = new MaterialResponse { MaterialId = 1, Description = "mat_desc", Title = "test" };
            var lesson = new LessonResponse { Title = "t", LearningMaterials = new List<MaterialResponse> { material } };
            var response = new CourseModerationDetailResponse { Lessons = new List<LessonResponse> { lesson } };

            //Arrange 2
            _htmlTextManipulationServiceMock.ExtractPlainText(Arg.Any<string>()).Returns(x => x.Arg<string>() + "_extracted");

            //Act
            InvokePrivateMethod("ExtractPlainTextForModerationResponse", response);

            //Assert
            material.Description.Should().Be("mat_desc_extracted");
        }

        [Fact]
        public void ExtractPlainTextForModerationResponse_LessonsWithNullMaterials_SkipsMaterialLoop()
        {
            //Arrange 1
            var lesson = new LessonResponse { Title = "l", LearningMaterials = null! };
            var response = new CourseModerationDetailResponse { Title = "c", Lessons = new List<LessonResponse> { lesson } };

            //Arrange 2
            _htmlTextManipulationServiceMock.ExtractPlainText(Arg.Any<string>()).Returns("extracted");

            //Act
            InvokePrivateMethod("ExtractPlainTextForModerationResponse", response);

            //Assert
            // Should not throw, should only extract main properties (3 times)
            _htmlTextManipulationServiceMock.Received(3).ExtractPlainText(Arg.Any<string>());
        }

        [Fact]
        public async Task GetModerationThresholdsAsync_FetchesConfig_ReturnsMappedDictionary()
        {
            //Arrange 1
            var config = new AiConfigurationDto { SimilarityScoreThreshold = 0.5f, SpamConfidenceThreshold = 0.6f, ToxicityConfidenceThreshold = 0.7f };

            //Arrange 2
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(config);

            //Act
            var result = await InvokePrivateMethodAsync<Dictionary<string, float>>("GetModerationThresholdsAsync");

            //Assert
            result[AiModelConst.Similarity].Should().Be(0.5f);
            result[AiModelConst.Spam].Should().Be(0.6f);
            result[AiModelConst.Toxic].Should().Be(0.7f);
        }

        [Fact]
        public async Task AssignAIModeratorsToCourseAsync_MissingModels_ExecutesIntegrationCommand()
        {
            //Arrange 1
            int courseId = 1;
            var models = new List<AiModelDto> { new AiModelDto { ModelId = 1, ModelType = "Type", ProcessType = "Process" } };
            
            //Arrange 2
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(new AiConfigurationDto());
            _courseQueryServiceMock.GetByModelAndCourseAsync(1, courseId).Returns((CourseAiIntegrationResponse?)null);
            _courseCommandServiceMock.IntegrateAItoCourseAsync(Arg.Any<CourseAIIntegrationCommand>()).Returns(new CourseAIIntegrationResult());

            //Act
            var result = await InvokePrivateMethodAsync<AssignAIModeratorsToCourseResult>("AssignAIModeratorsToCourseAsync", courseId, models);

            //Assert
            result.CourseId.Should().Be(courseId);
            result.ModelIds.Should().Contain(1);
            await _courseCommandServiceMock.Received(1).IntegrateAItoCourseAsync(Arg.Is<CourseAIIntegrationCommand>(c => c.ModelId == 1 && c.Role == "type_process"));
        }

        [Fact]
        public async Task AssignAIModeratorsToCourseAsync_ExistingModels_SkipsIntegrationCommand()
        {
            //Arrange 1
            int courseId = 1;
            var models = new List<AiModelDto> { new AiModelDto { ModelId = 1 } };
            
            //Arrange 2
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(new AiConfigurationDto());
            _courseQueryServiceMock.GetByModelAndCourseAsync(1, courseId).Returns(new CourseAiIntegrationResponse());

            //Act
            var result = await InvokePrivateMethodAsync<AssignAIModeratorsToCourseResult>("AssignAIModeratorsToCourseAsync", courseId, models);

            //Assert
            result.CourseId.Should().Be(courseId);
            await _courseCommandServiceMock.DidNotReceive().IntegrateAItoCourseAsync(Arg.Any<CourseAIIntegrationCommand>());
        }

        [Fact]
        public async Task PrepareForCourseAIModeration_Success_ReturnsPreparedResult()
        {
            //Arrange 1
            int courseId = 1;

            //Arrange 2
            _systemConfigRepositoryMock.GetValueAsync(Arg.Any<string>()).Returns("");
            _aiModelManagementServiceMock.GetModelsByTypeAsync(Arg.Any<string>()).Returns(new List<AiModelDto>());
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(new AiConfigurationDto());
            _aiIntegrationRepositoryMock.GetByCourseIdAsync(courseId).Returns(new List<CourseAiIntegration>());
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns(new CourseModerationDetailResponse());
            _embeddingServiceMock.PrepareMaterialEmbeddingsAsync().Returns(true);

            //Act
            var result = await InvokePrivateMethodAsync<PrepareForCourseAIModerationResult>("PrepareForCourseAIModeration", courseId);

            //Assert
            result.CourseId.Should().Be(courseId);
            await _embeddingServiceMock.Received(1).PrepareMaterialEmbeddingsAsync();
        }

        [Fact]
        public async Task PrepareForCourseAIModeration_ThrowsException_RethrowsException()
        {
            //Arrange 1
            int courseId = 1;

            //Arrange 2
            _systemConfigRepositoryMock.GetValueAsync(Arg.Any<string>()).Throws(new Exception("Inner error"));

            //Act
            Func<Task> act = async () => await InvokePrivateMethodAsync<PrepareForCourseAIModerationResult>("PrepareForCourseAIModeration", courseId);

            //Assert
            var ex = await act.Should().ThrowAsync<Exception>();
            ex.WithMessage("Inner error");
        }

        [Fact]
        public async Task GetCourseModerationModelsAsync_ConfigPathsValidAndModelsExist_ReturnsModelsFromDb()
        {
            //Arrange 1
            var classifierModel = new AiModelDto { ModelId = 1 };
            var textModel = new AiModelDto { ModelId = 2 };
            var mediaModel = new AiModelDto { ModelId = 3 };

            //Arrange 2
            _systemConfigRepositoryMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns("path1");
            _systemConfigRepositoryMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns("path2");
            _systemConfigRepositoryMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns("path3");

            _aiModelRepositoryMock.GetByModelPathAsync("path1").Returns(classifierModel);
            _aiModelRepositoryMock.GetByModelPathAsync("path2").Returns(textModel);
            _aiModelRepositoryMock.GetByModelPathAsync("path3").Returns(mediaModel);

            //Act
            var result = await InvokePrivateMethodAsync<(List<AiModelDto> classifiers, List<AiModelDto> emb_generators)>("GetCourseModerationModelsAsync");

            //Assert
            result.classifiers.Should().Contain(classifierModel);
            result.emb_generators.Should().Contain(textModel);
            result.emb_generators.Should().Contain(mediaModel);
            await _aiModelManagementServiceMock.DidNotReceive().GetModelsByTypeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetCourseModerationModelsAsync_ConfigPathsEmpty_ReturnsModelsFromFallbackService()
        {
            //Arrange 1
            var fallbackClassifiers = new List<AiModelDto> { new AiModelDto { ModelId = 4 } };
            var fallbackGenerators = new List<AiModelDto> { new AiModelDto { ModelId = 5 } };

            //Arrange 2
            _systemConfigRepositoryMock.GetValueAsync(Arg.Any<string>()).Returns((string?)null);
            _aiModelManagementServiceMock.GetModelsByTypeAsync(AiModelConst.Classifier).Returns(fallbackClassifiers);
            _aiModelManagementServiceMock.GetModelsByTypeAsync(AiModelConst.EmbeddingGenerator).Returns(fallbackGenerators);

            //Act
            var result = await InvokePrivateMethodAsync<(List<AiModelDto> classifiers, List<AiModelDto> emb_generators)>("GetCourseModerationModelsAsync");

            //Assert
            result.classifiers.Should().BeEquivalentTo(fallbackClassifiers);
            result.emb_generators.Should().BeEquivalentTo(fallbackGenerators);
            await _aiModelRepositoryMock.DidNotReceive().GetByModelPathAsync(Arg.Any<string>());
        }
        
        [Fact]
        public async Task GetModelConfigPathsAsync_Valid_ReturnsPaths()
        {
            //Arrange 1
            string expectedClassifer = "path1";
            string expectedText = "path2";
            string expectedMedia = "path3";

            //Arrange 2
            _systemConfigRepositoryMock.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier).Returns(expectedClassifer);
            _systemConfigRepositoryMock.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator).Returns(expectedText);
            _systemConfigRepositoryMock.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator).Returns(expectedMedia);
            
            //Act
            var result = await InvokePrivateMethodAsync<(string? classifierPath, string? textGeneratorPath, string? mediaGeneratorPath)>("GetModelConfigPathsAsync");

            //Assert
            result.classifierPath.Should().Be(expectedClassifer);
            result.textGeneratorPath.Should().Be(expectedText);
            result.mediaGeneratorPath.Should().Be(expectedMedia);
        }
        
        [Fact]
        public async Task UpdateCourseAIIntegrationsAsync_IntegrationsNull_CallsAssignAIModerators()
        {
            //Arrange 1
            int courseId = 1;
            var classifiers = new List<AiModelDto> { new AiModelDto { ModelId = 1, ProcessType = "process", ModelType = "type" } };
            var embGenerators = new List<AiModelDto>();
            var thresholds = new Dictionary<string, float>();

            //Arrange 2
            _aiIntegrationRepositoryMock.GetByCourseIdAsync(courseId).Returns((List<CourseAiIntegration>)null!);
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(new AiConfigurationDto());

            //Act
            await InvokePrivateMethodAsync("UpdateCourseAIIntegrationsAsync", courseId, classifiers, embGenerators, thresholds);

            //Assert
            // Should call AssignAIModeratorsToCourseAsync which calls IntegrateAItoCourseAsync since integration is empty
            await _courseCommandServiceMock.Received(1).IntegrateAItoCourseAsync(Arg.Any<CourseAIIntegrationCommand>());
        }
        
        [Fact]
        public async Task UpdateCourseAIIntegrationsAsync_IntegrationsEmpty_CallsAssignAIModerators()
        {
            //Arrange 1
            int courseId = 1;
            var classifiers = new List<AiModelDto> { new AiModelDto { ModelId = 1, ProcessType = "process", ModelType = "type" } };
            var embGenerators = new List<AiModelDto>();
            var thresholds = new Dictionary<string, float>();

            //Arrange 2
            _aiIntegrationRepositoryMock.GetByCourseIdAsync(courseId).Returns(new List<CourseAiIntegration>());
            _aiConfigurationServiceMock.GetConfigurationsAsync().Returns(new AiConfigurationDto());

            //Act
            await InvokePrivateMethodAsync("UpdateCourseAIIntegrationsAsync", courseId, classifiers, embGenerators, thresholds);

            //Assert
            await _courseCommandServiceMock.Received(1).IntegrateAItoCourseAsync(Arg.Any<CourseAIIntegrationCommand>());
        }

        [Fact]
        public async Task UpdateCourseAIIntegrationsAsync_IntegrationsExistAndMatch_UpdatesAndSaves()
        {
            //Arrange 1
            int courseId = 1;
            var integration = new CourseAiIntegration { Role = "type_process", ModelId = 2 };
            var integrations = new List<CourseAiIntegration> { integration };
            var classifiers = new List<AiModelDto> { new AiModelDto { ModelId = 1, ProcessType = "process", ModelType = "type" } };
            var embGenerators = new List<AiModelDto>();
            var thresholds = new Dictionary<string, float>();

            //Arrange 2
            _aiIntegrationRepositoryMock.GetByCourseIdAsync(courseId).Returns(integrations);
            _aiIntegrationRepositoryMock.SaveChangesAsync().Returns(1);

            //Act
            await InvokePrivateMethodAsync("UpdateCourseAIIntegrationsAsync", courseId, classifiers, embGenerators, thresholds);

            //Assert
            integration.ModelId.Should().Be(1);
            _aiIntegrationRepositoryMock.Received(1).Update(integration);
            await _aiIntegrationRepositoryMock.Received(1).SaveChangesAsync();
        }
        
        [Fact]
        public async Task UpdateCourseAIIntegrationsAsync_IntegrationsExistNoMatch_DoesNotUpdate()
        {
            //Arrange 1
            int courseId = 1;
            var integration = new CourseAiIntegration { Role = "different_role", ModelId = 2 };
            var integrations = new List<CourseAiIntegration> { integration };
            var classifiers = new List<AiModelDto> { new AiModelDto { ModelId = 1, ProcessType = "process", ModelType = "type" } };
            var embGenerators = new List<AiModelDto>();
            var thresholds = new Dictionary<string, float>();

            //Arrange 2
            _aiIntegrationRepositoryMock.GetByCourseIdAsync(courseId).Returns(integrations);

            //Act
            await InvokePrivateMethodAsync("UpdateCourseAIIntegrationsAsync", courseId, classifiers, embGenerators, thresholds);

            //Assert
            integration.ModelId.Should().Be(2);
            _aiIntegrationRepositoryMock.DidNotReceive().Update(integration);
            await _aiIntegrationRepositoryMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task SaveCourseAiIntegrationChangesAsync_Success_ReturnsRowsAffected()
        {
            //Arrange 1
            int expectedRows = 5;

            //Arrange 2
            _aiIntegrationRepositoryMock.SaveChangesAsync().Returns(expectedRows);

            //Act
            var result = await InvokePrivateMethodAsync<int>("SaveCourseAiIntegrationChangesAsync");

            //Assert
            result.Should().Be(expectedRows);
            await _aiIntegrationRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SaveCourseAiIntegrationChangesAsync_ThrowsCourseAiIntegrationException_ThrowsBadRequestException()
        {
            //Arrange 1
            
            //Arrange 2
            _aiIntegrationRepositoryMock.SaveChangesAsync().Throws(new CourseAiIntegrationException("save failed"));

            //Act
            Func<Task> act = async () => await InvokePrivateMethodAsync<int>("SaveCourseAiIntegrationChangesAsync");

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("save failed");
        }
        
        [Fact]
        public async Task GetCourseMaterialIdsAsync_CourseNull_ReturnsEmptyList()
        {
            //Arrange 1
            int courseId = 1;
            
            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns((CourseModerationDetailResponse?)null);
            _courseRepositoryMock.GetCourseWithDetailsAsync(courseId).Returns((Course?)null);

            //Act
            var result = await InvokePrivateMethodAsync<List<int>>("GetCourseMaterialIdsAsync", courseId);

            //Assert
            result.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetCourseMaterialIdsAsync_LessonsNull_ReturnsEmptyList()
        {
            //Arrange 1
            int courseId = 1;
            var response = new CourseModerationDetailResponse { Lessons = null! };
            
            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns(response);

            //Act
            var result = await InvokePrivateMethodAsync<List<int>>("GetCourseMaterialIdsAsync", courseId);

            //Assert
            result.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetCourseMaterialIdsAsync_ValidLessons_ReturnsMaterialIds()
        {
            //Arrange 1
            int courseId = 1;
            var response = new CourseModerationDetailResponse { 
                Lessons = new List<LessonResponse> { 
                    new LessonResponse { 
                        LearningMaterials = new List<MaterialResponse> { 
                            new MaterialResponse { MaterialId = 5, Title = "a" },
                            new MaterialResponse { MaterialId = 6, Title = "b" }
                        } 
                    } 
                } 
            };
            
            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseModerationDetailResponse>(Arg.Any<string>()).Returns(response);

            //Act
            var result = await InvokePrivateMethodAsync<List<int>>("GetCourseMaterialIdsAsync", courseId);

            //Assert
            result.Should().BeEquivalentTo(new List<int> { 5, 6 });
        }

        [Fact]
        public async Task ResolveCourseAIModerationResult_Standard_UpdatesStatusAndNotifiesManagers()
        {
            //Arrange 1
            var result = new CourseModerationResult { CourseId = 1, ModerationStatus = ModerationStatus.Approved.ToValue(), StageLogs = new List<StageLog>() };
            
            //Arrange 2
            _courseCommandServiceMock.UpdateCourseStatusAndFeedbackAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<AiThreatLevel>()).Returns(Task.CompletedTask);
            _userRepoMock.GetAllManagerIdsAsync().Returns(new List<int> { 101 });
            _notificationServiceMock.SendBulkNotificationsAsync(Arg.Any<List<NotificationBulkDto>>()).Returns(true);

            //Act
            await InvokePrivateMethodAsync("ResolveCourseAIModerationResult", result);

            //Assert
            await _courseCommandServiceMock.Received(1).UpdateCourseStatusAndFeedbackAsync(1, null, "AI moderation passed. Awaiting manual review.", AiThreatLevel.Approved);
            await _notificationServiceMock.Received(1).SendBulkNotificationsAsync(Arg.Is<List<NotificationBulkDto>>(l => l.Count == 1 && l[0].Title == "Manual Review Required"));
        }

        [Fact]
        public void EvaluateModerationFeedback_StatusRejectedWithMatchFoundLogs_ReturnsFlaggedThreatLevelAndReasons()
        {
            //Arrange 1
            var log = new StageLog { Result = StageLogResult.MatchFound.ToValue(), FlaggedFields = new List<string> { "desc" }, Reason = "dup" };
            var result = new CourseModerationResult { ModerationStatus = ModerationStatus.Rejected.ToValue(), StageLogs = new List<StageLog> { log } };

            //Arrange 2
            
            //Act
            var feedbackRes = InvokePrivateMethod<(AiThreatLevel ThreatLevel, string Feedback)>("EvaluateModerationFeedback", result);

            //Assert
            feedbackRes.ThreatLevel.Should().Be(AiThreatLevel.FlaggedOrRejected);
            feedbackRes.Feedback.Should().Contain("[desc] dup");
        }

        [Fact]
        public void EvaluateModerationFeedback_StatusFlaggedWithNoLogs_ReturnsGenericFeedback()
        {
            //Arrange 1
            var result = new CourseModerationResult { ModerationStatus = ModerationStatus.Flagged.ToValue(), StageLogs = new List<StageLog>() };

            //Arrange 2
            
            //Act
            var feedbackRes = InvokePrivateMethod<(AiThreatLevel ThreatLevel, string Feedback)>("EvaluateModerationFeedback", result);

            //Assert
            feedbackRes.ThreatLevel.Should().Be(AiThreatLevel.FlaggedOrRejected);
            feedbackRes.Feedback.Should().Be("Course content violates the moderation policy according to AI.");
        }

        [Fact]
        public void EvaluateModerationFeedback_StatusApproved_ReturnsApprovedThreatLevel()
        {
            //Arrange 1
            var result = new CourseModerationResult { ModerationStatus = ModerationStatus.Approved.ToValue(), StageLogs = new List<StageLog>() };

            //Arrange 2
            
            //Act
            var feedbackRes = InvokePrivateMethod<(AiThreatLevel ThreatLevel, string Feedback)>("EvaluateModerationFeedback", result);

            //Assert
            feedbackRes.ThreatLevel.Should().Be(AiThreatLevel.Approved);
        }

        [Fact]
        public void EvaluateModerationFeedback_StatusManualAudit_ReturnsManualAuditThreatLevel()
        {
            //Arrange 1
            var result = new CourseModerationResult { ModerationStatus = ModerationStatus.ManualAudit.ToValue(), StageLogs = new List<StageLog>() };

            //Arrange 2
            
            //Act
            var feedbackRes = InvokePrivateMethod<(AiThreatLevel ThreatLevel, string Feedback)>("EvaluateModerationFeedback", result);

            //Assert
            feedbackRes.ThreatLevel.Should().Be(AiThreatLevel.ManualAudit);
        }
        
        [Fact]
        public void EvaluateModerationFeedback_StatusOther_ReturnsNoneThreatLevel()
        {
            //Arrange 1
            var result = new CourseModerationResult { ModerationStatus = "unknown", StageLogs = new List<StageLog>() };

            //Arrange 2
            
            //Act
            var feedbackRes = InvokePrivateMethod<(AiThreatLevel ThreatLevel, string Feedback)>("EvaluateModerationFeedback", result);

            //Assert
            feedbackRes.ThreatLevel.Should().Be(AiThreatLevel.None);
        }

        [Fact]
        public async Task LogCourseAiModeration_IntegrationFound_SavesLog()
        {
            //Arrange 1
            var stage = new StageLog { ModelId = 1, Stage = 1 };
            var modResult = new CourseModerationResult { CourseId = 2, StageLogs = new List<StageLog> { stage } };
            var command = new LogCourseAiModerationCommand { CourseModerationResult = modResult, SemanticDuplicationRequest = new SemanticDuplicationRequest { MaterialIds = new List<int>() }, CourseHarmfulRequest = new CourseHarmfulRequest() };
            var integration = new CourseAiIntegration { Id = 3 };

            //Arrange 2
            _aiIntegrationRepositoryMock.GetByModelAndCourseAsync(1, 2).Returns(integration);
            _aiModerationLogServiceMock.SaveCourseAiUsageLog(Arg.Any<SaveCourseAiUsageLogCommand>()).Returns(1);

            //Act
            await InvokePrivateMethodAsync("LogCourseAiModeration", command);

            //Assert
            await _aiIntegrationRepositoryMock.Received(1).GetByModelAndCourseAsync(1, 2);
            await _aiModerationLogServiceMock.Received(1).SaveCourseAiUsageLog(Arg.Is<SaveCourseAiUsageLogCommand>(c => c.IntegrationId == 3));
        }

        [Fact]
        public async Task LogCourseAiModeration_IntegrationNotFoundForStage_SkipsStage()
        {
            //Arrange 1
            var stage = new StageLog { ModelId = 1 };
            var modResult = new CourseModerationResult { CourseId = 2, StageLogs = new List<StageLog> { stage } };
            var command = new LogCourseAiModerationCommand { CourseModerationResult = modResult, SemanticDuplicationRequest = new SemanticDuplicationRequest { MaterialIds = new List<int>() }, CourseHarmfulRequest = new CourseHarmfulRequest() };

            //Arrange 2
            _aiIntegrationRepositoryMock.GetByModelAndCourseAsync(1, 2).Returns((CourseAiIntegration?)null);

            //Act
            await InvokePrivateMethodAsync("LogCourseAiModeration", command);

            //Assert
            await _aiModerationLogServiceMock.DidNotReceive().SaveCourseAiUsageLog(Arg.Any<SaveCourseAiUsageLogCommand>());
        }
        
        [Fact]
        public async Task LogCourseAiModeration_EmptyStageLogs_DoesNothing()
        {
            //Arrange 1
            var modResult = new CourseModerationResult { CourseId = 2, StageLogs = new List<StageLog>() };
            var command = new LogCourseAiModerationCommand { CourseModerationResult = modResult, SemanticDuplicationRequest = new SemanticDuplicationRequest { MaterialIds = new List<int>() }, CourseHarmfulRequest = new CourseHarmfulRequest() };

            //Arrange 2
            
            //Act
            await InvokePrivateMethodAsync("LogCourseAiModeration", command);

            //Assert
            await _aiIntegrationRepositoryMock.DidNotReceive().GetByModelAndCourseAsync(Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task CreateModerationRequests_ValidInput_ReturnsRequests()
        {
            //Arrange 1
            int courseId = 1;
            var prep = new PrepareForCourseAIModerationResult
            {
                CourseId = courseId,
                MaterialIds = new List<int> { 1, 2 },
                Thresholds = new Dictionary<string, float> { { AiModelConst.Similarity, 0.8f }, { AiModelConst.Spam, 0.7f } },
                SemanticDeDuplicationModels = new List<AiModelDto> { new AiModelDto { ModelId = 1 } },
                CourseHarmfulDetectionModels = new List<AiModelDto> { new AiModelDto { ModelId = 2 } }
            };

            //Act
            var result = InvokePrivateMethod<(SemanticDuplicationRequest SemanticReq, CourseHarmfulRequest HarmfulReq)>("CreateModerationRequests", courseId, prep);

            //Assert
            result.SemanticReq.CourseId.Should().Be(courseId);
            result.SemanticReq.SimilarityScoreThreshold.Should().Be(0.8f);
            result.HarmfulReq.SpamScoreThreshold.Should().Be(0.7f);
        }

    }
}
