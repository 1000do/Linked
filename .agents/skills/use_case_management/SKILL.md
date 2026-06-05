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

## 8. Example Use Case Specification
Below is a complete, well-formatted example adhering to these rules.

---

### **UC-58 - Publish courses**

**Primary Actor:**  
Instructor  

**Secondary Actors:**  
AI Service  

**Trigger:**  
Instructor clicks the "Submit for Review" button.

**Description:**  
Allows instructors to request system approval to publish draft courses. The system validates the submission immediately, returns a confirmation status, and kicks off AI-assisted moderation asynchronously.

**Preconditions:**  
**PRE-1.** User must be successfully authenticated as an approved instructor.  
**PRE-2.** The course must exist and belong to the active instructor.

**Post-conditions:**  
**POST-1.** The course is updated with a pending status in the database.  
**POST-2.** AI moderation runs in the background and saves resolved results to the database upon completion.  
**POST-3.** Stale cache data is cleared.  
**POST-4.** The course is queued on the admin manual audit dashboard.

---

### **Normal Flow**

#### **A. Submit Course for Review Successfully**
1. Instructor accesses the course edit page.  
2. Instructor clicks the "Submit for Review" button.  
3. The system validates form inputs.  
4. The system validates course content against business rule limits.  
5. The system updates the course to pending status in the database.  
6. The system invalidates cached course data.  
7. The system triggers the background AI moderation process:
   * 7a. The system requests moderation from the AI Service.  
   * 7b. The system resolves the moderation results from the AI Service.  
   * 7c. The system updates the course metadata and priority level in the database.  
   * 7d. The system logs the moderation transaction details in the database.  
   * 7e. The system sends a notification to the administrator requesting a manual audit.  
8. The system displays a success confirmation message to the instructor.

---

### **Alternative Flows**

#### **A.3 Invalid input data**
1. The system displays a validation error message.  
2. Back to step 2.

#### **A.4 Missing required video content**
1. The system displays an error message regarding missing video content.  
2. Back to step 2.

---

### **Exceptions**

#### **EX-2 Session Invalid (Not Logged In)**
1. The system displays an authentication error message.  
2. The system redirects the user to the login page.  

#### **EX-4 Course Not Found**
1. The system halts execution and displays a course not found error message.  

#### **EX-5 Database update failed**
1. The system fails to commit the course status changes.  
2. The system displays a database transaction failure message.  
3. Back to step 1.

#### **EX-7a.1 AI Service unhealthy or offline**
1. The system bypasses automated moderation and flags the course directly for admin manual review.  
2. Back to step 7e.

#### **EX-7a.2 AI Moderation request timeout or pipeline error**
1. The system logs the failure and flags the course directly for admin manual review.  
2. Back to step 7e.

#### **EX-7c Database update failure**
1. The system logs the database write error.  
2. Back to step 7e.

#### **EX-7d Database logging failure**
1. The system logs the database write error.  
2. Back to step 7e.

---

### **Business Rules**
1. **Stripe Link Limits:** Instructors who have not fully onboarded with an active Stripe Connect profile are only allowed to publish up to 2 courses and are restricted to a maximum video content threshold of 30 minutes.  
2. **Free Course Video Ceiling:** Any course with a price parameter of 0 has a strict video content cap of 60 minutes. Paid courses require an active Stripe Connect configuration.  
3. **Content Re-activation:** When a rejected course is resubmitted for review, the system automatically sanitizes lesson and material states, resetting rejection flags back to active for the admin moderator's review.

---

### **Assumptions**
Instructor has stable internet connection.  
AI Service is working properly.
