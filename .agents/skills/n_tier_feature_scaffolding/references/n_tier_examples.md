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

// Rule 4: Query DTO Inheritance over Composition for GET requests
public class FilterAiModelsDto : PagedRequestDto 
{
    [StringLength(50)]
    public string? Search { get; set; }
    
    [StringLength(20)]
    public string? Status { get; set; }
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
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<AiModelViewModel>>>(json, _jsonOpts);
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
        
        // Rule 6: Check auth status and centralize redirection
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToAction("Login", "Account");

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
        await SaveModelChangesAsync();
        
        return _mapper.Map<AiModelAdminDto>(addedModel);
    }

    // Rule 2 & 8: SaveChanges Try/Catch extracted to a private helper
    private async Task SaveModelChangesAsync()
    {
        try
        {
            int affected = await _aiModelRepo.SaveChangesAsync();
            if (affected == 0) throw new InvalidOperationException("Failed to add AI Model.");
        }
        catch (AiModelException ex) // Rule 2: Catches Domain exception
        {
            throw new BadRequestException(ex.Message); // Rule 2: Re-throws as App exception
        }
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
                // Rule 1: Throw custom domain exception explicitly naming the entity
                throw new CourseMarketplaceBE.Domain.Exceptions.AiModelException("Database operation failed due to a constraint violation or data issue while saving AI Model.", ex);
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

    private async Task SaveReportChangesAsync()
    {
        try
        {
            int affected = await _reportRepo.SaveChangesAsync();
            if (affected == 0) throw new InvalidOperationException("Failed to resolve course report.");
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

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

## 5. External Integration Service (Rule 0 & Rule 9)
This example demonstrates how external integrations (like HTML manipulation using `HtmlAgilityPack` and `HtmlSanitizer`) belong in the `Infrastructure/Services` layer, and must accept their dependencies via Dependency Injection (Rule 9). The interface is declared in the Application layer.

```csharp
// CourseMarketplaceBE/Application/IServices/IHtmlTextManipulationService.cs
namespace CourseMarketplaceBE.Application.IServices
{
    public interface IHtmlTextManipulationService
    {
        string SanitizeHtml(string rawHtml);
        string ExtractPlainText(string rawHtml);
    }
}
```

```csharp
// CourseMarketplaceBE/Infrastructure/Services/HtmlTextManipulationService.cs
using HtmlAgilityPack;
using Ganss.Xss; // Rule 9: Import first, use later
using CourseMarketplaceBE.Application.IServices;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class HtmlTextManipulationService : IHtmlTextManipulationService
    {
        private readonly IHtmlSanitizer _sanitizer;

        // Rule 9: Dependency Injection First - Accept external library via constructor, do not instantiate it manually here.
        public HtmlTextManipulationService(IHtmlSanitizer sanitizer)
        {
            _sanitizer = sanitizer;
        }

        public string SanitizeHtml(string rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml)) return rawHtml;
            return _sanitizer.Sanitize(rawHtml);
        }

        public string ExtractPlainText(string rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml)) return rawHtml;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);
            var plainText = htmlDoc.DocumentNode.InnerText;
            return plainText;
        }
    }
}

## 6. Modern Frontend MVC Patterns (Rules 6 & 7)
This section demonstrates how to handle AJAX fetching, safe state mutations, and state preservation.

### Isolated Partial View Fetching & Centralized Redirection (Rule 6)
The Backend-For-Frontend handles redirection via `RedirectToAction()`, never raw `401 Unauthorized()`.

```csharp
// CourseMarketplaceFE/Controllers/AdminAiServiceController.cs
[HttpGet]
public async Task<IActionResult> GetModelsPartial(int page = 1)
{
    var model = new AdminAiServicePageViewModel();
    ViewBag.ModelPage = page;
    
    try {
        var resp = await _api.GetAsync($"admin/ai-service/models?page={page}&pageSize=10");
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToAction("Login", "Account"); // Rule 6: Centralized Redirection

        if (resp.IsSuccessStatusCode) {
            // parse response...
        }
    } catch { }

    // Rule 6: Return isolated partial view instead of full page
    return PartialView("_ModelsTablePartial", model);
}
```

### Seamless State Mutations & Fetch Redirect Handling (Rules 6 & 7)
The frontend performs the state mutation via AJAX, and on success, seamlessly triggers an isolated refetch of the updated collection.

```javascript
// CourseMarketplaceFE/Views/AdminAiService/Index.cshtml
async function submitAddAiModel(e) {
    e.preventDefault();
    // ... gather form data ...
    
    const res = await fetch('/AdminAiService/AddModel', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    // Rule 6: Handle redirect dictated by controller natively
    if (res.redirected) {
        window.location.href = res.url;
        return;
    }

    if (res.ok) {
        Swal.fire('Success', 'Model added successfully', 'success');
        closeAddAiModelModal();
        
        // Rule 7: Seamlessly mutate only the target container by explicitly refetching its isolated partial
        fetchPage('models', 1);
    }
}

async function fetchPage(type, page) {
    let url = `/AdminAiService/GetModelsPartial?page=${page}`;
    
    const res = await fetch(url);
    if (res.redirected) { window.location.href = res.url; return; }

    if (res.ok) {
        const html = await res.text();
        document.getElementById(`${type}TableContainer`).innerHTML = html;
        // update URL
    }
}
```

### Safe JavaScript Data Passing (Rule 7)
Dynamic variables must be safely passed into JS using `data-*` attributes to avoid unescaped quote syntax errors.

```html
<!-- CourseMarketplaceFE/Views/AdminAiService/_ModelsTablePartial.cshtml -->
<!-- Rule 7: Use data-* attributes and 'this', NEVER use onclick="myFunc('@item.Desc')" -->
<!-- For complex objects, serialize using CamelCase explicitly -->
<button type="button" onclick="openEditAiModelModal(this)"
        data-id="@item.ModelId"
        data-desc="@item.Description"
        data-model='@System.Text.Json.JsonSerializer.Serialize(item, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase })'
        class="text-slate-400">
    Edit
</button>

<script>
    function openEditAiModelModal(btn) {
        const id = btn.getAttribute('data-id');
        const desc = btn.getAttribute('data-desc');
        const fullModel = JSON.parse(btn.getAttribute('data-model'));
        // ...
    }
</script>
```

### Smart State Preservation on Detail Navigation (Rule 7)
Detail views dynamically preserve query parameters to ensure the user returns to their exact spot in a paginated list/grid.

```html
<!-- CourseMarketplaceFE/Views/AdminAiService/CourseModerationLogDetail.cshtml -->
<div class="flex items-center gap-2">
    @{
        // Rule 7: Preserve state parameters from URL query string
        var q = Context.Request.QueryString.Value;
        var backUrl = Url.Action("Index", "AdminAiService") + (string.IsNullOrEmpty(q) ? "?tab=logs&subtab=course_logs" : q);
    }
    <a href="@backUrl" class="text-xs font-semibold text-slate-500">
        Back to Activity Logs
    </a>
</div>
```

## 7. Background Tasks (Fire-and-Forget) (Rule 2)
This example demonstrates how to safely enqueue background work without exhausting thread pools (`Task.Run`) or violating DI scopes (Service Locator).

### Application Layer: Queue Interface
```csharp
// CourseMarketplaceBE/Application/IServices/IBackgroundTaskQueue.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync<TService>(Func<TService, CancellationToken, ValueTask> workItem) where TService : notnull;
        ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
    }
}
```

### Infrastructure Layer: Queue Implementation
```csharp
// CourseMarketplaceBE/Infrastructure/BackgroundServices/BackgroundTaskQueue.cs
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.Extensions.DependencyInjection;

namespace CourseMarketplaceBE.Infrastructure.BackgroundServices
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> _queue;

        public BackgroundTaskQueue()
        {
            var options = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
            _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, ValueTask>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync<TService>(Func<TService, CancellationToken, ValueTask> workItem) where TService : notnull
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));

            Func<IServiceProvider, CancellationToken, ValueTask> wrapper = (serviceProvider, token) =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                return workItem(service, token);
            };

            await _queue.Writer.WriteAsync(wrapper);
        }

        public async ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
```

### Infrastructure Layer: Hosted Service Processor
```csharp
// CourseMarketplaceBE/Infrastructure/BackgroundServices/QueuedHostedService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Infrastructure.BackgroundServices
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger, IServiceProvider serviceProvider)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is running.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    await workItem(scope.ServiceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing a background work item.");
                }
            }
        }
    }
}
```

### Registration in `Program.cs`
```csharp
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
```

### Usage in Application Service
```csharp
using CourseMarketplaceBE.Application.IServices;

namespace CourseMarketplaceBE.Application.Services;

public class CourseCommandService : ICourseCommandService
{
    private readonly IBackgroundTaskQueue _taskQueue;

    // Inject the Singleton queue into the Scoped service
    public CourseCommandService(IBackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public async Task SubmitCourseAsync(int courseId)
    {
        // ... synchronous validation ...

        // Queue the heavy background processing safely
        // The queue will automatically resolve ICourseAiModerationService in its own scope
        await _taskQueue.QueueBackgroundWorkItemAsync<ICourseAiModerationService>(async (moderationService, token) =>
        {
            await moderationService.ModerateCourseAsync(courseId, token);
        });
    }
}
```
```
