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
Do NOT invent custom actor names (like "Learner" or "Customer"). You must exclusively use one of the following exact standardized actors as the `act` participant depending on the use case context:
- `Guest` (Unauthenticated actor)
- `User` (Authenticated customer/learner)
- `Instructor` (Originally a User but applied to become an Instructor. Can switch views flexibly)
- `Staff` (Authorized manager)
- `Admin` (Highest Authority manager)

### Standardized Participant Aliases
| Participant Type | PlantUML Definition Example | Alias | Role |
|---|---|---|---|
| Actor | `actor User as act` | `act` | The human actor interacting with the system (choose from the standardized list above) |
| View / UI | `participant "Form/ViewName" as view` | `view` | Razor views, frontend pages (`.cshtml`, HTML) |
| Redirect Target | `participant "RedirectForm" as r_view` | `r_view` | Target view after redirection |
| Frontend Controller | `participant "ControllerName" as fe_ctrl <<frontend controller>>` | `fe_ctrl` | Standard MVC controller handling client requests |
| Backend Controller | `participant "ControllerName" as be_ctrl <<backend controller>>` | `be_ctrl` | API Controller exposing REST services |
| Service | `participant ":ServiceClass" as sv` | `sv` | Business logic service (e.g. `:CourseService`) |
| Data Transfer / Response | `participant ":ResponseDto" as res` | `res` | Response or DTO class |
| Entity | `participant ":EntityClass" as ett` | `ett` | Domain entity class |
| Repository | `participant ":RepositoryClass" as repo` | `repo` | Data access repository class |
| DB Context | `participant ":AppDbContext" as ctx` | `ctx` | Entity Framework DB context |
| Database | `database Database as db` | `db` | The physical database storage (cylinder) |

### Modeling Rules
- **Controller Stereotypes:** Frontend controllers and Backend controllers MUST be explicitly distinguished using PlantUML stereotypes (`<<frontend controller>>` and `<<backend controller>>`) directly in their participant declarations.
- **Implementations Only:** Do not model interfaces (e.g., use `:CourseRepository` rather than `ICourseRepository`).
- **Instance Prefixing:** Prefix class instances with a colon (e.g., `:CourseService`, `:CourseRepository`). Do not prefix views, controllers, or actor aliases.
- **Visuals:** All participants except actors (stick figure) and databases (cylinder figure) must be drawn as rectangles.

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
- `->` (Solid Arrow): Synchronous calls, requests, redirects, or internal method invocation.
- `-->>` (Dashed Arrow): Returns, responses, thrown exceptions, or messages displayed to the actor.

---

## 5. Lifeline Activation and Deactivation

Correctly managing activation (`++`) and deactivation (`--`) is critical to show when an object is executing or waiting.

### General Rules
1. **Initial Call Activation (`++`):** Append `++` to the receiving participant when it receives its first call.
   - Example: `fe_ctrl -> be_ctrl++: Send POST request`
2. **Return Deactivation (`--`):** Deactivate a participant's lifeline when it returns or throws an exception.
   - Example: `be_ctrl -->> fe_ctrl--: Return statusCode 200`
3. **Completeness:** Every participant's lifeline (excluding the actor) must be fully deactivated by the time the flow finishes its execution paths.

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
- **Flat Structures:** In fail-fast flat structures, long-running trailing lifelines (`sv`, `be_ctrl`, `fe_ctrl`, `view`, `act`) MUST NOT be deactivated (`--`) in the early exception/validation branches. They must be trailed entirely down the diagram and deactivated **only** in the final failure branch visually rendered at the bottom (typically the `numberOfRowsAffected = 0` branch inside the nested database check).
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
- **Local Lifelines:** Lifelines activated entirely *inside* an `alt` branch (e.g., `db`, `ctx`, `ext_repo` inside the valid flow). If their flow terminates inside a nested `alt` block, they must strictly follow the visual deferral rule: **only the final branch of that nested `alt`** is responsible for deactivating them. Do not deactivate them in earlier branches (e.g., normal flow), otherwise PlantUML will end their activation bars early and draw them as thin lines in the subsequent exception branches.
- **Trailing/Main Lifelines:** Lifelines activated *before* the outermost `alt` block began (e.g., `sv`, `be_ctrl`). These must **never** be manually deactivated inside inner exception branches (e.g., do not place `sv--` in an inner exception block). They must strictly obey the rule above and only be deactivated in the final branch of the outermost `alt`.

---

## 6. Flow Logic Guidelines

### Mapping Use Case to Diagram
- When creating, fixing, editing, updating, validating, or modifying a diagram based on a use case specification, follow this strict mapping:
  - **Alternative flows** must be modeled as `opt` fragments.
  - **Exceptions** must be modeled as `alt` fragments.
- **Form Input Validation (Alt Flow):** The invalid form input alternative flow could be just a client-side check on the view, or it could include an additional model state check in the frontend controller. If a diagram already models these two cases separately, keep them as they are.
- **Session Invalid Exception:** You can assume that "session invalid" is never going to happen. Skip that exception and only model the rest.

### Conditional Branches (Alt/Opt)
- `alt` branch conditions must be mutually exclusive. Always use clear, opposing labels (e.g., `numRows > 0` vs `numRows = 0`, `Valid` vs `Invalid`, `Duplicate` vs `No Duplicate`).
- **Read vs Write Branching Conditions:** 
  - For **Write/Mutate Operations** (Insert/Update/Delete), use `numberOfRowsAffected > 0` (or similar DB-level checks) as the core condition.
  - For **Read/Select Operations**, `alt` blocks should trigger based on data presence (e.g., `Entity == null` vs `Entity found`) or authorization states (e.g., `Unauthenticated`, `Unauthorized Access`).
- **Prefer Flat Structures:** Whenever possible, prefer a flat `alt/else` structure over deep nesting to improve diagram readability. You may reorder the logical flow in the diagram to make a flat structure work sensibly (e.g., grouping early validation failures as `else` branches, and placing the successful execution path in a final `else All checks pass` branch).
- **Semantic Triggers for Flat Structures:** The conditional trigger (the self-call `sv->sv++`) immediately preceding a flat `alt` fragment must have a broad, semantic label covering *all* nested checks (e.g., `Validate course update request`). Furthermore, the final success branch of the `alt` block must explicitly pair with this trigger (e.g., `else Valid course update request`).
- **Condensing Validation Logic:** Encourage condensing fine-grained, related validation checks into unified `else` branches. For example, combine a "course ownership check" and an "instructor lockout check" into a single `else Invalid Instructor Rights` branch to reduce visual noise.

### Abstracting Auxiliary Logic & Fetches
- Verbose inline code blocks (like `foreach` loops) or secondary read operations (like fetching overall category lists, wishlist statuses, or generic stats when the main focus is a course list) should be aggressively abstracted into descriptive self-calls, but **only** if they apply to *auxiliary* changes/fetches outside the primary context. For example, in an operation for Courses, fetching/modifying Lessons is auxiliary and should be abstracted. In an operation for Lessons, modifying Courses or Learning Materials would be auxiliary. These self-calls should use natural language (e.g., `sv->sv++: Remove lessons in course` or `fe_ctrl->fe_ctrl++: Fetch overall performance statistics`).

### Frontend Authentication/Authorization Validation
- Complex authentication and authorization checks (e.g., checking cookies, roles, or calling backend APIs via action filters to validate access) should be abstracted into a single self-call at the frontend controller level (e.g., `fe_ctrl -> fe_ctrl++: Validate Dashboard Access`), immediately followed by a flat `alt` block handling the rejection branches (Login redirect, Not Found redirect, etc.).

### Form Validation
- Standard client-side UI input validations must be shown in the UI/view first.
- Only show server-side controller `Validate ModelState` if it is explicitly written in the controller code.

### Controller Return Messages
- Always return `statusCode` for interactions between the backend and frontend, and the frontend and view. Keep the exact message template as `Return statusCode ...` (e.g., `fe_ctrl -->> view: Return statusCode 302`). Do not use custom phrases like `Redirect to /Cart` as a return message from a controller.
  - Example: `be_ctrl -->> fe_ctrl: Return statusCode 201`
  - Example: `fe_ctrl -->> view: Return statusCode 400`
- **Nuance Rule (FE vs BE Status Codes):** Do not blindly assume that `fe_ctrl` always translates backend errors into a `302`. You MUST look at the exact implementation of the `fe_ctrl` method. Some methods return `RedirectToAction` (302), others might return JSON (for AJAX calls), and others might return a `View` (200). The `Return statusCode` in the diagram must reflect the actual code behavior.
- Do not return generic `JSON (success=true/false)` messages for these steps unless explicitly returning `Json(...)` in code.

### Void and Task<void> Returns
- Whenever modeling a call to a backend method that returns `Task<void>` or `void`, the return step in the sequence diagram must simply be modeled as `Return result` (e.g., `repo -->> sv--: Return result`).

### UI Feedback and Redirections
- Do not over-specify UI implementation details (e.g., "popup message", "toast", "simple p tag"). Use generic interaction descriptions like `Display success message` or `Display error message`.
- If an action ends with displaying a success/error message and requires manual user intervention to proceed (e.g., clicking "Edit Now" to redirect), **do not model the subsequent manual click or redirect**. End the sequence path at the display of the initial message.
- **Automatic Redirections Only**: Only include a redirection step when it occurs automatically without manual interaction from the actor. Model redirects directly from the Controller to the target View using a solid arrow (`->`), followed by the target view displaying and returning to the actor.
  ```plantuml
  fe_ctrl -> login_view++: Redirect to Login page
  login_view -->> act--: Display Login page
  ```

### Layer and Component Omissions
- Omit `fe_ctrl` if the frontend requests the backend directly (e.g. via direct AJAX/Fetch call without server-side MVC controller routing).
- Omit ViewModels/DTOs from the diagram participants list if they are handled implicitly by ASP.NET Core binding and not explicitly manipulated in custom controller/service logic.

### Database Operations (Insert/Update/Delete)
- Assume that calls to `SaveChangesAsync()` at the repository level return an `int` variable named `numberOfRowsAffected` representing the number of mutated database rows.
- Use `numberOfRowsAffected > 0` as the branching condition for the successful path vs. the failed path.
- **Standardizing Check Trigger:** The conditional trigger for verifying database saves must be cleanly labeled as `Check numberOfRowsAffected`. Do not include the logical condition (like `> 0`) in the trigger label itself; leave the specific condition evaluations (`> 0` vs `= 0`) entirely to the `alt` branches.
- In the failed path (`numberOfRowsAffected = 0`), show that the service throws an `InvalidOperationException` and the controllers catch and return a `400 BadRequest`.
- **Database Exceptions:** Any exceptions that occur during database save operations (e.g., `DbUpdateException` for duplicate content) must be modeled as an `alt` fragment starting immediately after the save to database call.

### Distinct Return Paths
- Paths for `alt` branches must remain separate from their entry point down to the final return to the actor. Even if two branches display a similar status code or error popup, they represent separate paths and must deactivate their lifelines independently.

---

## 7. Reference Templates

To see the standard, working PlantUML sequence diagrams depicting flows with these guidelines, please refer to the `.plantuml` files in the `references/` directory within this skill folder.

Always study these reference examples before generating a new sequence diagram code to ensure your output strictly follows the established patterns, tone, and granularity.