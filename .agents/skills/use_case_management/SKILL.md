---
name: use_case_management
description: Specializes in creating, formatting, and editing Use Case specifications based on current source code state, adhering strictly to established formatting and structural rules (ambiguity limits, single-job steps, alt flow/exception references).
---

# Use Case Management Skill

This skill provides comprehensive instructions for formatting, writing, and editing Use Case Specifications based on existing source code. The rules below ensure consistency, maintain correct granularity, and accurately represent alternative and exception flows.

## 1. Actor Naming Convention

### Standardized Primary Actors
Do NOT invent custom actor names (like "Learner" or "Customer"). You must exclusively use one of the following exact standardized actors depending on the use case context:
- `Guest` (Unauthenticated actor)
- `User` (Authenticated customer/learner)
- `Instructor` (Originally a User but applied to become an Instructor. Can switch views flexibly)
- `Staff` (Authorized manager)
- `Admin` (Highest Authority manager)

### Use Case ID and Name Convention
The use case ID and name (e.g., in the "UC ID and Name" metadata field) MUST follow the exact format: `UC-[Number]-[Name]` where the name contains spaces.
- *Correct:* `UC-02-View Course List`, `UC-03-View Course Details`
- *Incorrect:* `UC-ViewCourseList - View Course List`, `UC-ViewCourseDetails - View Course Details`

### Multiple Primary Actors Rule
If a use case has multiple primary actors listed in its metadata (e.g., `User, Instructor`), the steps in the Normal Flow and Alternative Flows must use the generic term `Actor` (or `Actors`) in triggers and steps (e.g., `Actor accesses the page` instead of `User accesses the page`), unless a specific step or flow strictly applies to only one of those actors.

## 2. Ambiguity vs. Precision Guidelines
*   **Be Descriptive, Not Exact:** Do not be too precise for everything. Avoid including specific examples in the specification. Maintain a descriptive, somewhat ambiguous tone for general UI elements, data, and system messages.
*   **Descriptive Language for UI/Messages:** For things like message strings, page names, form names, and status strings, do NOT include the exact string. Use descriptive language instead.
    *   *Correct:* "The system displays an error message regarding database update failure", "course edit form", "pending status".
    *   *Incorrect:* "The system displays 'Error 500: Failed to update DB'", "CourseEditPage.jsx", "Status='Pending'".
*   **100% Precision for Actionable Elements:** Names of **buttons, icons, and links** MUST be 100% precise, matching their exact implementation in the UI. Wrap them in standard double quotes `""` (e.g., `"Apply Filters"`, `"grid_view"` icon) instead of `<code>` tags, as `<code>` tags disrupt font styling and size consistency on copy-pasting. Keep business rules details (like exact numbers or limits) strictly precise as well.

## 3. Step Granularity (The "1-Job" Rule)
*   **One Job per Step:** Every step in the Normal Flow should do exactly ONE job.
    *   *Incorrect:* "The system validates form inputs and course content against business rule limits."
    *   *Correct:* 
        *   "1. The system validates form inputs."
        *   "2. The system validates course content against business rule limits."
*   **Transactional Grouping:** If multiple database updates happen in a single transaction in the code, they should be grouped into one step.
*   **Split Retrieval and Verification:** System retrieval of data and validation/check of that data must be represented as separate steps.
    *   *Incorrect:* "The system retrieves the course details and checks the user's enrollment status."
    *   *Correct:*
        *   "1. The system retrieves the course details."
        *   "2. The system validates the user's enrollment status."

## 4. Flow Types and Exclusions
*   **Normal Flow Label:** The normal flow must include a label describing the successful process right below the "Normal Flow" header, format: `**A. [Description] Successfully**` (e.g., `**A. Remove Learning Materials Successfully**`).
*   **Normal Flow Steps only:** For flow steps, ONLY include steps that are in the main execution flow. 
    *   **First Step:** The first step of the Normal Flow usually should be: `[Primary Actor(s)] access [page/form/etc.]`.
*   **Do not include auxiliary steps:** Background resets or minor state cleanups that do not affect the main business process directly should be omitted. 
    *   *Example to omit:* "The system resets any previously rejected lesson and material flags back to active state."
*   **Admin Notifications:** Important system actions like admin notifications are NOT auxiliary and should be placed in the normal flow (and cascaded appropriately to alt flows/exceptions).

## 5. Labeling and Numbering Conventions
*   **Alternative Flows (Alt Flows):**
    *   If an alternative flow originates from step `A.5` of the Normal Flow, label it as `A.5 Alt Flow Name`.
    *   If *multiple* alternative flows originate from the same step `A.5`, use numbering: `A.5.1 [Name]`, `A.5.2 [Name]`, etc. **Do NOT use letter suffixes (like `A.5a`, `A.5b`) for Alt Flows, as letters (e.g., `5a`) are strictly reserved for normal flow steps that trigger async background tasks.**
*   **Exceptions (EX):**
    *   If an exception originates from step `5`, label it as `EX-5 Exception Name`. Do not use flow letter prefixes like `EX-A.5` or `EX-B.3`. If the step identifier includes a letter (like `7a`), use that directly: `EX-7a`.
    *   If *multiple* exceptions originate from the same step `7a`, use numbering: `EX-7a.1 [Name]`, `EX-7a.2 [Name]`, etc.
    *   *Note:* Only add the extra `.1, .2` numbering levels if there are multiple branches originating from the exact same step. If there is only one exception at step 5, name it `EX-5`.

## 6. Alternative Flows vs Exceptions
*   **Alternative Flow:** A flow is considered an alt flow if it simply halts execution due to some restrictions, validations, or invalidation (e.g., frontend form validation, cancelling a modal), and then brings the actor back to a normal flow step. This includes intentional user-triggered navigation actions that cause a page reload (e.g., pagination, tab switching) — these should be modeled as alt flows ending with `Back to step 1.`, NOT as exceptions.
    *   *Rule:* Alternative flows MUST reference back to a normal flow step (e.g., "3. Back to step 3.").
*   **Exception:** A flow is considered an exception if it:
    1. Kicks the actor out of the normal flow entirely (e.g., Session Invalid redirects to login).
    2. Makes the actor go back to step 1 due to a full page reload caused by an **error or system-triggered redirect** — NOT by an intentional user navigation action (e.g., clicking a pagination button or switching tabs). Those belong in Alternative Flows.
    3. Explicitly throws a backend exception (e.g., `UnauthorizedAccessException`, database failure). *Note: Exceptions that don't kick the user out should also end with a reference back to a normal flow step (e.g., "3. Back to step 2.").*

## 7. Assumptions and Authentication Exceptions
*   **Assumptions Section:** Every use case specification must include an **Assumptions** section as the very last section of the document (after Business Rules). 
    *   For **Primary Actors** (always present), state: `[Primary Actor(s)] has stable internet connection`.
    *   For **Secondary Actors** (if any are present), state: `[Secondary Actor(s)] are working properly`. Only reference secondary actors if they are actually present in the use case.
    *   *(e.g., "Instructor has stable internet connection. AI Service is working properly.")*
*   **Session Invalid Exception:** For use cases that require authentication, always include a Session Invalid exception in the Exceptions section. Format it as follows:
    ```markdown
    #### **EX-1 Session Invalid (Not Logged In)**
    1. The system displays an authentication error message.  
    2. The system redirects the user to the login page.
    ```

## 8. Pre/Post-conditions Labeling
*   **Preconditions** must be labeled as `**PRE-1.**`, `**PRE-2.**`, etc.
*   **Post-conditions** must be labeled as `**POST-1.**`, `**POST-2.**`, etc.
*   *Note:* In HTML exports, use paragraph tags with bold labels (e.g., `<p><b>PRE-1.</b> User must be...</p>`) instead of standard list tags (`<ol>`/`<li>`) to preserve the exact prefix format.
*   **Actor-Precondition Alignment:** Ensure preconditions do not lock out any listed Primary Actor. If an `Instructor` can access a page without being enrolled (due to ownership), the precondition must reflect this bypass (e.g., `Actor must be enrolled in the course, or be the owner of the course`), rather than a blanket `Actor must be enrolled`.

## 9. Output & Export Workflow
When creating a use case specification, you MUST follow this workflow:
1. **Preview First:** Generate the initial use case specification as a standard Markdown (`.md`) artifact. Present this to the user for review.
2. **Export on Approval:** Once the user reviews and explicitly approves the preview, you must export the specification as an HTML file to the `docs/uc_spec` directory in the workspace. 
    * The HTML file MUST be structured as a 4-column bordered table to be copy-paste friendly for Google Docs.
    * Required inline styles for the container `div` (e.g., `<div class="uc-container" style="font-family: 'Times New Roman', serif; font-size: 12pt; background-color: transparent; color: black;">`).
    * You MUST include a `<style>` block immediately inside the container `div` to strictly enforce the font-family, font-size, and color for all child elements (including tables, cells, lists, list items, paragraphs, spans, bold/strong tags, etc.) so that browser-specific defaults do not override the style when copy-pasting:
      ```html
      <style>
          .uc-container, .uc-container table, .uc-container tr, .uc-container td, .uc-container p, .uc-container ol, .uc-container ul, .uc-container li, .uc-container span, .uc-container strong, .uc-container b {
              font-family: 'Times New Roman', serif !important;
              font-size: 12pt !important;
              color: black !important;
          }
      </style>
      ```
    * The table should use `border-collapse: collapse; width: 100%; border: 1px solid black;`.
    * All `<td>` cells must have `border: 1px solid black; padding: 5px; vertical-align: top;`.
    * Left-side label columns (e.g., "UC ID and Name", "Created By:", "Primary Actor:") must have `text-align: right;` and typically take up `width: 20%;` where applicable. Right-side value columns must have `text-align: left;`.
    * The top rows contain metadata (UC ID and Name, Created By, Date Created, Primary Actor, Secondary Actors). Most subsequent rows (Trigger, Description, Normal Flow, etc.) will have the label in the first column, and the content in the second column utilizing `colspan="3"`.
    * Do NOT use HTML heading tags (e.g., `<h3>`, `<h4>`). Use normal paragraph tags with bold text (e.g., `<p style="margin: 0 0 5px 0;"><strong>Section Name</strong></p>`) inside the table cells.
    * Maintain `<ol>` and `<ul>` list structures for flows to keep the layout clean within the cells.

## 10. Example Use Case Specifications
For complete, well-formatted examples adhering to these rules, please refer to all the `.html` files located in the `references/` directory within this skill folder.

Always study these reference examples before generating a new use case specification to ensure your output strictly follows the established patterns, tone, and granularity.
