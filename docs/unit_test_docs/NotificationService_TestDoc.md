# NotificationService Unit Test Documentation

---

## Function: `GetNotificationsForUserAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetNotificationsForUserAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetNotificationsForUserAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetNotificationsForUserAsync</td>
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

- **Test requirement:** Retrieve a paginated list of notifications for a specific user and map them to DTOs.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Notifications exist |  | O |  |
|  |  | No notifications found |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Mapped notifications and count |  | O |  |
|  | **Exception** |  |  |  |  |
|  |  | Throws KeyNotFoundException |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |

---

## Function: `GetAllNotificationsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllNotificationsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllNotificationsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">6</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetAllNotificationsAsync</td>
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

- **Test requirement:** Retrieve a paginated list of all system notifications and map them to DTOs.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |
|  |  | Notifications exist |  | O |  |
|  |  | No notifications found |  |  | O |
| **Confirm** | **Return** |  |  |  |  |
|  |  | Mapped notifications and count |  | O |  |
|  | **Exception** |  |  |  |  |
|  |  | Throws KeyNotFoundException |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A |
|  | **Passed/Failed** |  |  | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 |

---

## Function: `GetAllNotificationsForAdminAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetAllNotificationsForAdminAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetAllNotificationsForAdminAsync</td>
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
    <td colspan="18">Tests for GetAllNotificationsForAdminAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

- **Test requirement:** Retrieve a paginated list of all notifications with advanced role mappings for admins, executing cleanup beforehand.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Notifications exist |  | O |  |  |
|  |  | Role mapping variations exist |  |  | O |  |
|  |  | No notifications found |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Mapped notifications and count |  | O |  |  |
|  |  | Maps correct roles (admin, student, staff, broadcast) |  |  | O |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws KeyNotFoundException |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 |

---

## Function: `GetUnreadCountAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">GetUnreadCountAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">GetUnreadCountAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">3</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for GetUnreadCountAsync</td>
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

- **Test requirement:** Retrieve the total number of unread notifications for a specific user.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Valid user ID |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns count |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 18/07/2026 |
|  | **Defect ID** |  |  |  |

---

## Function: `SearchEmailsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SearchEmailsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SearchEmailsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">1</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SearchEmailsAsync</td>
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

- **Test requirement:** Search emails securely using a query text and sender constraints.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 |
| :--- | :--- | :--- | :--- | :---: |
| **Condition** | **Precondition** |  |  |  |
|  |  | Valid query, sender ID, and role |  | O |
| **Confirm** | **Return** |  |  |  |
|  |  | Returns list of matching emails |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N |
|  | **Passed/Failed** |  |  | P |
|  | **Executed Date** |  |  | 18/07/2026 |
|  | **Defect ID** |  |  |  |

---

## Function: `SendNotificationAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SendNotificationAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SendNotificationAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">21</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SendNotificationAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

- **Test requirement:** Send a single notification and broadcast to real-time clients.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Valid request with other managers |  | O |  |  |
|  |  | No other managers exist |  |  | O |  |
|  |  | Repository throws exception |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Saves and broadcasts successfully |  | O | O |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws BadRequestException |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 |

---

## Function: `SendAdvancedAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SendAdvancedAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SendAdvancedAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">11</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SendAdvancedAsync</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">13</td>
    <td colspan="3" style="text-align: center;">0</td>
    <td colspan="6" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">4</td>
    <td colspan="1" style="text-align: center;">5</td>
    <td colspan="6" style="text-align: center;">13</td>
  </tr>
</table>

- **Test requirement:** Handle advanced targeted broadcasting (ALL vs SPECIFIC), filtering by role restrictions and ignoring sender's own email.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Target ALL, Sender is Staff |  | O |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Target ALL, Sender is Admin |  |  | O |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Target ALL, Sender is System |  |  |  | O |  |  |  |  |  |  |  |  |  |  |
|  |  | Target ALL, No other managers |  |  |  |  | O |  |  |  |  |  |  |  |  |  |
|  |  | Target ALL, filtered list is empty |  |  |  |  |  | O |  |  |  |  |  |  |  |  |
|  |  | Target SPECIFIC, valid emails |  |  |  |  |  |  | O |  |  |  |  |  |  |  |
|  |  | Target SPECIFIC, account not found for email |  |  |  |  |  |  |  | O |  |  |  |  |  |  |
|  |  | Target SPECIFIC, sender email included |  |  |  |  |  |  |  |  | O |  |  |  |  |  |
|  |  | Target SPECIFIC, Sender Staff targeting Admin |  |  |  |  |  |  |  |  |  | O |  |  |  |  |
|  |  | Target SPECIFIC, Sender Staff targeting Staff |  |  |  |  |  |  |  |  |  |  | O |  |  |  |
|  |  | Target SPECIFIC, Sender Admin targeting Admin |  |  |  |  |  |  |  |  |  |  |  | O |  |  |
|  |  | Emails list is null |  |  |  |  |  |  |  |  |  |  |  |  | O |  |
|  |  | Emails list is empty |  |  |  |  |  |  |  |  |  |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Returns number of sent notifications |  | O | O | O | O |  | O | O |  |  |  |  |  |  |
|  |  | Returns zero without sending |  |  |  |  |  | O |  |  |  |  |  |  | O | O |
|  | **Exception** |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  |  | Throws InvalidOperationException |  |  |  |  |  |  |  |  | O | O | O | O |  |  |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | N | N | B | B | N | B | A | A | A | A | B | B |
|  | **Passed/Failed** |  |  | P | P | P | P | P | P | P | P | P | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |  |  |  | DFID001 | DFID002 | DFID003 | DFID004 |  |  |

---

## Function: `SendBulkNotificationsAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">SendBulkNotificationsAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">SendBulkNotificationsAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">10</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for SendBulkNotificationsAsync</td>
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
    <td colspan="1" style="text-align: center;">0</td>
    <td colspan="1" style="text-align: center;">3</td>
    <td colspan="6" style="text-align: center;">4</td>
  </tr>
</table>

- **Test requirement:** Send multiple notifications simultaneously using bulk DTOs and broadcast to respective receivers and managers.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |  |
|  |  | Valid request with other managers |  | O |  |  |  |
|  |  | No other managers exist |  |  | O |  |  |
|  |  | DTO list is empty |  |  |  | O |  |
|  |  | DTO list is null |  |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |  |
|  |  | Saves and broadcasts successfully |  | O | O |  |  |
|  |  | Returns true immediately |  |  |  | O | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | B | B |
|  | **Passed/Failed** |  |  | P | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  |  |  |

---

## Function: `DeleteNotificationAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">DeleteNotificationAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">DeleteNotificationAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">11</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for DeleteNotificationAsync</td>
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

- **Test requirement:** Delete a specific notification and broadcast the deletion.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Valid request |  | O |  |  |
|  |  | Notification not found or wrong user |  |  | O |  |
|  |  | Repository throws exception |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Deletes and broadcasts successfully |  | O |  |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws KeyNotFoundException |  |  | O |  |
|  |  | Throws BadRequestException |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 |

---

## Function: `MarkAsReadAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">MarkAsReadAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">MarkAsReadAsync</td>
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
    <td colspan="18">Tests for MarkAsReadAsync</td>
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

- **Test requirement:** Update a notification's state to read and broadcast.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Valid request |  | O |  |  |
|  |  | Notification not found or wrong user |  |  | O |  |
|  |  | Repository throws exception |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Updates state to true and broadcasts |  | O |  |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws KeyNotFoundException |  |  | O |  |
|  |  | Throws BadRequestException |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | A | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  | DFID001 | DFID002 |

---

## Function: `MarkAllAsReadAsync`
<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">MarkAllAsReadAsync</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">MarkAllAsReadAsync</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">HungT</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">HungT</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">17</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">Tests for MarkAllAsReadAsync</td>
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
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="1" style="text-align: center;">1</td>
    <td colspan="6" style="text-align: center;">3</td>
  </tr>
</table>

- **Test requirement:** Update all unread notifications for a user to read.

| Category | Sub-category | Detail 1 | Detail 2 | UTCID01 | UTCID02 | UTCID03 |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Condition** | **Precondition** |  |  |  |  |  |
|  |  | Has unread notifications |  | O |  |  |
|  |  | No unread notifications |  |  | O |  |
|  |  | Repository throws exception |  |  |  | O |
| **Confirm** | **Return** |  |  |  |  |  |
|  |  | Updates all and broadcasts |  | O |  |  |
|  |  | Returns true immediately |  |  | O |  |
|  | **Exception** |  |  |  |  |  |
|  |  | Throws BadRequestException |  |  |  | O |
| **Result** | **Type(N : Normal, A : Abnormal, B : Boundary)** |  |  | N | B | A |
|  | **Passed/Failed** |  |  | P | P | P |
|  | **Executed Date** |  |  | 18/07/2026 | 18/07/2026 | 18/07/2026 |
|  | **Defect ID** |  |  |  |  | DFID001 |
