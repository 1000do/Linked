## Function: `CreateCourseAsync`

<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">35</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate course creation logic including instructor rights, lockout checking, stripe onboarding status, and thumbnail uploading.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">14</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">11</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">14</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Valid Request & Instructor Exists |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Request is Null |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Instructor Not Approved |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Active Lockout Exists |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | No Stripe Limit Exceeded |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Duplicate Content (Title) |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Duplicate Description |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Duplicate WhatYouWillLearn |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Duplicate Requirements |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Duplicate Thumbnail |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Duplicate Fallback |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | With Thumbnail File |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Empty Description |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Thumbnail Upload Failure |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns CourseResponse |  | O |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Uploads Thumbnail |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Returns Null ThumbnailUrl |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Missing required data for course creation. |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | You must be an approved instructor to create a course. |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Your instructor rights are locked until... |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Instructors who have not linked a Stripe account are only allowed to create up to 2 courses. |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Content duplication detected on 'Title' |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Content duplication detected on 'Description' |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Content duplication detected on 'What You Will Learn' |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Content duplication detected on 'Requirements' |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Content duplication detected on 'Thumbnail' |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Content duplication detected on 'Some Random' |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | A | A | A | A | A | A | A | A | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 | DFID008 | DFID009 | DFID010 |  |  |  |

## Function: `UpdateCourseAsync`

<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">73</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate course update logic including permissions, lockouts, status constraints, and partial updates.</td>
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
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">12</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course is Published |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Instructor Does Not Own Course |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Active Lockout Exists |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Archived With 3 Flags |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Course Is Pending |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | No Changes |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Ext Not Found |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Empty Thumbnail Url |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | With Thumbnail File |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | With Partial Updates |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Existing Ext |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Updates Status To Draft and Not Notify Admin |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns Course |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Creates New Ext |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Keeps Old Thumbnail |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Uploads New Thumbnail |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Updates Only Provided Fields |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Updates Fingerprint |  |  |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | You do not have permission to modify this course. |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Course not found. |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Your instructor rights are locked until... |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | This course has been permanently discontinued due to policy violations and cannot be edited. |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Cannot modify course while it is pending review. |  |  |  |  |  |  | O |  |  |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | A | A | A | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 |  |  |  |  |  |  |

## Function: `UpdateCourseStatusAsync`

<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateCourseStatusAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateCourseStatusAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">27</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate course status update, ensuring all requirements for pending status and published status are met.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">17</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">13</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">17</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 | UTCID15 | UTCID16 | UTCID17 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Status Is Pending |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Unauthorized Instructor |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Active Lockout Exists |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Archived With 3 Flags |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | To Published When Not Archived |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Invalid Status |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Pending Missing WhatYouWillLearn |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Pending Missing Requirements |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Pending Missing Thumbnail |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Pending No Lessons |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Pending Missing Video In Lesson |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Pending No Stripe Exceeds 30 Mins |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Pending Free Course Exceeds 60 Mins |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Pending Valid Duration |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | To Published Not Archived |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | To Published From Archived |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Updates Status And Notify Admin |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Sends Notification |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Updates Status Successfully |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course not found. |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | You do not have permission to modify this course. |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Your instructor rights are locked until... |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | This course has been permanently discontinued due to policy violations and its status cannot be changed. |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Only archived courses can be set back to published by instructor. |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Invalid status. Allowed values are 'pending', 'archived', or 'published' (for unarchiving). |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | What you will learn is required and must be at least 20 characters long. |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Requirements are required and must be at least 20 characters long. |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Course thumbnail is required. |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Cannot submit course for review. The course must have at least one lesson. |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Cannot submit course for review. Every lesson must contain at least one video. |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | The total video duration of the course is currently 30 minutes. Instructors who have not linked a Stripe account are only allowed a maximum of 30 minutes. |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | The total video duration of the free course is currently 60 minutes. Free courses are only allowed a maximum of 60 minutes. |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | A | A | A | A | A | A | A | A | A | A | A | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 | DFID008 | DFID009 | DFID010 | DFID011 | DFID012 | DFID013 |  | DFID014 |  |

## Function: `DeleteCourseAsync`

<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">DeleteCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">DeleteCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">40</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate course deletion logic, ensuring lessons are removed and status checks are performed.</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Instructor Owns Course |  | O |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  |  | O |  |  |  |  |  |  |  |
|  |  | Active Lockout Exists |  |  |  | O |  |  |  |  |  |  |
|  |  | Has Enrollments |  |  |  |  | O |  |  |  |  |  |
|  |  | Multiple Lessons |  |  |  |  |  | O |  |  |  |  |
|  |  | Instructor Locked Out |  |  |  |  |  |  | O |  |  |  |
|  |  | Course Archived With 3 Flags |  |  |  |  |  |  |  | O |  |  |
|  |  | Course Pending |  |  |  |  |  |  |  |  | O |  |
|  |  | Unauthorized |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Deletes Course |  | O |  |  |  |  |  |  |  |  |
|  |  | Sets IsRemoved On All Lessons |  |  |  |  |  | O |  |  |  |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course not found. |  |  | O |  |  |  |  |  |  |  |
|  |  | Your instructor rights are locked until... |  |  |  | O |  |  | O |  |  |  |
|  |  | Cannot delete a course with active students. Please archive it instead. |  |  |  |  | O |  |  |  |  |  |
|  |  | This course has been permanently discontinued due to policy violations and cannot be deleted by the instructor. |  |  |  |  |  |  |  | O |  |  |
|  |  | Cannot delete course while it is pending review. |  |  |  |  |  |  |  |  | O |  |
|  |  | You do not have permission to delete this course. |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | A | N | A | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 |  | DFID004 | DFID005 | DFID006 | DFID007 |

## Function: `IntegrateAItoCourseAsync`

<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">IntegrateAItoCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">IntegrateAItoCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">26</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate AI integration logic, checking default configuration loading.</td>
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
|  |  | Config Json Is Null |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns Result With Default Thresholds |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `UpdateCourseStatusAndFeedbackAsync`

<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateCourseStatusAndFeedbackAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateCourseStatusAndFeedbackAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">17</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Validate status and feedback updates.</td>
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
|  |  | Updates Status And Threat Level |  |  | O |  |
|  |  | Null Status And Threat Level |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Updates Status And ThreatLevel |  |  | O |  |
|  |  | Keeps Old Values |  |  |  | O |
|  | **Exception** |  |  |  |  |  |
|  |  | Course X not found |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |  |
