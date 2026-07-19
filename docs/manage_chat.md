###### **2.2.3.97  View Chat List** 

| UC ID and Name | UC-83-View Chat List  |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 16/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Actor accesses the Chat page under the Admin Panel dashboard.  |  |  |
| Description: | Allows the Actor to view their list of active support or moderation conversations, view pending support request tickets, search for specific conversations, filter by unread status, and sort the conversation list.  |  |  |
| Preconditions: | **PRE-1**. Actor  must be successfully authenticated. |  |  |
| Post-conditions: | **POST-1.** The system renders the list of active support/moderation conversations in the chat sidebar. **POST-2.** The system displays the list of pending support requests in the sidebar. |  |  |
| Normal Flow: | **A. View Chat List Successfully** 1.Actor accesses the Chat Management page. 2.The system checks the Actor's authentication status. 3.The system validates that the Actor's role is Staff or Admin. 4.The system requests the list of active support/moderation conversations. 5.The system requests the list of pending support requests. 6.The system retrieves the list of active support/moderation conversations from the database. 7.The system retrieves the list of pending support requests from the database. 8.The system displays the Chat page with the chat. |  |  |
| Alternative Flows: | **A.8.6 Contact Admin** 1.Actor clicks the \`"Contact Admin"\` button. 2.The system requests the Admin account details. 3.The system creates a direct chat channel with the Admin. 4.The system updates the conversation list and opens the chat room. 5.Back to step 8\. **A.8.7 Accept Support Request** 1.Actor clicks the \`"Accept Request"\` button next to a pending support ticket. 2.The system updates the support ticket status to accepted. 3.The system creates a support chat room between the Actor and the requester. 4.The system removes the support ticket from the pending requests list. 5.The system updates the conversation list and opens the support chat room. 6.Back to step 8\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Actor. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-36,BR-37,BR-220,BR-221 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor  has stable internet connection. |  |  |

###### **2.2.3.100  View Chat History**

| UC ID and Name | UC-84-View Chat History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 16/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Actor  clicks on a conversation card in the sidebar. |  |  |
| Description: | Allows authenticated Actor  users to view the entire message exchange history of a selected conversation, which automatically marks unread messages in the room as read. |  |  |
| Preconditions: | **PRE-1**. Actor must be successfully authenticated. **PRE-2**. Actor  must have access to the selected chat room. |  |  |
| Post-conditions: | **POST-1**. The chat history is displayed to the Staff or Admin. **POST-2**. All unread messages in the conversation for this participant are marked as read in the database and cache. |  |  |
| Normal Flow: | **A. View Chat History Successfully** 1\. Staff or Admin accesses the Chat Management page. 2\. Actor  clicks a conversation card in the sidebar chat list. 3\. The system hides the empty chat state panel. 4\. The system displays the chat room panel. 5\. The system updates the chat header with partner name, avatar, and online status. 6\. The system requests the backend to retrieve the chat history. 7\. The server verifies that the active Staff or Admin has permission to access the chat (either as a participant or through a support/moderation override). 8\. The server marks all unread messages in the chat for the active Staff or Admin as read in the database. 9\. The server clears the unread count for the active actor  in the cache. 10\. The server retrieves the conversation messages from the database. 11\. The server filters out messages sent before or on the participant's cleared timestamp. 12\. The system receives the message history payload. 13\. The system renders the messages in the message list viewport. 14\. The system scrolls the message list viewport to the bottom. 15\. The system broadcasts a read notification to other participants. |  |  |
| Alternative Flows: | **A.12 Render empty chat room** 1\. The system receives an empty message list payload. 2\. The system displays a start-chatting message in the viewport. 3\. Back to step 14 of the Normal Flow. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-36,BR-37 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff or Admin has stable internet connection. |  |  |

###### **2.2.3.99  Send Messages**

| UC ID and Name | UC-85-Send Messages  |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 16/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Actor in clicks the send button or presses the Enter key in the message input area of the active chat. |  |  |
| Description: | Allows authenticated Actor  users to send text messages and optionally attach files (images or documents) within an active support/moderation conversation room. The message is transmitted in real time to the recipient. |  |  |
| Preconditions: | **PRE-1**.Actor  must be successfully authenticated. **PRE-2**. Actor  must have an active conversation open. |  |  |
| Post-conditions: | **POST-1**. The message record is saved in the database. **POST-2**. The message is broadcasted to all participants in the conversation in real time. **POST-3**. The unread count of the recipients is updated in the system cache. |  |  |
| Normal Flow: | **A. Send Message Successfully** 1\. Actor accesses the active conversation panel. 2\. Actor enters the message text in the "Type a message..." textarea. 3.Actor clicks the "send" button. 4\. The system validates that the message content is not empty. 5\. The system sends the message payload to the server via the active connection. 6\. The server checks if the sender is an authorized participant of the chat. 7\. The server creates a new message record with "ok" status. 8\. The server saves the message record to the database. 9\. The server increments the unread count for recipients in the cache. 10\. The server broadcasts the message to all participants in the chat room. 11\. The system displays the sent message in the message list viewport with a sent indicator. 12\. The system clears the input textarea. |  |  |
| Alternative Flows: |  **A.12 Remove selected attachment before sending** 1\. Staff or Admin clicks the "close" icon next to the attached file name in the preview container. 2\. The system removes the file from the preview list. 3\. Back to step 2 of the Normal Flow. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3.1 Unauthorized Access** 1\. The server detects that the user is not a Actor. 2\. The system redirects the user to the access denied error page.  **EX-3.2 Empty message** 1\. The system detects both the message text and selected attachments are empty. 2\. The system cancels the submission. 3\. Back to step 2 of the Normal Flow. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-46 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. |  |  |

###### **2.2.3.98  Delete Chats** 

| UC ID and Name | UC-86-Delete Chats |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 11/07/2025 |
| Primary Actor: | Staff, Admin	 | Secondary Actors: | N/A |
| Trigger: | Staff or Admin clicks the delete icon in the active chat header. |  |  |
| Description: | Allows authenticated Staff or Admin users to clear their personal chat history of a conversation. Clearing history only removes the messages from the active participant's perspective by updating their participant cleared timestamp. |  |  |
| Preconditions: | **PRE-1**. Staff or Admin must be successfully authenticated. **PRE-2**. Staff or Admin must have an active conversation open. |  |  |
| Post-conditions: | **POST-1**. The participant's cleared timestamp is updated in the database. **POST-2**. The message viewport is cleared. **POST-3**. The active chat list is refreshed in the sidebar. |  |  |
| Normal Flow: | **A. Delete Chats Successfully** 1\. Staff or Admin accesses the messages page under the Admin Panel dashboard. 2\. Staff or Admin clicks the "delete" icon in the active conversation header. 3\. The system displays a confirmation dialog. 4\. Staff or Admin clicks the "OK" button. 5\. The system requests the backend to clear the chat history. 6\. The server validates that the active user possesses a Staff or Admin role. 7\. The server updates the cleared timestamp of the participant to the current time. 8\. The server saves the participant record changes to the database. 9\. The system clears the message list from the viewport. 10\. The system displays the empty chat state. 11\. The system updates the active chat list in the sidebar. |  |  |
| Alternative Flows: | **A.4 Cancel chat deletion** 1\. Staff or Admin clicks the "Cancel" button. 2\. The system closes the confirmation dialog. 3\. Back to step 2\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page.  **EX-6 Participant not found** 1\. The system displays an error message regarding chat access permission. 2\. The system halts the execution. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-45 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. |  |  |

###### **2.2.3.101 Search Chat History**

| UC ID and Name | UC-87-Search Chat History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 29/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin enters a search query in the search input field of the chat sidebar. |  |  |
| Description: | Allows authenticated Staff or Admin users to search their conversation list by sending a query to the backend, which searches participant profiles and returns matching conversation rooms. |  |  |
| Preconditions: | **PRE-1**.Actor  must be successfully authenticated. **PRE-2**. Actor is on the Chat page under the Admin Panel dashboard. |  |  |
| Post-conditions: | **POST-1**. The sidebar chat list displays only conversations matching the search query. |  |  |
| Normal Flow: | **A. Search Chats Successfully** 1.Actor accesses the Chat page. 2\. Actor types a search query in the "Search users..." input box. 3\. The system detects the input change and waits for a typing pause. 4\. The system requests the backend to search conversations matching the query. 5\. The server validates that the active Staff or Admin is authenticated. 6\. The server queries the database for chat rooms where the current Staff or Admin is a participant and the other participant's name matches the search query. 7\. The server returns the matching conversation list. 8\. The system displays the matching conversations in the sidebar. |  |  |
| Alternative Flows: | **A.3 Clear search query** 1\. Staff or Admin clears the search query text box. 2\. The system restores the full list of active conversations from the local cache. 3\. The system displays the full active conversation list. **A.8 No matching results found** 1\. The server returns an empty list. 2\. The system displays a message in the sidebar indicating "No conversations found." |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Actor. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. |  |  |

###### 

