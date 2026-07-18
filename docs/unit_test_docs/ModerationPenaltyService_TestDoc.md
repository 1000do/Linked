# ModerationPenaltyService Unit Test Documentation

---

## Function: `ProcessCourseStrikeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ProcessCourseStrikeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ProcessCourseStrikeAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">49</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Manage penalty strikes on a course, enforcing warnings or archiving/lockouts upon reaching 3 strikes.</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">7</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |
|  |  | Course is null |  | O |  |  |  |  |  |  |
|  |  | Under 3 strikes, has InstructorId |  |  | O |  |  |  |  |  |
|  |  | Under 3 strikes, no InstructorId |  |  |  |  |  | O |  |  |
|  |  | Reaches 3 strikes, has InstructorId |  |  |  | O |  |  |  |  |
|  |  | Reaches 3 strikes, no InstructorId |  |  |  |  |  |  | O |  |
|  |  | Reaches 3 strikes, instructor not found |  |  |  |  |  |  |  | O |
|  |  | Already at 3 strikes, has InstructorId |  |  |  |  | O |  |  |  |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |
|  |  | Returns `false` |  | O |  |  |  |  |  |  |
|  |  | Returns `true` |  |  | O | O | O | O | O | O |
|  | **State** |  |  |  |  |  |  |  |  |  |
|  |  | Increments flag count |  |  | O | O | O | O | O |  |
|  |  | Caps flag count at 3 |  |  |  |  | O |  |  |  |
|  |  | Archives course |  |  |  | O | O |  | O | O |
|  |  | Sends warning notification |  |  | O |  |  |  |  |  |
|  |  | Sends discontinuation notice |  |  |  | O | O |  |  | O |
|  |  | Adds severe instructor lockout |  |  |  | O | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N | A | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |


---

## Function: `ProcessReviewStrikeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ProcessReviewStrikeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ProcessReviewStrikeAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">46</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Manage penalty strikes on a user account for review violations, applying progressive lockouts and notifications.</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">7</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |
|  |  | Account not found |  | O |  |  |  |  |  |  |
|  |  | First strike (Flag Count = 0) |  |  | O |  |  |  |  |  |
|  |  | Second strike (Flag Count = 1) |  |  |  | O |  |  |  |  |
|  |  | Third strike, instructor approved |  |  |  |  | O |  |  |  |
|  |  | Already at 3 strikes |  |  |  |  |  | O |  |  |
|  |  | Third strike, instructor null |  |  |  |  |  |  | O |  |
|  |  | Third strike, instructor not approved |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |
|  |  | Returns `false` |  | O |  |  |  |  |  |  |
|  |  | Returns `true` |  |  | O | O | O | O | O | O |
|  | **State** |  |  |  |  |  |  |  |  |  |
|  |  | Sends 1st warning notice |  |  | O |  |  |  |  |  |
|  |  | Adds moderate lockout, sends 2nd notice |  |  |  | O |  |  |  |  |
|  |  | Adds severe lockout, bans account, sends 3rd notice, sends SignalR lockout |  |  |  |  | O | O | O | O |
|  |  | Notifies students of suspension |  |  |  |  | O | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N | N | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |


---

## Function: `NotifyStudentsAboutInstructorSuspensionAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">NotifyStudentsAboutInstructorSuspensionAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">NotifyStudentsAboutInstructorSuspensionAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">26</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Notify unique enrolled students when an instructor is suspended.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">5</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | Instructor not found |  | O |  |  |  |  |
|  |  | Instructor has no courses |  |  | O |  |  |  |
|  |  | Courses list is null |  |  |  |  | O |  |
|  |  | Courses have no students |  |  |  |  |  | O |
|  |  | Instructor has courses with unique and overlapping students |  |  |  | O |  |  |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | Returns `false` |  | O | O |  | O | O |
|  |  | Returns `true` |  |  |  | O |  |  |
|  | **State** |  |  |  |  |  |  |  |
|  |  | Notifies each unique student exactly once |  |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |
