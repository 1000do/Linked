using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingRepository _textEmbeddingRepository;
    private readonly IMediaEmbeddingRepository _mediaEmbeddingRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly IRedisService _redisService;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        ITextEmbeddingRepository textEmbeddingRepository,
        IMediaEmbeddingRepository mediaEmbeddingRepository,
        IMaterialRepository materialRepository,
        IRedisService redisService,
        ILogger<EmbeddingService> logger)
    {
        _textEmbeddingRepository = textEmbeddingRepository;
        _mediaEmbeddingRepository = mediaEmbeddingRepository;
        _materialRepository = materialRepository;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<List<MaterialEmbeddingResponse>> GetAllMaterialEmbeddingsAsync()
    {
        var textList = await _textEmbeddingRepository.GetAllAsync();
        var mediaList = await _mediaEmbeddingRepository.GetAllAsync();

        var result = new List<MaterialEmbeddingResponse>();

        result.AddRange(textList.Select(e => new MaterialEmbeddingResponse
        {
            EmbeddingId = e.TextEmbeddingId,
            MaterialId = e.MaterialId,
            Embedding = e.Embedding,
            EmbeddingType = "text"
        }));

        result.AddRange(mediaList.Select(e => new MaterialEmbeddingResponse
        {
            EmbeddingId = e.MediaEmbeddingId,
            MaterialId = e.MaterialId,
            Embedding = e.Embedding,
            EmbeddingType = "media"
        }));

        return result;
    }

    public async Task<int> SaveMaterialEmbeddingsAsync(int materialId, List<float> embedding, string embeddingType)
    {
        _logger.LogInformation("Saving new material embeddings for material {matId} with embedding type as {type}", materialId, embeddingType);
        int rows = 0;
        if (string.Equals(embeddingType, "media", StringComparison.OrdinalIgnoreCase))
        {
            rows = await SaveMediaEmbeddingAsync(materialId, embedding);
        }
        else
        {
            rows = await SaveTextEmbeddingAsync(materialId, embedding);
        }

        if (rows > 0)
        {
            _logger.LogInformation("Saved embeddings for material {matId} with embedding type as {type}", materialId, embeddingType);
        }
        else
        {
            _logger.LogError("Failed to save embeddings for material {matId} with embedding type as {type}", materialId, embeddingType);
        }

        await _redisService.RemoveCacheAsync(CacheKeys.MaterialEmbedding.GetKey(materialId));
        await _redisService.RemoveCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey());
        return rows;
    }

    private async Task<int> SaveMediaEmbeddingAsync(int materialId, List<float> embedding)
    {
        _logger.LogInformation("Retrieving existing media embeddings for material {matId}", materialId);
        var existingList = await _mediaEmbeddingRepository.GetByMaterialIdAsync(materialId);
        var existing = existingList.FirstOrDefault();

        if (existing == null)
        {
            _logger.LogInformation("No existing media embeddings found. Inserting new media embeddings for material {matId}", materialId);
            var entity = new MediaEmbedding
            {
                MaterialId = materialId,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow
            };
            await _mediaEmbeddingRepository.AddAsync(entity);
        }
        else
        {
            _logger.LogInformation("Existing media embeddings found. Updating media embeddings for material {matId}", materialId);
            existing.Embedding = embedding;
            _mediaEmbeddingRepository.Update(existing);
        }

        return await SaveMediaEmbeddingChangesAsync();
    }

    private async Task<int> SaveMediaEmbeddingChangesAsync()
    {
        try
        {
            int rowsAffected = await _mediaEmbeddingRepository.SaveChangesAsync();
            /* zero rows exception removed */
            return rowsAffected;
        }
        catch (MediaEmbeddingException ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    private async Task<int> SaveTextEmbeddingAsync(int materialId, List<float> embedding)
    {
        _logger.LogInformation("Retrieving existing text embeddings for material {matId}", materialId);
        var existingList = await _textEmbeddingRepository.GetByMaterialIdAsync(materialId);
        var existing = existingList.FirstOrDefault();

        if (existing == null)
        {
            _logger.LogInformation("No existing text embeddings found. Inserting new text embeddings for material {matId}", materialId);
            var entity = new TextEmbedding
            {
                MaterialId = materialId,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow
            };
            await _textEmbeddingRepository.AddAsync(entity);
        }
        else
        {
            _logger.LogInformation("Existing text embeddings found. Updating text embeddings for material {matId}", materialId);
            existing.Embedding = embedding;
            _textEmbeddingRepository.Update(existing);
        }

        return await SaveTextEmbeddingChangesAsync();
    }

    private async Task<int> SaveTextEmbeddingChangesAsync()
    {
        try
        {
            int rowsAffected = await _textEmbeddingRepository.SaveChangesAsync();
            /* zero rows exception removed */
            return rowsAffected;
        }
        catch (TextEmbeddingException ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    public async Task<int> PersistPendingMaterialEmbeddingsAsync(int courseId, HashSet<int> excludedMaterialIds)
    {
        var allMaterials = await _materialRepository.GetByCourseIdAsync(courseId);
        if (allMaterials == null || !allMaterials.Any())
            return 0;

        int processed = 0;
        foreach (var material in allMaterials)
        {
            await ProcessPendingMaterialEmbeddingAsync(material.MaterialId, shouldSave: !excludedMaterialIds.Contains(material.MaterialId));
            processed++;
        }
        return processed;
    }

    public async Task<bool> PrepareMaterialEmbeddingsAsync()
    {
        var isInitialized = await _redisService.GetCacheAsync<bool>(CacheKeys.MaterialEmbeddingInitialized.GetKey());
        if (isInitialized) return false;

        var allEmbeddings = await GetAllMaterialEmbeddingsAsync();
        foreach (var e in allEmbeddings)
        {
            string cacheKey = CacheKeys.MaterialEmbedding.GetKey(e.MaterialId.GetValueOrDefault());
            var cached = await _redisService.GetCacheAsync<MaterialEmbeddingResponse>(cacheKey);
            if (cached == null)
            {
                await _redisService.SetCacheAsync(cacheKey, e, CacheTtl.Medium.GetTtl());
            }
        }

        await _redisService.SetCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey(), true, CacheTtl.Medium.GetTtl());
        return true;
    }



    private async Task ProcessPendingMaterialEmbeddingAsync(int materialId, bool shouldSave)
    {
        string cacheKey = CacheKeys.PendingMaterialEmbedding.GetKey(materialId);

        try
        {
            if (!shouldSave) return;

            var cachedResponse = await _redisService.GetCacheAsync<MaterialEmbeddingResponse>(cacheKey);
            if (cachedResponse == null || cachedResponse.Embedding == null || !cachedResponse.Embedding.Any())
                return;

            string embeddingType = GetEmbeddingType(cachedResponse);

            await SaveMaterialEmbeddingsAsync(
                materialId,
                cachedResponse.Embedding,
                embeddingType);
        }
        finally
        {
            await _redisService.RemoveCacheAsync(cacheKey);
        }
    }



    private string GetEmbeddingType(MaterialEmbeddingResponse cachedResponse)
    {
        string embeddingType = cachedResponse.EmbeddingType ?? "";
        if (embeddingType != "text" && embeddingType != "media")
        {
            embeddingType = cachedResponse.Embedding.Count == AiModelConst.MediaEmbeddingDim ? "media" : "text";
        }
        return embeddingType;
    }


    // public async Task PersistMaterialEmbeddingsAsync(int courseId)
    // {
    //     var allMaterials = await _materialRepository.GetByCourseIdAsync(courseId);
    //     if (allMaterials == null || !allMaterials.Any())
    //     {
    //         _logger.LogWarning("No learning materials found for courseId {id}. Skipping embedding persistence.", courseId);
    //         return;
    //     }

    //     var materialIds = allMaterials.Select(m => m.MaterialId).ToList();
    //     _logger.LogInformation("Persisting embeddings for {n} materials.", materialIds.Count);

    //     foreach (var matId in materialIds)
    //     {
    //         await TryPersistSingleMaterialEmbeddingAsync(matId);
    //     }
    // }

    // private async Task TryPersistSingleMaterialEmbeddingAsync(int matId)
    // {
    //     string cacheKey = CacheKeys.MaterialEmbedding.GetKey(matId);
    //     _logger.LogInformation("Retrieving MaterialEmbeddingResponse from cache key {cacheKey}", cacheKey);
    //     var cachedResponse = await _redisService.GetCacheAsync<MaterialEmbeddingResponse>(cacheKey);

    //     if (cachedResponse == null)
    //     {
    //         _logger.LogWarning("MaterialEmbeddingResponse for material {matId} is not found in cache", matId);
    //         return;
    //     }

    //     if (cachedResponse.Embedding == null)
    //     {
    //         _logger.LogWarning("Embedding is not provided for material {matId}", matId);
    //         return;
    //     }

    //     if (!cachedResponse.Embedding.Any())
    //     {
    //         _logger.LogWarning("Embedding contains no coordinate values for material {matId}", matId);
    //         return;
    //     }

    //     _logger.LogInformation("Successfully retrieved MaterialEmbeddingResponse from cache key {cacheKey}\n{embedding}", cacheKey, JsonSerializer.Serialize(cachedResponse));

    //     await SaveMaterialEmbeddingsAsync(
    //         matId,
    //         cachedResponse.Embedding, 
    //         GetEmbeddingType(cachedResponse));
    // }
}
