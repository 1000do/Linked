## Function: `InitiateGiftCheckoutAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">InitiateGiftCheckoutAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">InitiateGiftCheckoutAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">37</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for InitiateGiftCheckoutAsync</td>
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

- **Test requirement:** Verify the functionality of InitiateGiftCheckoutAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |
|  |  | CourseNotFound |  | O |  |  |  |  |  |  |
|  |  | CourseNotPublished |  |  | O |  |  |  |  |  |
|  |  | RecipientAlreadyEnrolled |  |  |  | O |  |  |  |  |
|  |  | InstructorStripeNotConnected |  |  |  |  | O |  |  |  |
|  |  | InstructorIdNull |  |  |  |  |  | O |  |  |
|  |  | ValidRequest |  |  |  |  |  |  | O |  |
|  |  | ValidRequest |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |
|  |  | SuccessUrlContainsPath_ReturnsCheckoutResponse |  |  |  |  |  |  | O |  |
|  |  | SuccessUrlDoesNotContainPath_ReturnsCheckoutResponse |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 |  |  |

## Function: `InitiateGiftPaymentIntentAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">InitiateGiftPaymentIntentAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">InitiateGiftPaymentIntentAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">18</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for InitiateGiftPaymentIntentAsync</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of InitiateGiftPaymentIntentAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | CourseNotFound |  | O |  |  |  |  |  |  |  |
|  |  | CourseNotPublished |  |  | O |  |  |  |  |  |  |
|  |  | RecipientAlreadyEnrolled |  |  |  | O |  |  |  |  |  |
|  |  | RecipientFoundNotEnrolled |  |  |  |  | O |  |  |  |  |
|  |  | InstructorStripeNotConnected |  |  |  |  |  | O |  |  |  |
|  |  | InstructorIdNull |  |  |  |  |  |  | O |  |  |
|  |  | ValidRequest |  |  |  |  |  |  |  | O |  |
|  |  | ValidRequest |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | ContinuesProcessing_ReturnsCheckoutResponse |  |  |  |  | O |  |  |  |  |
|  |  | SuccessUrlContainsPath_ReturnsCheckoutResponse |  |  |  |  |  |  |  | O |  |
|  |  | SuccessUrlDoesNotContainPath_ReturnsCheckoutResponse |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  | DFID004 | DFID005 |  |  |

## Function: `ProcessPaymentSuccessAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ProcessPaymentSuccessAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ProcessPaymentSuccessAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">16</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ProcessPaymentSuccessAsync</td>
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
    <td colspan="1" style="text-align: center;">7</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">10</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of ProcessPaymentSuccessAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ExistingTransactionNotSucceeded |  | O |  |  |  |  |  |  |  |  |  |
|  |  | AlreadyProcessed |  |  | O |  |  |  |  |  |  |  |  |
|  |  | NoMetadata |  |  |  | O |  |  |  |  |  |  |  |
|  |  | NoCourseIds |  |  |  |  | O |  |  |  |  |  |  |
|  |  | ValidRequest |  |  |  |  |  | O |  |  |  |  |  |
|  |  | CourseNotPublished |  |  |  |  |  |  | O |  |  |  |  |
|  |  | DbException |  |  |  |  |  |  |  | O |  |  |  |
|  |  | ExistingTransactionNotSucceeded_ReturnsFalseAndProceeds |  |  |  |  |  |  |  |  | O |  |  |
|  |  | ExistingTransactionNotPayoutCreated_ReturnsFalseAndProceeds |  |  |  |  |  |  |  |  |  | O |  |
|  |  | PaymentGatewayGetSessionMetadataThrows |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ProcessesNormally |  | O |  |  |  |  |  |  |  |  |  |
|  |  | ReturnsEarly |  |  | O |  |  |  |  |  |  |  |  |
|  |  | ProcessesSuccessfully |  |  |  |  |  | O |  |  |  |  |  |
|  |  | RollsBackTransaction |  |  |  |  |  |  | O |  |  |  |  |
|  |  | RollsBackTransaction |  |  |  |  |  |  |  | O |  |  |  |
|  |  | ProcessesNormally |  |  |  |  |  |  |  |  | O |  |  |
|  |  | ProcessesNormally |  |  |  |  |  |  |  |  |  | O |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  | O |  |  |  |  |  |  |
|  |  | ThrowsException |  |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | A | A | N | N | N | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 | DFID002 |  |  |  |  |  | DFID003 |

## Function: `ProcessPaymentIntentSuccessAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ProcessPaymentIntentSuccessAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ProcessPaymentIntentSuccessAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">15</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ProcessPaymentIntentSuccessAsync</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of ProcessPaymentIntentSuccessAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | AlreadyProcessed |  | O |  |  |  |
|  |  | NoMetadata |  |  | O |  |  |
|  |  | ValidRequest |  |  |  | O |  |
|  |  | CourseNull |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | ReturnsEarly |  | O |  |  |  |
|  |  | ProcessesSuccessfully |  |  |  | O |  |
|  |  | ReturnsWithoutAddingOrder |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 27/07/2026 | 27/07/2026 | 27/07/2026 | 27/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |  |  |

## Function: `ProcessGiftFulfillmentAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ProcessGiftFulfillmentAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ProcessGiftFulfillmentAsync</td>
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
    <td colspan="18">Tests for ProcessGiftFulfillmentAsync</td>
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

- **Test requirement:** Verify the functionality of ProcessGiftFulfillmentAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | EmailAndNotiExceptions |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | CatchesAndContinues |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 27/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `CalculatePayoutDateAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CalculatePayoutDateAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CalculatePayoutDateAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">19</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for CalculatePayoutDateAsync</td>
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

- **Test requirement:** Verify the functionality of CalculatePayoutDateAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | VariousConfigs |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsExpectedDate |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 27/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `GetCurrencyFromCountry`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetCurrencyFromCountry</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetCurrencyFromCountry</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">25</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetCurrencyFromCountry</td>
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

- **Test requirement:** Verify the functionality of GetCurrencyFromCountry.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidCountry |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | MapsCorrectly |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 27/07/2026 |
|  | **Defect ID** |  |  |  |
