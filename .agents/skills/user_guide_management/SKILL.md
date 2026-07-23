---
name: user_guide_management
description: Specializes in creating, formatting, editing, and validating User Guide / User Manual documents based on project source code, System Test Suites, and Use Case specifications. Ensures output is in English and adheres to HTML format with Times New Roman 12pt text and an h2 heading for the feature title.
---
# User Guide Management Skill

This skill guides the agent in generating, formatting, editing, or validating User Guide documents for the project. It ensures that all manuals are accurately tied to the system's actual implementation (derived from `system_test_docs` or `uc_spec`) and formatted exactly according to the strict visual template expected by the project stakeholders.

## 1. Core Principles
1. **Source of Truth:** Never hallucinate features. Always base the workflow steps on the actual system test cases (e.g., `docs/system_test_docs/*.html`) or use case specifications (`docs/uc_spec/`).
2. **Action-Oriented:** Write steps from the user's perspective, clearly stating what they need to do (click, enter, select) and what the system does in response (displays, redirects, opens modal).
3. **Format Strictness:** The output MUST be a valid HTML file. It should predominantly use normal text, styled with Times New Roman font at 12pt size, but the main feature heading MUST be an `<h4>` tag. Rely heavily on specific bolding, unordered lists (`<ul>`), and centered italicized image captions.
4. **Language:** Use professional, instructional English.

## 2. Structural Template

Every feature documented must strictly follow this HTML layout:

```html
<div style="font-family: 'Times New Roman', Times, serif; font-size: 12pt; color: black; line-height: 1.5;">
    
    <h4>3.X.Y.Z [Feature Name]</h4>
    
    <p>The purpose of this workflow is to allow [User Role] to [Purpose of the feature].</p>
    
    <p><b style="color: black;">Execution steps:</b></p>
    
    <p><b>Step 1:</b> [First user action to trigger the feature, e.g., accessing a page].</p>
    
    <p><b>Step 2:</b> [Next action]. The user inputs information / selects actions:</p>
    <ul>
        <li><b>[Field Name 1]:</b> [Detailed explanation of this data field or option. Is it required?].</li>
        <li><b>[Field Name 2]:</b> [Detailed explanation].</li>
    </ul>
    
    <p style="text-align: center;">
        <i>(Insert a screenshot of the [Context] here, highlighting or numbering the [Action Element] in red)</i>
    </p>
    
    <p style="text-align: center;"><i>Figure X. [Action Name] operation</i></p>
    
    <p><b>Step 3:</b> The user clicks the <b>[Submit Button Name]</b> button.</p>
    
</div>
```

## 3. Formatting Rules
- **File Format:** Output must be valid HTML. Do not output Markdown formatting outside of code blocks.
- **Font & Sizing:** All text must be rendered in Times New Roman, 12pt. Use CSS styling on the container `<div style="font-family: 'Times New Roman', Times, serif; font-size: 12pt;">`.
- **Headings:** The main feature title must use an `<h4>` tag. Do not use other heading tags for the rest of the document (use `<p>` tags with bold text `<b>` for emphasis).
- **Feature Headings:** Must be wrapped in `<h4>` and numbered using a `3.X.Y.Z` format (e.g., `<h4>4.2.1.1 Register</h4>`).
- **Labels:** The phrase "**Execution steps:**" must be bolded and colored black using `<b style="color: black;">`. The phrase "**Step X:**" must always be bolded using `<b>`.
- **Data Fields/Options:** In step descriptions where users input data or choose options, use HTML unordered lists (`<ul>` and `<li>`). The name of the field/option must be bolded (`<b>`), followed by a colon and a normal text description.
- **Action Buttons:** The final action to confirm a form or process (like clicking a submit or confirm button) should be a distinct step (e.g., Step 3) separate from the list of form fields. Do NOT explain what the system does after the click (e.g., do not add 'The system redirects to...'). The guide should stop strictly at the user's final button click.
- **Image Placeholders:** Always insert an instructional placeholder in italics (`<i>`), centered using `<p style="text-align: center;">`.
- **Image Captions:** Must be centered, placed directly under the placeholder, italicized (`<i>`), and prefixed with "Figure X." (e.g., `<i>Figure 1. Add to Cart operation</i>`).

## 4. Quality Assurance Checklist
Use this checklist to validate any generated User Guide.
- [ ] **Data Accuracy:** Are all steps, fields, and behaviors derived entirely from the system tests or use case specifications (no hallucinations)?
- [ ] **Output Format:** Is the output formatted strictly as HTML?
- [ ] **Styling Constraints:** Is the font explicitly set to Times New Roman, 12pt on the container? Is the `<h4>` tag used ONLY for the main feature heading?
- [ ] **Feature Heading:** Is the feature title numbered (e.g., `3.X.Y.Z`) and wrapped in an `<h4>` tag?
- [ ] **Structural Labels:** Is "**Execution steps:**" colored black and bolded? Are all "**Step X:**" labels bolded?
- [ ] **List Items:** Are input fields or options listed using `<ul>` and `<li>` tags with the field name **bolded**?
- [ ] **Image Placeholders & Captions:** Are the screenshot instructions and figure captions both wrapped in `<p style="text-align: center;"><i>...</i></p>`?
- [ ] **Language:** Is the output in professional, instructional English?

## 5. Execution Workflow & Continuous Validation
When asked to create or edit a User Guide:
1. **Analyze:** Identify the requested feature. Find and review its corresponding `System Test Suite` (in `docs/system_test_docs/`) or `Use Case` (in `docs/uc_spec/`) to extract the exact behavior and validation rules. **CRITICAL:** Always cross-reference the actual frontend UI code (e.g., `.cshtml` files) to ensure that the field names and button labels used in your documentation match the actual UI text perfectly verbatim.
2. **Draft:** Construct the manual applying the exact HTML structural template and formatting rules defined above.
3. **Continuous Validation Loop:**
   - **Evaluate** your drafted output against the **Quality Assurance Checklist**.
   - **Iterate:** If any item in the checklist is NOT passed, you MUST revise your draft to fix the missing or incorrect formatting/content.
   - **Repeat** this evaluation and revision loop internally until ALL items in the checklist are PASSED.
4. **Output:** You MUST create and write the fully validated formatted HTML directly to a `.html` file for the requested feature (using the `write_to_file` tool). Do not just output the HTML in a markdown code block in the chat. You may optionally output the final checklist state in your response to prove compliance.
