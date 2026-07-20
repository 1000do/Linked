## Function: `GetPendingCoursesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPendingCoursesAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPendingCoursesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">3</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetPendingCoursesAsync</td>
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
|  |  | Valid Filter |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns Paged Pending Courses |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `ApproveCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ApproveCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ApproveCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">63</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ApproveCourseAsync</td>
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
    <td colspan="6" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |  |  |  |
|  |  | Course Is Pending |  |  | O | O | O | O | O | O | O |
|  |  | Instructor Null |  |  |  | O | O | O | O |  |  |
|  |  | Has Enrolled Users |  |  | O |  |  |  |  |  | O |
|  |  | With Materials Pending/Removed |  |  |  |  | O |  |  |  |  |
|  |  | With Materials Active |  |  |  |  |  | O |  |  |  |
|  |  | With Lessons Pending |  |  |  |  |  |  | O |  |  |
|  |  | With Lessons Active |  |  |  |  |  |  |  | O |  |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns False |  | O |  |  |  |  |  |  |  |
|  |  | Approves Course (Returns True) |  |  | O | O | O | O | O | O | O |
|  |  | Updates Material Status |  |  |  |  | O |  |  |  |  |
|  |  | Updates Lesson Status |  |  |  |  |  |  | O |  |  |
|  |  | Sends Bulk Notifications |  |  | O |  |  |  |  |  | O |
|  |  | Sends Instructor Notification |  |  | O |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |  |  |  |  |  |  |

## Function: `RejectCourseDetailedAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RejectCourseDetailedAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RejectCourseDetailedAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">105</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RejectCourseDetailedAsync</td>
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
    <td colspan="1" style="text-align: center;">10</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">13</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | DB Update Exception |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Empty Items |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Target File With Lesson |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Target File Without Lesson |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Target File Material Not Found |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Target Lesson Title |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Target Course Field New |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Target Course Field Existing |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Target Course Various Fields |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | General Pending Course |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns False |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws BadRequestException |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Default Feedback |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Updates Material And Lesson |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Updates Material Only |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Ignores Missing Material |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Updates Lesson Only |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Adds Field Feedback |  |  |  |  |  |  |  |  | O | O | O |  |  |  |
|  |  | Rejects Course |  |  |  | O | O | O | O | O | O | O | O | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N | N | A | N | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |  | DFID003 |  |  |  |  |  |  |  |

## Function: `FlagCourseDetailedAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">FlagCourseDetailedAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">FlagCourseDetailedAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">131</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for FlagCourseDetailedAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">15</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">12</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">15</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 | UTCID15 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Has 3 Flags |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | DB Exception |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | First Flag |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Second Flag |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Third Flag |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Instructor Null |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Material Pending |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Lesson Pending |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Target File With Lesson |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Target Lesson Title |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Target Course Fields |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | General Published Course |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns False |  | O | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws Exception |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns True |  |  |  |  | O | O | O | O | O | O | O | O | O | O |  |  |
|  |  | Sends 1st Warning |  |  |  |  | O |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Sends 2nd Warning |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Sends 3rd Warning & Archives |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Updates Material To Active |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Updates Lesson To Active |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Flags Material & Rejects Lesson |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Adds Target Feedback |  |  |  |  |  |  |  |  |  |  |  | O | O | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | N | N | N | N | N | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  |  |  |  |  |  |  |  |  |  |  |  |

## Function: `GetCourseModerationStatsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetCourseModerationStatsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetCourseModerationStatsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">3</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetCourseModerationStatsAsync</td>
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
|  |  | Valid Call |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns Stats |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `RejectCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RejectCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RejectCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">20</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RejectCourseAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">3</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |
|  |  | Instructor Null |  |  | O |  |
|  |  | With Instructor |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns False |  | O |  |  |
|  |  | Rejects Course (Returns True) |  |  | O | O |
|  |  | Sends Notification |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |  |

## Function: `FlagCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">FlagCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">FlagCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">41</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for FlagCourseAsync</td>
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
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">6</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |  |
|  |  | Course Has 3 Flags |  |  | O |  |  |  |  |
|  |  | First Flag |  |  |  | O |  |  |  |
|  |  | Second Flag |  |  |  |  | O |  |  |
|  |  | Third Flag |  |  |  |  |  | O |  |
|  |  | Instructor Null |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | Returns False |  | O | O |  |  |  |  |
|  |  | Flags Course (Returns True) |  |  |  | O | O | O | O |
|  |  | Sends Warning Notification |  |  |  | O | O | O |  |
|  |  | Updates To Archived |  |  |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |  |  |

## Function: `UnflagCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UnflagCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UnflagCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">29</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UnflagCourseAsync</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |
|  |  | Flag Count Is Zero |  |  | O |  |  |  |
|  |  | Flag Count > 0 |  |  |  | O |  |  |
|  |  | Flag Count 3 And Archived |  |  |  |  | O |  |
|  |  | With Instructor |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | Returns False |  | O | O |  |  |  |
|  |  | Unflags Course (Returns True) |  |  |  | O | O | O |
|  |  | Updates Status To Rejected |  |  |  |  | O |  |
|  |  | Sends Notification |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |  |

## Function: `NotifyAdminAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">NotifyAdminAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">NotifyAdminAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">4</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for NotifyAdminAsync</td>
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
|  |  | Admin Id Found |  | O |  |
|  |  | Admin Id Null |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Sends Notification |  | O |  |
|  |  | Does Not Send Notification |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |
