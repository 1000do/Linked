# InstructorApprovalService Unit Test Documentation

---

## Function: `GetPendingListAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPendingListAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPendingListAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetPendingListAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">4</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Retrieve a paginated list of pending instructors and correctly map navigation properties to DTOs.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Instructors exist |  | O |  |  |  |
|  |  | Instructors are empty |  |  | O |  |  |
|  |  | Instructor navigation property is null |  |  |  | O |  |
|  |  | User navigation property is null |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | PagedResult with correct items |  | O |  |  |  |
|  |  | Empty PagedResult |  |  | O |  |  |
|  |  | Maps properties to default values (N/A) |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

---

## Function: `ApproveOrRejectAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ApproveOrRejectAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ApproveOrRejectAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">9</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ApproveOrRejectAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">4</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Update the approval status of an instructor, set rejection reason if applicable, and send notification.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Instructor not found |  | O |  |  |  |
|  |  | Instructor found |  |  | O | O | O |
|  | **Input** |  |  |  |  |  |  |
|  |  | Status is Rejected |  |  | O |  |  |
|  |  | Status is Approved |  |  |  | O |  |
|  |  | Status is Other |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Returns false |  | O |  |  |  |
|  |  | Sets reason, returns true |  |  | O |  |  |
|  |  | Clears reason, returns true |  |  |  | O |  |
|  |  | Unchanged reason, returns true |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

---

## Function: `GetDetailAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetDetailAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetDetailAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetDetailAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">4</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Retrieve the detail of an instructor by ID and map correctly to a DTO, handling null navigation properties gracefully.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Instructor not found |  | O |  |  |  |
|  |  | Instructor found with valid navigations |  |  | O |  |  |
|  |  | Instructor found with null InstructorNavigation |  |  |  | O |  |
|  |  | Instructor found with null UserNavigation |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Returns null |  | O |  |  |  |
|  |  | Returns mapped DTO |  |  | O |  |  |
|  |  | Maps properties to default values (N/A) |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |
