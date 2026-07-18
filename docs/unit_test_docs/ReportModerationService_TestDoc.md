# ReportModerationService Unit Test Documentation

---

## Function: `GetAllCourseReportsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllCourseReportsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllCourseReportsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve a paginated list of course reports by status and map them to DTOs.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Reports list is empty |  | O |  |
|  |  | Reports list has items |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns mapped `PagedReportResponseDto` |  |  | O |
|  | **Exception** |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |


---

## Function: `GetAllCourseReviewReportsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllCourseReviewReportsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllCourseReviewReportsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve a paginated list of course review reports by status and map them to DTOs.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Reports list is empty |  | O |  |
|  |  | Reports list has items |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns mapped `PagedReportResponseDto` |  |  | O |
|  | **Exception** |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |


---

## Function: `GetAllLessonReviewReportsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllLessonReviewReportsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllLessonReviewReportsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve a paginated list of lesson review reports by status and map them to DTOs.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Reports list is empty |  | O |  |
|  |  | Reports list has items |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns mapped `PagedReportResponseDto` |  |  | O |
|  | **Exception** |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |


---

## Function: `GetReportStatsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetReportStatsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetReportStatsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">22</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Calculate aggregate statistics across all report types (course, course review, lesson review) for pending, resolved today, and rejected today statuses.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Multiple report repositories return counts |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns accurately summed `ReportStatsResponseDto` |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `ResolveCourseReportAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ResolveCourseReportAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ResolveCourseReportAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">20</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Process resolution actions for a course report, applying penalties, clearing caches, and sending relevant notifications.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">13</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">13</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | CourseId missing |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Report not found |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course not found |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | DB save throws exception |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Current status escalated, resolver is staff |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Current status escalated, resolver is admin |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Status resolved, remove content = true |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Status resolved, remove content = false |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Status rejected |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Status under review |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Status escalated |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Reporter ID is null |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Remove content = false, applies warning |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns `true` |  |  |  |  |  |  | O | O | O | O | O | O | O | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  | O |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `UnauthorizedAccessException` |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  | **State** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Applies strike, clears cache, notifies reporter |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Sends warning to instructor, does not apply strike |  |  |  |  |  |  |  |  | O |  |  |  |  | O |
|  |  | Sends dismissed notification to reporter |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Sends under review notification to reporter |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Sends escalated notification to reporter and admin |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Skips notifying reporter if ReporterId is null |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N | N | N | N | N | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 |  |  |  |  |  |  |  |  |


---

## Function: `ResolveCourseReviewReportAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ResolveCourseReviewReportAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ResolveCourseReviewReportAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">16</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Process resolution actions for a course review report, applying penalties, removing reviews, and sending notifications.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">12</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">12</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ReviewId missing |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Report not found |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Review not found |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Resolved, remove=true, valid enrollment |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Resolved, remove=false, valid enrollment |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Rejected |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Under review |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Escalated |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Enrollment is null, remove=true |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | UserId is zero, remove=true |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Remove=false, Enrollment is null |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Remove=false, UserId is zero |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns `true` |  |  |  |  | O | O | O | O | O | O | O | O | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O | O |  |  |  |  |  |  |  |  |  |
|  | **State** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Applies strike, removes review, notifies reporter |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Sends warning to author, does not apply strike |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Sends dismissed notification to reporter |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Sends under review notification to reporter |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Sends escalated notification to reporter and admin |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Removes review without strike |  |  |  |  |  |  |  |  |  | O | O |  |  |
|  |  | Does not send warning to author |  |  |  |  |  |  |  |  |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | N | N | N | N | A | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  |  |  |  |  |  |  |  |  |


---

## Function: `ResolveLessonReviewReportAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ResolveLessonReviewReportAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ResolveLessonReviewReportAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">16</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Process resolution actions for a lesson review report, matching course review behavior exactly.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">12</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">12</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ReviewId missing |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Report not found |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Review not found |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Resolved, remove=true, valid enrollment |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Resolved, remove=false, valid enrollment |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Rejected |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Under review |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Escalated |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Enrollment is null, remove=true |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | UserId is zero, remove=true |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Remove=false, Enrollment is null |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Remove=false, UserId is zero |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns `true` |  |  |  |  | O | O | O | O | O | O | O | O | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O | O |  |  |  |  |  |  |  |  |  |
|  | **State** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Applies strike, removes review, notifies reporter |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Sends warning to author, does not apply strike |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Sends dismissed notification to reporter |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Sends under review notification to reporter |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Sends escalated notification to reporter and admin |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Removes review without strike |  |  |  |  |  |  |  |  |  | O | O |  |  |
|  |  | Does not send warning to author |  |  |  |  |  |  |  |  |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | N | N | N | N | A | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  |  |  |  |  |  |  |  |  |


---

## Function: `ResolveLinkActionAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ResolveLinkActionAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ResolveLinkActionAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">9</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Return correct root/link based on report type.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">1</td>
  </tr>
</table>

(Private)

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Report type is unknown |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns `/` (root URL) |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `ValidateReportResolutionAccessAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ValidateReportResolutionAccessAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ValidateReportResolutionAccessAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">9</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Verify that the resolver has manager (staff/admin) privileges.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

(Private)

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Resolver account is null |  | O |  |
|  |  | Resolver manager object is null |  |  | O |
| **Confirm** | **Exception** |  |  |  |  |
|  |  | Throws `UnauthorizedAccessException` |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |
