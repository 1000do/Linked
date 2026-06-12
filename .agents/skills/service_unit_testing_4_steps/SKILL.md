---
name: service_unit_testing_4_steps
description: Guides the creation of robust unit tests for service interfaces targeting 100% coverage, utilizing a strict 4-step Arrange/Act/Assert layout and method reference assertions. Enforces testability checks, specific testing frameworks, and an implementation plan phase.
---

# Service Unit Testing (4 Steps)

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

### 2. Testability Audit
*   Evaluate the testability of the implementation methods.
*   If a method is difficult to test (e.g., returns `void` or `Task<void>` and relies heavily on untracked database side-effects), you MUST suggest proper refactoring to make it testable (e.g., changing the return type to `bool`, `Task<bool>`, or returning the tracked entity instead of refetching it).

### 3. Implementation Plan Phase
*   **Do NOT write the tests immediately.**
*   Create an `implementation_plan.md` artifact outlining:
    *   The required changes to make the service testable (if any).
    *   The list of test cases to be written for each method to achieve 100% coverage. Use extremely descriptive names for your test methods (length does not matter as long as it describes the specific scenario and expected outcome).
*   Request user feedback by setting `request_feedback = true` and wait for explicit user approval. Do NOT proceed until the user approves.

### 4. Strict 4-Step Test Format
*   Once approved, implement the unit tests.
*   **CRITICAL**: You MUST consult the example tests provided in the `references/` directory within this skill folder (e.g., `AdminAiServiceTests.cs`) to study a complete, perfect implementation of this structure before writing code.
*   **CRITICAL**: Every single test MUST strictly follow this exact 4-step structure:
    *   `//Arrange 1`: Setup data objects, inputs, expected outputs, and variables.
    *   `//Arrange 2`: Setup the behavior of mocked dependencies (`.Returns(...)`).
    *   `//Act`: Invoke the method under test.
    *   `//Assert`: Perform assertions on the result/state, handle exception assertions, AND assert method reference invocations (e.g., `.Received(1)` and `.DidNotReceive()`).

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
