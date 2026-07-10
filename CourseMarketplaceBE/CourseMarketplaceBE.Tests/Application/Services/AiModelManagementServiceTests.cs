using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class AiModelManagementServiceTests
    {
        private readonly IAiModelRepository _aiModelRepoMock;
        private readonly IMapper _mapperMock;
        private readonly IRedisService _redisServiceMock;
        private readonly AiModelManagementService _sut;

        public AiModelManagementServiceTests()
        {
            _aiModelRepoMock = Substitute.For<IAiModelRepository>();
            _mapperMock = Substitute.For<IMapper>();
            _redisServiceMock = Substitute.For<IRedisService>();

            _sut = new AiModelManagementService(_aiModelRepoMock, _mapperMock, _redisServiceMock);
        }

        // ── GetAllModelsAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllModelsAsync_ModelsExist_ReturnsMappedModels()
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
        public async Task GetAllModelsAsync_EmptyModelsList_ThrowsKeyNotFoundException()
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

        [Fact]
        public async Task GetAllModelsAsync_NullModelsList_ThrowsKeyNotFoundException()
        {
            //Arrange 1
            List<AiModel>? nullModels = null;

            //Arrange 2
            _aiModelRepoMock.GetAllAdminAsync().Returns(nullModels);

            //Act
            Func<Task> act = async () => await _sut.GetAllModelsAsync();

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No AI models found.");
            
            await _aiModelRepoMock.Received(1).GetAllAdminAsync();
            _mapperMock.DidNotReceive().Map<List<AiModelAdminDto>>(Arg.Any<List<AiModel>>());
        }

        // ── GetPagedModelsAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetPagedModelsAsync_ValidRequest_ReturnsPagedResult()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var models = new List<AiModel> { new AiModel { ModelId = 1 } };
            int totalCount = 10;
            var dtos = new List<AiModelAdminDto> { new AiModelAdminDto { ModelId = 1 } };

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(page, pageSize).Returns((models, totalCount));
            _mapperMock.Map<List<AiModelAdminDto>>(models).Returns(dtos);

            //Act
            var result = await _sut.GetPagedModelsAsync(req);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);
            
            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<AiModelAdminDto>>(models);
        }

        [Fact]
        public async Task GetPagedModelsAsync_EmptyItems_ThrowsKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var emptyModels = new List<AiModel>();

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyModels, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetPagedModelsAsync(req);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No AI models found.");
            
            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<AiModelAdminDto>>(Arg.Any<List<AiModel>>());
        }

        [Fact]
        public async Task GetPagedModelsAsync_NullItems_ThrowsKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            List<AiModel>? nullModels = null;

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(page, pageSize).Returns((nullModels, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetPagedModelsAsync(req);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No AI models found.");
            
            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<AiModelAdminDto>>(Arg.Any<List<AiModel>>());
        }

        [Fact]
        public async Task GetPagedModelsAsync_PageLessThanZero_DefaultsPageToOne()
        {
            //Arrange 1
            var req = new PagedRequestDto { Page = 0, PageSize = 10 };
            var models = new List<AiModel> { new AiModel { ModelId = 1 } };
            var dtos = new List<AiModelAdminDto> { new AiModelAdminDto { ModelId = 1 } };

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(1, 10).Returns((models, 1));
            _mapperMock.Map<List<AiModelAdminDto>>(models).Returns(dtos);

            //Act
            var result = await _sut.GetPagedModelsAsync(req);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.Page.Should().Be(1);

            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(1, 10);
        }

        [Fact]
        public async Task GetPagedModelsAsync_PageSizeLessThanZero_DefaultsPageSizeToTen()
        {
            //Arrange 1
            var req = new PagedRequestDto { Page = 1, PageSize = 0 };
            var models = new List<AiModel> { new AiModel { ModelId = 1 } };
            var dtos = new List<AiModelAdminDto> { new AiModelAdminDto { ModelId = 1 } };

            //Arrange 2
            _aiModelRepoMock.GetPagedAdminAsync(1, 10).Returns((models, 1));
            _mapperMock.Map<List<AiModelAdminDto>>(models).Returns(dtos);

            //Act
            var result = await _sut.GetPagedModelsAsync(req);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.PageSize.Should().Be(10);

            await _aiModelRepoMock.Received(1).GetPagedAdminAsync(1, 10);
        }

        // ── GetModelByIdAsync ────────────────────────────────────────────────────

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

        // ── AddModelAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task AddModelAsync_ShouldReturnAddedModel()
        {
            //Arrange 1
            var req = new CreateAiModelRequest { ModelName = "Test", ModelProvider = "Provider", ModelType = "Type", ProcessType = "Process" };
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
                                m.ModelName == req.ModelName && m.ModelProvider == req.ModelProvider));
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
            _mapperMock.Received(1)
                        .Map<AiModelAdminDto>(addedModel);
        }



        [Fact]
        public async Task AddModelAsync_WhenAiModelException_ShouldThrowBadRequestException()
        {
            //Arrange 1
            var req = new CreateAiModelRequest { ModelName = "Test" };
            var addedModel = new AiModel {ModelId = 1, ModelName = req.ModelName};

            //Arrange 2
            _aiModelRepoMock.Add(Arg.Any<AiModel>()).Returns(addedModel);
            _aiModelRepoMock.When(x => x.SaveChangesAsync()).Throw(new AiModelException("DB Error"));

            //Act
            Func<Task> act = async () => await _sut.AddModelAsync(req);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");

            _aiModelRepoMock.Received(1).Add(Arg.Is<AiModel>(m => m.ModelName == req.ModelName));
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        // ── UpdateModelAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateModelAsync_WhenExists_ShouldReturnUpdatedModel()
        {   
            //Arrange 1
            var mockId = 1;
            var req = new UpdateAiModelRequest { ModelProvider = "New", ModelVersion = "2.0" };
            var existingModel = new AiModel { ModelId = mockId, ModelProvider = "Old" };
            var updatedModel = new AiModel{ModelId = existingModel.ModelId, ModelProvider = req.ModelProvider, ModelVersion = "2.0"};
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
                                    m.ModelProvider == req.ModelProvider &&
                                    m.ModelVersion == req.ModelVersion));
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
        public async Task UpdateModelAsync_WhenAiModelException_ShouldThrowBadRequestException()
        {
            //Arrange 1
            var mockId = 1;
            var req = new UpdateAiModelRequest { ModelProvider = "New" };
            var existingModel = new AiModel { ModelId = mockId };
            var updatedModel = new AiModel { ModelId = mockId, ModelProvider = req.ModelProvider };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(existingModel);
            _aiModelRepoMock.Update(Arg.Any<AiModel>()).Returns(updatedModel);
            _aiModelRepoMock.When(x => x.SaveChangesAsync()).Throw(new AiModelException("DB Error"));

            //Act
            Func<Task> act = async () => await _sut.UpdateModelAsync(mockId, req);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            _aiModelRepoMock.Received(1).Update(Arg.Is<AiModel>(m => m.ModelProvider == req.ModelProvider));
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        // ── ToggleModelStatusAsync ───────────────────────────────────────────────

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
            _aiModelRepoMock.Received(1).Update(existingModel);
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
            _aiModelRepoMock.Received(1).Update(existingModel);
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
        public async Task ToggleModelStatusAsync_WhenAiModelException_ShouldThrowBadRequestException()
        {
            //Arrange 1
            var mockId = 1;
            var existingModel = new AiModel { ModelId = mockId, ModelStatus = AiModelConst.Active };

            //Arrange 2
            _aiModelRepoMock.GetByIdAsync(mockId).Returns(existingModel);
            _aiModelRepoMock.When(x => x.SaveChangesAsync()).Throw(new AiModelException("DB Error"));

            //Act
            Func<Task> act = async () => await _sut.ToggleModelStatusAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");

            await _aiModelRepoMock.Received(1).GetByIdAsync(mockId);
            _aiModelRepoMock.Received(1).Update(existingModel);
            await _aiModelRepoMock.Received(1).SaveChangesAsync();
        }

        // ── GetModelIdsByType ────────────────────────────────────────────────────

        [Fact]
        public async Task GetModelIdsByType_ValidType_ReturnsListOfIds()
        {
            //Arrange 1
            string type = "GPT";
            var models = new List<AiModel> 
            { 
                new AiModel { ModelId = 1 },
                new AiModel { ModelId = 2 }
            };
            var expectedIds = new List<int> { 1, 2 };

            //Arrange 2
            _aiModelRepoMock.GetByTypeAsync(type).Returns(models);

            //Act
            var result = await _sut.GetModelIdsByType(type);

            //Assert
            result.Should().BeEquivalentTo(expectedIds);

            await _aiModelRepoMock.Received(1).GetByTypeAsync(type);
        }

        // ── GetModelsByTypeAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetModelsByTypeAsync_CacheHit_ReturnsCachedModels()
        {
            //Arrange 1
            string modelType = "GPT";
            string cacheKey = CacheKeys.AiModelType.GetKey(modelType);
            var cachedModels = new List<AiModelDto> 
            { 
                new AiModelDto { ModelId = 1, ModelName = "GPT-4" }
            };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<List<AiModelDto>>(cacheKey).Returns(cachedModels);

            //Act
            var result = await _sut.GetModelsByTypeAsync(modelType);

            //Assert
            result.Should().BeEquivalentTo(cachedModels);

            await _redisServiceMock.Received(1).GetCacheAsync<List<AiModelDto>>(cacheKey);
            await _aiModelRepoMock.DidNotReceive().GetModelsByTypeAsync(Arg.Any<string>());
            await _redisServiceMock.DidNotReceive().SetCacheAsync(Arg.Any<string>(), Arg.Any<List<AiModelDto>>(), Arg.Any<TimeSpan>());
        }

        [Fact]
        public async Task GetModelsByTypeAsync_CacheMiss_FetchesFromRepoSetsCacheAndReturnsModels()
        {
            //Arrange 1
            string modelType = "GPT";
            string cacheKey = CacheKeys.AiModelType.GetKey(modelType);
            var dbModels = new List<AiModel> 
            { 
                new AiModel 
                { 
                    ModelId = 1, 
                    ModelName = "GPT-4",
                    ModelType = "Language",
                    ModelProvider = "OpenAI",
                    ModelVersion = "4.0",
                    ModelStatus = AiModelConst.Active,
                    Description = "Desc",
                    ModelPath = "path",
                    ProcessType = "Text"
                }
            };
            var expectedDtos = new List<AiModelDto> 
            { 
                new AiModelDto 
                { 
                    ModelId = 1, 
                    ModelName = "GPT-4",
                    ModelType = "Language",
                    ModelProvider = "OpenAI",
                    ModelVersion = "4.0",
                    ModelStatus = AiModelConst.Active,
                    Description = "Desc",
                    ModelPath = "path",
                    ProcessType = "Text"
                }
            };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<List<AiModelDto>>(cacheKey).Returns((List<AiModelDto>?)null);
            _aiModelRepoMock.GetModelsByTypeAsync(modelType).Returns(dbModels);

            //Act
            var result = await _sut.GetModelsByTypeAsync(modelType);

            //Assert
            result.Should().BeEquivalentTo(expectedDtos);

            await _redisServiceMock.Received(1).GetCacheAsync<List<AiModelDto>>(cacheKey);
            await _aiModelRepoMock.Received(1).GetModelsByTypeAsync(modelType);
            await _redisServiceMock.Received(1).SetCacheAsync(cacheKey, Arg.Is<List<AiModelDto>>(list => list.Count == 1 && list[0].ModelId == 1), CacheTtl.Medium.GetTtl());
        }
    }
}
