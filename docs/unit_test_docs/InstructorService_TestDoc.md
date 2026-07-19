## Function: `SubmitApplicationAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SubmitApplicationAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SubmitApplicationAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">97</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SubmitApplicationAsync</td>
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
    <td colspan="6" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">9</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">13</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of SubmitApplicationAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | AccountNotFound |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | AccountNotVerified |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | TotalFilesLessThanOne |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | TotalFilesGreaterThanThree |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | ExistingInstructorNotRejected |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | ResubmitWithLessThanOneValidUrl |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | ResubmitWithMoreThanThreeValidUrls |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | ResubmitValid |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | NewApplicationWithUploadFailures |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | NewApplicationValid |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | WithNullDocumentFile |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | NoValidRetainedUrlsAndNoFiles |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | UpdatesAndReturnsSuccessMessage |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | AddsAndReturnsSuccessMessage |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | SkipsAndUsesValidRetained |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | ThrowsExceptionForNewApp |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | A | A | A | A | N | A | N | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 | DFID004 | DFID005 | DFID006 | DFID007 |  | DFID008 |  |  | DFID009 |

## Function: `ApproveApplicationAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ApproveApplicationAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ApproveApplicationAsync</td>
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
    <td colspan="18">Tests for ApproveApplicationAsync</td>
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

- **Test requirement:** Verify the functionality of ApproveApplicationAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | InstructorNotFound |  | O |  |  |  |
|  |  | InvalidStatus |  |  | O |  |  |
|  |  | ValidApproved |  |  |  | O |  |
|  |  | ValidRejected |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | UpdatesAndReturnsTrue |  |  |  | O |  |
|  |  | UpdatesAndReturnsTrue |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |

## Function: `SetupStripePayoutAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SetupStripePayoutAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SetupStripePayoutAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">42</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SetupStripePayoutAsync</td>
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
    <td colspan="6" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">6</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of SetupStripePayoutAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | InstructorNotFound |  | O |  |  |  |  |
|  |  | NotApproved |  |  | O |  |  |  |
|  |  | SameStripeAccountId |  |  |  | O |  |  |
|  |  | NewStripeAccountId |  |  |  |  | O |  |
|  |  | InstructorNavigationNull |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | ReturnsResponseWithoutSaving |  |  |  | O |  |  |
|  |  | SavesAndReturnsResponse |  |  |  |  | O |  |
|  |  | UsesEmptyEmail |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |  |

## Function: `VerifyStripeOnboardingAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">VerifyStripeOnboardingAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">VerifyStripeOnboardingAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">29</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for VerifyStripeOnboardingAsync</td>
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

- **Test requirement:** Verify the functionality of VerifyStripeOnboardingAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | InstructorNotFound |  | O |  |  |  |  |
|  |  | NoStripeAccount |  |  | O |  |  |  |
|  |  | AlreadyActive |  |  |  | O |  |  |
|  |  | DetailsSubmittedTrue |  |  |  |  | O |  |
|  |  | DetailsSubmittedFalse |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | ReturnsActiveDirectly |  |  |  | O |  |  |
|  |  | UpdatesAndReturnsActive |  |  |  |  | O |  |
|  |  | UpdatesAndReturnsPending |  |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |  |

## Function: `GetAllApplicationsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllApplicationsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllApplicationsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">2</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetAllApplicationsAsync</td>
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

- **Test requirement:** Verify the functionality of GetAllApplicationsAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidRequest |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsApplications |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `GetInstructorDashboardAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetInstructorDashboardAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetInstructorDashboardAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">24</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetInstructorDashboardAsync</td>
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

- **Test requirement:** Verify the functionality of GetInstructorDashboardAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | DtoNull |  | O |  |  |
|  |  | StatsNull |  |  | O |  |
|  |  | Valid |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | ReturnsNull |  | O |  |  |
|  |  | ReturnsDtoWithDefaultStats |  |  | O |  |
|  |  | ReturnsPopulatedDto |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

## Function: `GetRejectedApplicationInfoAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetRejectedApplicationInfoAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetRejectedApplicationInfoAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">2</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetRejectedApplicationInfoAsync</td>
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

- **Test requirement:** Verify the functionality of GetRejectedApplicationInfoAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidRequest |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsResult |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `ResetStripeAccountAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">ResetStripeAccountAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">ResetStripeAccountAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">30</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for ResetStripeAccountAsync</td>
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

- **Test requirement:** Verify the functionality of ResetStripeAccountAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | InstructorNotFound |  | O |  |  |  |
|  |  | NoStripeAccount |  |  | O |  |  |
|  |  | DeleteAccountFails |  |  |  | O |  |
|  |  | DeleteAccountSucceeds |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | IgnoresErrorAndResetsDb |  |  |  | O |  |
|  |  | ResetsDb |  |  |  |  | O |
|  | **Exception** |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |  |

## Function: `GetStripeLoginLinkAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetStripeLoginLinkAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetStripeLoginLinkAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">12</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetStripeLoginLinkAsync</td>
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

- **Test requirement:** Verify the functionality of GetStripeLoginLinkAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | InstructorNotFound |  | O |  |  |
|  |  | NoStripeAccount |  |  | O |  |
|  |  | Valid |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | ReturnsLoginLink |  |  |  | O |
|  | **Exception** |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 |  |

## Function: `SetStripeCountryAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SetStripeCountryAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SetStripeCountryAsync</td>
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
    <td colspan="18">Tests for SetStripeCountryAsync</td>
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
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of SetStripeCountryAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |
|  |  | InvalidCountry |  | O |  |  |  |  |
|  |  | InstructorNotFound |  |  | O |  |  |  |
|  |  | HasStripeAccount |  |  |  | O |  |  |
|  |  | Valid |  |  |  |  | O |  |
|  |  | EmptyCountry |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |
|  |  | UpdatesCountry |  |  |  |  | O |  |
|  | **Exception** |  |  |  |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  | O |  |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  | O |  |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  | O |  |  |
|  |  | ThrowsInvalidOperationException |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | A | A | A | N | A |
|  | **Passed/Failed** |  |  | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  | DFID001 | DFID002 | DFID003 |  | DFID004 |

## Function: `GetPayoutsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPayoutsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPayoutsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">5</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetPayoutsAsync</td>
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

- **Test requirement:** Verify the functionality of GetPayoutsAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | ValidRequest |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | ReturnsPagedResult |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 19/07/2026 |
|  | **Defect ID** |  |  |  |

## Function: `SyncPayoutsWithStripeAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SyncPayoutsWithStripeAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SyncPayoutsWithStripeAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">44</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SyncPayoutsWithStripeAsync</td>
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
    <td colspan="1" style="text-align: center;">8</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">8</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of SyncPayoutsWithStripeAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |
|  |  | InstructorNotFound |  | O |  |  |  |  |  |  |  |
|  |  | NoStripePayouts |  |  | O |  |  |  |  |  |  |
|  |  | PayoutNotInDbAndNoValidTransfers |  |  |  | O |  |  |  |  |  |
|  |  | PayoutNotInDbAndValidTransfer |  |  |  |  | O |  |  |  |  |
|  |  | PayoutInDb |  |  |  |  |  | O |  |  |  |
|  |  | StatusFailed |  |  |  |  |  |  | O |  |  |
|  |  | StatusRefunded |  |  |  |  |  |  |  | O |  |
|  |  | StatusCanceled |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |
|  |  | ReturnsWithoutDoingAnything |  | O |  |  |  |  |  |  |  |
|  |  | ReturnsWithoutDoingAnything |  |  | O |  |  |  |  |  |  |
|  |  | Skips |  |  |  | O |  |  |  |  |  |
|  |  | UpdatesLocalPayout |  |  |  |  | O |  |  |  |  |
|  |  | UpdatesAllLocalPayouts |  |  |  |  |  | O |  |  |  |
|  |  | UpdatesStatusAndIsPaidToFalse |  |  |  |  |  |  | O |  |  |
|  |  | Ignores |  |  |  |  |  |  |  | O |  |
|  |  | UpdatesStatusAndIsPaidToFalse |  |  |  |  |  |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | N | N | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  |  |

## Function: `GetPublicProfileAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetPublicProfileAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetPublicProfileAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungPH</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungPH</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">43</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetPublicProfileAsync</td>
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
    <td colspan="6" style="text-align: center;">2</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">5</td>
  </tr>
</table>

- **Test requirement:** Verify the functionality of GetPublicProfileAsync.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | ReturnsProfile |  | O |  |  |
|  |  | ReturnsNull |  |  | O |  |
|  |  | NullUserAndStats |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | WhenInstructorExists |  | O |  |  |
|  |  | WhenInstructorDoesNotExistOrNotApproved |  |  | O |  |
|  |  | HandlesNulls |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 19/07/2026 | 19/07/2026 | 19/07/2026 |
|  | **Defect ID** |  |  |  |  |  |

