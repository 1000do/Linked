###### **2.2.3.110  View Instructor Applications**

| UC ID and Name | UC-95-View Instructor Applications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin accesses the instructor approval/reject requests page. |  |  |
| Description: | Allows authenticated Staff and Admin users to view the list of pending instructor registration requests with applicant details, search, filter, and sort options. |  |  |
| Preconditions: | **PRE-1.** User must be successfully authenticated as a Staff member or Admin. |  |  |
| Post-conditions: | **POST-1**. The system displays the list of pending instructor applications with applicant details, portfolio/resume links, and processing controls. |  |  |
| Normal Flow: | **A. View Instructor Applications Successfully** 1\. Staff/Admin accesses the instructor registration requests page. 2\. The system requests the list of pending instructor applications from the server. 3\. The server validates that the active user possesses a Staff or Admin role. 4\. The server retrieves the pending instructor applications from the database. 5\. The system displays the list of pending instructor applications to the Staff/Admin, including applicant details, specialization, LinkedIn link, and resume/portfolio documents. |  |  |
| Alternative Flows: | **A.5 No pending applications found** 1\. The server returns an empty list of pending applications. 2\. The system displays an empty state message indicating no registration requests were found. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff/Admin has stable internet connection. |  |  |

###### **2.2.3.111  Approve Instructor Applications**

| UC ID and Name | UC-96-Approve Instructor Applications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin clicks the "Approve" button next to a pending application. |  |  |
| Description: | Allows authenticated Staff or Admin users to approve an instructor's pending registration application, which promotes their role to Instructor in the database and updates their status. |  |  |
| Preconditions: | **PRE-1**. User must be successfully authenticated as a Staff member or Admin. **PRE-2**. The instructor's application must exist in a pending status. |  |  |
| Post-conditions: | **POST**\-**1**. The instructor's status is updated to approved in the database. **POST**\-**2**. The user's account role is updated to Instructor in the database. **POST**\-**3**. The approved application is removed from the active pending list dashboard. |  |  |
| Normal Flow: | **A. Approve Instructor Application Successfully** 1\. Staff/Admin accesses the instructor registration requests page. 2\. Staff/Admin locates the desired pending application. 3\. Staff/Admin clicks the "Approve" button. 4\. The system displays a confirmation dialog. 5\. Staff/Admin clicks the "Approve" button on the confirmation dialog. 6\. The system updates both the instructor's application status to "Approved" and the user's role to "Instructor" in the database. 7\. The server returns a success response. 8\. The system displays a success notification message. 9\. The system removes the approved application row from the pending list dashboard. |  |  |
| Alternative Flows: | **A.5 Cancel approval** 1\. Staff/Admin clicks the "Cancel" button on the confirmation dialog. 2\. The system closes the confirmation dialog. 3\. Back to step 2\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page.  **EX-6 Processing error on server** 1\. The server fails to update the application status or user role in the database. 2\. The system displays an error message regarding server processing failure. 3\. Back to step 2\. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-47 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff/Admin has stable internet connection. |  |  |

###### 

###### 

###### **2.2.3.112  Reject Instructor Application**

| UC ID and Name | UC-97-Reject Instructor Application |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin clicks the "Reject" button next to a pending application. |  |  |
| Description: | Allows authenticated Staff or Admin users to reject an instructor's pending registration application. A detailed rejection reason is required, which is recorded in the database. |  |  |
| Preconditions: | **PRE-1.** Users must be successfully authenticated as a Staff member or Admin. **PRE-2.** The instructor's application must exist in a pending status. |  |  |
| Post-conditions: | **POST-1.** The instructor's status is updated to reject in the database, along with the specified reason. **POST-2.** The rejected application is removed from the active pending list dashboard. |  |  |
| Normal Flow: | **A. Reject Instructor Application Successfully** 1\. Staff/Admin accesses the instructor registration requests page. 2\. Staff/Admin locates the desired pending application. 3\. Staff/Admin clicks the "Reject" button. 4\. The system displays a dialog prompting the user to enter a reason for rejection. 5\. Staff/Admin enters a reason in the rejection text field. 6\. Staff/Admin clicks the "Reject" button on the dialog. 7\. The system requests to process the application rejection from the server. 8\. The server validates that the active user possesses a Staff or Admin role. 9\. The server updates the instructor's application status to rejected in the database, including the reason. 10\. The server returns a success response. 11\. The system displays a success notification message. 12\. The system removes the rejected application row from the pending list dashboard. |  |  |
| Alternative Flows: | **A.5 Cancel rejection** 1\. Staff/Admin clicks the "Cancel" button on the rejection dialog. 2\. The system closes the rejection dialog. 3\. Back to step 2\. **A.6 Submit empty rejection reason** 1\. Staff/Admin leaves the rejection reason blank and clicks the "Reject" button on the dialog. 2\. The system displays a validation error message requiring a reason for rejection. 3\. Back to step 5\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page.  **EX-8 Processing error on server** 1\. The server fails to update the application status or user role in the database. 2\. The system displays an error message regarding server processing failure. 3\. Back to step 2\. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-48 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff/Admin has stable internet connection. |  |  |

###### **2.2.3.109 View Instructor Applications Detail**

| UC ID and Name | UC-98-View Instructor Applications Detail |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin clicks the "View Info" link for a pending instructor application. |  |  |
| Description: | Allows authenticated Staff or Admin users to inspect the detailed credentials of an instructor applicant, including their external LinkedIn, YouTube, and Facebook profiles, and submitted resume or portfolio documents. |  |  |
| Preconditions: | **PRE-1.** User must be successfully authenticated as a Staff member or Admin. **PRE-2.** The instructor's application must exist in a pending status. |  |  |
| Post-conditions: | **POST-1.** The system displays the detailed credentials of the selected instructor applicant. |  |  |
| Normal Flow: | **A. View Application Details Successfully.** 1.Staff or Admin accesses the pending applications list page. 2.Staff or Admin clicks the "View Info" link. 3.The system retrieves the detailed data of the selected instructor 4.application. 5.The system displays the applicant's profile details, including their avatar, full name, email, professional title, expertise areas, social media links, and submitted documents. 6.The system displays the "Approve" and "Reject" buttons.  |  |  |
| Alternative Flows: | **A.6 Cancel viewing details** Staff or Admin clicks the "Cancel" button or the "Back to List" link. Back to step 1\.  |  |  |
| Exceptions: | **EX.1 Session Invalid** 1\. If the session is invalid or the role is insufficient. 2\. The system redirects to the login page.  **EX.3 Application Not Found** 1\. If the application ID is invalid or was processed by another manager moments before. 2\. The system displays an "Application not found" error and may redirect the user back to the refreshed list. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff and Admin have stable internet connection. |  |  |

###### **2.2.3.113  Search Instructor Applications**

| UC ID and Name | UC-99-Search Instructor Applications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin enters search text in the search input field. |  |  |
| Description: | Allows authenticated Staff or Admin users to search the pending instructor applications list by name, email, professional title, or expertise specialization in real-time. |  |  |
| Preconditions: | **PRE-1**. User must be successfully authenticated as a Staff member or Admin. **PRE-2**. The instructor applications list is loaded on the page. |  |  |
| Post-conditions: | **POST-1**. The system filters the visible applications table based on the search query. |  |  |
| Normal Flow: | **A. Search Instructor Applications Successfully** 1\. Staff/Admin accesses the instructor registration requests page. 2\. Staff/Admin enters a search query (e.g. name, email, title, or expertise) in the search input field. 3\. The system filters the loaded list of pending applications to match the search query. 4\. The system displays the matching applications in the table. |  |  |
| Alternative Flows: | **A.3 Clear search query** 1\. Staff/Admin clears the text from the search input field. 2\. The system resets the filters and displays all applications in the table.  **A.4 No matching applications found** 1\. Staff/Admin enters a query that does not match any application. 2\. The system displays an empty state message indicating no matching registration requests were found. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff/Admin has stable internet connection. |  |  |

###### **2.2.3.114  Filter Instructor Applications**

| UC ID and Name | UC-100-Filter Instructor Applications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin selects a specialization option from the filter dropdown. |  |  |
| Description: | Allows authenticated Staff or Admin users to filter the pending instructor applications list by applicant expertise or specialization. |  |  |
| Preconditions: | **PRE-1**. User must be successfully authenticated as a Staff member or Admin. **PRE-2**. The instructor applications list is loaded on the page. |  |  |
| Post-conditions: | **POST-1**. The system displays only those applications that match the selected specialization. |  |  |
| Normal Flow: | **A. Filter Instructor Applications Successfully** 1\. Staff/Admin accesses the instructor registration requests page. 2\. The system dynamically populates the filter dropdown with unique expertise categories from the loaded pending applications. 3\. Staff/Admin selects an expertise specialization option from the dropdown menu. 4\. The system filters the pending applications list to display only those containing the selected expertise specialization. 5\. The system updates the table rows to show only matching applicants. |  |  |
| Alternative Flows: | **A.3 Select "All Specializations"** 1\. Staff/Admin selects the "All Specializations" option from the dropdown. 2\. The system resets the expertise filter and displays all applications.  **A.4 No matching applications found** 1\. Staff/Admin combines the expertise filter with a search query that has no match. 2\. The system displays an empty state message indicating no matching registration requests were found. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff/Admin has stable internet connection. |  |  |

###### 

###### **2.2.3.115  Sort Instructor Applications**

| UC ID and Name | UC-101-Sort Instructor Applications |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Truong Hung | Date Created:  | 17/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Staff or Admin selects a sorting option from the sort dropdown. |  |  |
| Description: | Allows authenticated Staff or Admin users to sort the pending instructor applications list by application date (newest/oldest) or alphabetical order of the applicant's name. |  |  |
| Preconditions: | **PRE-1**. User must be successfully authenticated as a Staff member or Admin. **PRE-2**. The instructor applications list is loaded on the page. |  |  |
| Post-conditions: | **POST-1**. The system updates the applications table to display applications in the selected sort order. |  |  |
| Normal Flow: | **A. Sort Instructor Applications Successfully** 1\. Staff/Admin accesses the instructor registration requests page. 2\. Staff/Admin selects a sorting criteria option from the sort dropdown menu. 3\. The system sorts the loaded pending applications list according to the selected criteria. 4\. The system updates the table rows to display the sorted list of applicants. |  |  |
| Alternative Flows: | **A.4 No matching applications found** 1\. Staff/Admin combines the expertise sort with a sort query that has no match. 2\. The system displays an empty state message indicating no matching registration requests were found. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Unauthorized Access** 1\. The server detects that the user is not a Staff or Admin. 2\. The system redirects the user to the access denied error page.  |  |  |
| Priority: | Medium  |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Staff/Admin has stable internet connection. |  |  |

###### 

