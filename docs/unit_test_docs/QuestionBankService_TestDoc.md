## Function: `AddQuestionAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">AddQuestionAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">AddQuestionAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">21</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for AddQuestionAsync</td>
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

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Options Less Than Two |  | O |  |  |  |
|  |  | No Correct Option |  |  | O |  |  |
|  |  | Course Not Found Or Not Owner |  |  |  | O |  |
|  |  | Valid Request |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Throws ArgumentException |  | O | O |  |  |
|  |  | Throws UnauthorizedAccessException |  |  |  | O |  |
|  |  | Adds Question And Returns Response |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

## Function: `UpdateQuestionAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateQuestionAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateQuestionAsync</td>
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
    <td colspan="18">Tests for UpdateQuestionAsync</td>
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
|  |  | Options Less Than Two |  | O |  |  |  |  |
|  |  | No Correct Option |  |  | O |  |  |  |
|  |  | Question Not Found |  |  |  | O |  |  |
|  |  | Course Not Found Or Not Owner |  |  |  |  | O |  |
|  |  | Valid Request |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | Throws ArgumentException |  | O | O |  |  |  |
|  |  | Throws KeyNotFoundException |  |  |  | O |  |  |
|  |  | Throws UnauthorizedAccessException |  |  |  |  | O |  |
|  |  | Clears Old Options And Updates |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |

## Function: `DeleteQuestionAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">DeleteQuestionAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">DeleteQuestionAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for DeleteQuestionAsync</td>
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
|  |  | Question Not Found |  | O |  |  |
|  |  | Course Not Found Or Not Owner |  |  | O |  |
|  |  | Valid Request |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns Silently |  | O |  |  |
|  |  | Throws UnauthorizedAccessException |  |  | O |  |
|  |  | Deletes Question |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `GetQuestionsByCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetQuestionsByCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetQuestionsByCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetQuestionsByCourseAsync</td>
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
|  |  | Course Not Found Or Not Owner |  | O |  |
|  |  | Valid Request |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Throws UnauthorizedAccessException |  | O |  |
|  |  | Returns Mapped Questions |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `GetQuestionsByLessonAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetQuestionsByLessonAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetQuestionsByLessonAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">9</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetQuestionsByLessonAsync</td>
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
|  |  | Empty Questions |  | O |  |  |
|  |  | Course Not Found Or Not Owner |  |  | O |  |
|  |  | Valid Request |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns Empty List |  | O |  |  |
|  |  | Throws UnauthorizedAccessException |  |  | O |  |
|  |  | Returns Mapped Questions |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `GetLessonsSummaryByCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetLessonsSummaryByCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetLessonsSummaryByCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">16</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetLessonsSummaryByCourseAsync</td>
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
|  |  | Course Not Found Or Not Owner |  | O |  |
|  |  | Valid Request |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Throws UnauthorizedAccessException |  | O |  |
|  |  | Filters Removed Lessons And Returns Summary |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |
