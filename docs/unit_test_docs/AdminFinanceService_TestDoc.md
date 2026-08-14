## Function: `GetInstructorCourseRevenuesByInstructorAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetInstructorCourseRevenuesByInstructorAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetInstructorCourseRevenuesByInstructorAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">15</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetInstructorCourseRevenuesByInstructorAsync</td>
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

- **Test requirement:** Verify the functionality of GetInstructorCourseRevenuesByInstructorAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidInstructor |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsMappedResponses |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `SetTransferRateAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SetTransferRateAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SetTransferRateAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">32</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SetTransferRateAsync</td>
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
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of SetTransferRateAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | RateLessThan30 |  | O |  |  |  |  |
|  |  | RateGreaterThan95 |  |  | O |  |  |  |
|  |  | ValidRateWithInstructors |  |  |  | O |  |  |
|  |  | ValidRateNoInstructors |  |  |  |  | O |  |
|  |  | NotificationThrowsException |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | UpsertsConfigAndSendsNotifications |  |  |  | O |  |  |
|  |  | UpsertsConfigAndSkipsNotifications |  |  |  |  | O |  |
|  |  | SwallowsExceptionAndCompletes |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |  |

## Function: `GetFinancialSummaryAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetFinancialSummaryAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetFinancialSummaryAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">38</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetFinancialSummaryAsync</td>
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

- **Test requirement:** Verify the functionality of GetFinancialSummaryAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidDate |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsCorrectCalculationsAndSums |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `GetInstructorPayoutsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetInstructorPayoutsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetInstructorPayoutsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">43</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetInstructorPayoutsAsync</td>
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

- **Test requirement:** Verify the functionality of GetInstructorPayoutsAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | NormalPayouts |  | O |  |
|  |  | RefundedPayout |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | CalculatesPlatformReceivedCorrectly |  | O |  |
|  |  | CalculatesNegativePlatformReceived |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |

## Function: `MarkPayoutAsPaidAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">MarkPayoutAsPaidAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">MarkPayoutAsPaidAsync</td>
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
    <td colspan="18">Tests for MarkPayoutAsPaidAsync</td>
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

- **Test requirement:** Verify the functionality of MarkPayoutAsPaidAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | PayoutNotFound |  | O |  |  |  |
|  |  | AlreadyPaid |  |  | O |  |  |
|  |  | ValidPendingPayout |  |  |  | O |  |
|  |  | SignalRThrowsException |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | UpdatesStatusAndBroadcastsSignalR |  |  |  | O |  |
|  |  | SwallowsExceptionAndCompletes |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |

## Function: `GetCurrentTransferRateAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetCurrentTransferRateAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetCurrentTransferRateAsync</td>
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
    <td colspan="18">Tests for GetCurrentTransferRateAsync</td>
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

- **Test requirement:** Verify the functionality of GetCurrentTransferRateAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | ConfigExists |  | O |  |  |
|  |  | ConfigEmptyOrInvalid |  |  | O |  |
|  |  | ConfigNotFound |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | ReturnsConfiguredRate |  | O |  |  |
|  |  | ReturnsDefaultRate |  |  | O |  |
|  |  | CreatesDefault |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `GetPayoutDaysConfigAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPayoutDaysConfigAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPayoutDaysConfigAsync</td>
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
    <td colspan="18">Tests for GetPayoutDaysConfigAsync</td>
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

- **Test requirement:** Verify the functionality of GetPayoutDaysConfigAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | NullConfig |  | O |  |  |
|  |  | HasConfig |  |  | O |  |
|  |  | ConfigNotFound |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | ReturnsDefault15 |  | O |  |  |
|  |  | ReturnsConfigValue |  |  | O |  |
|  |  | CreatesDefault |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `SetPayoutDaysConfigAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SetPayoutDaysConfigAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SetPayoutDaysConfigAsync</td>
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
    <td colspan="18">Tests for SetPayoutDaysConfigAsync</td>
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

- **Test requirement:** Verify the functionality of SetPayoutDaysConfigAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | EmptyInput |  | O |  |  |
|  |  | InvalidFormat |  |  | O |  |
|  |  | ValidInput |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | UpsertsConfig |  |  |  | O |
|  | **Exception** |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |

## Function: `PerformStripeTransferAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">PerformStripeTransferAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">PerformStripeTransferAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">50</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for PerformStripeTransferAsync</td>
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

- **Test requirement:** Verify the functionality of PerformStripeTransferAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |
|  |  | PayoutNotFound |  | O |  |  |  |  |  |  |
|  |  | AlreadyPaid |  |  | O |  |  |  |  |  |
|  |  | NoStripeAccount |  |  |  | O |  |  |  |  |
|  |  | Valid |  |  |  |  | O |  |  |  |
|  |  | StripeError |  |  |  |  |  | O |  |  |
|  |  | SignalRError |  |  |  |  |  |  | O |  |
|  |  | StripeErrorAndSignalRError |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |
|  |  | TransfersAndUpdates |  |  |  |  | O |  |  |  |
|  |  | CatchesException |  |  |  |  |  |  | O |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsException |  | O |  |  |  |  |  |  |
|  |  | ThrowsException |  |  | O |  |  |  |  |  |
|  |  | ThrowsException |  |  |  | O |  |  |  |  |
|  |  | SetsFailedAndThrows |  |  |  |  |  | O |  |  |
|  |  | CatchesSignalRAndThrowsStripeError |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | A | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  | DFID004 |  | DFID005 |

## Function: `BulkPayAllViaStripeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">BulkPayAllViaStripeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">BulkPayAllViaStripeAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">29</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for BulkPayAllViaStripeAsync</td>
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

- **Test requirement:** Verify the functionality of BulkPayAllViaStripeAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidData |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | FiltersAndProcessesCorrectly |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `GetPlatformBalanceAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPlatformBalanceAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPlatformBalanceAsync</td>
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
    <td colspan="18">Tests for GetPlatformBalanceAsync</td>
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

- **Test requirement:** Verify the functionality of GetPlatformBalanceAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidState |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsCorrectMappedData |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `GetWithdrawalHistoryAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetWithdrawalHistoryAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetWithdrawalHistoryAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">42</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetWithdrawalHistoryAsync</td>
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

- **Test requirement:** Verify the functionality of GetWithdrawalHistoryAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | NoChanges |  | O |  |  |
|  |  | HasPending |  |  | O |  |
|  |  | StripeError |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | ReturnsData |  | O |  |  |
|  |  | SyncsStatusAndSaves |  |  | O |  |
|  |  | CatchesException |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `RefundTransactionAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RefundTransactionAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RefundTransactionAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">130</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RefundTransactionAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">14</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">6</td>
    <td colspan="1" style="text-align: center;">8</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">14</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of RefundTransactionAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | TransactionNotFound |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | AlreadyRefunded |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | NotSucceeded |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | NoPaymentIntent |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | HasTransfer |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | ReversalFails |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | WithExistingExt |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | NoInstructorPayout |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | InstructorPayoutPaid |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | WithGiftAndRecipientEnrollment |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | WithBuyerEnrollment |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | SignalRFails |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | StripeTransferIdNull |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | RefundStripeError |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ReversesTransferAndRefunds |  |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | ProcessesRefund |  |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | NoTransfer |  |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | WithNoTransferId_SkipsReversal |  |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | RevokesBoth |  |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | RevokesEnrollment |  |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsException |  | O |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsException |  |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsException |  |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsException |  |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsException |  |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | ContinuesWithoutThrowing |  |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | ThrowsException |  |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | ThrowsException |  |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | N | A | N | N | N | N | N | A | A | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 |  | DFID005 |  |  |  |  |  | DFID006 | DFID007 | DFID008 |

## Function: `SyncAllPayoutsWithStripeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SyncAllPayoutsWithStripeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SyncAllPayoutsWithStripeAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">46</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SyncAllPayoutsWithStripeAsync</td>
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
    <td colspan="1" style="text-align: center;">9</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">9</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of SyncAllPayoutsWithStripeAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | NoInstructors |  | O |  |  |  |  |  |  |  |  |
|  |  | HasInstructors |  |  | O |  |  |  |  |  |  |  |
|  |  | GetPayoutsThrows |  |  |  | O |  |  |  |  |  |  |
|  |  | ExistingPayout |  |  |  |  | O |  |  |  |  |  |
|  |  | StatusInTransit |  |  |  |  |  | O |  |  |  |  |
|  |  | StatusFailed |  |  |  |  |  |  | O |  |  |  |
|  |  | StatusUnknown |  |  |  |  |  |  |  | O |  |  |
|  |  | GetPayoutsByStripePayoutIdThrows |  |  |  |  |  |  |  |  | O |  |
|  |  | SignalRError |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ReturnsEarly |  | O |  |  |  |  |  |  |  |  |
|  |  | SyncsSuccessfully |  |  | O |  |  |  |  |  |  |  |
|  |  | LogsAndContinues |  |  |  | O |  |  |  |  |  |  |
|  |  | Refunded_DoesNotUpdate |  |  |  |  | O |  |  |  |  |  |
|  |  | MarksInTransit |  |  |  |  |  | O |  |  |  |  |
|  |  | MarksFailed |  |  |  |  |  |  | O |  |  |  |
|  |  | DoesNotUpdate |  |  |  |  |  |  |  | O |  |  |
|  |  | Caught |  |  |  |  |  |  |  |  | O |  |
|  |  | CaughtAndIgnored |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |  |

## Function: `GetPendingRefundRequestsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPendingRefundRequestsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPendingRefundRequestsAsync</td>
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
    <td colspan="18">Tests for GetPendingRefundRequestsAsync</td>
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

- **Test requirement:** Verify the functionality of GetPendingRefundRequestsAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidState |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsData |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `RejectRefundAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RejectRefundAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RejectRefundAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">33</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RejectRefundAsync</td>
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

- **Test requirement:** Verify the functionality of RejectRefundAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Valid |  | O |  |  |  |
|  |  | TransactionNotFound |  |  | O |  |  |
|  |  | NotPending |  |  |  | O |  |
|  |  | Valid |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Rejects |  | O |  |  |  |
|  |  | WithExistingExtAndFailingNoti |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | ThrowsException |  |  | O |  |  |
|  |  | ThrowsException |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 |  |

## Function: `GetInstructorCourseRevenuesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetInstructorCourseRevenuesAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetInstructorCourseRevenuesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">15</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetInstructorCourseRevenuesAsync</td>
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

- **Test requirement:** Verify the functionality of GetInstructorCourseRevenuesAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidRequest |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsData |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `GetStripeCountriesAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetStripeCountriesAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetStripeCountriesAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetStripeCountriesAsync</td>
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

- **Test requirement:** Verify the functionality of GetStripeCountriesAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | NoConfig |  | O |  |
|  |  | HasConfig |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | ReturnsEmptyList |  | O |  |
|  |  | ReturnsList |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |



## Function: `CreateWithdrawalAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">CreateWithdrawalAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">CreateWithdrawalAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">44</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for CreateWithdrawalAsync</td>
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

- **Test requirement:** Verify the functionality and validation rules of CreateWithdrawalAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Amount < 0.50 |  | O |  |  |  |
|  |  | Amount > Available balance |  |  | O |  |  |
|  |  | Stripe create withdrawal fails |  |  |  | O |  |
|  |  | Valid withdrawal amount |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Returns WithdrawResponse |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | InvalidOperationException: withdrawal amount must be at least $0.50 |  | O |  |  |  |
|  |  | InvalidOperationException: Insufficient balance |  |  | O |  |  |
|  |  | InvalidOperationException: Stripe Payout error |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  |

## Function: `RequestRefundAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">RequestRefundAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">RequestRefundAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">99</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for RequestRefundAsync</td>
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
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">8</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">10</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality and validation rules of RequestRefundAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Transaction not found |  | O |  |  |  |  |  |  |  |  |  |
|  |  | AccountFrom != studentId |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Transaction status is RefundPending |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Transaction status is Refunded |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Transaction status is not Succeeded |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Past 14-day refund period |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Gift has already been claimed |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Course not found for transaction |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Auto-reject conditions met |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Valid refund request |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns IsAutoRejected = true |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Returns IsAutoRejected = false |  |  |  |  |  |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | InvalidOperationException: Transaction not found |  | O |  |  |  |  |  |  |  |  |  |
|  |  | InvalidOperationException: You do not own this transaction |  |  | O |  |  |  |  |  |  |  |  |
|  |  | InvalidOperationException: pending refund approval |  |  |  | O |  |  |  |  |  |  |  |
|  |  | InvalidOperationException: already been refunded |  |  |  |  | O |  |  |  |  |  |  |
|  |  | InvalidOperationException: only allowed for successful transactions |  |  |  |  |  | O |  |  |  |  |  |
|  |  | InvalidOperationException: exceeded the 14-day refund period |  |  |  |  |  |  | O |  |  |  |  |
|  |  | InvalidOperationException: gift has already been claimed |  |  |  |  |  |  |  | O |  |  |  |
|  |  | InvalidOperationException: Course not found |  |  |  |  |  |  |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 | DFID008 |  |  |

## Function: `ApproveRefundAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ApproveRefundAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ApproveRefundAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">AnHK</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">AnHK</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">66</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ApproveRefundAsync</td>
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

- **Test requirement:** Verify the functionality and validation rules of ApproveRefundAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Transaction not found |  | O |  |  |  |
|  |  | Transaction status is not RefundPending |  |  | O |  |  |
|  |  | Gift has already been claimed |  |  |  | O |  |
|  |  | Valid approval request |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Refund Transaction successful |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | InvalidOperationException: Transaction not found |  | O |  |  |  |
|  |  | InvalidOperationException: not in pending refund approval status |  |  | O |  |  |
|  |  | InvalidOperationException: gift has already been claimed |  |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 10/08/2026 | 10/08/2026 | 10/08/2026 | 10/08/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  |
