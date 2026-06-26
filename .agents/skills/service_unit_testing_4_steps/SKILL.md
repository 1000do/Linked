---
name: service_unit_testing_4_steps
description: Guides the creation of robust unit tests for service interfaces targeting 100% coverage, utilizing a strict 4-step Arrange/Act/Assert layout, coverage verification, and method reference assertions. Enforces testability checks, specific testing frameworks, and an implementation plan phase.
---

# Service Unit Testing (5 Steps)

This skill provides a standardized workflow for generating unit tests for public-facing service interface methods to ensure 100% code coverage. 

## Strict Framework Requirements
You MUST use the following testing frameworks when writing the unit tests:
*   **xUnit**: For test execution and attributes (`[Fact]`, `[Theory]`).
*   **NSubstitute**: For all mocking and method reference assertions (`Substitute.For<T>()`, `.Returns()`, `.Received(1)`).
*   **FluentAssertions**: For all result evaluations (`.Should().Be(...)`, `.Should().ThrowAsync<T>()`).

## Workflow

### 1. Read and Analyze
*   Carefully read the interface defining the public methods.
*   Carefully read the concrete implementation of the service.
*   Identify all execution paths, including success paths, edge cases, and all possible exception throws, to target 100% code coverage.
*   **Empty Branch Hunt**: Explicitly look for conditions checking empty collections (e.g., `!list.Any()`, `if (collection.Count == 0)`) or bypassed `foreach` loops, as these branches are frequently missed during test coverage analysis.

### 2. Testability Audit
*   Evaluate the testability of the implementation methods.
*   If a method is difficult to test (e.g., returns `void` or `Task<void>` and relies heavily on untracked database side-effects), you MUST suggest proper refactoring to make it testable (e.g., changing the return type to `bool`, `Task<bool>`, or returning the tracked entity instead of refetching it).

### 3. Implementation Plan Phase
*   **Do NOT write the tests immediately.**
*   Create an `implementation_plan.md` artifact outlining:
    *   The required changes to make the service testable (if any).
    *   The list of test cases to be written for each method to achieve 100% coverage.
    *   You MUST use the strict `[Method]_[State]_[Outcome]` pattern for your test methods (e.g., `SendAdvancedAsync_NoOtherManagers_SkipsBroadcastingToManagers`). Do not use vague names like "WhenValid" or "ShouldSucceed".
*   Request user feedback by setting `request_feedback = true` and wait for explicit user approval. Do NOT proceed until the user approves.

### 4. Strict 4-Step Test Format
*   Once approved, implement the unit tests.
*   **Bulk Additions**: If you need to inject many new test cases (10+) into a massive existing test file, consider using a PowerShell or Python script to securely append the new cases directly before the final closing brace. This prevents structural corruption (like missing curly braces `CS1513`) caused by large text replacements.
*   **CRITICAL**: You MUST consult the example tests provided in the `references/` directory within this skill folder (e.g., `AdminAiServiceTests.cs`) to study a complete, perfect implementation of this structure before writing code.
*   **CRITICAL**: Every single test MUST strictly follow this exact 4-step structure:
    *   `//Arrange 1`: Setup data objects, inputs, expected outputs, and variables.
    *   `//Arrange 2`: Setup the behavior of mocked dependencies (`.Returns(...)`).
    *   `//Act`: Invoke the method under test.
    *   `//Assert`: Perform assertions on the result/state, handle exception assertions, AND assert method reference invocations (e.g., `.Received(1)` and `.DidNotReceive()`).
        *   **String Matching Warning**: When asserting `.Received(1)` on methods with string arguments (like titles or messages), either perfectly copy the magic strings directly from the implementation source code, or use `Arg.Any<string>()` to prevent brittle tests and `ReceivedCallsException` failures.

### 5. Coverage Validation
*   Do not assume 100% coverage just because the tests pass.
*   Run a coverage tool using a command like `dotnet test --collect:"XPlat Code Coverage"`.
*   Parse the resulting `coverage.cobertura.xml` (using a script or manual inspection) to mathematically verify 100% line coverage for the target service implementation. 
*   **Note**: In C#, coverage for `async` methods often appears under compiler-generated state machine structs (e.g., `<MethodName>d__12`) rather than the parent class. Ensure your parser accounts for this when verifying 100% coverage.

## Structure Example
```csharp
[Fact]
public async Task MethodName_ExtremelyDescriptiveCondition_DescriptiveResult()
{
    //Arrange 1
    var req = new RequestType { Prop = "Value" };
    var expected = new ExpectedType { Output = "Result" };

    //Arrange 2
    _dependencyMock.DependencyMethod(Arg.Any<Type>()).Returns(mockedValue);
    
    //Act
    var result = await _sut.MethodName(req);

    //Assert
    result.Should().BeEquivalentTo(expected);
    
    await _dependencyMock.Received(1).DependencyMethod(Arg.Is<Type>(x => x.Prop == req.Prop));
    _otherMock.DidNotReceive().OtherMethod();
}
```
