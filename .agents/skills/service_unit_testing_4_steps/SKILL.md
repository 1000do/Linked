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
*   Isolate the target class for testing.
*   Carefully read the interface defining the public methods.
*   Carefully read the concrete implementation of the service, from the public methods down to every single private helper.
*   **Redundant Defensive Code**: Identify if the code has redundant defensive code. Suggest proper changes to reduce these, thereby reducing redundant tests. Consult the `/method_extraction` skill for a clean refactor.
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
*   **CRITICAL RULE**: You MUST emit the literal comments `//Arrange 1`, `//Arrange 2`, `//Act`, and `//Assert` in EVERY single test you generate, without exception. Every single test MUST strictly follow this exact 4-step structure:
    *   `//Arrange 1`: Setup data objects, inputs, expected outputs, and variables.
    *   `//Arrange 2`: Setup the behavior of mocked dependencies (`.Returns(...)`).
    *   `//Act`: Invoke the method under test.
    *   `//Assert`: Perform assertions on the result/state, handle exception assertions, AND assert method reference invocations (e.g., `.Received(1)` and `.DidNotReceive()`).
        *   **String Matching Warning**: When asserting `.Received(1)` on methods with string arguments (like titles or messages), either perfectly copy the magic strings directly from the implementation source code, or use `Arg.Any<string>()` to prevent brittle tests and `ReceivedCallsException` failures.
*   **Testing Private Methods via Reflection**: When required to cover private helper methods, use reflection:
    *   Use `BindingFlags.NonPublic | BindingFlags.Instance`.
    *   Handle async invocations properly by casting the result to `(Task)` or `(Task<T>)` and `await`ing it (e.g., `await (Task)method.Invoke(...)!`).
    *   Deal with `TargetInvocationException` exceptions, or use `Func<Task>` wrappers for `FluentAssertions`.

### 5. Coverage Validation
*   Do not assume 100% coverage just because the tests pass.
*   **Prerequisites**: Prompt the user to install necessary dependencies if they haven't already (e.g., `coverlet.collector` for the test project, and `dotnet-reportgenerator-globaltool` globally).
*   Run the test with coverage calculated and ensure the output is placed explicitly inside the `TestResults` folder of the test project (e.g., `dotnet test --collect:"XPlat Code Coverage" --results-directory ./YourProject.Tests/TestResults`).
*   Run `reportgenerator` with a class filter that concerns ONLY the target class for testing, using its **fully qualified name** with a trailing wildcard to capture async state machines (e.g., `-classfilters:+Your.Namespace.YourServiceClassName*`). The report output MUST be placed explicitly inside the `TestHTMLReports` folder of the test project (e.g., `-targetdir:./YourProject.Tests/TestHTMLReports`).
*   Parse the resulting HTML or XML report to mathematically verify coverage.
*   **Branch Coverage Thresholds**: If the report shows 100% line coverage but NOT 100% branch coverage:
    *   Prompt the user if they want to go further to increase branch coverage.
    *   **WARN them** that aiming for 100% branch coverage might result in multiple back-and-forth refactors and excessive code generation, hurting token quotas.
    *   Advise them to aim for at least 90% branch coverage alongside the 100% line coverage.
    *   Prioritize covering explicit conditional or iteration branches (`for`, `while`, `if`, `else`, `switch`). Only tackle shorthand branches (like `?.`, `??`) if all explicit branches are already covered.
*   **Note**: In C#, coverage for `async` methods often appears under compiler-generated state machine structs (e.g., `<MethodName>d__12`) rather than the parent class. Ensure your parser accounts for this when verifying 100% coverage.
*   **Presentation**: At the very end of the workflow, ALWAYS provide the absolute paths to the specific generated HTML test report files for the requested services (e.g., `file:///path/to/TestHTMLReports/Namespace_ClassName.html`) in your final response. Do NOT just link the generic `index.html`. This allows the user to click and manually review the exact coverage details for the specific classes they requested.

## Structure Example
```csharp
// Standard Execution Test
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

// Async Exception Test
[Fact]
public async Task MethodName_ErrorCondition_ThrowsSpecificException()
{
    //Arrange 1
    var req = new RequestType { Prop = "Invalid" };

    //Arrange 2
    _dependencyMock.DependencyMethod(Arg.Any<Type>()).Returns(mockedValue);
    
    //Act
    Func<Task> act = async () => await _sut.MethodName(req);

    //Assert
    var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
    ex.WithMessage("Expected message here");
}
```
