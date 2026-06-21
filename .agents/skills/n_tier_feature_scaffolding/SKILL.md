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
    - Wrap `SaveChangesAsync()` in `try/catch` blocks. Catch Entity Framework's `DbUpdateException` (from constraint violations) and re-throw it as a custom Domain Exception (e.g., `AiModelException`, `CourseException`) with a clear error message. 
    - Explicitly add new custom exception files to `Domain/Exceptions` if the domain exception class doesn't already exist.

### 2. Backend: Application (Services)
- **Responsibility**: Orchestrate logic, perform business validations, and map Entities to DTOs using AutoMapper (`IMapper`).
- **Early Exit Pattern (Guard Clauses)**: Enforce the use of early exits for validation. Null checks, state validations (e.g., explicit `.HasValue` checks on nullable foreign keys), and authorization checks must be performed at the very beginning of the method. Throw exceptions immediately (e.g., `BadRequestException` for missing keys). Do not wrap the main "happy path" logic in `if` blocks.
- **Testability**: All scaffolded or refactored service methods must be designed for testability. Consult the `/service_unit_testing_4_steps` skill for rules and structural expectations regarding test generation.
- **Return Types**:
  - **List Retrievals**: Check if the list is empty and throw `KeyNotFoundException`. Map the tuple from the repository into a `PagedResult<Dto>` and return the `PagedResult<Dto>` to the controller.
  - **Write Operations**: 
    - Check the rows affected returned by the repository. If `rows == 0`, throw an `InvalidOperationException`.
    - Wrap repository write operations in a `try/catch` block. Catch custom Domain Exceptions thrown by the Repository and re-throw them as an Application-level `BadRequestException` to ensure the controller knows the operation failed due to invalid state/data.

### 3. Backend: Presentation (API Controllers)
- **Responsibility**: Handle HTTP routing, Authorization, and payload deserialization.
- **Form/Payload Binding**: The payload from the frontend (ViewModel/DTO) should be automatically parsed as a DTO argument in the controller method by the ASP.NET Core framework.
- **Error Handling**: Use explicit `try/catch` blocks. Translate `KeyNotFoundException` to 404 Not Found, `InvalidOperationException` to 400 Bad Request, and `BadRequestException` to 400 Bad Request.
- **Response Format**: Always wrap responses in the standardized generic API response envelope (e.g., `ApiResp<T>` or `ApiResponse<T>`).

### 4. Backend: DTOs & Mappings
- **Application/DTOs, Application/Mappings**:
  - Strict separation of data transfer objects for requests and responses.
  - AutoMapper profiles define the explicit conversions.
  - **Data Validation**: Include data validation attributes (DataAnnotations) based on table structures in the DB.

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
  - **Security**: Check for `HttpStatusCode.Unauthorized` from API responses and redirect to the Account/Login page.
  - **Write Operations**: Provide standalone actions (`[HttpPost]`/`[HttpPut]`) that proxy data to the backend and return JSON for frontend AJAX handlers.

### 7. UI & UX Rules
- **Form Validation**: Add standard HTML5 data validations to form inputs.
- **Write Operations**: Every write operation (add, update, delete, status toggle) must have a confirmation popup (using SweetAlert).

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
