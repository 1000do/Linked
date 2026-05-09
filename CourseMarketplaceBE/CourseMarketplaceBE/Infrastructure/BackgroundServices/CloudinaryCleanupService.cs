using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Infrastructure.BackgroundServices;

public class CloudinaryCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CloudinaryCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run daily

    public CloudinaryCleanupService(
        IServiceProvider serviceProvider,
        ILogger<CloudinaryCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cloudinary Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldTrashMaterialsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Cloudinary cleanup.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupOldTrashMaterialsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();

        _logger.LogInformation("Scanning for old materials in trash...");

        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        
        // Find materials marked as removed more than 30 days ago
        var oldMaterials = context.LearningMaterials
            .Where(m => m.LearningStatus == "removed" && m.UpdatedAt < cutoffDate)
            .ToList();

        if (!oldMaterials.Any())
        {
            _logger.LogInformation("✅ No old materials found to clean up.");
            return;
        }

        foreach (var material in oldMaterials)
        {
            try
            {
                _logger.LogInformation("Permanently deleting material {id}: {title}", material.MaterialId, material.Title);

                // 1. Delete from Cloudinary
                if (!string.IsNullOrEmpty(material.CloudPublicId))
                {
                    var resourceType = material.MaterialMetadata?.FileType ?? "image";
                    await uploadService.DeleteFileByPublicIdAsync($"trash/{material.CloudPublicId}", resourceType);
                }

                // 2. Delete from DB
                context.LearningMaterials.Remove(material);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup material {id}", material.MaterialId);
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Cleanup completed. Deleted {count} materials.", oldMaterials.Count);
    }
}
