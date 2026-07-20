## Function: `GetAll`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAll</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAll</td>
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
    <td colspan="18">Tests for GetAll</td>
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
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="6" style="text-align: center;">2</td>
  </tr>
</table>

- **Test requirement:** Verify that coupons can be retrieved based on role (Admin vs Manager) and filters.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | IsAdmin is true |  | O |  |
|  |  | IsAdmin is false |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Passes null filter and returns mapped |  | O |  |
|  |  | Passes managerId and returns mapped |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | B | B |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `GetById`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetById</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetById</td>
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
    <td colspan="18">Tests for GetById</td>
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

- **Test requirement:** Verify that a coupon can be fetched by ID and correctly mapped to a response.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Not found |  | O |  |
|  |  | Found |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Null |  | O |  |
|  |  | Mapped response |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `Create`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">Create</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">Create</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">8</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for Create</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

- **Test requirement:** Verify that creating a coupon properly validates all inputs (type, dates, value, usage limit) and ensures uniqueness before saving.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | Invalid Type |  | O |  |  |  |  |  |  |  |
|  |  | Invalid Discount Value |  |  | O |  |  |  |  |  |  |
|  |  | Percentage > 100 |  |  |  | O |  |  |  |  |  |
|  |  | Invalid Dates |  |  |  |  | O |  |  |  |  |
|  |  | Invalid Usage Limit |  |  |  |  |  | O |  |  |  |
|  |  | Code Already Exists |  |  |  |  |  |  | O |  |  |
|  |  | Valid Request Fixed |  |  |  |  |  |  |  | O |  |
|  |  | Valid Request Percentage |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | Builds and saves coupon |  |  |  |  |  |  |  | O | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |
|  |  | `ArgumentException` |  | O | O | O | O | O |  |  |  |
|  |  | `InvalidOperationException` |  |  |  |  |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |

## Function: `Update`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">Update</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">Update</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">9</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for Update</td>
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

- **Test requirement:** Verify that updating a coupon properly validates end date and usage limit before saving the partial updates.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Not found |  | O |  |  |  |
|  |  | EndDate before StartDate |  |  | O |  |  |
|  |  | UsageLimit < UsedCount |  |  |  | O |  |
|  |  | Valid Partial Request |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Updates properties and saves |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | `KeyNotFoundException` |  | O |  |  |  |
|  |  | `ArgumentException` |  |  | O | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

## Function: `SoftDelete`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SoftDelete</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SoftDelete</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">10</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SoftDelete</td>
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

- **Test requirement:** Verify that a coupon can be soft-deleted by setting IsActive to false and expiring its EndDate.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Not found |  | O |  |
|  |  | Found |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Sets IsActive false and saves |  |  | O |
|  | **Exception** |  |  |  |  |
|  |  | `KeyNotFoundException` |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `GetActivePlatformCouponsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetActivePlatformCouponsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetActivePlatformCouponsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">4</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetActivePlatformCouponsAsync</td>
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

- **Test requirement:** Verify that all active platform coupons are returned and mapped to responses.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Call function |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns Mapped Responses |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 20/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `ApplyCouponToCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ApplyCouponToCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ApplyCouponToCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ApplyCouponToCourseAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">11</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">9</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">11</td>
  </tr>
</table>

- **Test requirement:** Verify that applying a coupon to a course validates ownership, course type (not free), cart status, and coupon validity.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Course Not Found |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Not Owner |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Course In Cart |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Free Course |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Coupon Not Found |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Coupon Not Active |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Coupon Expired |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Coupon Not Started |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Coupon Usage Limit Reached |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Valid Published Course |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Valid Draft Course |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Sets Draft and Applies Coupon |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Applies Coupon Without Status Change |  |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | `KeyNotFoundException` |  | O |  |  |  | O |  |  |  |  |  |  |
|  |  | `UnauthorizedAccessException` |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | `InvalidOperationException` |  |  |  | O | O |  | O | O | O | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | A | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |  |  |  |

## Function: `RemoveCouponFromCourseAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RemoveCouponFromCourseAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RemoveCouponFromCourseAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">4</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RemoveCouponFromCourseAsync</td>
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

- **Test requirement:** Verify that a coupon can be removed from a course if validation passes.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Valid Request |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Removes Coupon And Clears Cache |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 20/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `ApplyCoupon`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ApplyCoupon</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ApplyCoupon</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">NgocNN</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">NgocNN</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">4</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ApplyCoupon</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

- **Test requirement:** Verify that price calculation applies discounts correctly and validates coupon state during checkout.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | Not Active |  | O |  |  |  |  |  |  |  |
|  |  | Not Started |  |  | O |  |  |  |  |  |  |
|  |  | Expired |  |  |  | O |  |  |  |  |  |
|  |  | Usage Limit Reached |  |  |  |  | O |  |  |  |  |
|  |  | Min Order Value Not Met |  |  |  |  |  | O |  |  |  |
|  |  | Fixed Discount |  |  |  |  |  |  | O |  |  |
|  |  | Percentage Discount |  |  |  |  |  |  |  | O |  |
|  |  | Discount > Price |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | Calculates Final Price |  |  |  |  |  |  | O | O |  |
|  |  | Returns Zero |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |
|  |  | `InvalidOperationException` |  | O | O | O | O | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N | N | B |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 | 20/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |
