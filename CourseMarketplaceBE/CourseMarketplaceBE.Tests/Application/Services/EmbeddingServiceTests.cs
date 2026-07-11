using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class EmbeddingServiceTests
{
    private readonly ITextEmbeddingRepository _textEmbeddingRepositoryMock;
    private readonly IMediaEmbeddingRepository _mediaEmbeddingRepositoryMock;
    private readonly IMaterialRepository _materialRepositoryMock;
    private readonly IRedisService _redisServiceMock;
    private readonly ILogger<EmbeddingService> _loggerMock;
    private readonly EmbeddingService _sut;

    public EmbeddingServiceTests()
    {
        _textEmbeddingRepositoryMock = Substitute.For<ITextEmbeddingRepository>();
        _mediaEmbeddingRepositoryMock = Substitute.For<IMediaEmbeddingRepository>();
        _materialRepositoryMock = Substitute.For<IMaterialRepository>();
        _redisServiceMock = Substitute.For<IRedisService>();
        _loggerMock = Substitute.For<ILogger<EmbeddingService>>();

        _sut = new EmbeddingService(
            _textEmbeddingRepositoryMock,
            _mediaEmbeddingRepositoryMock,
            _materialRepositoryMock,
            _redisServiceMock,
            _loggerMock
        );
    }

    [Fact]
    public async Task GetAllMaterialEmbeddingsAsync_HasTextAndMedia_ReturnsCombinedList()
    {
        //Arrange 1
        var textItems = new List<TextEmbedding>
        {
            new TextEmbedding { TextEmbeddingId = 1, MaterialId = 10, Embedding = new List<float> { 0.1f, 0.2f } }
        };
        var mediaItems = new List<MediaEmbedding>
        {
            new MediaEmbedding { MediaEmbeddingId = 2, MaterialId = 20, Embedding = new List<float> { 0.3f, 0.4f } }
        };
        var expectedCount = 2;

        //Arrange 2
        _textEmbeddingRepositoryMock.GetAllAsync().Returns(textItems);
        _mediaEmbeddingRepositoryMock.GetAllAsync().Returns(mediaItems);

        //Act
        var result = await _sut.GetAllMaterialEmbeddingsAsync();

        //Assert
        result.Should().HaveCount(expectedCount);
        result.Should().ContainSingle(x => x.EmbeddingId == 1 && x.MaterialId == 10 && x.EmbeddingType == "text");
        result.Should().ContainSingle(x => x.EmbeddingId == 2 && x.MaterialId == 20 && x.EmbeddingType == "media");
        await _textEmbeddingRepositoryMock.Received(1).GetAllAsync();
        await _mediaEmbeddingRepositoryMock.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_TypeIsMediaAndNoExisting_AddsMediaEmbeddingAndReturnsRows()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "media";
        var existingList = new List<MediaEmbedding>();
        int rows = 1;

        //Arrange 2
        _mediaEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _mediaEmbeddingRepositoryMock.SaveChangesAsync().Returns(rows);

        //Act
        var result = await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        result.Should().Be(rows);
        await _mediaEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _mediaEmbeddingRepositoryMock.Received(1).AddAsync(Arg.Is<MediaEmbedding>(x => x.MaterialId == materialId && x.Embedding == embedding));
        _mediaEmbeddingRepositoryMock.DidNotReceive().Update(Arg.Any<MediaEmbedding>());
        await _mediaEmbeddingRepositoryMock.Received(1).SaveChangesAsync();
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_TypeIsMediaAndExisting_UpdatesMediaEmbeddingAndReturnsRows()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "media";
        var existing = new MediaEmbedding { MediaEmbeddingId = 1, MaterialId = materialId, Embedding = new List<float>() };
        var existingList = new List<MediaEmbedding> { existing };
        int rows = 1;

        //Arrange 2
        _mediaEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _mediaEmbeddingRepositoryMock.SaveChangesAsync().Returns(rows);

        //Act
        var result = await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        result.Should().Be(rows);
        await _mediaEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _mediaEmbeddingRepositoryMock.DidNotReceive().AddAsync(Arg.Any<MediaEmbedding>());
        _mediaEmbeddingRepositoryMock.Received(1).Update(existing);
        existing.Embedding.Should().BeEquivalentTo(embedding);
        await _mediaEmbeddingRepositoryMock.Received(1).SaveChangesAsync();
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_TypeIsTextAndNoExisting_AddsTextEmbeddingAndReturnsRows()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "text";
        var existingList = new List<TextEmbedding>();
        int rows = 1;

        //Arrange 2
        _textEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _textEmbeddingRepositoryMock.SaveChangesAsync().Returns(rows);

        //Act
        var result = await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        result.Should().Be(rows);
        await _textEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _textEmbeddingRepositoryMock.Received(1).AddAsync(Arg.Is<TextEmbedding>(x => x.MaterialId == materialId && x.Embedding == embedding));
        _textEmbeddingRepositoryMock.DidNotReceive().Update(Arg.Any<TextEmbedding>());
        await _textEmbeddingRepositoryMock.Received(1).SaveChangesAsync();
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_TypeIsTextAndExisting_UpdatesTextEmbeddingAndReturnsRows()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "text";
        var existing = new TextEmbedding { TextEmbeddingId = 1, MaterialId = materialId, Embedding = new List<float>() };
        var existingList = new List<TextEmbedding> { existing };
        int rows = 1;

        //Arrange 2
        _textEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _textEmbeddingRepositoryMock.SaveChangesAsync().Returns(rows);

        //Act
        var result = await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        result.Should().Be(rows);
        await _textEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _textEmbeddingRepositoryMock.DidNotReceive().AddAsync(Arg.Any<TextEmbedding>());
        _textEmbeddingRepositoryMock.Received(1).Update(existing);
        existing.Embedding.Should().BeEquivalentTo(embedding);
        await _textEmbeddingRepositoryMock.Received(1).SaveChangesAsync();
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_TypeIsMediaAndFailsToSave_ReturnsZeroAndLogsError()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "media";
        var existingList = new List<MediaEmbedding>();
        int rows = 0;

        //Arrange 2
        _mediaEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _mediaEmbeddingRepositoryMock.SaveChangesAsync().Returns(rows);

        //Act
        var result = await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        result.Should().Be(0);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_TypeIsTextAndFailsToSave_ReturnsZeroAndLogsError()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "text";
        var existingList = new List<TextEmbedding>();
        int rows = 0;

        //Arrange 2
        _textEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _textEmbeddingRepositoryMock.SaveChangesAsync().Returns(rows);

        //Act
        var result = await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        result.Should().Be(0);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_SaveMediaEmbeddingThrowsMediaEmbeddingException_ThrowsBadRequestException()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "media";
        var existingList = new List<MediaEmbedding>();
        var exceptionMsg = "Media error";

        //Arrange 2
        _mediaEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _mediaEmbeddingRepositoryMock.SaveChangesAsync().Throws(new MediaEmbeddingException(exceptionMsg));

        //Act
        Func<Task> act = async () => await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.WithMessage(exceptionMsg);
    }

    [Fact]
    public async Task SaveMaterialEmbeddingsAsync_SaveTextEmbeddingThrowsTextEmbeddingException_ThrowsBadRequestException()
    {
        //Arrange 1
        int materialId = 10;
        var embedding = new List<float> { 0.1f, 0.2f };
        string type = "text";
        var existingList = new List<TextEmbedding>();
        var exceptionMsg = "Text error";

        //Arrange 2
        _textEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(existingList);
        _textEmbeddingRepositoryMock.SaveChangesAsync().Throws(new TextEmbeddingException(exceptionMsg));

        //Act
        Func<Task> act = async () => await _sut.SaveMaterialEmbeddingsAsync(materialId, embedding, type);

        //Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.WithMessage(exceptionMsg);
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_MaterialsNull_ReturnsZero()
    {
        //Arrange 1
        int courseId = 5;
        var excluded = new HashSet<int>();

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns((List<LearningMaterial>)null!);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(0);
        await _materialRepositoryMock.Received(1).GetByCourseIdAsync(courseId);
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_MaterialsEmpty_ReturnsZero()
    {
        //Arrange 1
        int courseId = 5;
        var excluded = new HashSet<int>();
        var emptyList = new List<LearningMaterial>();

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(emptyList);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(0);
        await _materialRepositoryMock.Received(1).GetByCourseIdAsync(courseId);
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_MaterialExcluded_SkipsProcessing()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int> { materialId };
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1); // Processed count is incremented
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
        await _redisServiceMock.DidNotReceive().GetCacheAsync<MaterialEmbeddingResponse>(Arg.Any<string>());
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheIsNull_SkipsSave()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns((MaterialEmbeddingResponse)null!);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
        await _mediaEmbeddingRepositoryMock.DidNotReceive().GetByMaterialIdAsync(Arg.Any<int>());
        await _textEmbeddingRepositoryMock.DidNotReceive().GetByMaterialIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheEmbeddingIsNull_SkipsSave()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };
        var cached = new MaterialEmbeddingResponse { Embedding = null! };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns(cached);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
        await _mediaEmbeddingRepositoryMock.DidNotReceive().GetByMaterialIdAsync(Arg.Any<int>());
        await _textEmbeddingRepositoryMock.DidNotReceive().GetByMaterialIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheEmbeddingIsEmpty_SkipsSave()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };
        var cached = new MaterialEmbeddingResponse { Embedding = new List<float>() };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns(cached);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
        await _mediaEmbeddingRepositoryMock.DidNotReceive().GetByMaterialIdAsync(Arg.Any<int>());
        await _textEmbeddingRepositoryMock.DidNotReceive().GetByMaterialIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheHasExplicitTextType_SavesAsText()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };
        var cached = new MaterialEmbeddingResponse { Embedding = new List<float> { 1f }, EmbeddingType = "text" };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns(cached);
        _textEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(new List<TextEmbedding>());
        _textEmbeddingRepositoryMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _textEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheHasExplicitMediaType_SavesAsMedia()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };
        var cached = new MaterialEmbeddingResponse { Embedding = new List<float> { 1f }, EmbeddingType = "media" };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns(cached);
        _mediaEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(new List<MediaEmbedding>());
        _mediaEmbeddingRepositoryMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _mediaEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheHasNoTypeAndCountIsMediaDim_SavesAsMedia()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };
        var embedding = Enumerable.Repeat(0.1f, AiModelConst.MediaEmbeddingDim).ToList();
        var cached = new MaterialEmbeddingResponse { Embedding = embedding, EmbeddingType = null };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns(cached);
        _mediaEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(new List<MediaEmbedding>());
        _mediaEmbeddingRepositoryMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _mediaEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
    }

    [Fact]
    public async Task PersistPendingMaterialEmbeddingsAsync_CacheHasNoTypeAndCountIsNotMediaDim_SavesAsText()
    {
        //Arrange 1
        int courseId = 5;
        int materialId = 10;
        var excluded = new HashSet<int>();
        var materials = new List<LearningMaterial> { new LearningMaterial { MaterialId = materialId } };
        var embedding = Enumerable.Repeat(0.1f, 384).ToList(); // Not MediaEmbeddingDim
        var cached = new MaterialEmbeddingResponse { Embedding = embedding, EmbeddingType = "" };

        //Arrange 2
        _materialRepositoryMock.GetByCourseIdAsync(courseId).Returns(materials);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(CacheKeys.PendingMaterialEmbedding.GetKey(materialId))
            .Returns(cached);
        _textEmbeddingRepositoryMock.GetByMaterialIdAsync(materialId).Returns(new List<TextEmbedding>());
        _textEmbeddingRepositoryMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.PersistPendingMaterialEmbeddingsAsync(courseId, excluded);

        //Assert
        result.Should().Be(1);
        await _textEmbeddingRepositoryMock.Received(1).GetByMaterialIdAsync(materialId);
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.PendingMaterialEmbedding.GetKey(materialId));
    }

    [Fact]
    public async Task PrepareMaterialEmbeddingsAsync_AlreadyInitialized_ReturnsFalse()
    {
        //Arrange 1
        var isInitialized = true;

        //Arrange 2
        _redisServiceMock.GetCacheAsync<bool>(CacheKeys.MaterialEmbeddingInitialized.GetKey()).Returns(isInitialized);

        //Act
        var result = await _sut.PrepareMaterialEmbeddingsAsync();

        //Assert
        result.Should().BeFalse();
        await _textEmbeddingRepositoryMock.DidNotReceive().GetAllAsync();
        await _mediaEmbeddingRepositoryMock.DidNotReceive().GetAllAsync();
    }

    [Fact]
    public async Task PrepareMaterialEmbeddingsAsync_NotInitializedAndCacheMiss_CachesEmbeddingsAndReturnsTrue()
    {
        //Arrange 1
        var textItems = new List<TextEmbedding>
        {
            new TextEmbedding { TextEmbeddingId = 1, MaterialId = 10, Embedding = new List<float> { 0.1f, 0.2f } }
        };

        //Arrange 2
        _redisServiceMock.GetCacheAsync<bool>(CacheKeys.MaterialEmbeddingInitialized.GetKey()).Returns(false);
        _textEmbeddingRepositoryMock.GetAllAsync().Returns(textItems);
        _mediaEmbeddingRepositoryMock.GetAllAsync().Returns(new List<MediaEmbedding>());
        
        string cacheKey = CacheKeys.MaterialEmbedding.GetKey(10);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(cacheKey).Returns((MaterialEmbeddingResponse)null!);

        //Act
        var result = await _sut.PrepareMaterialEmbeddingsAsync();

        //Assert
        result.Should().BeTrue();
        await _redisServiceMock.Received(1).SetCacheAsync(cacheKey, Arg.Is<MaterialEmbeddingResponse>(x => x.MaterialId == 10), CacheTtl.Medium.GetTtl());
        await _redisServiceMock.Received(1).SetCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey(), true, CacheTtl.Medium.GetTtl());
    }

    [Fact]
    public async Task PrepareMaterialEmbeddingsAsync_NotInitializedAndCacheHit_SkipsCachingIndividualItemsAndReturnsTrue()
    {
        //Arrange 1
        var textItems = new List<TextEmbedding>
        {
            new TextEmbedding { TextEmbeddingId = 1, MaterialId = 10, Embedding = new List<float> { 0.1f, 0.2f } }
        };
        var existingCached = new MaterialEmbeddingResponse();

        //Arrange 2
        _redisServiceMock.GetCacheAsync<bool>(CacheKeys.MaterialEmbeddingInitialized.GetKey()).Returns(false);
        _textEmbeddingRepositoryMock.GetAllAsync().Returns(textItems);
        _mediaEmbeddingRepositoryMock.GetAllAsync().Returns(new List<MediaEmbedding>());
        
        string cacheKey = CacheKeys.MaterialEmbedding.GetKey(10);
        _redisServiceMock.GetCacheAsync<MaterialEmbeddingResponse>(cacheKey).Returns(existingCached);

        //Act
        var result = await _sut.PrepareMaterialEmbeddingsAsync();

        //Assert
        result.Should().BeTrue();
        await _redisServiceMock.DidNotReceive().SetCacheAsync(cacheKey, Arg.Any<MaterialEmbeddingResponse>(), Arg.Any<TimeSpan>());
        await _redisServiceMock.Received(1).SetCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey(), true, CacheTtl.Medium.GetTtl());
    }
}
