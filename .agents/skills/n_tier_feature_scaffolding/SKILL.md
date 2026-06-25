---
name: n_tier_feature_scaffolding
description: Guides the agent to implement or refactor a feature using the strict N-tier architecture pattern defined in the project. Enforces separation of concerns, explicit error handling, and authorization rules.
---

# N-Tier Feature Scaffolding

This skill enforces the exact N-tier architectural pattern used in the Course Marketplace project. When asked to create, refactor, or validate a functionality (CRUD, workflow) or a full feature, you must rigorously apply these rules and generate an Implementation Plan before writing code.

**Important**: Consult the `references/n_tier_examples.md` file in this skill's directory for full-stack code examples demonstrating how these rules are applied across the entire call hierarchy.

## The Architecture Pattern Rules

### 0. General Components, Locations & Dependency Chain
- **Frontend (FE)**:
  - `Controllers/`: Contains Frontend MVC Controllers.
  - `Views/`: Contains Razor `.cshtml` files.
  - `Models/`: Contains ViewModels.
- **Backend (BE)**:
  - `Domain/`: Contains Entities, IRepositories, Enums, Constants.
  - `Application/`: Contains IServices, Services (internal business logic), DTOs.
  - `Infrastructure/`: Contains Repositories, Services (external integrations, see `references/n_tier_examples.md` Section 5), Data (AppDbContext), BackgroundServices (includes scheduled tasks).
  - `Presentation/`: Contains Backend API Controllers.
- **Dependency Chain**:
  - `Presentation` (Backend Controllers) use `Application/IServices`.
  - `Application/Services` and `Infrastructure/Services` implement `Application/IServices`.
  - `Services` use `Domain/IRepositories`.
  - `Infrastructure/Repositories` implement `Domain/IRepositories`.
  - `Infrastructure/Repositories` use `AppDbContext`.
- **Call Hierarchy**: `UI -> Frontend Controller -> Backend Controller -> Service -> Repository -> AppDBContext -> DB`
- **Separation of Concerns**:
  - **Repository**: Only queries or commands the DB. No business logic.
  - **Service**: Handles all business rules and logic (null checks, empty checks, numrows checks, etc.) and throws appropriate exceptions to the controller.
  - **Controllers**: Handle HTTP routing and catch exceptions from the service layer. **Do not** include any business logic here.

### 1. Backend: Infrastructure (Repositories)
- **Data Access**: Use Entity Framework Core directly (`_context.Set<T>()`).
- **Return Types**:
  - Always return Domain Entities, **never** DTOs.
  - **List Retrievals**: Return `Task<(List<Entity> Items, int TotalCount)>`.
  - **Write Operations**: 
    - Explicitly call `await _context.SaveChangesAsync()` and return its result (`Task<int>`), representing the number of rows affected. Do not return void.
    - Wrap `SaveChangesAsync()` in `try/catch` blocks. Catch Entity Framework's `DbUpdateException` (from constraint violations or data issues) and re-throw it as a custom Domain Exception (e.g., `AiModelException`, `CourseException`) with a clear error message explicitly naming the entity (e.g., `"Database operation failed due to a constraint violation or data issue while saving [Entity Name]."`).
    - Explicitly add new custom exception files to `Domain/Exceptions` if the domain exception class doesn't already exist.

### 2. Backend: Application (Services)
- **Responsibility**: Orchestrate logic, perform business validations, and map Entities to DTOs using AutoMapper (`IMapper`).
- **Early Exit Pattern (Guard Clauses)**: Enforce the use of early exits for validation. Null checks, state validations (e.g., explicit `.HasValue` checks on nullable foreign keys), and authorization checks must be performed at the very beginning of the method. Throw exceptions immediately (e.g., `BadRequestException` for missing keys). Do not wrap the main "happy path" logic in `if` blocks.
- **Testability**: All scaffolded or refactored service methods must be designed for testability. Consult the `/service_unit_testing_4_steps` skill for rules and structural expectations regarding test generation.
- **Return Types**:
  - **List Retrievals**: Check if the list is empty and throw `KeyNotFoundException`. Map the tuple from the repository into a `PagedResult<Dto>` and return the `PagedResult<Dto>` to the controller.
  - **Write Operations**: 
    - **Private Helper Extraction:** The `SaveChangesAsync()` call, the `try/catch` block catching the Domain Exception, and the `rows == 0` check MUST be extracted into a **private helper method** (e.g., `Save[Entity]ChangesAsync()`) to keep the main business logic method clean.
    - Inside the helper, check the rows affected returned by the repository. If `rows == 0`, throw an `InvalidOperationException` with an entity-specific message (e.g., `"Failed to save course report"`).
    - Wrap repository write operations in a `try/catch` block within the private helper. Catch custom Domain Exceptions thrown by the Repository and re-throw them as an Application-level `BadRequestException` to ensure the controller knows the operation failed due to invalid state/data.
  - **Background Tasks (Fire-and-Forget)**: 
    - **DON'T:** Never use inline `Task.Run()` for fire-and-forget logic (exhausts the thread pool).
    - **DON'T:** Never inject `IServiceProvider` into the Application Layer to manually resolve DI scopes (Service Locator anti-pattern).
    - **DO:** Always use a strongly-typed generic `IBackgroundTaskQueue`.
      1. **`Application/IServices/IBackgroundTaskQueue.cs`**: `ValueTask QueueBackgroundWorkItemAsync<TService>(Func<TService, CancellationToken, ValueTask> workItem) where TService : notnull;`
      2. **`Infrastructure/BackgroundServices/BackgroundTaskQueue.cs`**: Implements the queue. Wraps the strongly typed action inside a scope resolver safely.
      3. **`Infrastructure/BackgroundServices/QueuedHostedService.cs`**: Long-running background worker process to consume the queue.
      4. **`Program.cs`**: Register via `AddSingleton<IBackgroundTaskQueue...>` and `AddHostedService<QueuedHostedService>`.
      5. **Usage in Service**: `await _taskQueue.QueueBackgroundWorkItemAsync<IMyService>(async (svc, token) => await svc.ExecuteAsync());`

### 3. Backend: Presentation (API Controllers)
- **Responsibility**: Handle HTTP routing, Authorization, and payload deserialization.
- **Form/Payload Binding**: The payload from the frontend (ViewModel/DTO) should be automatically parsed as a DTO argument in the controller method by the ASP.NET Core framework.
- **Encapsulate GET Parameters**: Avoid parameter bloat in Service and Controller GET methods. When handling multiple filters, pagination, or search queries, consolidate them into a single unified Request DTO parameter rather than passing 3+ primitive types (e.g., use `([FromQuery] PagedReportRequestDto request)` instead of `([FromQuery] string status, [FromQuery] int page, [FromQuery] int pageSize)`).
- **Error Handling**: Use explicit `try/catch` blocks. Translate `KeyNotFoundException` to 404 Not Found. Catch both `InvalidOperationException` and `BadRequestException` and pass `ex.Message` directly to `ApiResponse<T>.ErrorResponse(ex.Message)` instead of hardcoding error strings.
- **Response Format**: Always wrap responses in the standardized generic API response envelope (e.g., `ApiResp<T>` or `ApiResponse<T>`).

### 4. Backend: DTOs & Mappings
- **Application/DTOs, Application/Mappings**:
  - Strict separation of data transfer objects for requests and responses.
  - AutoMapper profiles define the explicit conversions.
  - **Data Validation**: Include data validation attributes (DataAnnotations) based on table structures in the DB.
  - **Query DTO Inheritance over Composition**: When creating complex Request DTOs for `[FromQuery]` GET endpoints (e.g., combining custom filters with pagination), always use **Inheritance** (e.g., `class FilterDto : PagedRequestDto`) rather than **Composition** (e.g., `public PagedRequestDto Pagination { get; set; }`). This ensures ASP.NET Core's default model binder natively flattens the parameters, allowing clean URL query strings (like `?page=1&pageSize=10`) without requiring nested prefixes. *Note: Ensure that the base `PagedRequestDto` (containing Page and PageSize) is explicitly created first if it does not already exist.*

### 5. Frontend: ViewModels
- **Models/**:
  - Custom classes that combine multiple data sources (Paginated results, static lists) into a single object for Razor Views.
  - **Data Validation**: Include data validation attributes mirroring the DB constraints.

### 6. Frontend: MVC Client Layer
- **MVC Controllers**:
  - Act as a Backend-For-Frontend (BFF) using a typed `ApiClient`.
  - **Form Input Binding**: For write operations, do not parse form input fields directly to individual method arguments. Use a ViewModel object as the argument so the framework automatically maps the fields behind the scenes.
  - **Page Load (Read)**: If retrieving multiple sets of data simultaneously, use `Task.WhenAll`. If only fetching from a single endpoint, a simple `await` is sufficient. Combine them into an aggregate ViewModel.
  - **State Management**: Persist UI state (tabs, pagination) via query parameters passed into `ViewBag`.
  - **Redirection**: The Frontend Controller must handle all routing logic (Unauthorized, Not Found, etc.) by returning standard `RedirectToAction()` or `Redirect()`. Client-side `fetch` handlers must not hardcode status code checks (e.g., avoid `if (res.status === 401)`). Instead, handle backend redirects natively by checking `if (res.redirected) { window.location.href = res.url; return; }`.
  - **Isolated Partial View Fetching**: For any view containing paginated collections (Admin Tables or Public Grids/Cards), navigation must ONLY fetch the target page of the specific collection. Do not trigger a full page reload or re-fetch other independent entities. Create standalone `[HttpGet]` endpoints in the FE Controller that return Razor Partial Views (`PartialView("_CollectionPartial", model)`), and use JS to seamlessly replace the specific container's inner HTML. *(Note: Purely SEO-driven primary catalog pages may use standard full-page routing, but partial views are the standard for dashboards, admin panels, and multi-collection pages).*
  - **Write Operations**: Provide standalone actions (`[HttpPost]`/`[HttpPut]`) that proxy data to the backend and return JSON for frontend AJAX handlers.
  - **Centralize API Response Wrappers**: Backend API response envelopes (like `ApiResponse<T>`) must be centralized in the `Models/Common` folder rather than being redefined as duplicate private wrapper classes inside each individual controller.

### 7. UI & UX Rules
- **Form Validation**: Add standard HTML5 data validations to form inputs.
- **Safe JavaScript Data Passing (`data-*` Attributes)**: Never use Razor string interpolation directly inside inline JavaScript function arguments for dynamic strings (e.g., `onclick="edit('@item.Description')"`), as unescaped quotes break JS syntax. Instead, carefully read the JS function implementation first to decide on the appropriate safe parsing action:
  1. If changing the function signature is acceptable, pass the DOM element (`this`) and use `element.getAttribute('data-*')` inside the JS function.
  2. If the function signature must remain as individual primitive values, pass them securely at the call site: `onclick="myFunc(this.getAttribute('data-value'))"`.
  3. For complex objects, serialize them to JSON in a `data-report='@JsonSerializer.Serialize(item, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })'` attribute and parse them safely inside the JS handler using `JSON.parse`. We explicitly enforce `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` to guarantee consistent JavaScript naming conventions and avoid frontend errors.
  Make sure to apply these safe parsing techniques for *all* string-type arguments.
- **Seamless State Mutations**: All write/state mutation operations (Add, Edit, Delete, Toggle Status — anything resulting in an INSERT, UPDATE, or DELETE) must be executed via AJAX. On success, avoid full page reloads. Instead, explicitly refetch and update ONLY the view of the target collection that was mutated using its specific Partial View endpoint (or DOM manipulation), leaving all other entities and collections on the page untouched. Every write operation must also have a confirmation popup (using SweetAlert).
- **Smart State Preservation on Detail Navigation**: When navigating from any paginated/tabbed list (Table or Grid/Card view) to a detail page, append the current `window.location.search` query parameters to the destination URL. The detail page must then dynamically read `Context.Request.QueryString.Value` and append it to its "Back" link. This ensures that clicking "Back" precisely restores the user's previous state.

### 8. Refactoring Rules
- **References**: Consider all references to the target (method calls, variable references, etc.). Changes should generally be isolated to the refactoring target.
- **Wrapper Methods**: Introduce wrapper methods if needed to preserve existing references.
- **Explicit Notification**: If you MUST make changes to the references outside the target, you must explicitly inform the user and get permission first.
- **Helper Extraction**: Require the extraction of private helper methods for logic such as fetching and validating multiple related entities or consolidated authorization logic (e.g., role checks combined with resource ownership). Explicitly consult the `/method_extraction` skill for rules and strategies on this.
- **Safe File Management**: NEVER permanently delete files when refactoring or replacing legacy code. Instead, rename them with a `.bak` extension to preserve them for backup and reference.

### 9. Coding Conventions
- **Import First, Use Later**: Forbid the use of inline, fully qualified namespace strings (e.g., `return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult...`). Require adding `using` directives at the top of the file and using the short class names in the code logic.
- **Dependency Injection First**: Never instantiate service dependencies or external libraries using the `new` keyword inside a class. Always inject them via Constructor Dependency Injection using their Interfaces, and ensure they are registered in the DI container (`Program.cs`). Only use `new` for instantiating simple DTOs, ViewModels, or native data structures.
- **Appropriate DI Lifecycle Registration**: When registering dependencies in `Program.cs`, you MUST explicitly evaluate and use the correct service lifetime:
  - Use `AddScoped` for the vast majority of Application Services and Infrastructure Repositories (especially those interacting with `AppDbContext`).
  - Use `AddSingleton` exclusively for stateless external utilities (e.g., `HtmlSanitizer`), centralized caches, or configuration wrappers. 
  - As always, apply the "Import First, Use Later" rule inside `Program.cs`—do not register using fully-qualified inline namespaces.

---

## Required Workflow

When invoked to create, refactor, or validate a feature, you MUST follow these steps:

### Phase 1: Analysis & Extraction
1. Thoroughly read the provided prompt, Use Case document, or Functional Specification.
2. Map out all required Read/Write operations, Entities, DTOs, and ViewModels.
3. Determine the "View Details" approach (Explicitly ask if unclear):
   - **UI-only approach**: Prefetched with the list and rendered dynamically in a popup.
   - **DB Retrieval approach**: Navigates to a new page or makes a new API call.
4. **Authorization**: Explicitly determine or **ask the user** which roles are authorized for the feature. Determine if the rule applies to the whole controller or specific methods (for both FE and BE).

### Phase 2: Create Implementation Plan
**MANDATORY PLANNING VALIDATION:** Before you output the `implementation_plan.md`, you MUST explicitly cross-reference your proposed folder structure, dependency registrations, and coding conventions against the rules in this skill document. If you propose an external integration in the Application layer, or use inline namespaces, you have failed.

Generate an `implementation_plan.md` artifact containing:
- **Goal & Completion Criteria**
- **Open Questions**: Clarifications on ambiguities, UI states, details approach, or authorization roles.
- **Proposed Changes**:
  - Detail the full `UI -> FE Ctrl -> BE Ctrl -> Service -> Repo -> DB` stack.
  - Include DTOs/ViewModels with validation attributes.
  - Specify SweetAlert confirmations and form bindings.
- **Verification Strategy**

### Phase 3: Await Approval
You **MUST STOP** and wait for the user to explicitly approve the Implementation Plan before generating or modifying any code.
