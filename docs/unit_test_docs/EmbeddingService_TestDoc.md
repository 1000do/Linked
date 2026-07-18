# EmbeddingService Unit Test Documentation

---

## Function: `GetAllMaterialEmbeddingsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllMaterialEmbeddingsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllMaterialEmbeddingsAsync</td>
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
    <td colspan="18">Retrieve all material embeddings across both text and media types, combining them into a unified response format.</td>
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
|  |  | Text and media repositories have items |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns combined list mapping both types correctly |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 17/07/2026 |
|  | **Defect ID** |  |  |  |


---

## Function: `SaveMaterialEmbeddingsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SaveMaterialEmbeddingsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SaveMaterialEmbeddingsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">21</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Save or update an embedding array (text or media) for a specific material and handle potential repository exceptions.</td>
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
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | Media embedding, no existing entry |  | O |  |  |  |  |  |  |  |
|  |  | Media embedding, existing entry |  |  | O |  |  |  |  |  |  |
|  |  | Text embedding, no existing entry |  |  |  | O |  |  |  |  |  |
|  |  | Text embedding, existing entry |  |  |  |  | O |  |  |  |  |
|  |  | Type is media, repo fails to save |  |  |  |  |  | O |  |  |  |
|  |  | Type is text, repo fails to save |  |  |  |  |  |  | O |  |  |
|  |  | Repository throws `MediaEmbeddingException` |  |  |  |  |  |  |  | O |  |
|  |  | Repository throws `TextEmbeddingException` |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | Adds new media/text entry, clears cache, returns rows |  | O |  | O |  |  |  |  |  |
|  |  | Updates existing media/text entry, clears cache, returns rows |  |  | O |  | O |  |  |  |  |
|  |  | Returns 0 and logs error |  |  |  |  |  | O | O |  |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws `BadRequestException` |  |  |  |  |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N | A | A | B | B |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  | DFID001 | DFID002 |


---

## Function: `PersistPendingMaterialEmbeddingsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">PersistPendingMaterialEmbeddingsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">PersistPendingMaterialEmbeddingsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">12</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Retrieve pending embeddings from Redis cache and persist them to DB for a given course's materials, filtering out excluded ones.</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">10</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">10</td>
  </tr>
</table>

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Materials list is null |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Materials list is empty |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Material is in excluded list |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Cache entry is null |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Cache embedding array is null |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Cache embedding array is empty |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Cache has explicit 'text' type |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Cache has explicit 'media' type |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Cache has no type, count = media dimension |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Cache has no type, count != media dimension |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns 0 (no items processed) |  | O | O |  |  |  |  |  |  |  |  |
|  |  | Skips material and clears its cache |  |  |  | O | O | O | O |  |  |  |  |
|  |  | Saves as explicit text |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Saves as explicit media |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Infers media based on dimension and saves |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Infers text based on dimension and saves |  |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | A | A | A | A | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |  |  |


---

## Function: `PrepareMaterialEmbeddingsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">PrepareMaterialEmbeddingsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">PrepareMaterialEmbeddingsAsync</td>
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
    <td colspan="18">Checks embedding initialization cache flag, caches all DB embeddings in Redis if missing, to bootstrap semantic similarity searches.</td>
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
|  |  | Initialization flag is `true` |  | O |  |  |
|  |  | Initialization flag is `false`, cache miss |  |  | O |  |
|  |  | Initialization flag is `false`, cache hit |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Returns `false` immediately |  | O |  |  |
|  |  | Queries DB, caches new items, returns `true` |  |  | O |  |
|  |  | Queries DB, skips existing items, returns `true` |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 17/07/2026 | 17/07/2026 | 17/07/2026 |
|  | **Defect ID** |  |  |  |  |  |
