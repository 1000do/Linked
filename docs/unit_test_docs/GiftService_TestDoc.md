## Function: `ValidateGiftTokenAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ValidateGiftTokenAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ValidateGiftTokenAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">17</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ValidateGiftTokenAsync</td>
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
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of ValidateGiftTokenAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | EmptyToken |  | O |  |  |  |  |  |  |  |  |
|  |  | TokenNotFound |  |  | O |  |  |  |  |  |  |  |
|  |  | GiftRefunded |  |  |  | O |  |  |  |  |  |  |
|  |  | CourseNotFound |  |  |  |  | O |  |  |  |  |  |
|  |  | SenderHasFullName |  |  |  |  |  | O |  |  |  |  |
|  |  | SenderHasUsernameOnly |  |  |  |  |  |  | O |  |  |  |
|  |  | SenderHasEmailOnly |  |  |  |  |  |  |  | O |  |  |
|  |  | SenderAccountNotFound |  |  |  |  |  |  |  |  | O |  |
|  |  | NoSenderId |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ReturnsWithFullName |  |  |  |  |  | O |  |  |  |  |
|  |  | ReturnsWithUsername |  |  |  |  |  |  | O |  |  |  |
|  |  | ReturnsWithEmail |  |  |  |  |  |  |  | O |  |  |
|  |  | ReturnsDefaultName |  |  |  |  |  |  |  |  | O |  |
|  |  | ReturnsDefaultName |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsArgumentException |  | O |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  | O |  |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 |  |  |  |  |  |

## Function: `ClaimGiftAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ClaimGiftAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ClaimGiftAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">21</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ClaimGiftAsync</td>
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
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">8</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">12</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of ClaimGiftAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | EmptyToken |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | GiftNotFound |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | GiftAlreadyClaimed |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | GiftRefunded |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | CourseIdNull |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | CourseIdNull |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | CourseNotFound |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | UserAlreadyEnrolled |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | ValidClaim |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | RefundNotRejected |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | NotificationFails |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | RepositoryThrowsException |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ProcessesTransactionAndSendsNotification |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | DoesNotSendNotification |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | SwallowsExceptionAndCompletes |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | RollsBackTransactionAndRethrows |  |  |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsArgumentException |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | BecauseOrderItemNull_ThrowsInvalidOperationException |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  |  |  | O |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | A | A | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 | DFID008 |  |  |  |  |

## Function: `CreateGiftRecordAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateGiftRecordAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateGiftRecordAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for CreateGiftRecordAsync</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of CreateGiftRecordAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | EmptyEmail |  | O |  |  |  |  |
|  |  | EmptyToken |  |  | O |  |  |  |
|  |  | GiftAlreadyExists |  |  |  | O |  |  |
|  |  | ValidInput |  |  |  |  | O |  |
|  |  | NoSenderId |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | CreatesGiftAndSaves |  |  |  |  | O |  |
|  |  | SetsSenderIdToNull |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |
|  |  | ThrowsArgumentException |  | O |  |  |  |  |
|  |  | ThrowsArgumentException |  |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  |  |

## Function: `IsRecipientEnrolledAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">IsRecipientEnrolledAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">IsRecipientEnrolledAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">9</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for IsRecipientEnrolledAsync</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of IsRecipientEnrolledAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | EmptyEmail |  | O |  |  |
|  |  | AccountNotFound |  |  | O |  |
|  |  | AccountFound |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | ReturnsFalse |  | O |  |  |
|  |  | ReturnsFalse |  |  | O |  |
|  |  | ReturnsCourseRepoResult |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

