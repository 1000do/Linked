---
name: method_extraction
description: "Specializes in method/function extraction for separation of concerns. Ensures methods do exactly one job. Extracts logic into distinct methods and groups common behaviors into new classes or interfaces as needed."
---

# Method Extraction Skill

This skill guides the agent in refactoring code to adhere strictly to the Single Responsibility Principle (SRP) by extracting complex, multi-purpose methods into smaller, focused functions, and moving shared behaviors into entirely new classes or interfaces.

## Goals
1. **Separation of Concerns**: Ensure every method or function does exactly one job.
2. **Readability & Maintainability**: Improve code readability by reducing method length and cognitive complexity.
3. **Reusability**: Identify common behaviors and extract them into cohesive shared classes or interfaces.
4. **Behavior Preservation**: Ensure the original behavior and functionality of the system remains identical after extraction.

## Refactoring Process

When asked to use this skill, follow these systematic steps:

### Step 1: Analyze the Target Code
- Read the target method or class completely to fully grasp its control flow, variable scoping, and dependencies.
- Identify discrete "jobs" or "responsibilities" within the method (e.g., input validation, data fetching, data processing, external API calls, database updates, logging).
- Map the data flow: note which local variables are required by each block of code and what state they mutate. These dictate the parameters and return types of your extracted methods.

### Step 2: Extract to Local Methods (First Pass)
- For each discrete job, extract the block of code into a private, highly descriptive helper method within the same class.
- Pass required variables as arguments.
- If multiple values must be returned to the calling method, utilize Tuples (e.g., in C# or Python) or distinct data structures (DTOs).
- Replace the original code block with a clean, readable call to the newly extracted method.
- Ensure the original method is simplified into a clear orchestrator of these high-level steps.

### Step 3: Identify and Group Common Behaviors (Second Pass)
- Review the newly extracted methods alongside the existing methods in the class.
- Determine if multiple methods share common behaviors, rely on the same subset of dependencies, or exclusively manipulate the same entity (e.g., purely caching logic, purely external integration logic).
- If common behaviors are identified:
  1. Create a new distinct Class (and an abstraction Interface, if applicable to the architecture) dedicated to this responsibility.
  2. Move the grouped methods into this new class.
  3. Refactor the original class to consume this new Class/Interface (e.g., via constructor Dependency Injection).
  4. Delegate the original responsibilities to the newly injected service.

### Step 4: Verification
- Carefully review the new code structure to ensure no scoping issues or syntax errors were introduced.
- Verify that asynchronous execution context (`async`/`await`) is strictly preserved across extractions.
- Run the compiler or linter (e.g., `dotnet build`) to prove that all references, return types, and access modifiers are robust and correct.

## Best Practices
- **Naming Conventions**: Extracted methods must have precise, verb-driven names that perfectly describe their single job (e.g., `GetCourseMaterialIdsAsync`, `ValidateUserPermissions`).
- **Cohesion over Fragmentation**: Only create a new class if the extracted methods form a highly cohesive unit of functionality. Avoid creating dumping grounds (like generic `Utils` or `Helpers` classes). Name classes after their explicit domain responsibility (e.g., `CourseIntegrationService`, `RedisCacheManager`).
- **Purity**: Where possible, extract logic into "pure" functions that take explicit inputs and return explicit outputs without hidden side effects on class-level state.
