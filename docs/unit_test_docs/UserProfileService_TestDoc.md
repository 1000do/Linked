## Function: `GetUserProfileAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetUserProfileAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetUserProfileAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetUserProfileAsync</td>
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

- **Test requirement:** Verify that user profile details are correctly fetched and mapped to the response.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | User not found |  | O |  |  |
|  |  | User found without Instructor |  |  | O |  |
|  |  | User found with Instructor |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Null |  | O |  |  |
|  |  | Mapped response without Instructor details |  |  | O |  |
|  |  | Mapped response with Instructor details |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `UpdateProfileAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">UpdateProfileAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">UpdateProfileAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">25</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for UpdateProfileAsync</td>
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
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

- **Test requirement:** Verify that a user can update their profile information, handling email validation, avatar upload, and data mapping.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | User not found |  | O |  |  |  |  |  |  |  |  |
|  |  | User Navigation null |  |  | O |  |  |  |  |  |  |  |
|  |  | Google user changes email |  |  |  | O |  |  |  |  |  |  |
|  |  | Invalid email domain (not @gmail.com) |  |  |  |  | O |  |  |  |  |  |
|  |  | Avatar upload returns null |  |  |  |  |  | O |  |  |  |  |
|  |  | Avatar upload throws exception |  |  |  |  |  |  | O |  |  |  |
|  |  | Valid request with all fields |  |  |  |  |  |  |  | O |  |  |
|  |  | Valid request without avatar file |  |  |  |  |  |  |  |  | O |  |
|  |  | Invalid DateOfBirth format |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | False |  | O | O |  |  | O | O |  |  |  |
|  |  | True |  |  |  |  |  |  |  | O | O | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | `InvalidOperationException` |  |  |  | O | O |  |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | N | N | B |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |  |

## Function: `ChangePasswordAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ChangePasswordAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ChangePasswordAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">14</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ChangePasswordAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">6</td>
  </tr>
</table>

- **Test requirement:** Verify that password change process validates the current password, ensures account supports passwords, and updates correctly.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | User not found |  | O |  |  |  |  |  |
|  |  | User Navigation null |  |  | O |  |  |  |  |
|  |  | User has no password hash |  |  |  | O |  |  |  |
|  |  | Incorrect current password |  |  |  |  | O |  |  |
|  |  | Correct password, repository returns true |  |  |  |  |  | O |  |
|  |  | Correct password, repository returns false |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | (False, Error Message) |  | O | O | O | O |  | O |
|  |  | (True, Success Message) |  |  |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |
