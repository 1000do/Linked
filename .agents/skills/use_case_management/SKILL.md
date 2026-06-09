---
name: use_case_management
description: Specializes in creating, formatting, and editing Use Case specifications based on current source code state, adhering strictly to established formatting and structural rules (ambiguity limits, single-job steps, alt flow/exception references).
---

# Use Case Management Skill

This skill provides comprehensive instructions for formatting, writing, and editing Use Case Specifications based on existing source code. The rules below ensure consistency, maintain correct granularity, and accurately represent alternative and exception flows.

## 1. Ambiguity vs. Precision Guidelines
*   **Be Descriptive, Not Exact:** Do not be too precise for everything. Avoid including specific examples in the specification. Maintain a descriptive, somewhat ambiguous tone for general UI elements, data, and system messages.
*   **Descriptive Language for UI/Messages:** For things like message strings, page names, form names, and status strings, do NOT include the exact string. Use descriptive language instead.
    *   *Correct:* "The system displays an error message regarding database update failure", "course edit form", "pending status".
    *   *Incorrect:* "The system displays 'Error 500: Failed to update DB'", "CourseEditPage.jsx", "Status='Pending'".
*   **100% Precision for Actionable Elements:** Names of **buttons, icons, and links** MUST be 100% precise, matching their exact implementation in the UI. Keep business rules details (like exact numbers or limits) strictly precise as well.

## 2. Step Granularity (The "1-Job" Rule)
*   **One Job per Step:** Every step in the Normal Flow should do exactly ONE job.
    *   *Incorrect:* "The system validates form inputs and course content against business rule limits."
    *   *Correct:* 
        *   "1. The system validates form inputs."
        *   "2. The system validates course content against business rule limits."
*   **Transactional Grouping:** If multiple database updates happen in a single transaction in the code, they should be grouped into one step.

## 3. Flow Types and Exclusions
*   **Normal Flow Label:** The normal flow must include a label describing the successful process right below the "Normal Flow" header, format: `**A. [Description] Successfully**` (e.g., `**A. Remove Learning Materials Successfully**`).
*   **Normal Flow Steps only:** For flow steps, ONLY include steps that are in the main execution flow. 
    *   **First Step:** The first step of the Normal Flow usually should be: `[Primary Actor(s)] access [page/form/etc.]`.
*   **Do not include auxiliary steps:** Background resets or minor state cleanups that do not affect the main business process directly should be omitted. 
    *   *Example to omit:* "The system resets any previously rejected lesson and material flags back to active state."
*   **Admin Notifications:** Important system actions like admin notifications are NOT auxiliary and should be placed in the normal flow (and cascaded appropriately to alt flows/exceptions).

## 4. Labeling and Numbering Conventions
*   **Alternative Flows (Alt Flows):**
    *   If an alternative flow originates from step `A.5` of the Normal Flow, label it as `A.5 Alt Flow Name`.
    *   If *multiple* alternative flows originate from the same step `A.5`, use numbering: `A.5.1 [Name]`, `A.5.2 [Name]`, etc. **Do NOT use letter suffixes (like `A.5a`, `A.5b`) for Alt Flows, as letters (e.g., `5a`) are strictly reserved for normal flow steps that trigger async background tasks.**
*   **Exceptions (EX):**
    *   If an exception originates from step `5`, label it as `EX-5 Exception Name`. Do not use flow letter prefixes like `EX-A.5` or `EX-B.3`. If the step identifier includes a letter (like `7a`), use that directly: `EX-7a`.
    *   If *multiple* exceptions originate from the same step `7a`, use numbering: `EX-7a.1 [Name]`, `EX-7a.2 [Name]`, etc.
    *   *Note:* Only add the extra `.1, .2` numbering levels if there are multiple branches originating from the exact same step. If there is only one exception at step 5, name it `EX-5`.

## 5. Alternative Flows vs Exceptions
*   **Alternative Flow:** A flow is considered an alt flow if it simply halts execution due to some restrictions, validations, or invalidation (e.g., frontend form validation, cancelling a modal), and then brings the actor back to a normal flow step (without a page reload).
    *   *Rule:* Alternative flows MUST reference back to a normal flow step (e.g., "3. Back to step 3.").
*   **Exception:** A flow is considered an exception if it:
    1. Kicks the actor out of the normal flow entirely (e.g., Session Invalid redirects to login).
    2. Makes the actor go back to step 1 (due to a full page reload).
    3. Explicitly throws a backend exception (e.g., `UnauthorizedAccessException`, database failure). *Note: Exceptions that don't kick the user out should also end with a reference back to a normal flow step (e.g., "3. Back to step 2.").*

## 6. Assumptions and Authentication Exceptions
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

## 7. Pre/Post-conditions Labeling
*   **Preconditions** must be labeled as `**PRE-1.**`, `**PRE-2.**`, etc.
*   **Post-conditions** must be labeled as `**POST-1.**`, `**POST-2.**`, etc.
*   *Note:* In HTML exports, use paragraph tags with bold labels (e.g., `<p><b>PRE-1.</b> User must be...</p>`) instead of standard list tags (`<ol>`/`<li>`) to preserve the exact prefix format.

## 8. Output & Export Workflow
When creating a use case specification, you MUST follow this workflow:
1. **Preview First:** Generate the initial use case specification as a standard Markdown (`.md`) artifact. Present this to the user for review.
2. **Export on Approval:** Once the user reviews and explicitly approves the preview, you must export the specification as an HTML file to the `docs/uc_spec` directory in the workspace. 
   * The HTML file must be styled to be copy-paste friendly for Google Docs.
   * Required inline styles: `font-family: 'Times New Roman', serif; font-size: 12pt; background-color: transparent; color: black;`.
   * Do NOT use HTML heading tags (e.g., `<h3>`, `<h4>`). Use normal paragraph tags with bold text (e.g., `<p><strong>Section Name</strong></p>`) so Google Docs interprets it as normal text level instead of a heading.

## 9. Example Use Case Specifications
For complete, well-formatted examples adhering to these rules, please refer to all the `.html` files located in the `references/` directory within this skill folder.

Always study these reference examples before generating a new use case specification to ensure your output strictly follows the established patterns, tone, and granularity.
