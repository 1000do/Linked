---
name: plantuml_class_diagram
description: Guides the generation and modification of standardized, high-quality PlantUML class diagrams for application modules following strict project design and styling guidelines.
---

# PlantUML Class Diagram Generation Skill

This skill provides comprehensive instructions, architectural guidelines, styling rules, and concrete examples for creating and refactoring PlantUML class diagrams. It ensures diagrams are technically accurate, visually consistent, and represent class definitions (fields, properties, methods, interfaces, and database contexts) according to the project's class diagram standard.

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
- **Exclude**: Exclude any framework-native, external library, or programming-language-native participants (e.g., `ApiClient`, `List`, `Dictionary`, `IMapper`, `HttpClient`, etc.). Additionally, if a class or interface is referenced but would be drawn without any methods or attributes, **skip it entirely** (do not include it as a field and do not draw relationship arrows to it).

---

## 2. Allowed Relationships & Syntax

Use **ONLY** the following relationship connections. Do not use any custom or standard notations not listed here:

| Relationship Type | PlantUML Syntax | Direction/Meaning |
| :--- | :---: | :--- |
| **Dependency** | `..>` | Points *towards* the class being depended on (e.g., `A ..> B`) |
| **Unidirectional Association** | `-->` | Points *towards* the reference target (e.g., `A --> B`) |
| **Bidirectional Association** | `--` | Standard reference link with no specific arrow direction |
| **Aggregation** | `o--` | Part-of relationship; hollow diamond on the container side |
| **Composition** | `*--` | Strong ownership; filled diamond on the owner/container side |
| **Realization (Implementation)**| `..|>` | Standard interface implementation; hollow arrow pointing to interface |
| **Inheritance** | `--|>` | Standard class inheritance; hollow arrow pointing to base class |

> [!IMPORTANT]
> - **Multiplicity / Cardinality**: DO NOT include any multiplicity labels or cardinality text on relationships (e.g., do not use `class1 "1" --> "*" class2`). Leave relationships clean of cardinality.
> - **No Relationship Labels / Text**: DO NOT include any text, description, or label on relationship lines (e.g., do not use `: uses`, `: calls`, `: creates`, `: queries`, etc.). Leave all relationships completely clean of text.
> - **No PK / FK Markers**: DO NOT include database key markers like `<<PK>>` or `<<FK>>` in entity classes.

---

## 3. Detailed Relationship Assignment Rules

To maintain consistency across diagrams, assign relationships strictly according to these contextual rules:

### A. Aggregation (`o--`) & Composition (`*--`)
- **Strict Limit**: Use aggregation and composition **ONLY** for relationships between:
  - An **Entity** and another **Entity**
  - A **ViewModel** and another **ViewModel**
  - A **DTO** and another **DTO**
- *Example*: If `Order` entity owns a collection of `OrderItem` entities, use composition: `Order *-- OrderItem`.
- **Fallbacks**: If composition/aggregation is not appropriate for these types, use **Unidirectional** (`-->`) or **Bidirectional** (`--`) Association.

### B. Dependency (`..>`)
- A **Dependency** relationship (`ClassA ..> ClassB`) indicates that Class A relies on Class B, but **Class B CANNOT be a field attribute of Class A (and vice versa)**. If it is a field, it must be mapped as an Association (`-->`).
- **Strict 3-Case Rule**: Class A ONLY has a dependency relationship to Class B if one of these 3 conditions is met:
  1. Class A **instantiates** an object of Class B inside its methods.
  2. Class A **accepts** an object of Class B as an argument in its methods.
  3. Class A **returns** an object of Class B to its callers.
- **Service-to-Entity Constraints based on Use Case Type**:
  - **INSERT (Create) Use Cases**: A Service has a direct **Dependency** on an Entity (e.g., `Service ..> Entity`) because the service instantiates the entity.
  - **UPDATE / DELETE / SELECT (Read) Use Cases**: A Service **DOES NOT** have a direct dependency on an Entity. Entities are queried, updated, or deleted by the Repository layer. The Repository returns the Entity (or DTO) to the Service. Therefore, for these use cases, the Service only depends on the DTO (`Service ..> DTO`), and the Repository depends on the Entity (`Repository ..> Entity`).
- Additionally, use a **Dependency** arrow from Frontend Controller to Backend Controller if Frontend Controller participates in the use case.

### C. Unidirectional Association (`-->`)
- Use a **Unidirectional Association** pointing *towards* the interface or DB context (i.e., **IService / IRepository / AppDbContext**) for relationships where these types are held as private fields/properties (e.g., dependency injection).
- *Triggers*: A controller, service, or repository holds an instance of `IService`, `IRepository`, or `AppDbContext` as a member field.
- *Example*: `CourseController --> ICourseService` (Controller references its service interface).

### D. Realization (`..|>`) and Inheritance (`--|>`):
- Apply standard object-oriented rules:
  - Use `..|>` when a class implements an interface (e.g., `CourseService ..|> ICourseService`).
  - Use `--|>` when a class inherits from a base class (e.g., `InstructorCourseController --|> Controller`).

---

## 4. Best Practices & Styling

- **Modern Clean Theme**: Use standard PlantUML styling directives at the top of the file for professional aesthetics:
  ```plantuml
  ' Styling & Settings
  skinparam style strictuml
  skinparam shadowing false
  skinparam classAttributeIconSize 0
  skinparam class {
      BackgroundColor White
      BorderColor #1A73E8
      ArrowColor #5F6368
  }
  ```
- **Namespaces / Packages**: Do not use namespaces/packages.
- **Members Representation & Signatures**:
  - **Field Attributes**: Include private field attributes related to the functionalities, formatted as `visibility attributeName : Interface/Class/Data Type` (e.g., `- _courseRepository : ICourseRepository`).
  - **Full Method Signatures**: Provide complete signatures for methods, specifying visibility (`+` for public, `-` for private, `#` for protected), arguments with their types, and the return type (e.g., `+ CreateCourseAsync(request: CourseCreateRequest, instructorId: int): Task<CourseResponse>`).
  - **Interface Content Styling**: For interfaces, write all internal contents (methods, properties, attributes) entirely in italics. In PlantUML, format them with double slashes (e.g., `//+ CreateCourseAsync(request: CourseCreateRequest, instructorId: int): Task<CourseResponse>//`).
  - **Relevance**: For Controllers, Services, Repositories, and AppDbContext, keep attributes and methods clean and relevant to the specific use case to avoid clutter.
    - Identify methods that contain the main execution flow (usually public methods defined in the interface contracts).
    - Only include dependencies and their corresponding private fields if they are directly implemented or referenced in these main public methods.
    - If the main public methods use private helper methods, and there are dependencies or fields abstracted behind these private helpers, **skip them** for simplicity.
  - **Data Models (ViewModels, DTOs, Entities)**: If a participant is a ViewModel, DTO, or Entity, you **MUST include ALL of its fields and properties** in the class block. Furthermore, if any of those properties map to another ViewModel, DTO, or Entity that is ALSO present in the diagram, you **MUST explicitly connect them** in the Relationships section using an appropriate relationship (e.g., `Account -- Lockout`).
  - **Shared Name Resolution (String Aliasing)**: If a Frontend Controller and a Backend Controller share the exact same class name, you **MUST** use PlantUML's string aliasing syntax to keep the visual name exact while allowing distinct relationships. Define them as separate blocks by placing the exact real name in quotes, followed by `as [InternalAlias]` and their respective stereotype (e.g., `class "AdminAiServiceController" as FrontendController <<frontend controller>>` and `class "AdminAiServiceController" as BackendController <<backend controller>>`). Similarly, if a ViewModel and a DTO share the exact same name, use the string aliasing syntax with `<<view model>>` and `<<dto>>` stereotypes (e.g., `class "CreateAiModelRequest" as CreateAiModelRequestVM <<view model>>`). Do not make up non-existent names for the quoted display name.
- **Standard Colors**:
  - Abstract/Interface: `ABSTRACT_COLOR` `#FFE6CC`
  - Service Implementation: `SERVICE_COLOR` `#CCE5FF`
  - ViewModel/DTO: `DTO_COLOR` `#E6CCFF`
  - Domain Entity: `ENTITY_COLOR` `#CCFFE6`
  - Repository Implementation: `REPOSITORY_COLOR` `#FFFFCC`
  - Controller: `CONTROLLER_COLOR` `#FFE6E6`
  - DbContext: `DbContext` `#E6F3FF`

---

## 5. Complete Reference Template (`create_course.plantuml`)

Below is the standard, working PlantUML class diagram depicting the "Create Course" structure:

```plantuml
@startuml CreateCourse_ClassDiagram

' Styling & Settings
skinparam style strictuml
skinparam shadowing false
skinparam classAttributeIconSize 0
skinparam class {
    BackgroundColor White
    BorderColor #1A73E8
    ArrowColor #5F6368
}

!define ABSTRACT_COLOR #FFE6CC
!define SERVICE_COLOR #CCE5FF
!define DTO_COLOR #E6CCFF
!define ENTITY_COLOR #CCFFE6
!define REPOSITORY_COLOR #FFFFCC
!define CONTROLLER_COLOR #FFE6E6

skinparam backgroundColor #FFFFFF
skinparam classBackgroundColor #F9F9F9
skinparam classBorderColor #333333
skinparam arrowColor #333333

' ==================== FRONTEND ====================

class InstructorCourseController #FFE6E6 {
    + Create() : Task<IActionResult>
    + Create(model: CreateCourseViewModel) : Task<IActionResult>
    - GetCategoriesAsync() : Task<List<CategoryViewModel>>
    - LoadStripeStatusAsync() : Task<void>
    - LoadTransferRateAsync() : Task<void>
}

class CreateCourseViewModel #E6CCFF {
    + Title : string
    + CategoryId : int
    + Description : string
    + Price : decimal
    + CourseThumbnailUrl : string
    + WhatYouWillLearn : string
    + Requirements : string
    + CouponId : int?
    + AvailableCategories : List<CategoryViewModel>
}

class CategoryViewModel #E6CCFF {
    + CategoryId : int
    + CategoriesName : string
}

' ==================== BACKEND - CONTROLLER ====================

class CourseController #FFE6E6 {
    - _courseQueryService : ICourseQueryService
    - _courseCommandService : ICourseCommandService
    - _aiModerationService : ICourseAiModerationService
    
    + CreateCourse(request: CourseCreateRequest) : Task<IActionResult>
    - GetInstructorId() : int
}

' ==================== BACKEND - DTOs ====================

class CourseCreateRequest #E6CCFF {
    + CategoryId : int
    + Title : string
    + Description : string?
    + Price : decimal
    + CourseThumbnailUrl : string?
    + WhatYouWillLearn : string?
    + Requirements : string?
    + ThumbnailFile : IFormFile?
}

class CourseResponse #E6CCFF {
    + CourseId : int
    + InstructorId : int?
    + CategoryId : int?
    + Title : string
    + Description : string?
    + Price : decimal
    + CourseThumbnailUrl : string?
    + CourseStatus : string?
    + CreatedAt : DateTime?
    + UpdatedAt : DateTime?
    + WhatYouWillLearn : string?
    + Requirements : string?
    + InstructorName : string?
    + TotalStudents : int
    + RatingAverage : decimal
    + CategoryName : string?
    + InstructorAvatarUrl : string?
    + InstructorBio : string?
    + InstructorProfessionalTitle : string?
    + InstructorReviewCount : int
    + InstructorStudentsCount : int
    + InstructorCoursesCount : int
    + IsEnrolled : bool
    + IsOwner : bool
    + TotalReviews : int
    + LastApprovedAt : DateTime?
    + AppliedCouponCode : string?
    + AppliedCouponType : string?
    + AppliedCouponValue : decimal?
    + IsRemoved : bool
    + IsInAnyCart : bool
}

' ==================== BACKEND - DOMAIN ENTITIES ====================

class Course #CCFFE6 {
    + CourseId : int
    + InstructorId : int?
    + CategoryId : int?
    + CouponId : int?
    + Title : string
    + Description : string?
    + Price : decimal
    + CourseThumbnailUrl : string?
    + CreatedAt : DateTime?
    + UpdatedAt : DateTime?
    + CourseStatus : string?
    + CourseFlagCount : int?
    + CourseAiIntegrations : ICollection<CourseAiIntegration>
    + CartItems : ICollection<CartItem>
    + Category : Category?
    + Coupon : Coupon?
    + Enrollments : ICollection<Enrollment>
    + Instructor : Instructor?
    + Lessons : ICollection<Lesson>
    + OrderItems : ICollection<OrderItem>
    + WishlistItems : ICollection<WishlistItem>
    + CourseExt : CourseExt?
    + WhatYouWillLearn : string?
    + Requirements : string?
    + ModerationFeedback : string?
    + LastApprovedAt : DateTime?
    + IsRemoved : bool
    + ThreatLevel : AiThreatLevel
}

class Category #CCFFE6 {
    + CategoryId : int
    + CategoriesName : string
    + Description : string?
    + CreatedAt : DateTime?
    + UpdatedAt : DateTime?
    + CategoryStatus : string?
    + Courses : ICollection<Course>
}

class Instructor #CCFFE6 {
    + InstructorId : int
    + ProfessionalTitle : string?
    + ExpertiseCategories : string?
    + LinkedinUrl : string?
    + YoutubeUrl : string?
    + FacebookUrl : string?
    + DocumentUrl : string?
    + ApprovalStatus : string?
    + RejectionReason : string?
    + StripeAccountId : string?
    + StripeOnboardingStatus : string?
    + PayoutsEnabled : bool?
    + ChargesEnabled : bool?
    + StripeCountry : string?
    + Courses : ICollection<Course>
    + InstructorPayouts : ICollection<InstructorPayout>
    + InstructorNavigation : User
}

class Coupon #CCFFE6 {
    + CouponId : int
    + ManagerId : int?
    + CouponCode : string
    + CouponType : string?
    + DiscountValue : decimal
    + MinOrderValue : decimal
    + StartDate : DateTime?
    + EndDate : DateTime?
    + UsageLimit : int?
    + UsedCount : int?
    + IsActive : bool?
    + Courses : ICollection<Course>
    + Manager : Manager?
}

' ==================== BACKEND - SERVICE INTERFACES ====================

interface ICourseCommandService #FFE6CC {
    //+ CreateCourseAsync(request: CourseCreateRequest, instructorId: int) : Task<CourseResponse>//
}

interface ICourseQueryService #FFE6CC {
    //+ GetCategoriesAsync() : Task<IEnumerable<CategoryResponse>>//
}

interface IInstructorRepository #FFE6CC {
    //+ GetByIdAsync(id: int) : Task<Instructor?>//
    //+ SaveChangesAsync() : Task<int>//
}

interface ICourseRepository #FFE6CC {
    //+ AddAsync(course: Course) : Task<void>//
    //+ SaveChangesAsync() : Task<int>//
}

interface IFileUploadService #FFE6CC {
    //+ UploadImageAsync(file: IFormFile) : Task<string?>//
    //+ UploadVideoAsync(file: IFormFile) : Task<string?>//
}

interface IContentHashService #FFE6CC {
    //+ ComputeCourseHashAsync(text: string) : Task<string>//
    //+ SaveCourseHashesAsync(command: SaveCourseHashesCommand) : Task<void>//
}

interface IRedisService #FFE6CC {
    //+ RemoveCacheAsync(key: string) : Task<void>//
}

' ==================== BACKEND - SERVICE IMPLEMENTATION ====================

class CourseCommandService #CCE5FF {
    - _courseRepository : ICourseRepository
    - _instructorRepository : IInstructorRepository
    - _uploadService : IFileUploadService
    - _redisService : IRedisService
    - _contentHashService : IContentHashService
    
    + CreateCourseAsync(request: CourseCreateRequest, instructorId: int) : Task<CourseResponse>
    - UpdateCourseHashesAsync(course: Course) : Task<void>
}

' ==================== BACKEND - REPOSITORY IMPLEMENTATION ====================

class CourseRepository #FFFFCC {
    - _context : AppDbContext
    
    + AddAsync(course: Course) : Task<void>
    + SaveChangesAsync() : Task<int>
}

' ==================== BACKEND - DbContext ====================

class AppDbContext #E6F3FF {
    + Categories : DbSet<Category>
    + Coupons : DbSet<Coupon>
    + Courses : DbSet<Course>
    + Instructors : DbSet<Instructor>
    ---
    # OnConfiguring(optionsBuilder: DbContextOptionsBuilder) : void
    # OnModelCreating(modelBuilder: ModelBuilder) : void
}

' ==================== RELATIONSHIPS ====================

' === COMPOSITION (filled diamond) ===
Instructor *-- Course

' === AGGREGATION (hollow diamond) ===
CreateCourseViewModel o-- CategoryViewModel
Category o-- Course
Course -- Coupon

' === UNIDIRECTIONAL ASSOCIATION (solid arrow) ===
CourseController --> ICourseCommandService
CourseController --> ICourseQueryService
CourseCommandService --> ICourseRepository
CourseCommandService --> IInstructorRepository
CourseCommandService --> IFileUploadService
CourseCommandService --> IContentHashService
CourseCommandService --> IRedisService
CourseRepository --> AppDbContext

' === INTERFACE REALIZATION ===
CourseCommandService ..|> ICourseCommandService
CourseRepository ..|> ICourseRepository

' === DEPENDENCY (dashed arrows) ===
InstructorCourseController ..> CourseController
InstructorCourseController ..> CreateCourseViewModel
CourseController ..> CourseCreateRequest
CourseCommandService ..> CourseCreateRequest
CourseCommandService ..> CourseResponse
CourseCommandService ..> Course
CourseRepository ..> Course

@enduml
```
