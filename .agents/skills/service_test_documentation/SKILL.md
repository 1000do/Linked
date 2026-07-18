---
name: service_test_documentation
description: Generates standardized _TestDoc.md files containing unit test execution matrices, accurate executable lines of code (LOC), and uncovered branch counts derived from ReportGenerator HTML reports.
---

# `service_test_documentation` Skill

This skill provides a 2-step workflow for generating complete `_TestDoc.md` artifacts from C# test files and HTML reports.

## Step 1: Semantic Analysis (LLM Task)

First, ensure the output directory exists: `docs\unit_test_docs` (relative to the project root). Create it if it does not exist.
Read the target `Service.cs` and `ServiceTests.cs`.
Generate the raw markdown document containing the test matrices for every function and save it to the output directory.

**Strict Markdown Structure:**
For each function, you MUST generate a markdown header, a test requirement description, and the test matrix exactly like this example snippet:

```markdown
## Function: `MyFunctionName`

- **Test requirement:** Brief description of what is being tested in this function.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
...
```

**Matrix Rules:**
- The table MUST have the headers: `| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | ... |`
- The alignment MUST be: `| :--- | :--- | :--- | :--- | :---: | :---: | ... |`
- **Category: Condition**:
  - The first sub-category row MUST look like this: `| **Condition** | **Precondition** |  |  |  | ... |`
  - The detailed preconditions follow on subsequent rows with empty Category/Sub-category columns: `|  |  | My precondition text |  | O | ... |`
  - The second sub-category row (if applicable) has an empty Category column: `|  | **Input** |  |  |  | ... |`
  - The detailed inputs follow on subsequent rows: `|  |  | My input text |  | O | ... |`
- **Category: Confirm**:
  - The first sub-category row MUST look like this: `| **Confirm** | **Return** |  |  |  | ... |`
  - The detailed returns follow on subsequent rows: `|  |  | My return text |  | O | ... |`
  - The second sub-category row (if applicable) has an empty Category column: `|  | **Exception** |  |  |  | ... |`
  - The detailed exceptions follow on subsequent rows: `|  |  | My exception text |  | O | ... |`
- *(Note: A matrix might omit `Input` or `Exception` sub-categories if not applicable).*
- The bottom rows MUST exactly be:
  - `| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | ... |`
  - `|  | **Passed/Failed** |  |  | P | F | ... |`
  - `|  | **Executed Date** |  |  | DD/MM/YYYY | DD/MM/YYYY | ... |` (Use the current date the document is generated)
  - `|  | **Defect ID** |  |  |  | DFID001 | ... |` 
- **Defect ID Rules:** `DFIDXXX` format (restarting from `001`, `002`, `003`... for each function). ONLY assign a Defect ID to a UTC column if that unit test case has an "O" mark (meaning the exception condition applies) under the `Exception` Sub-category rows. Otherwise, leave the cell blank.

## Step 2: Metric Injection (Python Script)

Once the raw Markdown with the matrices is generated, run the bundled python script to compute LOC and branch data, and to inject the exact HTML summary tables. 

**Important:** Before running this script, you must obtain the user's full name (ask them if it is not already provided in the context). Convert their name into the standard abbreviation format (e.g., "Huynh Kien An" -> "AnHK", "Nguyen Nguyen Ngoc" -> "NgocNN"). The abbreviation rule is: Capitalize the last word first, followed by the capitalized first letters of all preceding words. Pass this formatted string as the 4th argument.

```bash
python scripts/generate_summary_tables.py /path/to/generated_TestDoc.md /path/to/ServiceTests.cs /path/to/Service.html <UserInitials>
```

The script will:
1. Parse the HTML report for true LOC (handling state machines properly).
2. Count the testing attributes (`[Fact]`, `[Theory]`, etc.) in the `.cs` file.
3. Extract `coverableline` branch metrics and apply the normalization formula for `Untested`:
   `Untested = round(uncovered_branches * (total_test_methods / total_covered_branches))`
4. Sum `Total Test Cases = Passed + Failed + Untested`.
5. Prepend the perfectly formatted 8-column HTML summary table right above each function's markdown matrix.

## Step 3: Validation and Iterative Fixing (LLM Task)

**CRITICAL RULE**: Whenever this skill is used, you MUST validate your final generated `_TestDoc.md` against the following checklist. If ANY checklist item fails, you MUST iteratively fix the markdown output until all items pass.

### Validation Checklist:
- [ ] **Header Row**: Is the table header exactly `| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | ... |`?
- [ ] **Alignment Row**: Is the alignment row exactly `| :--- | :--- | :--- | :--- | :---: | :---: | ... |`?
- [ ] **Category Structure**: Do the `Condition` and `Confirm` categories have their sub-category labels (`Precondition`, `Input`, `Return`, `Exception`) perfectly aligned one row above their respective detail rows?
- [ ] **Detail Alignment**: Do all detailed precondition/input/return/exception rows have empty `Category` and `Sub-category` columns (i.e. `|  |  | Detail text |  | O | ... |`)?
- [ ] **Result Rows**: Are the bottom 4 rows exactly `Result -> Type`, `Passed/Failed`, `Executed Date`, and `Defect ID`?
- [ ] **Defect IDs**: Are Defect IDs strictly formatted as `DFIDXXX` (starting at 001 for each function)?
- [ ] **Defect ID Placement**: Are Defect IDs ONLY placed in UTC columns that have an "O" marked under the `Exception` sub-category?
- [ ] **User Name Format**: Did you prompt the user for their full name and format it properly (e.g. "Nguyen Nguyen Ngoc" -> "NgocNN") and pass it to the metric injection script?
- [ ] **Script Execution**: Did you successfully execute the `generate_summary_tables.py` script?
- [ ] **HTML Generation**: Did you successfully execute the `generate_html_test_doc.py` script?
- [ ] **No Overall Coverage Line**: Ensure there is no `**Overall File Coverage:**` text at the top of the file (agents must read the HTML report directly for coverage details).

Use the files in the `references/` directory as ground-truth examples of how perfectly formatted documents should look.

## Step 4: Generate HTML Test Document (Python Script)

After generating the complete `_TestDoc.md`, you MUST run the following script to create the HTML version (`_TestDoc.html`):

```bash
python scripts/generate_html_test_doc.py
```
This script will read the markdown file and generate an HTML file containing the summary table and the detailed test matrix, strictly formatted with Tahoma font, size 8, and size 11 for the "O" marks, with the category/sub-category labels stripped out as required.

## Step 5: Update Master Summary Tables (Python Scripts)

After successfully generating the `_TestDoc.md` files for all requested services in the output directory, you MUST run the following two python scripts to update the master lists:

```bash
python scripts/extract.py
python scripts/generate_function_list.py
```
This will update `docs\unit_test_docs\unit_test_summary_table.md` and `docs\unit_test_docs\FunctionList.md`.
