# CartService Unit Test Documentation

---

## Function: `AddToCartAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">AddToCartAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">AddToCartAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">12</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for AddToCartAsync</td>
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

- **Test requirement:** Validate rules and add a course to the user's cart securely, handling edge cases like not found, not published, ownership, already enrolled, or already in cart.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |
|  |  | Course is not found |  | O |  |  |  |  |  |
|  |  | Course is not published |  |  | O |  |  |  |  |
|  |  | Course belongs to the user |  |  |  | O |  |  |  |
|  |  | User is already enrolled in course |  |  |  |  | O |  |  |
|  |  | Course is already in the cart |  |  |  |  |  | O |  |
|  |  | Request is valid and course is available |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |
|  |  | Adds to cart and saves successfully |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |
|  |  | Throws InvalidOperationException |  | O | O | O | O | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 |  |

---

## Function: `RemoveFromCartAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RemoveFromCartAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RemoveFromCartAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">7</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RemoveFromCartAsync</td>
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

- **Test requirement:** Remove a specific course from the user's cart, handling the case where the item is not found.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Item is not found in the cart |  | O |  |
|  |  | Item exists in the cart |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Removes from cart and saves successfully |  |  | O |
|  | **Exception** |  |  |  |  |
|  |  | Throws InvalidOperationException |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  | DFID001 |  |

---

## Function: `GetCartSummaryAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetCartSummaryAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetCartSummaryAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">19</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetCartSummaryAsync</td>
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
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="6" style="text-align: center;">12</td>
  </tr>
</table>

- **Test requirement:** Calculate the cart subtotal, apply multiple coupons or single coupons correctly, check eligibility bounds, and gracefully map incomplete properties.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Cart has items, no coupon provided |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Multiple coupons provided |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Valid percentage coupon provided |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Valid fixed coupon provided |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Valid coupon but not applicable to any course |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Cart item has null navigations and course |  |  |  |  |  |  |  |  |  |  |  |  | O |
|  |  | Coupon code is invalid |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Coupon is inactive |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Coupon is before start date |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Coupon is after end date |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Coupon usage limit reached |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Available coupons eligibility evaluation |  |  |  |  |  |  |  |  |  |  |  | O |  |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns summary successfully without discount |  | O |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Returns summary with percentage discount |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Returns summary with fixed discount |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Returns summary with multiple discounts |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Evaluates eligible and ineligible available coupons |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Handles null properties gracefully |  |  |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws InvalidOperationException |  |  | O | O | O | O | O |  |  |  |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | A | A | A | B | N | N | N | N | B |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 |  |  |  |  |  |  |
