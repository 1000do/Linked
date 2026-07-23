---
name: system_test_suite_generation
description: Guides the generation of a comprehensive, executable HTML system test suite for a feature based on use case specs, business rules, sequence diagrams, and code implementation, including automated updates to the Function Overview and Module Summary HTML files.
---

# System Test Suite Generation

You are tasked with generating a System Test Suite for a specific feature and updating the project's global test tracking documents. You MUST iterate through the two checklists below. You must complete Checklist 1 (Content) before moving to Checklist 2 (Formatting & Updates).

## Terminology
For clarity, ensure you understand the following definitions before proceeding:
- **Feature/Module**: A group of related use cases.
- **Function**: A use case.
- **Test case**: A test case generated from this skill is a system-test-level test case (use-case-level test case), meaning it requires the entire working application from the UI down the stack to the database.

## Phase 1: Context Gathering & Executability Analysis
1. **Context Verification**: The very first thing you must do is check: "Did the user provide enough context for this skill to run correctly (use case specs, business rules, sequence diagrams)?" and "Did the user specify a Tester name?". If they did not, prompt the user to provide this necessary context and the Tester name before proceeding. Do NOT hardcode names like "TaiTP".
2. **Analyze Sources**: Review the Use Case Specs, Business Rules, Sequence Diagrams, and actual Code Implementation (Controllers, Services, SignalR hubs, Background tasks, etc.).
3. **Validate Executability**: You MUST validate that every proposed test case is genuinely executable. 
   - *Executable* means the actor can perform the whole test procedure and see the result on the UI.
   - Trace the sequence of interactions and participants in the code.
   - **Background Tasks**: Do not omit test cases for background tasks if they result in an observable UI change (e.g., SignalR notifications). Be aware that background tasks:
     - Might require multiple actors (e.g., Actor A triggers it, Actor B receives the notification - like TC-051 -> TC-055 in Manage User Report).
     - Might require a delay to see the response (e.g., a background task queue calling an external API for heavy processing, then notifying the appropriate actors when the response is returned).

## Phase 2: Execution & Validation Checklists

### Checklist 1: Content Verification
Iterate through this checklist and validate the test suite content. Do not proceed to Checklist 2 until ALL items here are checked and passed.
- [ ] Has the user provided enough context (Use case specs, business rules, sequence diagrams) to begin? If not, stop and ask.
- [ ] Are all test cases from the Use Case specs and Business Rules covered?
- [ ] Is every test case genuinely executable according to the actual code implementation?
- [ ] Have background task scenarios been properly designed with multiple actors and/or delays accounted for where necessary?
- [ ] **Procedure Completeness**: Is EVERY Test Case Procedure a complete, standalone sequence? (e.g., ALWAYS start from login/accessing the page. NEVER use shorthand continuations like "Back to step 3").
- [ ] **Expected Result Specificity**: Are the Expected Results descriptive and specific to the scenario (e.g., stating exactly what happens on the UI or database)? NEVER use generic phrases like "The system handles the alternative flow successfully."
- [ ] Are preconditions reasonably inferred when original preconditions are empty or "None" (e.g., "Actor can be a Guest or an authenticated User.")?
- [ ] Are use case titles robustly parsed (handling HTML formatting edge cases like line breaks) and stripped of the `UC-XX-` prefix (e.g. `View Course List` instead of `UC-02-View Course List`) before using them as test suite group headers?
- [ ] **Description Formatting**: Are Test Case Descriptions explicitly stripped of ANY technical Use Case prefixes (e.g., remove "A.2.1", "EX-1", "B.3") while keeping the full human-readable scenario name?
- [ ] Is the language used descriptive and human-readable, with no technical jargon? (It must clearly describe what can be done and observed on the UI).
- [ ] Are use case hints (e.g., `(UC-15, Normal Flow)`) excluded from the final output?

### Checklist 2: Styling, Format, and Global Updates
Only proceed to this checklist once Checklist 1 is fully passed.
- [ ] **Output Location**: Is the HTML file being saved to `<project_root>/docs/system_test_docs/`?
- [ ] **Style Block**: Does the HTML file reuse the exact `<style>` tag block from the gold standard reference file located at `references/Manage User Report.html`?
- [ ] **Summary Layout**: Is the summary section formatted as a bulleted list (Feature, Test requirement, Number of TCs) instead of a table?
- [ ] **ID Formatting**: Are test case IDs plain text without HTML tags (e.g., `TC-001`, NOT `<b>TC-001</b>`)?
- [ ] **Function Rows**: Do function rows use `<tr class="func-row">` and span all columns?
- [ ] **Test Rounds**: Are there exactly 3 full execution rounds populated (Round 1, Round 2, and Round 3) for each test case to ensure no rounds are missing?
- [ ] **Test Dates**: Are the test dates dynamically calculated based on the current date: Round 1 (3 weeks ago), Round 2 (2 weeks ago), Round 3 (1 week ago)?
- [ ] **Tester**: Is the Tester column populated with the name explicitly provided by the user?
- [ ] **Update Function Overview.html**: 
    - Has the `Function Overview.html` been updated without altering any of its existing styling (content updates only)?
    - Does `Function Name` = the exact Use Case?
    - Does `Description` = the exact Use Case spec description (1-1 mapping)?
    - Is `Pre-condition` = a summarized version of the Use Case spec Pre-condition?
    - Does the `No` column auto-increment correctly?
- [ ] **Update Module Summary.html**:
    - Has `Module Summary.html` been updated without altering any of its existing styling (content updates only)?
    - Does `Module code` = the exact Feature Name (no made-up IDs or abbreviations, 1-1 mapping)?
    - Are the `Pending` and `N/A` columns strictly set to `0`?
    - Are the `Passed` and `Failed` counts pulled from the latest execution round (e.g., Round 3) of the new test suite?
    - Is the `Number of test cases` column set to the total number of test cases from the test suite HTML?
    - Is the `Sub total` row recalculated accurately?
