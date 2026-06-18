# N-Tier Feature Scaffolding References

This document contains reference implementations demonstrating the project's strict N-tier architectural patterns and the 10 core rules from `SKILL.md`.

## 1. DTOs & Mappings (Rule 4)
Data Transfer Objects MUST use validation attributes to ensure payloads are validated by the framework before hitting controllers.

```csharp
// CourseMarketplaceBE/Application/DTOs/AdminAiServiceDTOs.cs
public class CreateAiModelRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string ModelName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^\d+\.\d+\.\d+$", ErrorMessage = "Must be semantic versioning")]
    public string ModelVersion { get; set; } = null!;

    // ...
}
```

## 2. Frontend MVC Client Layer (Rule 6)
The Frontend Controller acts as a Backend-For-Frontend (BFF). It uses `Task.WhenAll` for parallel fetching and handles `Unauthorized` by redirecting to Login.

```csharp
// CourseMarketplaceFE/Controllers/AdminAiServiceController.cs
[Authorize(Roles = "admin")]
public class AdminAiServiceController : Controller
{
    public async Task<IActionResult> Index(int modelPage = 1, int coursePage = 1)
    {
        var model = new AdminAiServicePageViewModel(); // Rule 5: ViewModels
        
        try
        {
            // Rule 6: Parallel fetching with Task.WhenAll
            var modelsTask = _api.GetAsync($"admin/ai-service/models?page={modelPage}&pageSize=10");
            var configTask = _api.GetAsync("admin/ai-service/configs");

            await Task.WhenAll(modelsTask, configTask);

            var modelsResp = await modelsTask;
            
            // Rule 6: Handle Unauthorized globally
            if (modelsResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (modelsResp.IsSuccessStatusCode) {
                 var json = await modelsResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<ApiResp<PagedResult<AiModelViewModel>>>(json, _jsonOpts);
                 if (parsed?.Data != null) model.AiModels = parsed.Data.Items;
            }
            // ...
        }
        catch { }

        return View(model);
    }
}
```

## 3. Full Call Hierarchy Example (Add AI Model)
This example traces the `AddModel` action from the UI all the way down to the Database Repository.

### Layer 0: UI & UX (Rule 7)
Uses SweetAlert for confirmation feedback instead of generic JS `alert()`.
```javascript
// CourseMarketplaceFE/Views/AdminAiService/Index.cshtml
const confirmResult = await Swal.fire({
    title: 'Are you sure?',
    text: "This will add the new AI Model.",
    icon: 'warning',
    showCancelButton: true
});

if (confirmResult.isConfirmed) {
    // Perform AJAX request ...
}
```

### Layer 1: Frontend Controller (`CourseMarketplaceFE/Controllers/AdminAiServiceController.cs`)
Returns standard HTTP status codes/JSON for the frontend UI AJAX to consume.

```csharp
    [HttpPost]
    public async Task<IActionResult> AddModel([FromBody] CreateAiModelRequest req)
    {
        // Rule 6 & 4: ModelState Validation Early Exit
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }

        var res = await _api.PostJsonAsync("admin/ai-service/models", req);
        
        // Rule 6: Check auth status
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Unauthorized(new { Message = "Unauthorized access." });

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            return BadRequest(new { Message = "Failed to add AI Model", Details = err });
        }
        return Ok(new { Message = "AI Model added successfully" });
    }
```

### Layer 2: Backend Controller (`CourseMarketplaceBE/Presentation/Controllers/AdminAiServiceController.cs`)
Handles routing and global exception translation into standardized generic API responses (Rule 3).

```csharp
        [HttpPost("models")]
        public async Task<IActionResult> AddModel([FromBody] CreateAiModelRequest req)
        {
            try
            {
                var model = await _aiModelService.AddModelAsync(req);
                // Rule 3: Use standardized ApiResponse wrapper
                return Ok(ApiResponse<object>.SuccessResponse(model, "Model added successfully"));
            }
            catch (InvalidOperationException ex)
            {
                // Rule 3: Catch 400 exceptions
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                // Rule 3: Catch 500 exceptions
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
```

### Layer 3: Application Service (`CourseMarketplaceBE/Application/Services/AiModelManagementService.cs`)
Orchestrates business logic and evaluates repository return values.

```csharp
// Rule 9: Import first, use later. Do not use inline fully qualified namespaces (e.g., CourseMarketplaceBE.Application.DTOs.AiModelAdminDto)
using System;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.Services;

public class AiModelManagementService : IAiModelManagementService
{
    // ...

    public async Task<AiModelAdminDto> AddModelAsync(CreateAiModelRequest req)
    {
        var model = new AiModel
        {
            ModelName = req.ModelName,
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
        catch (AiModelException ex) // Rule 2: Catches Domain exception
        {
            throw new BadRequestException(ex.Message); // Rule 2: Re-throws as App exception
        }
        
        // Rule 2: Early Exit / Guard Clause Check
        if (affected == 0) throw new InvalidOperationException("No changes were saved to the database.");
        
        return _mapper.Map<AiModelAdminDto>(addedModel);
    }
```

### Layer 4: Infrastructure Repository (`CourseMarketplaceBE/Infrastructure/Repositories/AiModelRepository.cs`)
Interacts directly with Entity Framework and throws explicit Domain-level exceptions for DB constraint violations (Rule 1).

```csharp
        public AiModel Add(AiModel model)
        {
            return _context.AiModels.Add(model).Entity; // Rule 1: Return Domain Entity
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                // Rule 1: Wrap in try/catch and return rows affected
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Rule 1: Throw custom domain exception
                throw new CourseMarketplaceBE.Domain.Exceptions.AiModelException("Database operation failed due to a constraint violation or data issue.", ex);
            }
        }
```

## 4. Second Full Call Hierarchy Example (Resolve Course Report) & Helper Extractions
This example traces the `ResolveCourseReport` action. This flow is more complex and additionally demonstrates Rule 8 (Helper Extractions).

### Layer 1: Frontend Controller (`CourseMarketplaceFE/Controllers/AdminModerationController.cs`)
The Backend-For-Frontend receives a POST from the UI but forwards it to the BE as a PATCH.

```csharp
        [HttpPost]
        public async Task<IActionResult> ResolveCourseReport(int reportId, [FromBody] ResolveReportViewModel model)
        {
            var response = await _apiClient.PatchJsonAsync($"/api/admin/moderation/reports/courses/{reportId}", model);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
```

### Layer 2: Backend Controller (`CourseMarketplaceBE/Presentation/Controllers/AdminModerationController.cs`)

```csharp
        [HttpPatch("reports/courses/{reportId}")]
        public async Task<IActionResult> ResolveCourseReport(int reportId, [FromBody] ResolveReportRequest request)
        {
            var resolverId = GetUserId();
            if (resolverId == null) return Unauthorized();

            try
            {
                var result = await _reportService.ResolveCourseReportAsync(reportId, resolverId.Value, request);
                return result
                    ? Ok(ApiResponse<string>.SuccessResponse("Report resolved successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
            }
            // ... Catch KeyNotFoundException, BadRequestException, InvalidOperationException
        }
```

### Layer 3: Application Service (`CourseMarketplaceBE/Application/Services/ReportModerationService.cs`)
Demonstrates Rule 8 (Helper Extractions) and Rule 2 (Early exit for null checks & HasValue).

```csharp
    public async Task<bool> ResolveCourseReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        // Rule 8: Extract fetching and validating into a helper method
        var (report, course) = await GetCourseReportAndEntityAsync(reportId);

        await ValidateReportResolutionAccessAsync(report.CourseReportsStatus!, resolverId);

        // ... state updates ...
        
        _reportRepo.UpdateCourseReport(report);
        
        // Rule 8: SaveChanges Try/Catch extracted to a helper
        await SaveReportChangesAsync(); 

        return true;
    }

    // ── Extracted Helper Methods ────────────────────────────────────────────

    private async Task<(CourseReport, Course)> GetCourseReportAndEntityAsync(int reportId)
    {
        var report = await _reportRepo.GetCourseReportByIdAsync(reportId);
        // Rule 2: Centralized exception throwing and early exit
        if (report == null) throw new KeyNotFoundException("Report not found.");

        // Rule 2: Nullable foreign key validation (.HasValue)
        if (!report.CourseId.HasValue) throw new BadRequestException("Course ID is missing from the report.");
        
        var course = await _courseRepo.GetByIdAsync(report.CourseId.Value);
        if (course == null) throw new KeyNotFoundException("Course not found.");

        return (report, course);
    }
```

### Layer 4: Infrastructure Repository (`CourseMarketplaceBE/Infrastructure/Repositories/ReportRepository.cs`)

```csharp
    public void UpdateCourseReport(CourseReport report)
        => _context.CourseReports.Update(report);

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Rule 1: Map DB Exception to Custom Domain Exception
            throw new ReportException("A database error occurred while saving report changes.", ex);
        }
    }
```
