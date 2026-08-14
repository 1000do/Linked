using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Infrastructure.Services;

public class MaterialStreamService : IMaterialStreamService
{
    private readonly IMaterialRepository _materialRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MaterialStreamService> _logger;

    public MaterialStreamService(
        IMaterialRepository materialRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<MaterialStreamService> logger)
    {
        _materialRepository = materialRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<MaterialStreamResult> GetMaterialStreamAsync(int materialId, int userId, string? userRole, string? rangeHeader)
    {
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material == null)
            throw new KeyNotFoundException("Material not found.");

        var lesson = await _lessonRepository.GetByIdAsync(material.LessonId ?? 0);
        if (lesson == null || lesson.Course == null)
            throw new KeyNotFoundException("Lesson or course not found.");

        bool isOwner = userId > 0 && lesson.Course.InstructorId == userId;
        bool isStaffOrAdmin = userRole != null && (userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || userRole.Equals("staff", StringComparison.OrdinalIgnoreCase));
        bool isEnrolled = userId > 0 && (isOwner || await _courseRepository.IsEnrolledAsync(userId, lesson.Course.CourseId));

        bool isAllowed = isOwner || isEnrolled || isStaffOrAdmin;

        if (!isAllowed)
        {
            // Check if it is the preview material (first video of the course)
            var courseLessons = await _lessonRepository.GetByCourseIdAsync(lesson.Course.CourseId);
            var firstLessonWithVideo = courseLessons.OrderBy(l => l.LessonId)
                .FirstOrDefault(l => l.LearningMaterials != null && 
                                     l.LearningMaterials.Any(m => m.MaterialMetadata?.FileType?.StartsWith("video", StringComparison.OrdinalIgnoreCase) == true));
                                     
            if (firstLessonWithVideo != null)
            {
                var firstVideo = firstLessonWithVideo.LearningMaterials
                    .OrderBy(m => m.MaterialId)
                    .FirstOrDefault(m => m.MaterialMetadata?.FileType?.StartsWith("video", StringComparison.OrdinalIgnoreCase) == true);
                    
                if (firstVideo != null && firstVideo.MaterialId == materialId)
                {
                    isAllowed = true;
                }
            }
        }

        if (!isAllowed)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this material.");
        }

        if (string.IsNullOrWhiteSpace(material.MaterialUrl))
            throw new InvalidOperationException("Material URL is empty.");

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, material.MaterialUrl);

            // Forward range header if present (important for video seeking)
            if (!string.IsNullOrEmpty(rangeHeader))
            {
                request.Headers.Add("Range", rangeHeader);
            }

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            string filename = material.Title;
            if (!string.IsNullOrEmpty(material.MaterialMetadata?.FileExtension) && !filename.EndsWith("." + material.MaterialMetadata.FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                filename += "." + material.MaterialMetadata.FileExtension;
            }
            if (string.IsNullOrEmpty(material.MaterialMetadata?.FileExtension) && filename.EndsWith(".txt") == false && response.Content.Headers.ContentType?.ToString()?.Contains("text") == true)
            {
                filename += ".txt";
            }

            var result = new MaterialStreamResult
            {
                StatusCode = (int)response.StatusCode,
                Stream = await response.Content.ReadAsStreamAsync(),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream",
                ContentLength = response.Content.Headers.ContentLength,
                FileName = filename
            };

            if (response.Content.Headers.ContentRange != null)
            {
                result.ContentRangeHeader = response.Content.Headers.ContentRange.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching stream for material {Id}", materialId);
            throw new InvalidOperationException("Could not fetch the requested material stream.");
        }
    }
}
