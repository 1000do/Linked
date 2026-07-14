---
name: mermaid_class_diagram
description: Guides the generation, modification, and translation (from PlantUML) of standardized, high-quality Mermaid class diagrams for application modules following strict project design and styling guidelines.
---

# Mermaid Class Diagram Generation Skill

This skill provides comprehensive instructions, architectural guidelines, styling rules, and concrete examples for creating and refactoring Mermaid class diagrams. It ensures diagrams are technically accurate, visually consistent, and represent class definitions (fields, properties, methods, interfaces, and database contexts) according to the project's class diagram standard.

> [!IMPORTANT]
> **PlantUML Translation Requirement**
> If the user's input contains a PlantUML class diagram (either via a tagged file or directly pasted code), your primary task is to **translate the PlantUML diagram into the Mermaid format**. You ensure the translated Mermaid diagram strictly adheres to the rules, styling, and formatting guidelines defined in this skill document (e.g., using `~` for generics, formatting interfaces with asterisks, applying allowed relationships, and using the minimalist style definition). Do not perform a mere 1-to-1 syntax swap; adapt the diagram to fully comply with this skill's standards.

> [!IMPORTANT]
> **Output Location and Naming Convention**
> All generated Mermaid class diagrams are exported and saved to the `diagrams/class_mermaid` directory within the workspace. The file naming convention must follow the format `cls_mm_use_case_name` or `cls_mm_functionality_name` with a `.mmd` extension. For example, a diagram for creating a course must be named `cls_mm_create_course.mmd`.

---

## 1. General Participants (Stereotypes & Components)

Depending on the specific use case, a class diagram may include:
- **Frontend Controller**: MVC or API controllers in the frontend layer.
- **ViewModel**: Client-side or UI-focused data models.
- **Backend Controller**: API controllers handling backend routes.
- **Service** & **IService**: Business logic implementation and its corresponding interface.
- **DTO (Data Transfer Object)**: Structured objects for data transfer between layers (e.g., Request, Response, Command/Result DTOs).
- **Repository** & **IRepository**: Data access logic and its corresponding interface.
- **AppDbContext**: EF Core / Database context class.
- **Entity**: Domain or database entities.
- **Hub**: SignalR hub handling real-time communications.
- **Exclude**: Exclude any framework-native, external library, or programming-language-native participants (e.g., `ApiClient`, `List`, `Dictionary`, `IMapper`, `HttpClient`, etc.). Additionally, if a class or interface is referenced but would be drawn without any methods or attributes, skip it entirely (do not include it as a field and do not draw relationship arrows to it).

---

## 2. Allowed Relationships & Syntax

Use the following relationship connections exclusively. Do not use any custom or standard notations not listed here:

| Relationship Type | Mermaid Syntax | Direction/Meaning |
| :--- | :---: | :--- |
| **Dependency** | `..>` | Points *towards* the class being depended on (e.g., `A ..> B`) |
| **Unidirectional Association** | `-->` | Points *towards* the reference target (e.g., `A --> B`) |
| **Bidirectional Association** | `--` | Standard reference link with no specific arrow direction |
| **Aggregation** | `o--` | Part-of relationship; hollow diamond on the container side (e.g., `Container o-- Part`) |
| **Composition** | `*--` | Strong ownership; filled diamond on the owner/container side (e.g., `Owner *-- Component`) |
| **Realization (Implementation)**| `..\|>` | Standard interface implementation; hollow arrow pointing to interface (e.g., `Implementation ..\|> IInterface`) |
| **Inheritance** | `--\|>` | Standard class inheritance; hollow arrow pointing to base class (e.g., `Child --\|> Base`) |

> [!IMPORTANT]
> - **Multiplicity / Cardinality**: Exclude all multiplicity labels or cardinality text on relationships. Leave relationships clean of cardinality.
> - **No Relationship Labels / Text**: Exclude all text, description, or label on relationship lines (e.g., do not use `: uses`, `: calls`, `: creates`, `: queries`, etc.). Leave all relationships completely clean of text.
> - **No PK / FK Markers**: Exclude database key markers like `<<PK>>` or `<<FK>>` in entity classes.

---

## 3. Detailed Relationship Assignment Rules

To maintain consistency across diagrams, assign relationships strictly according to these contextual rules:

### A. Aggregation (`o--`) & Composition (`*--`)
- **Strict Limit**: Use aggregation and composition for relationships between:
  - An **Entity** and another **Entity**
  - A **ViewModel** and another **ViewModel**
  - A **DTO** and another **DTO**
- *Example*: If `Order` entity owns a collection of `OrderItem` entities, use composition: `Order *-- OrderItem`.
- **Fallbacks**: If a strict lifecycle dependency (ownership) does not exist between two Entities, use a **Bidirectional Association** (`--`) for their navigation properties. Only use Composition (`*--`) for strict parent-child relationships where the child cannot exist without the parent (e.g., `Order *-- OrderItem`).

### B. Dependency (`..>`)
- A **Dependency** relationship (`ClassA ..> ClassB`) indicates that Class A relies on Class B, but **Class B CANNOT be a field attribute of Class A (and vice versa)**. If it is a field, it must be mapped as an Association (`-->`).
- **Strict 3-Case Rule**: Class A has a dependency relationship to Class B only if one of these 3 conditions is met:
  1. Class A **instantiates** or **populates** an object of Class B inside its methods.
  2. Class A **accepts** an object of Class B as an argument in its methods.
  3. Class A **returns** an object of Class B to its callers.
- This dependency rule (`..>`) applies universally to **ViewModels**, **DTOs**, and **Entities**. If a class (e.g., a Frontend Controller) constructs, populates, or returns a specific ViewModel, there is a dependency relationship (`..>`) drawn from the Frontend Controller to that ViewModel. Do not omit ViewModels that are mapped and returned to Views!
- **Service-to-Entity Constraints based on Use Case Type**:
  - **INSERT (Create) Use Cases**: A Service has a direct **Dependency** on an Entity (e.g., `Service ..> Entity`) because the service instantiates the entity.
  - **UPDATE / DELETE / SELECT (Read) Use Cases**: A Service does not have a direct dependency on an Entity. Entities that are merely retrieved from a Repository (e.g., `var entity = await repo.GetByIdAsync()`) and used for validation or mapping within the Service do not constitute a Dependency (`..>`) for the Service. The Service only depends on the Entity if it explicitly instantiates it. Therefore, for these use cases, the Service only depends on the DTO (`Service ..> DTO`), and the Repository depends on the Entity (`Repository ..> Entity`).
- Additionally, use a **Dependency** arrow from Frontend Controller to Backend Controller if Frontend Controller participates in the use case.

### C. Unidirectional Association (`-->`)
- Use a **Unidirectional Association** pointing *towards* the interface or DB context (i.e., **IService / IRepository / AppDbContext**) for relationships where these types are held as private fields/properties (e.g., dependency injection).
- *Triggers*: A controller, service, or repository holds an instance of `IService`, `IRepository`, or `AppDbContext` as a member field.
- *Example*: `CourseController --> ICourseService` (Controller references its service interface).

### D. Realization (`..|>`) and Inheritance (`--|>`)
- Apply standard object-oriented rules:
  - Use `..|>` when a class implements an interface (e.g., `CourseService ..|> ICourseService`).
  - Use `--|>` when a class inherits from a base class (e.g., `InstructorCourseController --|> Controller`).

---

## 4. Best Practices & Styling

- **Standard Layout**: Start your diagram with `classDiagram` or `classDiagram-v2` followed by `direction LR` on the next line to render the diagram from left to right.
- **Namespaces / Packages**: Do not use namespaces/packages.
- **Members Representation & Signatures**:
  - **Field Attributes**: Include private field attributes related to the functionalities, formatted as `visibility attributeName : Interface/Class/Data Type` (e.g., `- _courseRepository : ICourseRepository`).
  - **Full Method Signatures**: Provide complete signatures for methods, specifying visibility (`+` for public, `-` for private, `#` for protected), arguments with their types, and the return type (e.g., `+ CreateCourseAsync(request: CourseCreateRequest, instructorId: int) Task~CourseResponse~`). Double-check the **EXACT** method signatures from the interface and class definitions in the codebase. Verify parameter names (e.g., `lessonId` vs `id`), exact collection types (e.g., `List` vs `IEnumerable`), and asynchronous return types (e.g., `Task~int~` vs `Task`). The diagram must be a strictly accurate 1:1 reflection of the code contracts.
  - **Interface and Implementation Consistency**: Ensure consistency of public method signatures between an interface and its implementation class. If a service implements an interface, the service class block contains the exact same set of public methods with the exact same signatures as the interface it implements. Additionally, the implementation class may contain private helper methods if required by the execution flow, but the public contract must be identical.
  - **Generics**: In Mermaid, you use tildes (`~`) instead of angle brackets (`<` `>`) for generic types inside method signatures or property types to avoid parsing errors. For example, use `Task~CourseResponse~` instead of `Task<CourseResponse>`, and `List~CategoryViewModel~` instead of `List<CategoryViewModel>`. For custom generic wrapper classes (such as `PagedResult<T>`, `ApiResponse<T>`, `BaseApiResponse<T>`), do not create a class block for the generic wrapper itself. Instead, drop the wrapper from the diagram for simplicity and draw the dependency (`..>`) or association (`-->`) directly to the inner wrapped type (e.g., the ViewModel, DTO, or Entity). When dropping a wrapper like `PagedResult<T>`, any parent class containing it (e.g., `ReportModerationPageViewModel` having `CourseReports: PagedResult<CourseReportDetailViewModel>`) establish a direct composition (`*--`) or dependency relationship to the inner wrapped type (`CourseReportDetailViewModel`), ensuring the relationship integrity is maintained visually.
  - **Concrete Stereotypes**: Classify every single class or interface in the diagram with a specific, concrete stereotype annotation on the first line inside the class block when applicable. If a special class doesn't fit any category, you may leave the stereotype blank, but follow these general heuristics:
    - **Service Interface** (`<<service interface>>`): Name usually starts with `I` and ends with `Service` (e.g., `ICourseService`), with special cases like `IBackgroundTaskQueue`. In general, any interface located under `Application/IServices` is a service interface.
    - **Service Implementation** (`<<service>>`): Name usually ends with `Service` and does not start with `I`, with special cases like `BackgroundTaskQueue`. In general, classes located under `Application/Services`, `Infrastructure/Services`, or `Infrastructure/BackgroundServices` are services.
    - **Repository Interface** (`<<repository interface>>`): Name starts with `I` and ends with `Repository`. Located under `Domain/IRepositories`.
    - **Repository Implementation** (`<<repository>>`): Name ends with `Repository` and does not start with `I`. Located under `Infrastructure/Repositories`.
    - **Database Context** (`<<dbcontext>>`): `AppDbContext`, located under `Infrastructure/Data`.
    - **SignalR Hub** (`<<hub>>`): Name ends with `Hub`, located under `Hubs`.
    - **Data Models**: Use `<<entity>>`, `<<dto>>`, or `<<view model>>` respectively.
    - **Controllers**: Use `<<frontend controller>>` or `<<backend controller>>`.
  - **Interface Methods**: All methods inside an interface are explicitly marked as abstract by appending an asterisk (`*`) at the end of the signature to render them in italics (e.g., `+ CreateCourseAsync(...) Task~CourseResponse~ *`).
  - **Relevance**: For Controllers, Services, Repositories, and AppDbContext, keep attributes and methods clean and relevant to the specific use case to avoid clutter.
    - Identify methods that contain the main execution flow (usually public methods defined in the interface contracts).
    - Thoroughly inspect the implementation body of these core methods. Do not just look at the method parameters. If the method instantiates, fetches, or updates entities from secondary repositories (e.g., calling `_lessonRepository.Update(lesson)` inside a `CourseService`), you include those secondary entities, their respective repositories, and any additional injected services utilized within the method's workflow.
    - Only include dependencies and their corresponding private fields if they are directly implemented or referenced in these main public methods.
    - If the main public methods use private helper methods, **keep the private helper methods documented in the class block**, but **skip drawing any external dependencies** (e.g., injected repository interfaces, entities) that are exclusively utilized by those private helpers to reduce relationship clutter.
  - **Data Models (ViewModels, DTOs, Entities)**: If a participant is a ViewModel, DTO, or Entity, you include all of its fields and properties in the class block. This must be a strict 1:1 property mapping reflecting the actual C# source code. Furthermore, if any of those properties map to another ViewModel, DTO, or Entity that is ALSO present in the diagram, you explicitly connect them in the Relationships section using an appropriate relationship (e.g., `Account -- Lockout`).
  - **Shared Name Resolution**: If a Frontend Controller and a Backend Controller share the exact same class name, you define them properly aliased into separate blocks (using `class Alias["ActualName"]`, e.g., `class AdminAiServiceControllerFE["AdminAiServiceController"]`) instead of making non-existing names for them. Explicitly add the stereotypes `<<frontend controller>>` and `<<backend controller>>` inside their blocks respectively to differentiate them. Similarly, if a ViewModel and a DTO share the exact same name, define them separately and add the stereotypes `<<view model>>` and `<<dto>>`.
- **Styling and Colors**:
  - The diagram should have a simplified, minimalist aesthetic: white background, white fill, black borders, and black text.
  - At the end of the diagram, define a single default style:
    ```mermaid
    classDef default fill:#FFFFFF,stroke:#000000,stroke-width:1px,color:#000000
    ```

---

## 5. Sequence Diagram Cross-Validation & Refactoring Triggers

When creating or validating a class diagram based on a corresponding sequence diagram:
- **Completeness**: The class diagram contains all classes, interfaces, and methods used in its respective sequence diagram.
- **Constraint Handling**: This cross-validation respects the strict rules of this class diagram skill (e.g., omitting generic wrappers like `PagedResult`).
- **Refactoring Suggestion**: If a sequence diagram contains a participant or method that violates the class diagram rules (for example, having a `participant ":PagedResult<X>"`), you do not inject it into the class diagram. Instead, you explicitly suggest to the user that they refactor the sequence diagram to align with the class diagram standards (e.g., stripping the wrapper in the sequence diagram).

---

## 6. Complete Reference Templates

For examples of standard, working Mermaid class diagrams depicting various use cases, please refer to the `.mmd` files located in the `references/` directory within this skill folder.

---

## 7. Verification Checklist and Workflow

Every time you are asked to create, validate, refactor, or edit a class diagram, you strictly follow this verification workflow to ensure no rules are missed.

### The Checklist

**1. General & Output**
- [ ] **Output Directory:** Are diagrams saved to `diagrams/class_mermaid`?
- [ ] **Naming Convention:** Is the filename `cls_mm_*.mmd`?
- [ ] **PlantUML Translation:** If translating from PlantUML, is the output fully adapted to Mermaid syntax and these rules rather than a 1:1 syntax swap?

**2. Participants & Exclusions**
- [ ] **Valid Participants:** Are only valid participants included? (Specifically: Frontend/Backend Controllers, ViewModels, Services/IServices, DTOs, Repositories/IRepositories, AppDbContext, Entities, and Hubs)
- [ ] **Framework/Native Types Excluded:** Are `ApiClient`, `List`, `Dictionary`, `IMapper`, etc., completely excluded?
- [ ] **Empty Participants Excluded:** Are referenced interfaces or classes without fields/methods completely skipped and omitted from relationships?
- [ ] **PagedResult/Wrapper Stripping:** Are custom wrappers (like `PagedResult`, `ApiResponse`) omitted and bypassed directly to the inner type?

**3. Relationships**
- [ ] **Allowed Types Only:** Are relationships strictly limited to `..>`, `-->`, `--`, `o--`, `*--`, `..|>`, and `--|>`?
- [ ] **No Labels/Multiplicity:** Are all relationship lines completely clean of text, labels, and cardinality markers?
- [ ] **Aggregation/Composition (`o--` / `*--`):** Are these used strictly between Entity-Entity, ViewModel-ViewModel, or DTO-DTO? Is composition reserved only for strict parent-child ownership?
- [ ] **Bidirectional (`--`):** Is this used as a fallback for Entities without strict ownership?
- [ ] **Dependency (`..>`):** Are dependencies drawn for instantiation, parameter acceptance, or return types (but NOT when held as a field)? Are they drawn from Frontend Controllers to returned ViewModels?
- [ ] **Service-to-Entity Constraints:** For Read/Update/Delete cases, does the Service depend ONLY on the DTO, avoiding direct dependency on the Entity unless it instantiates it?
- [ ] **Unidirectional Association (`-->`):** Is this used for DI fields (pointing towards `IService`, `IRepository`, `AppDbContext`)?
- [ ] **Realization/Inheritance:** Are `..|>` and `--|>` correctly applied for interfaces and base classes?

**4. Member Formatting & Signatures**
- [ ] **Field Attributes:** Are injected private fields formatted properly (e.g., `- _repo : IRepository`)?
- [ ] **Full Method Signatures:** Do public methods include visibility, argument names/types, and exact async return types? Do they strictly match the code?
- [ ] **Interface & Implementation Parity:** Does the implementing class have the exact same public method signatures as the interface?
- [ ] **Generics (`~`):** Are tildes (`~`) used instead of angle brackets (`< >`) for generic types?
- [ ] **Interface Abstract Marker:** Are all methods in an interface suffixed with an asterisk (`*`) to italicize them?
- [ ] **Primary Key/Foreign Key Markers:** Are database markers like `<<PK>>` or `<<FK>>` completely excluded?

**5. Stereotypes & Data Models**
- [ ] **Concrete Stereotypes:** Does every class/interface block define its stereotype on the first line (e.g., `<<service>>`, `<<entity>>`, `<<dto>>`)?
- [ ] **Data Model Completeness:** Do ViewModels, DTOs, and Entities include ALL of their properties (strict 1:1 reflection of code)?
- [ ] **Data Model Connectivity:** If a property maps to another data model present in the diagram, is an explicit relationship drawn between them?
- [ ] **Shared Name Resolution:** If classes share a name (e.g., Frontend/Backend controllers), are they properly aliased into separate blocks (using `class Alias["ActualName"]`) instead of inventing non-existing names for them?

**6. Relevance & Sequence Diagram Cross-Validation**
- [ ] **Main Execution Flow:** Are only the core public methods (and their required private helpers) included for Controllers, Services, and Repos?
- [ ] **Secondary Workflow Dependencies:** If a main method calls a secondary repository (e.g., a CourseService updating a Lesson), are those secondary repositories/entities correctly included?
- [ ] **Private Helper Trimming:** Are external dependencies used exclusively by private helper methods omitted from the relationship lines to reduce clutter?
- [ ] **Cross-Validation:** Does the class diagram contain everything present in a corresponding sequence diagram? Are wrappers stripped to comply with these rules?

**7. Styling**
- [ ] **Direction:** Does it start with `direction LR`?
- [ ] **Default Style:** Is the exact minimalist default style definition (`classDef default...`) placed at the bottom?
