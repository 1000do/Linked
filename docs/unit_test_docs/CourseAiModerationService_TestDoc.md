# CourseAiModerationService Unit Test Documentation

---

## Function: `StartCourseModerationAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">StartCourseModerationAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">StartCourseModerationAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">15</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Enforce business validation checks, update status, and queue AI moderation for background processing.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Valid moderation request |  | O | O |
|  |  | Delegate executes successfully |  | O |  |
|  |  | Delegate throws exception |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Updates course status |  | O | O |
|  |  | Queues background work item |  | O | O |
|  |  | Returns `true` |  | O | O |
|  | **Exception** |  |  |  |  |
|  |  | Swallows delegate exception safely |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |


---

## Function: `GetCourseForModerationAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">GetCourseForModerationAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">GetCourseForModerationAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">15</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Retrieve course details for moderation, prioritizing cache before falling back to the database.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Cache hit |  | O |  |  |
|  |  | Cache miss, course not found in DB |  |  | O |  |
|  |  | Cache miss, course exists in DB |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns cached response directly |  | O |  |  |
|  |  | Returns null |  |  | O |  |
|  |  | Caches response and returns mapped data |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |


---

## Function: `UpdateCourseStatusAndClearCacheAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">UpdateCourseStatusAndClearCacheAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">UpdateCourseStatusAndClearCacheAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">4</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Update the course status and clear its associated cache.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Valid courseId and status |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Executes status update command |  | O |
|  |  | Removes course moderation cache |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `ExtractPlainTextForModerationResponse`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">ExtractPlainTextForModerationResponse</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">ExtractPlainTextForModerationResponse</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">18</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Extract plain text from HTML content for various fields within the course moderation response.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Null properties |  | O |  |  |  |
|  |  | Empty lessons list |  |  | O |  |  |
|  |  | Valid lessons and materials |  |  |  | O |  |
|  |  | Lessons with null materials |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Handles gracefully without crashing |  | O | O |  | O |
|  |  | Bypasses loops appropriately |  |  | O |  | O |
|  |  | Extracts text for main properties & materials |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |


---

## Function: `GetModerationThresholdsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">GetModerationThresholdsAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">GetModerationThresholdsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">9</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Fetch AI configuration thresholds and return them as a mapped dictionary.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Config service returns valid configs |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns dictionary of mapped thresholds |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `AssignAIModeratorsToCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">AssignAIModeratorsToCourseAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">AssignAIModeratorsToCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">26</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Integrate new AI models to a course if they are not already integrated.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Missing models in course integrations |  | O |  |
|  |  | Models already exist in course |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Executes integration command |  | O |  |
|  |  | Skips integration command |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |


---

## Function: `PrepareForCourseAIModeration`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">PrepareForCourseAIModeration</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">PrepareForCourseAIModeration</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">20</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Gather all necessary data, settings, and integrations before initiating moderation.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | All dependencies succeed |  | O |  |
|  |  | Dependency throws exception |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns complete PreparedResult |  | O |  |
|  | **Exception** |  |  |  |  |
|  |  | Rethrows exception |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |


---

## Function: `GetCourseModerationModelsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">GetCourseModerationModelsAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">GetCourseModerationModelsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">23</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Retrieve the required AI models for course moderation based on config paths or fallback.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Config paths are valid and exist in DB |  | O |  |
|  |  | Config paths are empty |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns models queried from DB |  | O |  |
|  |  | Returns models from fallback service |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |


---

## Function: `GetModelConfigPathsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">GetModelConfigPathsAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">GetModelConfigPathsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">6</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Retrieve the paths from the system configuration.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Values present in config DB |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns paths tuple |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `UpdateCourseAIIntegrationsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">UpdateCourseAIIntegrationsAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">UpdateCourseAIIntegrationsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">28</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Update the existing AI integrations or assign new ones for the course.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">7</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Integrations are null |  | O |  |  |  |
|  |  | Integrations are empty |  |  | O |  |  |
|  |  | Integrations exist and match |  |  |  | O |  |
|  |  | Integrations exist but don't match |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Calls AssignAIModeratorsToCourseAsync |  | O | O |  |  |
|  |  | Updates model and saves changes |  |  |  | O |  |
|  |  | Does not update unmatching models |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |


---

## Function: `SaveCourseAiIntegrationChangesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">SaveCourseAiIntegrationChangesAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">SaveCourseAiIntegrationChangesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">8</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Save changes to AI integrations and wrap exceptions safely.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Repository saves changes successfully |  | O |  |
|  |  | Repository throws `CourseAiIntegrationException` |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns rows affected integer |  | O |  |
|  | **Exception** |  |  |  |  |
|  |  | Throws `BadRequestException` |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |


---

## Function: `GetCourseMaterialIdsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">GetCourseMaterialIdsAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">GetCourseMaterialIdsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">7</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Gather all material IDs connected to a specific course.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Course is null |  | O |  |  |
|  |  | Lessons are null |  |  | O |  |
|  |  | Valid lessons with materials |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns empty list |  | O | O |  |
|  |  | Returns list of material IDs |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |


---

## Function: `ResolveCourseAIModerationResult`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">ResolveCourseAIModerationResult</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">ResolveCourseAIModerationResult</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">11</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Conclude AI moderation by updating the course status and notifying managers based on results.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Standard moderation result provided |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Updates status and adds feedback |  | O |
|  |  | Sends bulk notification to managers |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `EvaluateModerationFeedback`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">EvaluateModerationFeedback</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">EvaluateModerationFeedback</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">37</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Convert the AI moderation result stage logs into threat levels and specific feedback strings.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">6</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | Status Rejected with MatchFound logs |  | O |  |  |  |  |
|  |  | Status Flagged with no logs |  |  | O |  |  |  |
|  |  | Status Approved |  |  |  | O |  |  |
|  |  | Status ManualAudit |  |  |  |  | O |  |
|  |  | Status Other/Unknown |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | ThreatLevel `FlaggedOrRejected`, precise feedback |  | O |  |  |  |  |
|  |  | ThreatLevel `FlaggedOrRejected`, generic feedback |  |  | O |  |  |  |
|  |  | ThreatLevel `Approved` |  |  |  | O |  |  |
|  |  | ThreatLevel `ManualAudit` |  |  |  |  | O |  |
|  |  | ThreatLevel `None` |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |


---

## Function: `LogCourseAiModeration`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">LogCourseAiModeration</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">LogCourseAiModeration</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">49</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Maintain an audit log of all AI usage during the moderation pipeline.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Integration found for stage model |  | O |  |  |
|  |  | Integration not found for stage |  |  | O |  |
|  |  | Empty stage logs |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Saves log calling log service |  | O |  |  |
|  |  | Skips stage gracefully |  |  | O |  |
|  |  | Does nothing (exits early) |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |


---

## Function: `HandleCourseModerationWithAIAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">HandleCourseModerationWithAIAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">HandleCourseModerationWithAIAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">27</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">The main orchestrator for the AI course moderation pipeline, running through preparation, evaluation, resolving and logging.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | AI Service is unhealthy |  | O |  |  |
|  |  | Pipeline throws an exception |  |  | O |  |
|  |  | Pipeline runs successfully |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns ManualAudit status & notifies managers |  | O |  |  |
|  |  | Catches exception, logs it, returns ManualAudit |  |  | O |  |
|  |  | Completes process and returns ModerationResult |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | B | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |


---

## Function: `CreateModerationRequests`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">CreateModerationRequests</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">CreateModerationRequests</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">27</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Map the preparation result into concrete requests suitable for the semantic duplication and harmful text APIs.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Valid `PrepareForCourseAIModerationResult` |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns Tuple of Semantic/Harmful Requests |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |
