# AiModelManagementService Unit Test Documentation

---

## Function: `GetAllModelsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllModelsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllModelsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve all AI models for admin view, handling empty or null result sets by throwing KeyNotFoundException.</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Repository returns list of models |  | O |  |  |
|  |  | Repository returns empty list |  |  | O |  |
|  |  | Repository returns null |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns mapped DTO list |  | O |  |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 |


---

## Function: `GetPagedModelsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPagedModelsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPagedModelsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">8</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve paged AI models, handling pagination defaults and empty/null result sets.</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | Repository returns items and totalCount |  | O |  |  | O | O |
|  |  | Repository returns empty items list |  |  | O |  |  |  |
|  |  | Repository returns null items |  |  |  | O |  |  |
|  | **Input** |  |  |  |  |  |  |  |
|  |  | `Page` > 0, `PageSize` > 0 |  | O | O | O |  |  |
|  |  | `Page` <= 0 |  |  |  |  | O |  |
|  |  | `PageSize` <= 0 |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | Returns `PagedResult` with items |  | O |  |  | O | O |
|  |  | `Page` defaults to 1 |  |  |  |  | O |  |
|  |  | `PageSize` defaults to 10 |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | A | B | B |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 |  |  |


---

## Function: `GetModelByIdAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetModelByIdAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetModelByIdAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve a specific AI model by ID.</td>
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
|  |  | Model exists in DB |  | O |  |
|  |  | Model does not exist in DB |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns mapped DTO |  | O |  |
|  | **Exception** |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |


---

## Function: `AddModelAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">AddModelAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">AddModelAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">24</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Add a new AI model to the database.</td>
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
|  |  | Repository works normally |  | O |  |
|  |  | Repository throws `AiModelException` on save |  |  | O |
|  | **Input** |  |  |  |  |
|  |  | Valid `CreateAiModelRequest` |  | O | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns mapped added DTO |  | O |  |
|  | **Exception** |  |  |  |  |
|  |  | Throws `BadRequestException` |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |


---

## Function: `UpdateModelAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateModelAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateModelAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">17</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Update an existing AI model's properties.</td>
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
|  |  | Model exists, repo works normally |  | O |  |  |
|  |  | Model does not exist |  |  | O |  |
|  |  | Repo throws `AiModelException` on save |  |  |  | O |
|  | **Input** |  |  |  |  |  |
|  |  | Valid `UpdateAiModelRequest` |  | O | O | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns mapped updated DTO |  | O |  |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  | O |  |
|  |  | Throws `BadRequestException` |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 |


---

## Function: `ToggleModelStatusAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ToggleModelStatusAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ToggleModelStatusAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">14</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Toggle the active/inactive status of a model.</td>
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

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Model exists, status is Active |  | O |  |  | O |
|  |  | Model exists, status is Inactive |  |  | O |  |  |
|  |  | Model does not exist |  |  |  | O |  |
|  |  | Repo throws `AiModelException` on save |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Returns `true`, sets Inactive |  | O |  |  |  |
|  |  | Returns `true`, sets Active |  |  | O |  |  |
|  | **Exception** |  |  |  |  |  |  |
|  |  | Throws `KeyNotFoundException` |  |  |  | O |  |
|  |  | Throws `BadRequestException` |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 | DFID002 |


---

## Function: `GetModelIdsByType`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetModelIdsByType</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetModelIdsByType</td>
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
    <td colspan="18">Fetch a list of IDs for a specific AI model type.</td>
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
|  |  | Repository returns models by type |  | O |
|  | **Input** |  |  |  |
|  |  | Valid model type string |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns list of integers (IDs) |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `GetModelsByTypeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetModelsByTypeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetModelsByTypeAsync</td>
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
    <td colspan="18">Fetch models by type utilizing Redis caching.</td>
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
|  |  | Cache hit (data exists in Redis) |  | O |  |
|  |  | Cache miss (null from Redis) |  |  | O |
|  | **Input** |  |  |  |  |
|  |  | Valid model type string |  | O | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Returns cached models |  | O |  |
|  |  | Fetches from repo, caches, returns models |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |
