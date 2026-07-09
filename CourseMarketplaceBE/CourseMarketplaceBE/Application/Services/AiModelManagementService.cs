using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using AutoMapper;
using System.Linq;

namespace CourseMarketplaceBE.Application.Services;

public class AiModelManagementService : IAiModelManagementService
{
    private readonly IAiModelRepository _aiModelRepo;
    private readonly IMapper _mapper;
    private readonly IRedisService _redisService;

    public AiModelManagementService(IAiModelRepository aiModelRepo, IMapper mapper, IRedisService redisService)
    {
        _aiModelRepo = aiModelRepo;
        _mapper = mapper;
        _redisService = redisService;
    }

    public async Task<List<AiModelAdminDto>> GetAllModelsAsync()
    {
        var models = await _aiModelRepo.GetAllAdminAsync();
        if (models == null || models.Count == 0) throw new KeyNotFoundException("No AI models found.");
        return _mapper.Map<List<AiModelAdminDto>>(models);
    }

    public async Task<PagedResult<AiModelAdminDto>> GetPagedModelsAsync(PagedRequestDto req)
    {
        if (req.Page <= 0) req.Page = 1;
        if (req.PageSize <= 0) req.PageSize = 10;

        var (items, totalCount) = await _aiModelRepo.GetPagedAdminAsync(req.Page, req.PageSize);
        if (items == null || items.Count == 0) throw new KeyNotFoundException("No AI models found.");
        var dtos = _mapper.Map<List<AiModelAdminDto>>(items);
        return new PagedResult<AiModelAdminDto>(dtos, totalCount, req.Page, req.PageSize);
    }

    public async Task<AiModelAdminDto> GetModelByIdAsync(int id)
    {
        var m = await _aiModelRepo.GetByIdAsync(id);
        if (m == null) throw new KeyNotFoundException("AI Model not found.");

        return _mapper.Map<AiModelAdminDto>(m);
    }

    public async Task<AiModelAdminDto> AddModelAsync(CreateAiModelRequest req)
    {
        var model = new AiModel
        {
            ModelName = req.ModelName,
            ModelVersion = req.ModelVersion,
            ModelPath = req.ModelPath,
            ModelProvider = req.ModelProvider,
            ModelType = req.ModelType,
            ProcessType = req.ProcessType,
            Description = req.Description,
            ModelStatus = req.ModelStatus,
            ModelCreatedAt = DateTime.UtcNow,
            ModelUpdatedAt = DateTime.UtcNow
        };

        var addedModel = _aiModelRepo.Add(model);
        int affected;
        try
        {
            affected = await _aiModelRepo.SaveChangesAsync();
        }
        catch (AiModelException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        /* zero rows exception removed */
        Console.WriteLine($"New Model Id {addedModel.ModelId}");
        return _mapper.Map<AiModelAdminDto>(addedModel);
    }

    public async Task<AiModelAdminDto> UpdateModelAsync(int id, UpdateAiModelRequest req)
    {
        var model = await _aiModelRepo.GetByIdAsync(id);
        if (model == null) throw new KeyNotFoundException("AI Model not found.");

        model.ModelProvider = req.ModelProvider;
        model.ModelVersion = req.ModelVersion;
        model.ModelPath = req.ModelPath;
        model.Description = req.Description;
        model.ModelUpdatedAt = DateTime.UtcNow;

        var updatedModel = _aiModelRepo.Update(model);
        int affected;
        try
        {
            affected = await _aiModelRepo.SaveChangesAsync();
        }
        catch (AiModelException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        /* zero rows exception removed */

        return _mapper.Map<AiModelAdminDto>(updatedModel);
    }

    public async Task<bool> ToggleModelStatusAsync(int id)
    {
        var model = await _aiModelRepo.GetByIdAsync(id);
        if (model == null) throw new KeyNotFoundException("AI Model not found.");

        model.ModelStatus = model.ModelStatus == AiModelConst.Active ? AiModelConst.Inactive : AiModelConst.Active;
        model.ModelUpdatedAt = DateTime.UtcNow;

        _aiModelRepo.Update(model);
        int affected;
        try
        {
            affected = await _aiModelRepo.SaveChangesAsync();
        }
        catch (AiModelException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        /* zero rows exception removed */
        return true;
    }

    public async Task<List<int>> GetModelIdsByType(string type)
    {
        var models = await _aiModelRepo.GetByTypeAsync(type);
        return models.Select(m => m.ModelId).ToList();
    }

    public async Task<List<AiModelDto>> GetModelsByTypeAsync(string modelType)
    {
        string cacheKey = CacheKeys.AiModelType.GetKey(modelType);
        var cached = await _redisService.GetCacheAsync<List<AiModelDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var dbModels = await _aiModelRepo.GetModelsByTypeAsync(modelType);
        var result = dbModels.Select(m => new AiModelDto
        {
            ModelId = m.ModelId,
            ModelName = m.ModelName,
            ModelType = m.ModelType,
            ModelProvider = m.ModelProvider,
            ModelVersion = m.ModelVersion,
            ModelStatus = m.ModelStatus,
            Description = m.Description,
            ModelPath = m.ModelPath,
            ProcessType = m.ProcessType
        }).ToList();

        await _redisService.SetCacheAsync(cacheKey, result, CacheTtl.Medium.GetTtl());
        return result;
    }
}
