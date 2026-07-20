## Function: `EnrollFreeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">EnrollFreeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">EnrollFreeAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">44</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for EnrollFreeAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">9</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |  |  |  |  |
|  |  | Course Not Published |  |  | O |  |  |  |  |  |  |  |
|  |  | Course Is Paid |  |  |  | O |  |  |  |  |  |  |
|  |  | Instructor Enrolls Own Course |  |  |  |  | O |  |  |  |  |  |
|  |  | Already Enrolled |  |  |  |  |  | O |  |  |  |  |
|  |  | Success, Notifies Instructor |  |  |  |  |  |  | O |  |  |  |
|  |  | Success, Instructor Null |  |  |  |  |  |  |  | O |  |  |
|  |  | Success, Fallback Name |  |  |  |  |  |  |  |  | O |  |
|  |  | Exception Thrown |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws InvalidOperationException |  | O | O | O | O | O |  |  |  |  |
|  |  | Creates Enrollment |  |  |  |  |  |  | O | O | O |  |
|  |  | Sends Notification |  |  |  |  |  |  | O |  | O |  |
|  |  | Rollbacks Transaction |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |  |

## Function: `GetProgressAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetProgressAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetProgressAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">28</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetProgressAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">6</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">6</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | Not Enrolled, Not Owner |  | O |  |  |  |  |  |
|  |  | Not Enrolled, Course Null |  |  | O |  |  |  |  |
|  |  | Not Enrolled, Is Owner |  |  |  | O | O |  |  |
|  |  | Enrolled |  |  |  |  |  | O | O |
|  |  | Course Stats Null |  |  |  |  | O |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | Returns Null |  | O | O |  |  |  |  |
|  |  | Returns Virtual Progress |  |  |  | O | O |  |  |
|  |  | Returns Real Progress |  |  |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |

## Function: `UpdateProgressAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateProgressAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateProgressAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">31</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UpdateProgressAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">7</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">7</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |
|  |  | Not Enrolled, Is Owner |  | O |  |  |  |  |  |  |
|  |  | Not Enrolled, Not Owner |  |  | O |  |  |  |  |  |
|  |  | Already Completed Material |  |  |  | O |  |  |  |  |
|  |  | Not Completed, Doesn't Complete Course |  |  |  |  | O |  |  |  |
|  |  | Not Completed, Course Stats Null |  |  |  |  |  | O |  |  |
|  |  | Not Completed, Completes Course |  |  |  |  |  |  | O |  |
|  |  | Total Materials Zero |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |
|  |  | Returns Early |  | O |  | O |  |  |  |  |
|  |  | Throws InvalidOperationException |  |  | O |  |  |  |  |  |
|  |  | Adds Completion |  |  |  |  | O | O | O | O |
|  |  | Completes Course |  |  |  |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |

## Function: `GetMyEnrolledCoursesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetMyEnrolledCoursesAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetMyEnrolledCoursesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">24</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetMyEnrolledCoursesAsync</td>
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
    <td colspan="6" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Has Enrollments |  | O |  |
|  |  | Empty Enrollments |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns Enrolled Courses |  | O |  |
|  |  | Returns Empty List |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |
