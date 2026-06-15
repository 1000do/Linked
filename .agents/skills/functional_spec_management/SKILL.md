---
name: functional_spec_management
description: Specializes in creating, formatting, and editing Functional Specification documents in HTML format. Ensures the output uses Times New Roman 12pt font and normal text without any HTML heading tags (h1-h6).
---

# Functional Specification Management Skill

This skill provides comprehensive instructions for formatting, writing, and editing Functional Specification documents based on the provided standard format. The output MUST be a valid HTML document using Times New Roman font, size 12pt.

## 1. Output Format and Styling
*   **Format:** The final output MUST be an `.html` file. Do NOT output Markdown.
*   **Font and Styling:** Use `font-family: 'Times New Roman', serif;` and `font-size: 12pt;`.
*   **No Headings Allowed:** DO NOT use any HTML heading tags (`<h1>`, `<h2>`, `<h3>`, etc.). Instead of headings, use normal `<p>` tags with `<strong>` or `<b>` for emphasis.
*   **Style Template:** Incorporate the following CSS in the `<head>` of the HTML:
    ```html
    <style>
        body {
            font-family: 'Times New Roman', serif;
            font-size: 12pt;
            line-height: 1.5;
            margin: 0;
            padding: 20px;
        }
        p { margin: 8px 0; }
        strong { font-weight: bold; }
        hr { border: none; border-top: 1px solid #ccc; margin: 20px 0; }
        ul, ol { margin: 8px 0; padding-left: 20px; }
        li { margin-bottom: 4px; }
    </style>
    ```

## 2. Document Structure
Every functional specification MUST follow this exact structure using valid HTML tags:

<p><strong>[Identifier]. [Function Name]</strong></p>
(e.g., `<p><strong>a. View Course List</strong></p>`)

<p><strong>Function Trigger:</strong> [Clear statement of the user action]</p>

<p><strong>Function Description:</strong> [High-level summary]</p>

<p><strong>Screen Layout:</strong><br>[Placeholder for UI mockups]</p>

<p><strong>Function Details</strong></p>

<ul>
    <li><strong>Preconditions</strong>
        <ul>
            <li>[Condition 1]</li>
        </ul>
    </li>
    <li><strong>Process</strong>
        <ul>
            <li><strong>[Step 1 Name/Title]:</strong>
                <ul>
                    <li>[Action detail/User action]</li>
                    <li>[Action detail/System response]</li>
                </ul>
            </li>
        </ul>
    </li>
    <li><strong>Alternative Flow [optional condition/name]</strong>
        <ul>
            <li>[Description of the alternative flow]:
                <ul>
                    <li>[Action 1]</li>
                </ul>
            </li>
        </ul>
    </li>
    <li><strong>Postconditions</strong>
        <ul>
            <li>[Outcome 1]</li>
        </ul>
    </li>
    <li><strong>Error Handling</strong>
        <ul>
            <li><strong>[Error Name/Type]:</strong> [How the system responds]</li>
        </ul>
    </li>
    <li><strong>Security Measures</strong>
        <ul>
            <li>[Security rule 1]</li>
        </ul>
    </li>
</ul>

## 3. Formatting Guidelines
*   **Bold Text:** Use `<strong>` for the main field headers (`<strong>Function Trigger:</strong>`, `<strong>Function Details</strong>`, etc.), process step titles, and error names.
*   **No Markdown:** Since the output must be an HTML file, do not use Markdown syntax (like `**bold**` or `# heading`) in the generated result.
*   **Alternative Flow Labeling:** If the alternative flow is triggered by a specific condition, append it to the label (e.g., `<strong>Alternative Flow (Invalid Email)</strong>`).
*   **Drop Identifiers:** Ensure the output DOES NOT contain any original identifiers from the use case (e.g., drop prefixes like `PRE-1.`, `POST-1.`, `EX-3`, `BR-1`, and flow alphanumeric identifiers like `A.5.1`). Only output the raw textual details for Preconditions, Postconditions, Error Handling, Alternative Flow titles, and Security Measures.

## 4. Tone and Style
*   **Action-Oriented:** Use active voice (e.g., "The system redirects...", "The user enters...").
*   **Clear and Concise:** Keep descriptions direct and easy to follow.
*   **Consistent Terminology:** Consistently use terms like "The system" and "The user" to describe actors.
