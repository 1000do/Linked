## Function: `CreateLessonAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateLessonAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateLessonAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">65</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for CreateLessonAsync</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">6</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | Course Published |  | O |  |  |  |  |  |
|  |  | DB Update Exception |  |  | O |  |  |  |  |
|  |  | Course Not Found |  |  |  | O |  |  |  |
|  |  | Without Stripe, Limit Exceeded |  |  |  |  | O |  |  |
|  |  | Duplicate Title |  |  |  |  |  | O |  |
|  |  | With Thumbnail File |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | Updates To Draft |  | O |  |  |  |  |  |
|  |  | Throws Exception |  |  | O | O |  |  |  |
|  |  | Throws BadRequestException |  |  |  |  | O | O |  |
|  |  | Uploads Image |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |

## Function: `UpdateLessonTitleAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateLessonTitleAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateLessonTitleAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">33</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UpdateLessonTitleAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Course Published |  | O |  |  |
|  |  | Duplicate Title |  |  | O |  |
|  |  | Lesson Not Found |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Updates To Draft |  | O |  |  |
|  |  | Throws BadRequestException |  |  | O |  |
|  |  | Throws Exception |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `AddMaterialToLessonAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">AddMaterialToLessonAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">AddMaterialToLessonAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">121</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for AddMaterialToLessonAsync</td>
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
    <td colspan="6" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">10</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | Course Published |  | O |  |  |  |  |  |
|  |  | Limit Exceeded (No Stripe) |  |  | O |  |  |  |  |
|  |  | Video Replaces Video |  |  |  | O |  |  |  |
|  |  | Only Update Metadata |  |  |  |  | O |  |  |
|  |  | Lesson Not Found |  |  |  |  |  | O |  |
|  |  | With Material File |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | Updates To Draft |  | O |  |  |  |  |  |
|  |  | Throws BadRequestException |  |  | O |  |  |  |  |
|  |  | Moves Old To Trash |  |  |  | O |  |  |  |
|  |  | No Upload |  |  |  |  | O |  |  |
|  |  | Throws Exception |  |  |  |  |  | O |  |
|  |  | Uploads Video |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |

## Function: `DeleteLessonAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">DeleteLessonAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">DeleteLessonAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">23</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for DeleteLessonAsync</td>
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
|  |  | Instructor Owns Lesson |  | O |  |  |
|  |  | Has Materials |  |  | O |  |
|  |  | Lesson Not Found |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Deletes Lesson |  | O |  |  |
|  |  | Moves Materials To Trash |  |  | O |  |
|  |  | Throws Exception |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `RemoveMaterialAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RemoveMaterialAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RemoveMaterialAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">18</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RemoveMaterialAsync</td>
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
|  |  | Material Found |  | O |  |
|  |  | Material Not Found |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Soft Deletes Material |  | O |  |
|  |  | Throws Exception |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `GetTrashMaterialsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetTrashMaterialsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetTrashMaterialsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">13</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetTrashMaterialsAsync</td>
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
|  |  | Trashed Exist |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns Trashed Materials |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `RestoreMaterialAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RestoreMaterialAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RestoreMaterialAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">50</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RestoreMaterialAsync</td>
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
    <td colspan="6" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Material In Trash |  | O |  |  |
|  |  | Restores With Update Status |  |  | O |  |
|  |  | Active Video Exists |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Restores Material |  | O |  |  |
|  |  | Updates Status |  |  | O |  |
|  |  | Moves Existing To Trash |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `PermanentDeleteMaterialAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">PermanentDeleteMaterialAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">PermanentDeleteMaterialAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">18</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for PermanentDeleteMaterialAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Material In Trash |  | O |  |
|  |  | Not Owner |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Hard Deletes Material |  | O |  |
|  |  | Throws UnauthorizedAccessException |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `UpdateMaterialDetailsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateMaterialDetailsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateMaterialDetailsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">34</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UpdateMaterialDetailsAsync</td>
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
|  |  | Material Not Found |  | O |  |  |  |  |  |  |
|  |  | Instructor Does Not Own Course |  |  | O |  |  |  |  |  |
|  |  | Instructor Locked Out |  |  |  | O |  |  |  |  |
|  |  | Course Is Pending |  |  |  |  | O |  |  |  |
|  |  | Course Is Archived And Flagged |  |  |  |  |  | O |  |  |
|  |  | Course Is Published |  |  |  |  |  |  | O |  |
|  |  | Course Is Draft |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |
|  |  | Throws Exception |  | O |  |  |  |  |  |  |
|  |  | Throws UnauthorizedAccessException |  |  | O |  |  |  |  |  |
|  |  | Throws BadRequestException |  |  |  | O |  |  |  |  |
|  |  | Throws InvalidOperationException |  |  |  |  | O | O |  |  |
|  |  | Updates To Draft & Clears Cache |  |  |  |  |  |  | O |  |
|  |  | Updates Without Course Change |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |

## Function: `SaveLessonChangesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SaveLessonChangesAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SaveLessonChangesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SaveLessonChangesAsync</td>
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
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | DB Exception |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Throws BadRequestException |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `SaveMaterialChangesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SaveMaterialChangesAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SaveMaterialChangesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SaveMaterialChangesAsync</td>
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
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">1</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | DB Exception |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Throws BadRequestException |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `UpdateLessonStatusByCourseIdAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateLessonStatusByCourseIdAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateLessonStatusByCourseIdAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">11</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UpdateLessonStatusByCourseIdAsync</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | No Lessons |  | O |  |
|  |  | With Lessons |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns Zero |  | O |  |
|  |  | Updates And Saves |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `UpdateLearningMaterialStatusByCourseIdAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateLearningMaterialStatusByCourseIdAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateLearningMaterialStatusByCourseIdAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">TaiTP</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">TaiTP</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">18</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UpdateLearningMaterialStatusByCourseIdAsync</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | No Lessons |  | O |  |
|  |  | With Lessons And Materials |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns Zero |  | O |  |
|  |  | Updates And Saves |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |
