using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class AiConfigurationService : IAiConfigurationService
{
    private readonly ISystemConfigRepository _configRepo;

    public AiConfigurationService(ISystemConfigRepository configRepo)
    {
        _configRepo = configRepo;
    }

    public async Task<AiConfigurationDto> GetConfigurationsAsync()
    {
        var dto = new AiConfigurationDto
        {
            SimilarityScoreThreshold = AiModelConst.DefaultSimilarityScoreThreshold,
            SpamConfidenceThreshold = AiModelConst.DefaultSpamScoreThreshold,
            ToxicityConfidenceThreshold = AiModelConst.DefaultToxicScoreThreshold
        };

        var modThresholdStr = await _configRepo.GetValueAsync(SystemConfigKeys.ModerationThreshold);
        if (!string.IsNullOrEmpty(modThresholdStr))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(modThresholdStr);
                if (doc.RootElement.TryGetProperty("spam", out var spamProp) && spamProp.TryGetSingle(out var sVal))
                    dto.SpamConfidenceThreshold = sVal;
                if (doc.RootElement.TryGetProperty("toxic", out var toxicProp) && toxicProp.TryGetSingle(out var tVal))
                    dto.ToxicityConfidenceThreshold = tVal;
                if (doc.RootElement.TryGetProperty("similarity", out var simProp) && simProp.TryGetSingle(out var simVal))
                    dto.SimilarityScoreThreshold = simVal;
            }
            catch { /* Ignore parsing errors and use defaults */ }
        }

        dto.CourseHarmfulTextClassifierPath = await _configRepo.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier);
        dto.CourseTextEmbeddingGeneratorPath = await _configRepo.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator);
        dto.CourseMediaEmbeddingGeneratorPath = await _configRepo.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator);
        dto.ReviewHarmfulTextClassifierPath = await _configRepo.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier);

        return dto;
    }

    public async Task<bool> UpdateThresholdsAsync(UpdateThresholdsRequest req)
    {
        var newThresholds = new
        {
            similarity = req.SimilarityScoreThreshold,
            spam = req.SpamConfidenceThreshold,
            toxic = req.ToxicityConfidenceThreshold
        };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(newThresholds);
        int affected;
        try
        {
            affected = await _configRepo.UpsertConfigAsync(SystemConfigKeys.ModerationThreshold, jsonString, "system config of AI moderation threshold");
        }
        catch (SystemConfigException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        /* zero rows exception removed */
        return true;
    }

    public async Task<bool> UpdateIntegrationAsync(UpdateIntegrationRequest req)
    {
        int affected;
        try
        {
            affected = await _configRepo.UpsertConfigAsync(req.ConfigKey, req.ConfigValue, $"system config of {req.ConfigKey}");
        }
        catch (SystemConfigException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        /* zero rows exception removed */
        return true;
    }
}
