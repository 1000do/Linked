using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

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
            var entity = new Domain.Entities.CourseExt
            {
                CourseId = command.CourseId,
                TitleHash = command.TitleHash,
                DescriptionHash = command.DescriptionHash,
                WhatYouWillLearnHash = command.WhatYouWillLearnHash,
                RequirementsHash = command.RequirementsHash,
                ThumbnailHash = command.ThumbnailHash
            };
            var existing = await _courseExtRepository.GetByIdAsync(command.CourseId);
            if (existing != null)
            {
                _courseExtRepository.Update(entity);
            }
            else
            {
                await _courseExtRepository.AddAsync(entity);
            }
            int numberOfRowsAffected = await _courseExtRepository.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
                throw new InvalidOperationException("Failed to save changes");
        }



        public async Task<CourseExtDto> GetCourseHashesAsync(int courseId)
        {
            var entity = await _courseExtRepository.GetByIdAsync(courseId);
            if (entity == null) return new();
            return new CourseExtDto
            {
                CourseId = entity.CourseId,
                TitleHash = entity.TitleHash ?? "",
                DescriptionHash = entity.DescriptionHash ?? "",
                WhatYouWillLearnHash = entity.WhatYouWillLearnHash ?? "",
                RequirementsHash = entity.RequirementsHash ?? "",
                ThumbnailHash = entity.ThumbnailHash ?? ""
            };
        }

        public async Task<List<CourseExtDto>> GetAllCourseHashesAsync()
        {
            var entities = await _courseExtRepository.GetAllAsync();
            return entities.Select(entity => new CourseExtDto
            {
                CourseId = entity.CourseId,
                TitleHash = entity.TitleHash ?? "",
                DescriptionHash = entity.DescriptionHash ?? "",
                WhatYouWillLearnHash = entity.WhatYouWillLearnHash ?? "",
                RequirementsHash = entity.RequirementsHash ?? "",
                ThumbnailHash = entity.ThumbnailHash ?? ""
            }).ToList();
        }
    }
}
