using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services
{
    /// <summary>
    /// Implementation of content hash service using MD5.
    /// </summary>
    public class ContentHashService : IContentHashService
    {
        private readonly ILogger<ContentHashService> _logger;
        private readonly ICourseExtRepository _courseExtRepository;

        public ContentHashService(ILogger<ContentHashService> logger, ICourseExtRepository courseExtRepository)
        {
            _logger = logger;
            _courseExtRepository = courseExtRepository;
        }

        /// <summary>
        /// Normalize text content for hashing (lowercase, trim, normalize whitespace).
        /// </summary>
        public string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            text = text.ToLowerInvariant().Trim();
            text = Regex.Replace(text, @"\n", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text;
        }

        public async Task<string> ComputeCourseHashAsync(string text)
        {
            var normalized = NormalizeText(text);
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(normalized);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task<string> ComputeFileHashAsync(byte[] fileBytes)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(fileBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task SaveCourseHashesAsync(SaveCourseHashesCommand command)
        {
            var entity = new Domain.Entities.CourseExt {
                CourseId = command.CourseId,
                TitleHash = command.title_hash,
                DescriptionHash = command.description_hash,
                WhatYouWillLearnHash = command.what_you_will_learn_hash,
                RequirementsHash = command.requirements_hash,
                ThumbnailHash = command.thumbnail_hash
            };
            var existing = await _courseExtRepository.GetByIdAsync(command.CourseId);
            if (existing != null) {
                _courseExtRepository.Update(entity);
            }else {
                await _courseExtRepository.AddAsync(entity);
            }
            await _courseExtRepository.SaveChangesAsync();
        }

        

        public async Task<CourseExtDto> GetCourseHashesAsync(int courseId)
        {
            var entity = await _courseExtRepository.GetByIdAsync(courseId);
            if (entity == null) return new();
            return new CourseExtDto
            {
                CourseId = entity.CourseId,
                title_hash = entity.TitleHash ?? "",
                description_hash = entity.DescriptionHash ?? "",
                what_you_will_learn_hash = entity.WhatYouWillLearnHash ?? "",
                requirements_hash = entity.RequirementsHash ?? "",
                thumbnail_hash = entity.ThumbnailHash ?? ""
            };
        }

        public async Task<List<CourseExtDto>> GetAllCourseHashesAsync()
        {
            var entities = await _courseExtRepository.GetAllAsync();
            return entities.Select(entity => new CourseExtDto
            {
                CourseId = entity.CourseId,
                title_hash = entity.TitleHash ?? "",
                description_hash = entity.DescriptionHash ?? "",
                what_you_will_learn_hash = entity.WhatYouWillLearnHash ?? "",
                requirements_hash = entity.RequirementsHash ?? "",
                thumbnail_hash = entity.ThumbnailHash ?? ""
            }).ToList();
        }
    }
}
