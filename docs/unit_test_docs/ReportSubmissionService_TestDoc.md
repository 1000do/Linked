# ReportSubmissionService Unit Test Documentation

---

## Function: `CreateCourseReportAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateCourseReportAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateCourseReportAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">19</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate constraints and create a report for a course.</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">9</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="6" style="text-align: center;">13</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Valid request, normal user, valid course |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course not found |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course is pending/draft/under review/rejected |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Course is archived and locked (3 flags) |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Reporter is the instructor of the course |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Reporter is not enrolled |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Reason is empty |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Reporter already has a pending report |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | DB save throws exception |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Reporter is a manager |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Course is archived but not locked, not enrolled |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Valid request, no managers exist |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Valid request, multiple statuses checked |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns `true` |  | O |  |  |  |  |  |  |  |  |  |  | O |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  |  |  | O | O | O | O | O | O | O | O | O |  | O |
|  | **State** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Sends bulk notification to managers |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Does not send bulk notification |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | B | B | N | A | A | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 | DFID008 | DFID009 | DFID010 |  | DFID011 |


---

## Function: `CreateCourseReviewReportAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateCourseReviewReportAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateCourseReviewReportAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">19</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate constraints and create a report for a course review.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">8</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | Valid request |  | O |  |  |  |  |  |  |  |
|  |  | Review not found |  |  | O |  |  |  |  |  |  |
|  |  | Reason is empty |  |  |  | O |  |  |  |  |  |
|  |  | Reporter already has a pending report |  |  |  |  | O |  |  |  |  |
|  |  | DB save throws exception |  |  |  |  |  | O |  |  |  |
|  |  | Review IsRemoved property is true |  |  |  |  |  |  | O |  |  |
|  |  | Review status is removed |  |  |  |  |  |  |  | O |  |
|  |  | Review status is violating |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns `true` |  | O |  |  |  |  |  |  |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  |  |  | O | O | O | O | O | O |
|  | **State** |  |  |  |  |  |  |  |  |  |  |
|  |  | Sends bulk notification to managers |  | O |  |  |  |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | B | B | N | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 |


---

## Function: `CreateLessonReviewReportAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateLessonReviewReportAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateLessonReviewReportAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">19</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate constraints and create a report for a lesson review.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">8</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | Valid request |  | O |  |  |  |  |  |  |  |
|  |  | Review not found |  |  | O |  |  |  |  |  |  |
|  |  | Reason is empty |  |  |  | O |  |  |  |  |  |
|  |  | Reporter already has a pending report |  |  |  |  | O |  |  |  |  |
|  |  | DB save throws exception |  |  |  |  |  | O |  |  |  |
|  |  | Review IsRemoved property is true |  |  |  |  |  |  | O |  |  |
|  |  | Review status is removed |  |  |  |  |  |  |  | O |  |
|  |  | Review status is violating |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns `true` |  | O |  |  |  |  |  |  |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  |  |  | O | O | O | O | O | O |
|  | **State** |  |  |  |  |  |  |  |  |  |  |
|  |  | Sends bulk notification to managers |  | O |  |  |  |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | B | B | N | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 |


---

## Function: `GetMyCourseReportsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetMyCourseReportsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetMyCourseReportsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">4</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve course reports submitted by a specific user.</td>
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
|  |  | User has submitted reports |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns mapped list of `MyCourseReportResponse` |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `GetMyReviewReportsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetMyReviewReportsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetMyReviewReportsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve and combine course and lesson review reports submitted by a user, ordered descending by creation date.</td>
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
|  |  | User has submitted both course and lesson review reports |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns mapped list of `MyReviewReportResponse` sorted descending by date |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `GetReportsOnMyCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetReportsOnMyCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetReportsOnMyCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Allow an instructor to retrieve reports filed against their own course.</td>
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
|  |  | Requesting user is NOT the owner of the course |  | O |  |
|  |  | Requesting user is the owner of the course |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns mapped list of `CourseReportDetailResponse` |  |  | O |
|  | **Exception** |  |  |  |  |
|  |  | Throws `UnauthorizedAccessException` |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |
