---
name: mermaid_class_specifications
description: Generates standardized, formatted HTML class specifications from Mermaid diagram `.mmd` files. Enforces interface parity, specific description phasing, layer mapping, and exact HTML styling.
---

# Mermaid Class Specifications

This skill provides a robust, repeatable workflow to convert Mermaid class diagrams into formatted, stylized HTML Class Specifications, ready for Word export.

## Overview
The script extracts classes and interfaces, cleans and normalizes types, guarantees interface method parity in concrete implementations, formats method/property descriptions intelligently based on their context (e.g. interface vs controller), and generates an HTML table representation of the classes grouped by layer and numbered sequentially.

## 1. Prerequisites
Ensure you have the source Mermaid files (e.g., inside a `diagrams/class_mermaid` folder).

## 2. Tools & Scripts
This skill comes with two Python scripts located in `scripts/`:
- `parse_mermaid.py`: Scans `.mmd` files, sanitizes the data, applies descriptions and interface parity, and generates a `.md` output.
- `export_html.py`: Reads the `.md` file, strips out specific layer headings, applies sequential numbering (`4.1`, `4.2`, etc.) and formats the result as styling-inlined HTML.

## 3. References
The `references/` directory contains standard examples to guide formatting:
- `class_specifications.md`: The expected intermediate Markdown structure.
- `class_specifications_exported.html`: The expected final styled HTML output.

## 4. Workflow Execution

**Step 1: Execute `parse_mermaid.py`**
Run the parser to generate the initial markdown. Output the files directly to the `docs\class_desc` directory:
```bash
python .agents/skills/mermaid_class_specifications/scripts/parse_mermaid.py [source_dir] [dest_md] [report_md]
```
*(Example: `python .agents/skills/mermaid_class_specifications/scripts/parse_mermaid.py diagrams\class_mermaid docs\class_desc\class_specifications.md docs\class_desc\extra_public_methods_report.md`)*

**Step 2: Execute `export_html.py`**
Convert the markdown to the styled HTML, also outputting to `docs\class_desc`:
```bash
python .agents/skills/mermaid_class_specifications/scripts/export_html.py [input_md] [output_html]
```
*(Example: `python .agents/skills/mermaid_class_specifications/scripts/export_html.py docs\class_desc\class_specifications.md docs\class_desc\class_specifications_exported.html`)*

## 5. Verification Checklist
After generating the HTML file, you MUST iterate through this exact checklist and validate the final HTML/MD output against each entry to ensure perfect compliance.

- [ ] **Type Formatting Rules**: `DbSet` properties MUST be dropped entirely from `AppDbContext`. Specific hardcoded typings must be honored (e.g., `Account.PasswordHash` is strictly `string?`).
- [ ] **Layer Consolidation**: Synonymous layers MUST be merged seamlessly. For instance, `Viewmodel` must merge into `View Model`; `Signalr Hub` must merge into `Hub`.
- [ ] **Interface vs Concrete Method Parity**: If a public method exists in an interface (e.g., `ICouponService`), that exact method signature MUST appear in its concrete implementation class (e.g., `CouponService`).
- [ ] **Method Description Formatting**:
  - For Interface methods: MUST read `"Defines the contract to execute the [Name] operation..."`
  - For Public Service/Repository methods (implementations): MUST read `"Provides the concrete implementation to execute the [Name] operation..."`
  - For Controller methods, or any Private methods (`-`, `#`, `~`): MUST read `"Executes the [Name] operation..."`
  - For ANY asynchronous method (ends in `Async` or returns a `Task`): MUST append `" asynchronously"` before the return type definition.
- [ ] **Property Description Formatting**:
  - Properties must clearly state what they represent or store. E.g., `"Represents the [Name]."`.
  - Boolean flags (`Is...`, `Has...`, `Can...`) MUST read `"Indicates whether it [Name]."`.
  - Dependencies (e.g., starting with `_`, or ending in `Service`/`Repository`/`Context`) MUST read `"Provides the [Name] dependency."`.
- [ ] **HTML Headings**:
  - Interface definitions MUST have the word `"Interface"` appended in the HTML heading (e.g., `4.1 ICouponService Interface`), never `"Class"`.
  - Concrete classes MUST have the word `"Class"` appended (e.g., `4.2 CouponService Class`).
- [ ] **HTML Structure/Numbering**: The final HTML output MUST NOT contain arbitrary `Layer: X` headings. All tables must be sequentially numbered in an unbroken sequence starting with a major prefix (e.g., `4.1`, `4.2`, `4.3`).

Once all items in the checklist are validated, the output is considered fully complete.
