###### **2.2.3.33 View Notification list**

| UC ID and Name | UC-29 \- View Notification list |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung	 | Date Created: | 10/07/2026 |
| Primary Actor: | User, Instructor | Secondary Actors: | N/A |
| Trigger: | User or Instructor clicks on the notification icon or accesses the notifications page. |  |  |
| Description: | Allows users and instructors to view their list of notifications, search, sort, filter, view details, mark notifications as read, mark all notifications as read, or delete notifications. |  |  |
| Preconditions: | **PRE-1.** Actor must be successfully authenticated. |  |  |
| Post-conditions: | **POST-1.** The system displays the list of notifications matching the search, filter, and pagination criteria. |  |  |
| Normal Flow: | **A. View Notification List Successfully** 1\. Actor accesses the notifications page. 2\. The system checks the active login session. 3\. The system requests the backend to retrieve the notification list for the authenticated Actor. 4\. The server validates the Actor's session token. 5\. The server queries the database for all notifications addressed to the Actor's account ID. 6\. The server returns the notification list. 7\. The system receives the notification list payload. 8\. The system displays the notifications list on the page, showing the title, content, system label, unread indicator, action links, and creation date. 9\. The system renders the pagination controls based on the total notifications count. |  |  |
| Alternative Flows: | **A.2.1 Filter notifications by read state** 1\. Actor selects a filter option (e.g. "Unread" or "Read") from the status dropdown. 2\. The system filters the notification list on the client side based on the selected status. 3\. The system updates the displayed list and pagination controls. 4\. Back to step 8 of the Normal Flow.  **A.2.2 Search notifications** 1\. Actor enters a keyword in the "Search by title or content..." input box. 2\. The system filters the notification list on the client side to only include notifications with titles or contents matching the keyword. 3\. The system updates the displayed list and pagination controls. 4\. Back to step 8 of the Normal Flow.  **A.2.3 Sort notifications** 1\. Actor selects a sorting option (e.g. "Title (A-Z)" or "Oldest First") from the sort dropdown. 2\. The system sorts the notification list on the client side based on the selected sorting order. 3\. The system updates the displayed list. 4\. Back to step 8 of the Normal Flow. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-32 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User or Instructor has stable internet connection. SignalR Service is working properly. |  |  |

###### **2.2.3.34 View Notification Detail**

| UC ID and Name | UC-30 \- View Notification Detail |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung	 | Date Created: | 10/07/2026 |
| Primary Actor: | User, Instructor | Secondary Actors: | N/A |
| Trigger: | User or Instructor clicks on a notification row in the notifications list. |  |  |
| Description: | Allows users and instructors to view the detailed content of a notification in a modal dialog. If the notification is unread, the system automatically marks it as read. |  |  |
| Preconditions: | **PRE-1.** User or Instructor must be successfully authenticated. **PRE-2.** The notifications page must be successfully loaded and displaying notifications. |  |  |
| Post-conditions: | **POST-1.** The detailed content of the notification is displayed in the modal. **POST-2.** If the notification was unread, its status is updated to read in the database. |  |  |
| Normal Flow: | **A. View Notification Detail Successfully** 1\. User or Instructor clicks on a notification row. 2\. The system retrieves the notification details from the client-side list. 3\. The system displays the notification details modal. |  |  |
| Alternative Flows: | **A.3.1 Auto-mark notification as read** 1\. The system detects the notification is unread. 2\. The system requests the backend to mark the notification as read. 3\. The system updates the notification status to read in the database. 4\. The system updates the list UI to display the notification as read. 5\. Back to step 3\.  **A.3.2 Redirect to action link** 1\. The system detects the notification has an action link. 2\. The system displays the "View Details" button. 3\. User or Instructor clicks the "View Details" button. 4\. The system redirects the actor to the corresponding action URL.  **A.3.3 Close details modal** 1\. User or Instructor clicks the close icon, clicks the "Close" button, or clicks the backdrop area. 2\. The system closes the details modal. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-32 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User or Instructor has stable internet connection. |  |  |

###### **2.2.3.35 Mark As Read**

| UC ID and Name | UC-31 \- Mark As Read |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created: | 10/07/2026 |
| Primary Actor: | User, Instructor | Secondary Actors: | N/A |
| Trigger: | User or Instructor clicks the "Mark as read" button (check circle icon) next to an unread notification OR clicks on an unread notification to view its details. |  |  |
| Description: | Allows users and instructors to mark their received notifications as read. The system updates the notification status in the database, notifies active client sessions in real-time, and updates the unread count in the UI. |  |  |
| Preconditions: | **PRE-1.** User or Instructor must be successfully authenticated. |  |  |
| Post-conditions: | **POST-1**. The selected notification (or all notifications) is updated with a read status in the database. **POST-2**. The unread notification count is updated in the UI. |  |  |
| Normal Flow: | **A. Mark Notification As Read Successfully** 1\. User or Instructor accesses the notifications page. 2\. User or Instructor clicks the "Mark as read" button on a specific unread notification. 3\. The system validates the user's session. 4\. The system validates the notification ownership. 5\. The system updates the notification's read status to 'read' in the database. 6\. The system sends a real-time update event to the user's active client connections. 7\. The system updates the UI to display the notification as read. 8\. The system updates the unread notification count in the navigation bar. |  |  |
| Alternative Flows: | **A.2.1 Click notification to view details** 1\. User or Instructor clicks on the unread notification item. 2\. The system displays the notification detail modal. 3\. Continue to step 3\.  **A.2.2 Mark all notifications as read** 1\. User or Instructor clicks the "Mark All as Read" button. 2\. The system validates if there are unread notifications. 3\. The system validates the user's session. 4\. The system updates all unread notifications for the user to read status in the database. 5\. The system sends a real-time update event to the user's active client connections. 6\. The system updates the UI to display all notifications as read. 7\. The system updates the unread notification count in the navigation bar. 8\. The system displays a success confirmation message. 9\. Back to step 2\.  **A.2.2.2 No unread notifications** 1\. The system displays a message indicating all notifications are already read. 2\. Back to step 2\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-4 Notification Not Found or Access Denied** 1.The system halts execution and displays a notification not found or access denied error message. 2.Back to step 2\. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-32,BR-33 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User or Instructor has stable internet connection. |  |  |

###### **2.2.3.36 Delete Notifications**

| UC ID and Name | UC-32 \- Delete Notifications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created: | 10/07/2026 |
| Primary Actor: | User, Instructor | Secondary Actors: | N/A |
| Trigger: | User or Instructor clicks the delete icon next to a notification. |  |  |
| Description: | Allows users and instructors to permanently delete a notification. The system displays a confirmation dialog before sending the request to the database. |  |  |
| Preconditions: | **PRE-1.** User or Instructor must be successfully authenticated. **PRE-2.** The notification must exist in the database and belong to the active user. |  |  |
| Post-conditions: | **POST-1.** The notification is permanently deleted from the database. **POST-2.** The notification is removed from the UI. |  |  |
| Normal Flow: | **A. Delete Notification Successfully** 1\. User or Instructor clicks the delete icon next to a notification. 2\. The system displays a confirmation dialog. 3\. User or Instructor clicks the "Delete" button. 4\. The system sends a delete request to the backend. 5\. The system deletes the notification from the database. 6\. The system displays a deletion success notification. 7\. The system updates the UI to remove the notification. |  |  |
| Alternative Flows: | **A.3.1 Cancel deletion** 1\. User or Instructor clicks the "Cancel" button on the confirmation dialog. 2\. The system closes the confirmation dialog without making any changes. 3\. Back to step 1\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-4 Delete request failed or access denied** 1\. The system displays an error message regarding missing notification or permission. 2\. Back to step 1\. |  |  |
| Priority: | Low |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-34 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User or Instructor has stable internet connection. |  |  |

###### 

###### 

###### 

