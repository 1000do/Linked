###### **2.2.3.92 Send Notifications**

| UC ID and Name | UC-77-Send Notifications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 16/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Actor clicks the "Create Notification" button, enters recipient and message details, and submits the form. |  |  |
| Description: | Allows authenticated Staff or Admin users to compose and send notifications either to a specific registered user (using email tags) or broadcasted to all users. |  |  |
| Preconditions: | **PRE-1**. User must be successfully authenticated as a Staff or Admin. |  |  |
| Post-conditions: | **POST-1.** The notification record is created and stored in the database. **POST-2.** The SignalR hub pushes the notification in real time to any active online recipients. **POST-3.** The system updates the sender's sent notifications audit log table. |  |  |
| Normal Flow: | **A. Send Notification to Specific User Successfully** 1\. Actor accesses the System Notifications page. 2\. Actor clicks the "Create Notification" button. 3\. The system opens the "Compose New Notification" modal dialog. 4\. Actor selects "Specific User (Email)" from the Recipient Group dropdown list. 5\. Actor types the recipient's email address in the tag box. 6\. Actor enters the notification title (maximum 255 characters). 7\. Actor enters the notification message content. 8\. Actor clicks the "Send Notification" button. 9\. The system requests the backend to send the advanced notification. 10\. The server validates that the active user possesses a Staff or Admin role. 11\. The server validates that the recipient list contains valid registered emails and does not include the sender's own email. 12\. The server validates the length constraints of the title. 13\. The server saves the notification record to the database for each recipient user. 14\. The server broadcasts the new notification event via SignalR. 15\. The server returns a success response. 16\. The system displays a success popup message indicating the notification was successfully sent. 17\. The system closes the modal dialog and refreshes the sent notifications table list. |  |  |
| Alternative Flows: | **A.4 Broadcast to all users** 1\. Actor selects "Broadcast (All Users)" from the Recipient Group dropdown list. 2\. The system hides the Recipient Email tag input box. 3\. Back to step 6\.  **A.5.1 Autocomplete email suggestions** 1\. Actor types characters in the Recipient Email input box. 2\. The system queries matching registered emails from the server. 3\. The system displays a dropdown list of matching emails that are not already selected. 4\. Actor clicks an email option from the dropdown. 5\. The system adds the email as a tag pill in the tag box.  **A.5.2 Add multiple emails** 1\. Actor types an email address and presses Enter or selects it from the suggestions. 2\. The system adds the email tag pill in the tag box. 3\. Actor continues typing additional email addresses.  **A.5.3 Remove email pill** 1\. Actor clicks the close (X) icon on one of the email tag pills. 2\. The system removes the tag pill from the recipient list. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page.  **EX-5 Missing Recipient Email** 1\. Actor attempts to submit the form under the "Specific User" group without adding any email tags. 2\. The system displays a warning notification indicating "Please select at least one recipient email." 3\. Back to step 4\.  **EX-6 Self-Notification Blocked** 1\. The actor attempts to add their own email address as a recipient. 2\. The system displays a warning notification: "You cannot send a notification to yourself." 3\. Back to step 5\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-38,BR-39 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff or Admin has stable internet connection. SignalR Hub service is online. |  |  |

###### **2.2.3.91  View Notification List** 

| UC ID and Name | UC-78-View Notification List  |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created: | 16/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin accesses the System Notifications settings page. |  |  |
| Description: | Allows authenticated Staff or Admin to view the list of all system-sent notifications, with search, role tab filtering, status filtering, sorting, page navigation, and detail view modal options. |  |  |
| Preconditions: | **PRE-1**. Staff or Admin must be successfully authenticated. |  |  |
| Post-conditions: | **POST-1.** The system displays the comprehensive notifications log table, role tabs, search bar, and filter controls. |  |  |
| Normal Flow: | **A. View Notification List Successfully** 1\. Staff or Admin accesses the System Notifications page. 2\. The system requests the list of all notifications from the server. 3\. The server validates that the active user possesses a Staff or Admin role. 4\. The server retrieves all notifications log from the database. 5\. The server returns the list of notifications. 6\. The system displays the notifications log table list, along with search and filter inputs. |  |  |
| Alternative Flows: | **A.2 Filter by role tab** 1\. Staff or Admin clicks a role tab (e.g. "Students", "Instructors", or "Staff & Admin"). 2\. The system filters the list to display only notifications sent to recipients matching the selected role.  **A.2 Search notifications** 1\. Staff or Admin enters a search term in the keyword search box. 2\. The system filters the list to display only notifications matching the keyword in recipient name, email, title, or content.  **A.2 Filter by status** 1\. Staff or Admin selects a status option (e.g. "Unread") from the status filter dropdown. 2\. The system filters the list to display only notifications matching the selected status.  **A.2 Sort notifications** 1\. Staff or Admin selects a sorting option (e.g. "Recipient (A \- Z)") from the sort dropdown. 2\. The system sorts the displayed notification list accordingly.  **A.2 Clear filters** 1\. Staff or Admin clicks the "Reset" button. 2\. The system resets the search keyword, status filter, sorting order, and active tab to their default states. 3\. The system displays the full list of notifications.  **A.2 View details modal** 1\. Staff or Admin clicks on a notification row. 2\. The system displays the notification detail modal showing the recipient, title, full content, timestamp, and status.  |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff or Admin has stable internet connection. SignalR service is online.  |  |  |

###### 

###### **2.2.3.93 View Notification Detail** 

| UC ID and Name | UC-79-View Notification Detail |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created: | 16/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin clicks on a notification row in the System Notifications table. |  |  |
| Description: | Allows authenticated Staff or Admin to view the detailed recipient, title, content, creation timestamp, and read status of a notification. If a Staff member views an unread notification addressed to themselves, the system automatically marks it as read. |  |  |
| Preconditions: | **PRE-1.** Staff or Admin must be successfully authenticated. **PRE-2.** The target notification must exist. |  |  |
| Post-conditions: | **POST-1.** The system displays the notification detail modal. **POST-2.** If the viewer is the recipient and the notification was unread, the read status is updated to read in the database and updated in the UI. |  |  |
| Normal Flow: | **A. View Notification Detail Successfully** 1\. Staff or Admin accesses the System Notifications page. 2\. Staff or Admin locates the desired notification in the list. 3\. Staff or Admin clicks the notification row. 4\. The system displays the notification detail modal, showing the recipient name and email, message title, full message details, creation timestamp, and read status. 5\. Staff or Admin clicks the "Close" button. 6\. The system closes the detail modal. |  |  |
| Alternative Flows: | **A.4 Staff views own unread notification** 1\. The system detects that the viewer is a Staff member, the notification recipient matches the viewer's user ID, and the notification is currently unread. 2\. The system requests the backend to mark the notification as read. 3\. The server validates that the active user is the recipient of the notification. 4\. The server updates the notification's status to read in the database. 5\. The server returns a success response. 6\. The system updates the detail status badge to Read and refreshes the background table view dynamically.  |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-40 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff or Admin has a stable internet connection. |  |  |

###### 

###### **2.2.3.94  Search Notifications**

| UC ID and Name | UC-80-Search Notifications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin enters text in the search input field on the System Notifications page. |  |  |
| Description: | Allows authenticated Staff or Admin to search the system notifications log by recipient name, email address, message title, or content. |  |  |
| Preconditions: | **PRE-1.** Staff or Admin must be successfully authenticated. **PRE-2.** Staff or Admin is on the System Notifications page. |  |  |
| Post-conditions: | **POST-1.** The system displays only those notifications in the table list that match the search query. |  |  |
| Normal Flow: | **A. Search Notifications Successfully** 1\. Staff or Admin accesses the System Notifications page. 2\. Staff or Admin types a search query (e.g. recipient name, email, title keyword, or content keyword) in the search input box. 3\. The system filters the notifications list in real time as the text is entered. 4\. The system displays the matching notifications in the table, preserving any active role tab filters, status filters, or sorting orders. |  |  |
| Alternative Flows: | **A.3 Clear search query** 1\. Staff or Admin deletes all text from the search input box. 2\. The system filters the notifications list using only the remaining active role tab filters, status filters, and sorting orders. 3\. The system displays the updated list in the table.  **A.4 No matching notifications found** 1\. The search query does not match any notifications in the list. 2\. The system displays a message indicating "No notifications found matching your filters". |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff or Admin has stable internet connection |  |  |

###### 

###### **2.2.3.95  Sort Notifications**

| UC ID and Name | UC-81-Sort Notifications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin selects a sorting option from the sort selector on the System |  |  |
| Description: | Allows authenticated Staff or Admin to sort the system notifications log by timestamp, title, or recipient name. |  |  |
| Preconditions: | **PRE-1.** Staff or Admin must be successfully authenticated. **PRE-2.** Staff or Admin is on the System Notifications page. |  |  |
| Post-conditions: | **POST-1**. The system displays the notifications in the table sorted according to the selected sorting option. |  |  |
| Normal Flow: | **A. Sort Notifications Successfully** 1\. Staff or Admin accesses the System Notifications page. 2\. Staff or Admin selects a sorting option (e.g. "Title (A \- Z)") from the sort dropdown list. 3\. The system sorts the notification list in real time as the selection changes. 4\. The system displays the sorted notifications list in the table, preserving any active role tab filters, status filters, or search keywords. |  |  |
| Alternative Flows: | **A.3 Clear sorting** 1\. Staff or Admin clicks the Reset button. 2\. The system resets the sort selector to its default ("Newest First"). 3\. The system displays the sorted notifications list in the table. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff or Admin has stable internet connection. |  |  |

###### **2.2.3.96  Filter Notifications**

| UC ID and Name |  UC-82-Filter Notifications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 18/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin clicks a recipient role tab or selects a status option from the filter dropdown on the System Notifications page. |  |  |
| Description: | Allows authenticated Staff or Admin users to filter the system notifications log by recipient role and/or read status. |  |  |
| Preconditions: | **PRE-1.** Staff or Admin must be successfully authenticated. **PRE-2.** Staff or Admin is on the System Notifications page. |  |  |
| Post-conditions: | **POST-1**. The system displays the notifications in the table filtered according to the selected filter criteria. |  |  |
| Normal Flow: | **A. Filter Notifications Successfully** 1\. Actor  accesses the System Notifications page. 2\. Actor  clicks a recipient role tab button (e.g. "Students"). 3\. Actor  selects a status filter option (e.g. "Unread") from the status dropdown. 4\. The system filters the notifications list in real time. 5\. The system displays the filtered notifications list in the table, preserving any active search keywords or sorting orders. |  |  |
| Alternative Flows: | **A.3 Clear filters** 1\. Staff or Admin clicks the Reset button. 2\. The system resets the active role tab to "All Notifications" and the status dropdown to "All Statuses". 3\. The system displays the unfiltered notifications list in the table. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Actor. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor  has stable internet connection.  |  |  |

###### 

###### 

