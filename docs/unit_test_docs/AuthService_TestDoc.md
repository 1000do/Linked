## Function: `LoginAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">LoginAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">LoginAsync</td>
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
    <td colspan="6">Tests for LoginAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Verify that a user can login with valid credentials, or appropriate errors/exceptions are handled for invalid scenarios.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Account exists |  | X | O | O | O |
|  |  | Account password is correct |  | X | X | O | O |
|  |  | Account is locked out |  | X | X | O | X |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | null |  | O | O | X | X |
|  |  | LoginResponse |  | X | X | X | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | UnauthorizedAccessException |  | X | X | O | X |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 |  |

## Function: `RefreshTokenAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">RefreshTokenAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">RefreshTokenAsync</td>
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
    <td colspan="6">Tests for RefreshTokenAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">5</td>
  </tr>
</table>

- **Test requirement:** Verify that a user can refresh their token if the refresh token is valid and not expired.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Token found in DB |  | X | O | O | O |
|  |  | Token ExpiryTime is not null |  | X | X | O | O |
|  |  | Token ExpiryTime is in the future |  | X | X | X | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | null |  | O | O | O | X |
|  |  | LoginResponse |  | X | X | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

## Function: `LogoutAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">LogoutAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">LogoutAsync</td>
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
    <td colspan="6">Tests for LogoutAsync</td>
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

- **Test requirement:** Verify that a user can logout by revoking their refresh token.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Input** |  |  |  |
|  |  | Valid account ID |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | true |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `RegisterAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">RegisterAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">RegisterAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">21</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Tests for RegisterAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="1" style="text-align: center;">Failed</th>
    <th colspan="1" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="1" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">6</td>
  </tr>
</table>

- **Test requirement:** Verify that a new user can register successfully or appropriate validation errors are returned.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | Valid email domain (@gmail.com) |  | X | O | O | O | O | O |
|  |  | Valid password format |  | X | X | O | O | O | O |
|  |  | Email does not exist |  | X | X | X | O | O | O |
|  |  | Username does not exist |  | X | X | X | X | O | O |
|  |  | DB Registration successful |  | X | X | X | X | X | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | "Email must be a @gmail.com address" |  | O | X | X | X | X | X |
|  |  | "Password must contain at least 1 special character" |  | X | O | X | X | X | X |
|  |  | "Email already exists" |  | X | X | O | X | X | X |
|  |  | "Username already exists" |  | X | X | X | O | X | X |
|  |  | "Error" |  | X | X | X | X | O | X |
|  |  | "Success" |  | X | X | X | X | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |

## Function: `GoogleLoginAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">GoogleLoginAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">GoogleLoginAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">13</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Tests for GoogleLoginAsync</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Verify Google login flow correctly creates or authenticates a user based on valid Google tokens.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Token is valid |  | O | O | O | O |
|  |  | Account exists |  | X | X | O | O |
|  |  | DB Registration successful |  | X | O | X | X |
|  |  | Account is locked out |  | X | X | O | X |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | null |  | O | X | X | X |
|  |  | LoginResponse |  | X | O | X | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | UnauthorizedAccessException |  | X | X | O | X |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 |  |

## Function: `SendOtpAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">SendOtpAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">SendOtpAsync</td>
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
    <td colspan="6">Tests for SendOtpAsync</td>
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

- **Test requirement:** Verify sending OTP logic based on account existence.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Account exists |  | X | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | false |  | O | X |
|  |  | true |  | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `VerifyEmailAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">VerifyEmailAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">VerifyEmailAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">5</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Tests for VerifyEmailAsync</td>
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

- **Test requirement:** Verify email verification using OTP.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | OTP is valid |  | X | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | false |  | O | X |
|  |  | true |  | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `ForgotPasswordAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">ForgotPasswordAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">ForgotPasswordAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">13</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Tests for ForgotPasswordAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Verify password reset request process and validations.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Account exists |  | X | O | O | O |
|  |  | AuthProvider is Local |  | X | X | O | O |
|  |  | Email is verified |  | X | X | X | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | "Email not found" |  | O | X | X | X |
|  |  | "This account uses Google login" |  | X | O | X | X |
|  |  | "Email is not verified" |  | X | X | O | X |
|  |  | "OTP sent" |  | X | X | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

## Function: `ResetPasswordAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">ResetPasswordAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">ResetPasswordAsync</td>
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
    <td colspan="6">Tests for ResetPasswordAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Verify password reset execution with valid OTP.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | OTP is valid |  | X | O | O | O |
|  |  | Account exists |  | X | X | O | O |
|  |  | AuthProvider is Local |  | X | X | X | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | false |  | O | O | O | X |
|  |  | true |  | X | X | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

## Function: `VerifyOtpForReset`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">VerifyOtpForReset</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">VerifyOtpForReset</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="2">AnHK</td>
    <td colspan="2"><strong>Executed By</strong></td>
    <td colspan="2">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="2">3</td>
    <td colspan="2"><strong>Lack of test cases</strong></td>
    <td colspan="2">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="6">Tests for VerifyOtpForReset</td>
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

- **Test requirement:** Verify checking OTP validity for reset flow.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | OTP is valid |  | X | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | false |  | O | X |
|  |  | true |  | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `IsEmailVerifiedAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="2">IsEmailVerifiedAsync</td>
    <td colspan="2"><strong>Function Name</strong></td>
    <td colspan="2">IsEmailVerifiedAsync</td>
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
    <td colspan="6">Tests for IsEmailVerifiedAsync</td>
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

- **Test requirement:** Verify retrieving email verification status.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Account exists |  | X | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | false |  | O | X |
|  |  | IsVerified (true) |  | X | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |
