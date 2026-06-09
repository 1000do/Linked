---
name: functional_spec_management
description: Specializes in creating, formatting, and editing Functional Specification documents (also known as Function Details), adhering to a specific structure including Triggers, Descriptions, Screen Layouts, and detailed Process steps with Pre/Post conditions, Error Handling, and Security Measures.
---

# Functional Specification Management Skill

This skill provides comprehensive instructions for formatting, writing, and editing Functional Specification documents based on the provided standard format. This format is distinct from standard Use Case Management and focuses on detailed, user-centric functional documentation.

## 1. Document Structure
Every functional specification MUST follow this exact structure and section naming in Markdown:

**[Identifier]. [Function Name]** (e.g., `a. View Course List`, `e. Forgot password`)

**Function Trigger:** [Clear statement of the user action that initiates the function. e.g., "The user clicks the 'Forgot password' link on the Login page."]

**Function Description:** [High-level summary of the function's purpose, the user roles that can access it, and its main outcome.]

**Screen Layout:**
[Placeholder for UI mockups or screenshots, usually an image embed or note]

**Function Details**
*(Use a standard list format for the main categories below)*

*   **Preconditions**
    *   [Condition 1, e.g., "The user has a registered account with a valid email."]
    *   [Condition 2, e.g., "The user is connected to the internet."]
*   **Process**
    *   [Step 1 Name/Title]:
        *   [Action detail/User action]
    *   [Step 2 Name/Title]:
        *   [Action detail/System response]
    *(Note: Process steps can also be formatted as a numbered list with descriptions below them, depending on the document flow)*
*   **Alternative Flow [optional condition/name]**
    *   [Description of the alternative flow, e.g., "If the email does not exist:"]
        *   [Action 1, e.g., "Display error message."]
        *   [Action 2, e.g., "Redirect back to the email input."]
*   **Postconditions**
    *   [Outcome 1, e.g., "The user's password is successfully reset."]
*   **Error Handling**
    *   **[Error Name/Type]:** [How the system responds, e.g., "Lost Connection: Display 'Lost Connection' screen and prompt reconnection."]
*   **Security Measures**
    *   [Security rule 1, e.g., "Password reset is sent only to the registered email."]

## 2. Formatting Guidelines
*   **Bold Text:** Use bold for the main field headers (`**Function Trigger:**`, `**Function Description:**`, `**Screen Layout:**`, `**Function Details**`).
*   **Process Steps Format:** Use clear step titles (e.g., **Navigate to Forgot Password Page:**) followed by sub-bullets describing the exact interactions.
*   **Error Handling Format:** Always bold the specific error name followed by a colon, then describe the system behavior.
*   **Alternative Flow Labeling:** If the alternative flow is triggered by a specific condition, append it to the label (e.g., `* **Alternative Flow (Invalid Email or Password Format)**`).

## 3. Tone and Style
*   **Action-Oriented:** Use active voice (e.g., "The system redirects...", "The user enters...").
*   **Clear and Concise:** Keep descriptions direct and easy to follow.
*   **Consistent Terminology:** Consistently use terms like "The system" and "The user" to describe actors.
