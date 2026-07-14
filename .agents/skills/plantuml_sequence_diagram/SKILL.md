---
name: plantuml_sequence_diagram
description: Guides the generation and modification of standardized, high-quality PlantUML sequence diagrams for application use cases following strict project design guidelines.
---

# PlantUML Sequence Diagram Generation Skill

This skill provides comprehensive instructions, architectural guidelines, styling rules, and concrete examples for creating and refactoring PlantUML sequence diagrams. It ensures diagrams are technically accurate, visually consistent, and represent runtime interactions (lifeline activation, database changes, return paths, and validation flows) according to the project's sequence diagram standard.

---

## 1. File Naming Convention

All sequence diagram files must follow the `sq_use_case_name.plantuml` naming convention.
- Prefix: `sq_`
- Use case name: lower snake_case (e.g., `create_course`)
- Extension: `.plantuml`
- Example: `sq_create_course.plantuml`

---

## 2. Participant Naming and Aliasing

To keep diagrams readable and consistent, participants should be defined at the top with standardized aliases.

### Standardized Primary Actors
Do not invent custom actor names (like "Learner" or "Customer"). Use one of the following exact standardized actors as the `act` participant depending on the use case context:
- `Guest` (Unauthenticated actor)
- `User` (Authenticated customer/learner)
- `Instructor` (Originally a User but applied to become an Instructor. Can switch views flexibly)
- `Staff` (Authorized manager)
- `Admin` (Highest Authority manager)

### Standardized Participant Aliases
| Participant Type | PlantUML Definition Example | Alias | Role |
|---|---|---|---|
| Actor | `actor User as act` | `act` | The human actor interacting with the system (choose from the standardized list above) |
| View / UI | `participant "PageName" as view` | `view` | Name views using domain context (e.g. `AiServiceManagementPage`), NOT file paths like `Views/.../Index`. |
| Redirect Target | `participant "RedirectForm" as r_view` | `r_view` | Target view after redirection |
| Frontend Controller | `participant "ControllerName" as fe_ctrl <<frontend controller>>` | `fe_ctrl` | Standard MVC controller handling client requests |
| View Model | `participant ":ViewModelClass" as view_model` | `view_model` | Page/View model mapped in the frontend controller |
| Backend Controller | `participant "ControllerName" as be_ctrl <<backend controller>>` | `be_ctrl` | API Controller exposing REST services |
| Service | `participant ":ServiceClass" as sv` | `sv` | Business logic service (e.g. `:CourseService`) |
| Data Transfer / Response | `participant ":ResponseDto" as res` | `res` | Response or DTO class |
| Entity | `participant ":EntityClass" as ett` | `ett` | Domain entity class |
| Repository | `participant ":RepositoryClass" as repo` | `repo` | Data access repository class |
| DB Context | `participant ":AppDbContext" as ctx` | `ctx` | Entity Framework DB context |
| Database | `database Database as db` | `db` | The physical database storage (cylinder) |

### Modeling Rules
- **Controller Stereotypes:** Frontend controllers and Backend controllers are explicitly distinguished using PlantUML stereotypes (`<<frontend controller>>` and `<<backend controller>>`) directly in their participant declarations.
- **Interfaces Only:** Always model the interfaces for injected dependencies, not their implementations. Prefix class instances with a colon and `I` (e.g., `:ICourseRepository` rather than `:CourseRepository`, `:ICourseService` rather than `:CourseService`).
- **Instance Prefixing:** Prefix class instances with a colon (e.g., `:CourseService`, `:CourseRepository`). Do not prefix views, controllers, or actor aliases.
- **Visuals:** All participants except actors (stick figure) and databases (cylinder figure) must be drawn as rectangles.
- **Participant Grouping Order:** Declare participants logically in the order of interaction. For example, place the `view_model` immediately after `fe_ctrl`, and the DTO (`res`) immediately after `sv`. The database context (`participant ":AppDbContext" as ctx`) is the final participant declared in the list, placed immediately preceding `database Database as db`.
- **PagedResult Stripping:** Do not include generic wrappers like `PagedResult<T>` or `ApiResponse<T>` as participants, because they are intentionally omitted from class diagrams. Instead, declare the participant as the inner wrapped type (e.g., `participant ":CourseReportDetailViewModel"` instead of `PagedResult<CourseReportDetailViewModel>`).

---

## 3. Style Definitions and Canvas Settings

Every `.plantuml` file must start with standard styling and settings for a clean, cohesive black-and-white look. Copy and paste the following header configuration exactly:

```plantuml
@startuml 
<style>
    sequenceDiagram{
        actor{
            BackgroundColor white
            LineThickness 1.0
        }
        participant{
            BackgroundColor white
            RoundCorner 0
        }
        database{
            BackgroundColor white
        }
        groupHeader{
            BackgroundColor white
        }
        referenceHeader{
            BackgroundColor white
        }
        reference{
            BackgroundColor white
        }
    }
</style>

hide footbox
```

Use `mainframe <Title>` (e.g., `mainframe Create Course`) right after the header, followed by the participant declarations.

---

## 4. Interaction Arrow Styles

Ensure correct arrow types to model request/response control flow:
- `->` (Solid Arrow): Synchronous calls, requests, or internal method invocation.
- `-->>` (Dashed Arrow): Returns, responses, redirects, thrown exceptions, or messages displayed to the actor.

---

## 5. Lifeline Activation and Deactivation

Correctly managing activation (`++`) and deactivation (`--`) is critical to show when an object is executing or waiting.

### General Rules
1. **Actor Activation (`++`):** Always activate the actor's lifeline as the first thing in the sequence diagram by adding `act ++` at the top before any interactions.
2. **Initial Call Activation (`++`):** Append `++` to the receiving participant when it receives its first call.
   - Example: `fe_ctrl -> be_ctrl++: POST /api/courses/moderate`
3. **Return Deactivation (`--`):** Deactivate a participant's lifeline when it returns or throws an exception.
   - **Note on PlantUML Shortcut Syntax**: In PlantUML, `++` activates the *target* participant, while `--` at the end of an arrow (e.g., `A -> B--: msg`) deactivates the *source* participant (`A`). While `fe_ctrl -->> view--: Return Ok()` is a valid shorthand to return a message and simultaneously deactivate `fe_ctrl`, you may also explicitly place the deactivation on the following line (e.g., `fe_ctrl--`) if it improves diagram clarity and prevents source/target confusion.
   - **Nested Return Order**: When multiple nested lifelines return in sequence (e.g., `db` to `ctx`, then `ctx` to `repo`), explicitly deactivate the inner lifeline *before* the outer lifeline makes its return call to its caller. This visually ensures the inner lifeline ends correctly before control passes up the stack. Example:
     ```plantuml
     db -->> ctx: Return data
     db--
     ctx -->> repo: Return data
     ctx--
     ```
4. **Completeness:** Every participant's lifeline (excluding the actor) must be fully deactivated by the time the flow finishes its execution paths.

### Self-Call Activations
- **Conditional Triggers (Opt/Alt):** If a self-call checks a condition to branch (e.g. `Validate ModelState`), activate the self-call and immediately deactivate it *before* opening the conditional block:
  ```plantuml
  fe_ctrl -> fe_ctrl++: Validate ModelState
  fe_ctrl--
  group opt [Invalid ModelState]
      fe_ctrl -->> view: Return status code 400
      view -->> act: Display error message
  end
  ```
- **Auxiliary Helper Calls:** For minor internal helper calls that do not branch or contain detailed nested execution flows, activate and instantly deactivate:
  ```plantuml
  sv -> sv++: Call CheckInstructorRights()
  sv--
  ```
- **Main Execution Helper Calls:** For internal helper methods where the internal logic is being expanded in the diagram, keep the activation open until that method returns.
- **Closing Inner Helper Activations:** The closing of an inner helper method activation depends heavily on the diagram's branch rendering order and the exact C# method implementation:
  - When using a fail-fast flat structure where the failure/exception branch is drawn *above* the success path, the helper method's activation can be explicitly closed (`sv--`) immediately after it returns its value (e.g., returning `numberOfRowsAffected`), before evaluating the condition.
  - If using a nested structure where the success path is drawn *first* and the failure path is drawn *below*, the activation must be extended to cover the entire condition block until its failure path is reached, ensuring visual alignment with the runtime stack. Always consult the exact implementation of the method to determine its lifespan.

### Trailing Lifeline Deactivation in Alt Blocks
When a diagram ends inside conditional branches, only the final branch of the outermost `alt` is responsible for deactivating the trailing lifelines along its return path.
- **Flat Structures:** In fail-fast flat structures, long-running trailing lifelines (`sv`, `be_ctrl`, `fe_ctrl`, `view`, `act`) are not deactivated (`--`) in the early exception/validation branches. They must be trailed entirely down the diagram and deactivated only in the final branch visually rendered at the bottom of the outermost `alt` (which could be a success or failure branch depending on the structure).
- **Final Actor Return:** At the very end of the diagram, ensure the final active view participant is fully deactivated when it returns control or displays the final message to the actor (e.g., `view -->> act--: Display success message` or placing `view--` on the next line).
- Example:
  ```plantuml
  alt condition_1
      sv -->> be_ctrl: Return CourseResponse object
      be_ctrl -->> fe_ctrl: Return statusCode 201
      fe_ctrl -->> view: Return JSON (success = true)
      view -->> act: Display course creation success message
  else condition_2
      sv-->>be_ctrl--: throw InvalidOperationException
      be_ctrl -->> fe_ctrl--: Return statusCode 400
      fe_ctrl -->> view--: Return statusCode 400
      view -->> act --: Display error message
  end
  ```

### Local vs Trailing Lifelines in Nested Alts
When dealing with nested `alt` fragments, clearly distinguish between "Local" and "Trailing" lifelines:
- **Local Lifelines:** Lifelines activated entirely *inside* an `alt` branch (e.g., `db`, `ctx`, `ext_repo` inside the valid flow). If their flow terminates inside a nested `alt` block, they must strictly follow the visual deferral rule: only the final branch of that nested `alt` is responsible for deactivating them. Do not deactivate them in earlier branches (e.g., normal flow), otherwise PlantUML will end their activation bars early and draw them as thin lines in the subsequent exception branches.
- **Trailing/Main Lifelines:** Lifelines activated *before* the outermost `alt` block began (e.g., `sv`, `be_ctrl`). These are not manually deactivated inside inner exception branches (e.g., do not place `sv--` in an inner exception block). They must strictly obey the rule above and only be deactivated in the final branch of the outermost `alt`.

---

## 6. Flow Logic Guidelines

### Mapping Use Case to Diagram
- When creating, fixing, editing, updating, validating, or modifying a diagram based on a use case specification, follow this strict mapping:
  - **Alternative flows** must be modeled as `opt` fragments.
  - **Exceptions** must be modeled as `alt` fragments.
  - **Omit Trivial UI-Only Alternative Flows:** Do not model purely client-side, trivial UI interactions (like closing a modal, canceling a form, or dynamically adding/removing UI input fields) as `opt` blocks. Only model UI alternative flows if they involve a request to the backend or represent a significant client-side validation barrier (e.g., `Invalid form input`).
- **UI Extension Use Cases:** If a use case is purely a UI interaction (e.g., "View details") that relies on pre-fetched data from a parent base use case (e.g., "View List"), model the entire backend flow identically to the base use case. Append the specific UI interaction (e.g., `act -> view: Click "Details"`) inside the final success branch, and terminate the sequence immediately when the core objective is reached (e.g., `view -->> act--: Display details in a popup`). Do not model auxiliary "close popup" actions unless they trigger distinct backend logic.
- **Form Input Validation (Alt Flow):** The invalid form input alternative flow could be just a client-side check on the view, or it could include an additional model state check in the frontend controller. If a diagram already models these two cases separately, keep them as they are.

### Diagram Complexity and Ref Fragments
To prevent sequence diagrams from becoming overwhelmingly complicated, large blocks of logic must be extracted into their own separate sequence diagrams and referenced using a `ref over` fragment. This applies to both sequential and concurrent execution flows.

### User Approval Required for Extraction
Before extracting logic into a `ref` fragment (whether for sequential complexity or concurrent background tasks), explicitly ask the user for permission. Propose the extraction (detailing which logic to extract and the new file name) and only proceed with the `ref` extraction if the user explicitly approves.

#### 1. Sequential Execution Extraction
- If a main execution flow contains a complex, multi-step sub-process (e.g., a massive data sync or a heavily nested calculation), abstract that entire sub-process into a new file (e.g., `sq_sub_process_name.plantuml`).
- In the main diagram, use `ref over` spanning the relevant participants to represent this sub-process, avoiding unnecessary clutter.
  ```plantuml
  sv -> sv++: ExecuteComplexProcess()
  ref over sv, db: Handle Complex Sub Process
  sv--
  ```

#### 2. Background Tasks and Concurrency (Par & Ref Fragments)
- When a request kicks off background tasks that run concurrently with the main thread (e.g., `QueueBackgroundWorkItemAsync`), use a `par` fragment to model the parallel execution.
- Every independent concurrent flow is separated by an `else` branch. If there are multiple background tasks, they each get their own `else` branch.
- **Main Thread Placement:** To be consistent, place the main thread's synchronous return path as the final `else` branch before the `end` keyword, explicitly separating it from the background tasks.
- **Trivial Background Tasks:** If a background thread only handles trivial secondary fetches (like writing basic logs or fetching dropdown configs) alongside a primary core fetch, you may entirely omit the `par` fragment and only model the primary fetch sequentially to reduce visual noise. Only use `par` when the concurrent tasks are both substantial components of the domain logic.
  ```plantuml
  sv -> queue++: QueueBackgroundWorkItemAsync(workItem)
  
  par Background Task 1
      ref over queue, db: Handle Course Moderation With AI
  else Background Task 2
      ref over queue, event_bus: Publish Course Moderated Event
  else Main Thread Return
      queue -->> sv: Return result (Task queued)
      sv -->> be_ctrl--: Return result
      be_ctrl -->> fe_ctrl--: Return statusCode 200
      fe_ctrl -->> view--: Return JSON (success=true)
      view -->> act--: Display success message
  end
  ```
### Layer and Component Omissions
- Omit `fe_ctrl` if the frontend requests the backend directly (e.g. via direct AJAX/Fetch call without server-side MVC controller routing).

### UI Feedback and Redirections
- Do not over-specify UI implementation details (e.g., "popup message", "toast", "simple p tag"). Use generic interaction descriptions like `Display success message` or `Display error message`.
- If an action ends with displaying a success/error message and requires manual user intervention to proceed (e.g., clicking "Edit Now" to redirect), **do not model the subsequent manual click or redirect**. End the sequence path at the display of the initial message.
- **Omit Secondary UI Refreshes:** Do not model secondary data fetches (like a `GET` request to reload a table or partial view) that occur automatically after the primary success message is displayed. Stop the flow at the success notification to keep the focus on the primary use case.
- ### Automatic Redirections Only
  Only include a redirection step when it occurs automatically without manual interaction from the actor. Because a redirect acts as a return response that completes the controller's lifecycle, model redirects using a dashed arrow (`-->>`) with a deactivation suffix (`--`) on the controller, followed by explicitly activating the target view on the next line.
  ```plantuml
  fe_ctrl -->> login_view--: Redirect to Login page
  activate login_view
  login_view -->> act--: Display Login page
  ```

### Form Validation and Pipeline Execution Ordering
- Standard client-side UI input validations must be shown in the UI/view first.
- Only show server-side controller `Validate ModelState` if it is explicitly written in the controller code.
- **Execution Ordering:** Because ASP.NET Core processes Authorization filters before Model Binding in the request pipeline, the `Validate ModelState` block is modeled *inside* the `else Valid actor's authentication and authorization` branch. Do not place ModelState validation before or outside the authentication check.

### Frontend Authentication/Authorization Validation
- Authentication and authorization checks (e.g., checking cookies, roles) must be abstracted into a single self-call at the frontend controller level (e.g., `fe_ctrl -> fe_ctrl++: Validate actor's authentication and authorization`), immediately followed by a standardized flat `alt` block handling the 401 and 403 branches.
- Explicitly include `LoginPage` as `login_view` and `ErrorPage` as `error_view` in the participant list.
- The block must be formatted exactly as follows, with the success label dynamically matching the self-call target:
  ```plantuml
  fe_ctrl -> fe_ctrl++: Validate actor's authentication and authorization
  fe_ctrl--
  
  alt Session Invalid
      fe_ctrl -->> login_view: Redirect to LoginPage
      activate login_view
      login_view -->> act--: Display login form
  else Unauthorized Access
      fe_ctrl -->> error_view: Redirect to ErrorPage
      activate error_view
      error_view -->> act--: Display error message
  else Valid actor's authentication and authorization
  ```

### API/Controller Request Messages
- When modeling requests between the view and frontend controller, or frontend controller and backend controller, explicitly state the HTTP Method and Route Path instead of using generic phrases like "Send POST request".
  - Frontend Example: `view -> fe_ctrl++: POST /InstructorCourse/ModerateCourse`
  - Backend Example: `fe_ctrl -> be_ctrl++: POST /api/courses/moderate`

### Controller Return Messages
- **Backend to Frontend Controller:** Use `Return statusCode ...` (e.g., `be_ctrl -->> fe_ctrl: Return statusCode 201`).
- **Frontend Controller to View:** Return exactly whatever the code returns, omitting verbose details like message strings or long content strings. Keep the redirect rule as is.
  - **Frontend Proxy Status Codes:** If the frontend controller acts as a proxy that evaluates the backend's status code to dynamically return a generic `HttpStatusCodeObjectResult` (e.g., the code uses `return StatusCode((int)response.StatusCode, jsonContent)`), do not copy this generic signature. Instead, translate it into the specific semantic ASP.NET Core helper method that corresponds to the backend's response in that specific branch (e.g., `Return Ok()`, `Return BadRequest()`, `Return NotFound()`, `Return Unauthorized()`, `Return Forbid()`), omitting all payload arguments.
  - **Redirects:** If the frontend controller redirects to a view, the message should explicitly state the redirect target, e.g., `fe_ctrl -->> r_view: Redirect to LoginPage`.
  - **JSON Payloads:** If the code returns a JSON payload (e.g., `return Json(new { success = true, message = "..." })`), omit the verbose message string: `Return Json(success=true)`.
  - **Standard Views:** If the code returns a standard view (e.g., `return View(viewModel)`), state it as `Return View(viewModel)`.
  - **Status Codes:** If the code returns a status code directly (e.g., `return BadRequest()`), state it as `Return BadRequest()`.

### Conditional Branches (Alt/Opt)
- `alt` branch conditions must be mutually exclusive. Always use clear, opposing labels (e.g., `numRows > 0` vs `numRows = 0`, `Valid` vs `Invalid`, `Duplicate` vs `No Duplicate`).
- **Fragment Labels:** Keep `alt` and `else` fragment labels as concise human-readable conditions or semantic clauses (e.g., `alt Course Not Found`, `object != null`, `totalCount > 0`). Do not include raw Exception class names in the fragment labels (e.g., use `alt Course Not Found`, not `alt Course Not Found (KeyNotFoundException)`). The specific exception type is documented by the throw message inside the block.
- **Read vs Write Branching Conditions:** 
  - For **Write/Mutate Operations** (Insert/Update/Delete), use `numberOfRowsAffected > 0` (or similar DB-level checks) as the core condition.
  - For **Read/Select Operations**, `alt` blocks should trigger based on data presence (e.g., `Entity == null` vs `Entity found`) or authorization states (e.g., `Unauthenticated`, `Unauthorized Access`).
- **Prefer Flat Structures:** Whenever possible, prefer a flat `alt/else` structure over deep nesting to improve diagram readability. You may reorder the logical flow in the diagram to make a flat structure work sensibly (e.g., grouping early validation failures as `else` branches, and placing the successful execution path in a final `else All checks pass` branch).
- **Semantic Triggers for Flat Structures:** The conditional trigger (the self-call `sv->sv++`) immediately preceding a flat `alt` fragment must have a broad, semantic label covering *all* nested checks (e.g., `Validate course update request`). Furthermore, the final success branch of the `alt` block must explicitly pair with this trigger (e.g., `else Valid course update request`).
- **Condensing Validation Logic:** Encourage condensing fine-grained, related validation checks into unified `else` branches. If multiple validation checks throw the exact same exception (e.g., `BadRequestException`) and result in the exact same HTTP response and view state, merge them into a single, broad `else` branch (e.g., `else Invalid course content`) to simplify the diagram and avoid repetitive boilerplate.
- **Collapsing Multiple Internal Validations:** If a service calls multiple distinct backend validation methods consecutively before the main execution logic (e.g., `GetEntityAsync` followed by `ValidateAccessAsync`), they should **not** be modeled as separate sequential self-calls. Instead, collapse them entirely into a single conceptual self-call (e.g., `sv -> sv++: Validate [action] request`), immediately followed by a flat `alt` block handling their respective distinct exceptions (e.g., `alt Entity not found`, `else Unauthorized`).

### Abstracting Auxiliary Logic & Fetches
- Verbose inline code blocks (like `foreach` loops) or secondary read operations (like fetching overall category lists, wishlist statuses, or generic stats when the main focus is a course list) should be aggressively abstracted into descriptive self-calls, but only if they apply to *auxiliary* changes/fetches outside the primary context. For example, in an operation for Courses, fetching/modifying Lessons is auxiliary and should be abstracted. In an operation for Lessons, modifying Courses or Learning Materials would be auxiliary. These self-calls should use natural language (e.g., `sv->sv++: Remove lessons in course`).
- **Complete Omission of Unrelated Fetches:** Completely omit auxiliary, unrelated API fetches (e.g., fetching general config dropdowns or unconnected moderation logs in a frontend controller) if they clutter the diagram and distract from the primary use case focus.
- **Concurrent/Parallel Independent Fetches (Task.WhenAll):** If a controller action fetches multiple independent datasets concurrently (e.g., using `Task.WhenAll` to fetch stats, courses, course-reviews, and lesson-reviews for a single page load), a use-case specific sequence diagram (e.g., "View List of Course Report") models the specific data fetching flow relevant to that use case (e.g., only fetching Course Reports). Intentionally omit the other parallel API calls from the diagram to keep diagrams strictly separated by their use case domain. Do not model them in a massive `par` block unless explicitly instructed.

### Distinct Return Paths
- Paths for `alt` branches must remain separate from their entry point down to the final return to the actor. Even if two branches display a similar status code or error popup, they represent separate paths and must deactivate their lifelines independently.

### DTO, Entity, and ViewModel Instantiation
- If the code contains explicit object instantiation syntax for a DTO, Entity, or ViewModel (e.g., `new CourseModerationResult()`, `new AiModel()`), it is modeled explicitly using its own participant, followed by a synchronous constructor call and return. Do not abstract this into a simple self-call.
- **IMapper Usage:** This explicit modeling scheme also applies when the code uses AutoMapper (`IMapper`) to create the object (e.g., `_mapper.Map<AiModelAdminDto>(addedModel)`). Instead of modeling the mapper itself, you treat it as if the DTO's constructor was called directly.
- Do not use PlantUML's native `**` instantiation syntax. Include key arguments to clarify its state.
  - Example (Explicit `new`): `sv -> res++: Call CourseModerationResult(courseId, "MANUAL_AUDIT")`
  - Example (IMapper map): `sv -> res++: Call AiModelAdminDto()`
  - Return: `res -->> sv--: Return AiModelAdminDto object`
- **When to Omit:** Only simplify, abstract, or skip modeling these objects if they are provided implicitly by the framework under the hood (e.g., through ASP.NET Core model binding when passing form data to a controller method).

### Void and Task<void> Returns
- Whenever modeling a call to a backend method that returns `Task<void>` or `void`, the return step in the sequence diagram must simply be modeled as `Return result` (e.g., `repo -->> sv--: Return result`).
- **Architectural Rationale:** Public methods returning `void` or `Task<void>` are considered untestable "trash code" in this architecture, as they do not yield verifiable state. Using `Return result` serves as a mandatory placeholder to force developers to design testable interfaces that return meaningful values (like status flags or entity states). Do not correct this to a simple `Return`.

### Database Operations (Read/Write)
- **Database Call Naming:** When modeling the database interaction (e.g., `ctx -> db++`), use a semantic message like `Save changes to database` or `Execute query` instead of raw SQL queries.
- **Abstracting DbSet (Read Operations):** All database read operations route through `ctx`. The repository calls `ctx` with a descriptive message (e.g., `repo -> ctx++: Query courses from DbSet`), and `ctx` calls `db` with the same or similar descriptive message. Raw SQL and indirect `DbSet` method calls (like `.Where()` or `.Include()`) are strictly forbidden.
- **Abstracting DbSet (Write Operations):** Indirect `DbSet` state mutations (e.g., `_context.Courses.Add()`, `.Update()`, `.Remove()`) are not modeled as interactions between `repo` and `ctx`. The only permitted write interaction from the repository to the context is the direct commit: `repo -> ctx++: Call SaveChangesAsync()`.
- **Transaction Simplification:** OMIT raw database transaction boundaries (`BeginTransaction`, `Commit`, `Rollback`) from sequence diagrams to reduce clutter. Write operations should instead be modeled abstractly as service calls to the repository for state updates, finalized by a single `SaveChangesAsync()` call.
- **Read Operations (No Explicit Exceptions):** Do not model unhandled database exceptions (e.g., `SqlException` or generic `alt Database query failed` blocks) for standard read/select operations. However, if the code explicitly checks for empty lists or null entities and returns a failure state, use `alt Database Query Failure` for the missing data path, and `else Database Query Success` for the found data path. The `alt` fragment branching starts right after the query execution call from `ctx` (AppDbContext) to `db` (Database).
- **Aligning with Codebase Implementation:** Before modeling database failure paths for write operations, always check the actual repository and service code:
  - **Exception-Based Saving (Preferred):** If the code uses a `try/catch` around `SaveChangesAsync()` to catch EF Core's `DbUpdateException` (or a domain exception), use `alt Database Save Failure` for the exception fragment, and `else Database Save Success` for the success path. Avoid raw exception names like `alt DbUpdateException` or phrases like `Saved Successfully`.
  - **Integer-Based Checking:** Only if the code explicitly checks the returned integer, assume `SaveChangesAsync()` returns an `int` variable named `numberOfRowsAffected`. Use a self-call `Check numberOfRowsAffected` and branch on `numberOfRowsAffected > 0` vs `numberOfRowsAffected = 0`.
- In the failed paths (whether exception or zero rows), accurately trace the exception thrown by the repository and service, ensuring it matches the actual codebase (e.g., throwing a `BadRequestException` which results in a `400 BadRequest` controller return).
- **Write Operation Returns:** When a database write operation succeeds and returns an integer representing the rows affected (from `db` up through `ctx`, `repo`, etc.), strictly use the phrase `Return numberOfRowsAffected`. Do not casually swing between "rows affected", "num rows", or other variations.
---

## 7. Cross-Diagram Synchronization

When you are provided with attached class diagrams (in Mermaid, PlantUML, etc.) alongside the prompt, and are asked to create, validate, refactor, or edit sequence diagrams, verify cross-diagram consistency:
- **Synchronization Check:** Ensure that every participant, class, method, and property used in the sequence diagram is accurately represented and included in the attached class diagrams.
- **Handling Missing Elements:** If you need to introduce a new method, participant, or property in the sequence diagram that is not yet included in the class diagrams, do not assume it exists. Explicitly ask the user for permission or confirmation on whether they want you to also modify the respective class diagrams to match the sequence diagram updates.

---

## 8. Verification Checklist and Workflow

Every time you are asked to create, validate, refactor, or edit a sequence diagram, strictly follow this verification workflow to ensure no rules are missed.

### Workflow Process
1. For each entry in the checklist below, look back at the original description in this skill file.
2. Evaluate the diagram to see if it adheres to the rule you are currently checking.
3. If it passes, temporarily mark the checklist entry as passed in your thought process. 
4. If it fails, think up a solution to correct it, but **do not** attempt to apply the solution just yet.
5. After going through all entries, analyze the passed/failed results and finalize the solutions to ensure they do not conflict (e.g., ensure the solution for rule 3 doesn't break rule 1).
6. Apply the finalized solutions to the diagram.
7. Go through the checklist again. If any entries fail, repeat the verification process. Stop only when all entries pass.

### The Checklist

**1. File and Participants**
- [ ] **File Naming Convention:** Is it `sq_use_case_name.plantuml`?
- [ ] **Primary Actors:** Is the actor exactly one of Guest, User, Instructor, Staff, Admin?
- [ ] **Participant Aliases & Types:** Are aliases standardized (`view`, `fe_ctrl`, `be_ctrl`, `sv`, `res`, `ett`, `repo`, `ctx`, `db`, `r_view`, `view_model`)? 
- [ ] **Modeling Rules:** Are stereotypes (`<<frontend controller>>`) included? Are interfaces used? Are class instances prefixed with `:`? Are participants ordered logically?
- [ ] **Database Context Position:** Is `ctx` placed immediately preceding `db` and is `db` the last participant?
- [ ] **Participant Filtering:** Are generic wrappers like `PagedResult` omitted as participants in favor of the inner wrapped type?

**2. Canvas and Styling**
- [ ] **Style Definitions:** Is the exact standard styling header included? Is `mainframe <Title>` present?
- [ ] **Interaction Arrows:** Are synchronous calls using `->` and returns/responses using `-->>`?

**3. Lifeline Activation and Deactivation**
- [ ] **Actor Activation:** Is `act ++` at the top?
- [ ] **Initial Calls:** Are lifelines activated on their first call (e.g. `++`)?
- [ ] **Nested Returns:** Are inner lifelines explicitly deactivated before the outer lifeline makes its return call (e.g., `db--` before `ctx -->> repo`)?
- [ ] **Self-Calls:** Are conditional trigger and auxiliary self-calls instantly deactivated? Are main helper calls kept open or closed depending on the branch rendering order?
- [ ] **Trailing Lifelines (Flat Structures):** Are long-running lifelines trailed entirely to the bottom in flat structures and deactivated only in the final visually rendered branch?
- [ ] **Final Actor Return:** Is the final view explicitly deactivated upon returning control to the actor?
- [ ] **Local Lifelines (Nested Alts):** Are local lifelines deactivated correctly within their nested alt (only in the final branch of that nested alt)?
- [ ] **Trailing Lifelines (Nested Alts):** Are main lifelines never manually deactivated inside inner exception branches?

**4. Flow Logic Guidelines**
- [ ] **Mapping Use Cases:** Are alternative flows modeled as `opt` and exceptions as `alt`?
- [ ] **UI Extensions:** Are trivial client-side only UI alternative flows omitted? Are UI extension use cases properly appending their UI interaction to the base use case's success path?
- [ ] **Complex Ref Fragments:** Are complex processes extracted to `ref over` fragments? Did you get user approval for the extraction?
- [ ] **Background Tasks (par):** Are concurrent flows modeled with `par`? Does every concurrent flow and the main thread return get its own `else` branch, with the main thread return placed last?
- [ ] **Omissions:** Is `fe_ctrl` omitted if the frontend directly calls the backend? Are unrelated auxiliary API fetches completely omitted?
- [ ] **Redirections:** Are manual clicks and secondary UI refreshes after success omitted? Are automatic redirects explicitly modeled with target view activation (e.g., `fe_ctrl -->> r_view: Redirect to...` then `activate r_view`)?
- [ ] **Authentication/Authorization:** Is the exact `fe_ctrl` auth block included (with `login_view` and `error_view`) if applicable?
- [ ] **Pipeline Execution Ordering:** Is `Validate ModelState` placed inside the valid authentication branch?
- [ ] **API Requests:** Do requests include HTTP Method and Route Path (e.g., `POST /api/...`)?
- [ ] **Controller Returns:** Are `Return statusCode ...` messages used for Backend to FE? For FE to View, is the exact C# return type used (with verbose strings omitted)? If the frontend acts as a proxy, are generic StatusCode returns mapped to specific semantic helper methods?
- [ ] **Conditional Branches:** Are fragment labels mutually exclusive without raw Exception class names? Is `numberOfRowsAffected` used for writes and data presence used for reads?
- [ ] **Flat Structures:** Are flat structures preferred?
- [ ] **Semantic Conditional Triggers:** Are semantic triggers used before conditional fragments (`alt`, `opt`)?
- [ ] **Condensing Validations:** Are similar validation checks condensed into unified `else` branches? Are multiple internal validations collapsed into a single conceptual self-call followed by a flat `alt`?
- [ ] **Abstracting Logic:** Are verbose operations properly abstracted into descriptive self-calls using natural language?
- [ ] **Distinct Return Paths:** Are `alt` branch return paths kept distinct all the way to the actor?
- [ ] **Explicit Instantiation:** Are DTOs, Entities, and ViewModels explicitly instantiated with `Call ClassName()` if instantiated with `new` or `IMapper` in code? Are key arguments included instead of PlantUML's `**` syntax?
- [ ] **Void/Task<void> Returns:** Are they modeled as `Return result`?

**5. Database Operations**
- [ ] **Database Call Naming:** Are semantic messages used (e.g., `Save changes to database`, `Execute query`)?
- [ ] **Abstracting DbSet (Read):** Do read operations route through `ctx` to `db` with a descriptive message? Are raw SQL and indirect DbSet calls omitted?
- [ ] **Abstracting DbSet (Write):** Are indirect DbSet state mutations omitted? Is the only permitted write interaction `Call SaveChangesAsync()`?
- [ ] **Transactions:** Are raw transaction boundaries omitted?
- [ ] **Read Operations Branching:** Does the alt fragment branching for reads start right after the query execution call from `ctx` to `db`? Are unhandled database exceptions omitted?
- [ ] **Write Operation Branching:** Does the save logic align with exception-based (`alt Database Save Failure`) or int-based (`alt numberOfRowsAffected = 0`) code implementation?
- [ ] **Write Returns:** Is `Return numberOfRowsAffected` strictly used if returning an integer?

**6. Cross-Diagram Synchronization**
- [ ] **Synchronization Check:** Are all sequence diagram elements present in any provided class diagrams?
- [ ] **Handling Missing Elements:** If adding new elements, did you ask for permission to modify the class diagrams?

---

## 9. Reference Templates

To see the standard, working PlantUML sequence diagrams depicting flows with these guidelines, please refer to the `.plantuml` files in the `references/` directory within this skill folder.

Always study these reference examples before generating a new sequence diagram code to ensure your output strictly follows the established patterns, tone, and granularity.