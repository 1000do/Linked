## Function: `InitiateCheckoutAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">InitiateCheckoutAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">InitiateCheckoutAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">53</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for InitiateCheckoutAsync</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of InitiateCheckoutAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | CartEmpty |  | O |  |  |  |  |  |  |  |  |
|  |  | CourseNotPublished |  |  | O |  |  |  |  |  |  |  |
|  |  | InstructorIsUser |  |  |  | O |  |  |  |  |  |  |
|  |  | UserAlreadyEnrolled |  |  |  |  | O |  |  |  |  |  |
|  |  | InvalidCouponCode |  |  |  |  |  | O |  |  |  |  |
|  |  | CouponExpiredOrLimitReached |  |  |  |  |  |  | O |  |  |  |
|  |  | ValidCartWithPercentageCoupon |  |  |  |  |  |  |  | O |  |  |
|  |  | ValidCartWithFixedCoupon |  |  |  |  |  |  |  |  | O |  |
|  |  | ValidCartWithoutCoupon |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ReturnsCheckoutResponse |  |  |  |  |  |  |  | O |  |  |
|  |  | ReturnsCheckoutResponse |  |  |  |  |  |  |  |  | O |  |
|  |  | ReturnsCheckoutResponse |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  | O |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  | O |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 |  |  |  |

