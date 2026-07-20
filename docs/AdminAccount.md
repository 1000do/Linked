###### **2.2.3.128  Add Account**

| UC ID and Name | UC-128-Add Account |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Add Staff Member” button, fills in details, and saves. |  |  |
| Description: | Allows an Admin to register a new Staff account within the account management panel by specifying details. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. |  |  |
| Post-conditions: | **POST-1.** A new Account and Manager record (role \= "staff") are successfully created in the database. **POST-2**. The system displays a success message and redirects to the account list view. |  |  |
| Normal Flow: | **A. Add Account Successfully** 1\. Admin accesses the Manage Accounts page. 2\. Admin clicks the Add Staff Member button. 3\. The system displays the account creation form. 4\. Admin enters the staff details: Email, Display Name, Temporary Password, Phone Number. 5\. Admin clicks the Save button. 6\. The system checks that the mandatory fields are provided. 7\. The system checks if the email is already registered in the database. 8\. The system hashes the password using BCrypt. 9\. The system saves the new Account details to the database. 10\. The system displays a success message and redirects to the account list view. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page.  **EX-6 Mandatory Fields Missing** 1\. The system detects that Email, Password, or Display Name is empty. 2\. The system displays a validation error message. 3\. Back to step 4\.  **EX-7 Email Already Registered** 1\. The system detects that the entered email address already exists in the database. 2\. The system displays the error message: "Email is already registered." 3\. Back to step 4\.  **EX-9 Database Save Failure** 1\. The system detects that saving the account or manager details to the database failed. 2\. The system displays an error message. 3\. Back to step 4\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-224, BR-249, BR-250  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.129  Update Account**

| UC ID and Name | UC-129-Update Account |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin  | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Edit" button next to a staff account, modifies details, and saves. |  |  |
| Description: | Allows an Admin to update profile details, change status, and reset the password of an existing Staff account. The system validates entries and manages lockout records if the account status is updated to or from banned. |  |  |
| Preconditions: | **PRE-1.** Admin must be logged in with Admin role privileges. **PRE-2.** The target account must exist and belong to a Staff member. |  |  |
| Post-conditions: | **POST-1**. The updated account profile details are saved to the database. **POST-2**. Lockout records are updated in the database if the status is changed to or from banned. **POST-3**. The system displays a success message and redirects to the account list. |  |  |
| Normal Flow: | **A. Update Account Successfully** 1\. Admin accesses the Account Management page. 2\. Admin clicks the Edit button on a specific staff account. 3\. The system displays the edit form populated with current staff details. 4\. Admin modifies the staff details. 5\. Admin clicks the Save button. 6\. The system checks that the Display Name field is provided. 7\. The system retrieves the target account and verifies it is a staff account. 8\. The system updates the staff profile fields. 9\. The system hashes the new password using BCrypt (if a new password is provided). 10\. The system adjusts the account status and creates or deletes lockout records in the database based on the status change. 11\. The system saves all changes to the database. 12\. The system displays a success message and redirects to the account list. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page.  **EX-6 Mandatory Fields Missing** 1\. The system detects that the Display Name is empty. 2\. The system displays a validation error message. 3\. Back to step 4\.  **EX-7 Staff Account Not Found** 1\. The system detects that the account does not exist or does not have a staff role. 2\. The system displays the error message: "Staff account not found." 3\. Back to step 1\.  **EX-9 Database Save Failure** 1\. The system detects that saving the updated staff details to the database failed. 2\. The system displays an error message. 3\. Back to step 4\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-224, BR-251, BR-252  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.130  Delete Account**

| UC ID and Name | UC-130-Delete Account  |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Ban"toggle button next to an account. |  |  |
| Description: | Allows an Admin to deactivate (ban) or reactivate a registered account. Banning an account places a severe 100-year lockout suspension on it. Reactivating the account deletes all active lockout entries, immediately restoring access. The Super Admin account is protected from deletion/banning. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2**. The target account must exist in the system. |  |  |
| Post-conditions: | **POST-1.** The account's status is toggled in the database between active and banned. **POST-2**. Lockout records are created (if banned) or deleted (if unbanned) in the database. |  |  |
| Normal Flow: | **A. Toggle Ban Successfully** 1\. Admin accesses the Account Management page. 2\. Admin clicks the Ban (or Toggle Ban) button next to a specific account. 3\. The system checks if the Admin is authenticated. 4\. The system retrieves the target account details. 5\. The system verifies that the target account is not the Super Admin. 6\. The system toggles the account status. If the account was active, the system changes the status to banned and creates a 100-year lockout record. If the account was banned, the system changes the status to active and deletes all active lockout records. 7\. The system saves the changes to the database. 8\. The system displays a success message showing the new status and refreshes the account list. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page. **EX-4 Account Not Found** 1\. The system detects that the target account ID does not exist in the database. 2\. The system displays the error message: "Account not found." 3\. Back to step 1\. **EX-6 Super Admin Ban Blocked** 1\. The system detects that the target account is the Super Admin account. 2\. The system displays the error message: "Cannot ban the Super Admin account." 3\. Back to step 1\. **EX-8 Database Save Failure** 1\. The system detects that saving the status changes failed (rows affected \<= 0). 2\. The system displays an error message. 3\. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: |  Low |  |  |
| Business Rules: | BR-252, BR-253  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.134  View Account Detail**

| UC ID and Name | UC-134-View Account Detail |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks on a specific account in the account list. |  |  |
| Description: | Allows an Admin to view detailed profile information of any account in the system. The details retrieved dynamically vary depending on the account's role (User, Instructor, Staff, or Admin), showing custom personal, academic, and financial/Stripe statistics. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. |  |  |
| Post-conditions: | **POST-1**. The account details are successfully retrieved and displayed on the screen. |  |  |
| Normal Flow: | **A. View Account Detail Successfully** 1\. Admin accesses the Account Management page. 2\. Admin clicks on a specific account in the list. 3\. The system verifies that the Admin is authenticated. 4\. The system queries the database to retrieve the account, associated manager, user, instructor, and lockout details. 5\. The system determines the role of the account. 6\. For User (leaner) accounts, the system calculates total spent on courses and total enrollment counts. 7\. For Instructor accounts, the system retrieves title, expertise categories, Stripe credentials, and calculates total revenue, processed payouts, and available balance. 8\. The system checks and retrieves the most recent active lockout timestamps. 9\. The system renders and displays the account details page. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page.  **EX-4 Account Not Found** 1\. The system detects that the target account ID does not exist in the database. 2\. The system displays the error message: "Account not found." 3\. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-254  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.127 View Account List**	

| UC ID and Name | UC-127-View Account List	 |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin accesses the Manage Accounts page. |  |  |
| Description: | Allows an Admin to view a paginated list of all registered accounts. The Admin can search for accounts, filter them by role, and navigate through the pages. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. |  |  |
| Post-conditions: | **POST-1**. The paginated account list is successfully displayed on the screen. |  |  |
| Normal Flow: | **A. View Account List Successfully** 1\. Admin accesses the Account List page. 2\. The system checks if the Admin is authenticated. 3\. The system requests the account list using default query parameters (page \= 1, pageSize \= 10, all roles). 4\. The system queries the database to fetch account records, including associated manager, user, and instructor details, ordered by creation date descending. 5\. The system maps the account records to list details. 6\. The system calculates pagination values and total record counts. 7\. The system renders and displays the Account List table and navigation controls. |  |  |
| Alternative Flows: | **A.3a Filter Accounts** 1\. Admin selects a role filter value from the dropdown menu (e.g. User, Instructor, Staff, Registered Staff). 2\. The system queries the database using the selected role filter criteria. 3\. Back to step 7\.  **A.3b Search Accounts** 1\. Admin enters a keyword in the search input box. 2\. The system queries the database for accounts whose email, phone, display name, or full name match the keyword. 3\. Back to step 7\.  **A.3c Paginate Accounts** 1\. Admin clicks a page number or page navigation button. 2\. The system requests the page index using page parameter. 3\. Back to step 7\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: |  High |  |  |
| Business Rules: | BR-255, BR-256, BR-257  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.131  Filter Accounts**

| UC ID and Name | UC-131-Filter Accounts |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin selects a role filter value from the table menu. |  |  |
| Description: | Allows an Admin to filter the displayed account list by selecting specific role criteria, such as User (student), Instructor, Staff, or Registered Staff. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2**. Admin is currently on the Account List page. |  |  |
| Post-conditions: | **POST-1**. The filtered account list matching the selected role is successfully retrieved and displayed on the screen. |  |  |
| Normal Flow: | **A. Filter Accounts Successfully** 1\. Admin selects a role filter value from the table menu. 2\. The system captures the selected role filter and triggers a query request. 3\. The system queries the database to retrieve accounts matching the selected role. 4\. The system paginates the results and calculates the total count. 5\. The system maps the matching account records to list details. 6\. The system renders and displays the filtered Account List table**.** |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-258  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.132  Search Accounts**

| UC ID and Name | UC-132-Search Accounts |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin enters a search keyword in the search input box. |  |  |
| Description: | Allows an Admin to search for accounts by specifying a keyword that matches the account's Email, Phone Number, Display Name, or Full Name. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2**. Admin is currently on the Account List page. |  |  |
| Post-conditions: | **POST-1**. The matching accounts are successfully retrieved and rendered on the screen. |  |  |
| Normal Flow: | **A. Search Accounts Successfully** 1\. Admin enters a keyword in the search input box. 2\. The system captures the keyword and triggers a query request. 3\. The system queries the database for accounts whose email, phone number, display name, or full name match the search keyword. 4\. The system paginates the matched results and calculates the total count. 5\. The system maps the matching account records to list details. 6\. The system renders and displays the searched Account List table. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: |  BR-256  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.133  Flag Accounts for Violation**

| UC ID and Name | UC-133-Flag Accounts for Violation |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 13/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Flag Account" button and enters a violation reason. |  |  |
| Description: | Allows an Admin to flag a user account for a policy violation. The flag count increments, and the account status transitions through warning levels. On the 3rd flag, the account is automatically banned for 30 days. The system also sends a notification alert to the user. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2**. The target account must exist and not be currently banned or already at the max flag limit. |  |  |
| Post-conditions: | **POST-1**. The account's flag count and status are updated in the database. **POST-2.** If the flag count reaches 3, a 30-day lockout record is created in the database. **POST-3**. A notification (warning or ban notice) is sent to the target user account. **POST-4.** The updated flag count and status are rendered to the Admin. |  |  |
| Normal Flow: | **A. Flag Account Successfully** 1\. Admin accesses the Account Management page. 2\. Admin clicks the Flag Account button next to the target account. 3\. The system displays a dialog box asking for a violation reason. 4\. Admin enters the reason and clicks Submit. 5\. The system verifies that the Admin is authenticated. 6\. The system retrieves the target account and checks that it is eligible for flagging (not banned and flags \< 3). 7\. The system increments the account flag count by 1 and updates the account status (to flagged\_1, flagged\_2, or banned). 8\. The system saves the changes to the database. 9\. The system sends a notification (warning or ban notice depending on flag level) to the target user. 10\. The system displays the updated flag count and account status to the Admin. |  |  |
| Alternative Flows: | **A.2a Unflag Account**  1.Admin clicks the "Unflag Account" button next to the target account. 2.The system displays a dialog box asking for a reason for unflagging. 3.Admin enters the reason and clicks Submit. 4.The system verifies that the Admin is authenticated. 5.The system retrieves the target account and checks that it is eligible for unflagging (flag count \> 0). 6.The system decrements the account flag count by 1 and updates the account status (e.g., from banned to flagged\_2, flagged\_2 to flagged\_1, or flagged\_1 to active). 7.If the account transitions from a banned state, the system removes the active lockout record from the database. 8.The system saves the changes to the database. 9.The system sends an unflagging notification to the target user. 10.Back to step 10\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects that the Admin is not authenticated or lacks admin privileges. 2\. The system redirects the Admin to the Login page. **EX-4 Account Not Found** 1\. The system detects that the target account ID does not exist. 2\. The system displays the error message: "Account not found." 3\. Back to step 1\. **EX-6 Account Ineligible** 1\. The system detects that the account is already banned or has reached the maximum flag limit of 3\. 2\. The system displays an error message indicating that the account cannot be flagged. 3\. Back to step 1\. **EX-8 Database Save Failure** 1\. The system detects that saving the flag update to the database failed (rows affected \<= 0). 2\. The system displays the error message: "Failed to flag account." 3\. Back to step 4\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-259, BR-260  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

