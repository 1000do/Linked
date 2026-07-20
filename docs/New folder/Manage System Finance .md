###### **2.2.3.135  View Balance (For System)**

| UC ID and Name | UC-135 \- View Balance (For System) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | Stripe |
| Trigger: | Administrator accesses the Platform Finance Dashboard page. |  |  |
| Description: | Allows administrators to track platform-wide financial summary statistics, splits, payouts, Stripe gateway balances, and view current configuration settings. |  |  |
| Preconditions: | **PRE-01:**The administrator must be successfully logged into the system.  **PRE-02:**The administrator account must have platform-wide financial management permissions**.** |  |  |
| Post-conditions: | **POST-1**. The system successfully displays the platform's consolidated financial overview and current configuration statuses. |  |  |
| Normal Flow: | **A. View Transaction Balance Successfully** 1\. Administrator accesses the Admin Finance dashboard page. 2\. The system requests platform-wide financial summary, Stripe gateway balances, split configurations, and payout details from the database and Stripe API. 3\. The system retrieves the data. 4\. The system displays the Course Revenue cards. 5\. The system displays the Revenue Splits & Distributions cards. 6\. The system displays the Stripe Gateway Balances cards. 7\. The system displays the status of the current configuration settings (default instructor split rate, automated payout trigger days). |  |  |
| Alternative Flows: |  **A.7.1 Filter Balances by Month and Year** 1\. Administrator selects a specific Month and Year from the period filter dropdowns. 2\. Administrator clicks the "Filter" button. 3\. The system requests updated financial statistics and chart data for the selected period from the database. 4\. The system retrieves the filtered financial statistics and chart data. 5\. The system displays the updated cards and charts. 6\. Back to step 7\. **A.7.2 Sync Balances Manually from Stripe** 1\. Administrator clicks the "Sync from Stripe" button. 2\. The system requests Stripe to synchronize the latest wallet and payout status data. 3\. Stripe returns the synchronized data. 4\. The system updates the wallet and payout status in the database. 5\. The system displays a success message indicating successful synchronization. 6\. Back to step 7\.  |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-197,BR-198,BR-199 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Administrator has stable internet connection. Stripe is working properly. |  |  |

###### 

###### **2.2.3.136  View Instructor Payouts**

| UC ID and Name | UC-136 \- View Instructor Payouts |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Administrator clicks the "Instructor Payouts" tab in the Admin Finance dashboard. |  |  |
| Description: | Allows administrators to track payout records generated for instructors from course sales, including split rates, payout amounts, and Stripe transfer statuses. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The administrator account must have platform-wide financial management permissions**.** |  |  |
| Post-conditions: | **POST-1.** The administrator views the instructor payouts table listing the instructor name, transaction ID, payout share, split rate, Stripe status, and payout action state. |  |  |
| Normal Flow: | **A. View Instructor Payouts Successfully** 1\. Administrator accesses the Admin Finance dashboard page. 2\. The system retrieves the list of instructor payouts from the database. 3\. Administrator clicks the "Instructor Payouts" tab. 4\. The system displays the payouts table containing the following columns:     \- Tx / Instructor: The instructor's full name and the associated Transaction ID (prefixed with Tx ID: \#).     \- Payout Share: The instructor's payout amount (prefixed with $ or displayed as a negative subtraction in red text e.g., \-$15.29 if refunded) and the split rate percentage (e.g. Split Rate: 80.00% or Refunded Split: 80.00%).     \- Stripe Status: The colored status badge representing the Stripe Connect state (Settled to Bank, In Transit, Stripe Wallet, Failed, Refunded, Unpaid Escrow).     \- Action: The action state (displaying either a "Pay via Stripe" button, a "Refunded" status, or a "Completed" status). 5\. Administrator reviews the list of instructor payouts.  |  |  |
| Alternative Flows: |  **A.4.1 Sync Payout Statuses from Stripe** 1\. Administrator clicks the "Sync with Stripe" button. 2\. The system requests Stripe Connect API to synchronize status updates for all payouts. 3\. Stripe returns the updated payout statuses. 4\. The system updates the payout statuses in the database. 5\. The system displays a success message indicating that payout statuses have been synchronized. 6\. Back to step 4\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-182 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### **2.2.3.137 View Funds Withdrawal History**

| UC ID and Name | UC-137 \- View Funds Withdrawal History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | The administrator clicks the "Manager Bank Withdrawals" tab in the Admin Finance dashboard.. |  |  |
| Description: |        Allows administrators to track payout records generated for instructors from course sales, including split rates, payout amounts, and Stripe transfer statuses. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The administrator account must have platform-wide financial management permissions**.** |  |  |
| Post-conditions: | **POST-1.** The administrator views the platform bank withdrawal history table containing withdrawal reference IDs, amounts, statuses, and initiation dates**.** |  |  |
| Normal Flow: | **A. View Platform Funds Withdrawal History Successfully** 1\. Administrator accesses the Admin Finance dashboard page. 2\. The system retrieves the list of platform withdrawals from the database. 3\. Administrator clicks the "Manager Bank Withdrawals" tab. 4\. The system displays the withdrawals table containing the following columns:     \- Reference ID: Sequential withdrawal ID prefixed with \# (and the name of the initiating manager).     \- Amount: The withdrawn amount      \- Status: The colored status badge representing the withdrawal state (Paid, In Transit, Pending, Failed, Canceled).     \- Initiated Date: The timestamp when the withdrawal request was created (formatted as dd/MM/yyyy and HH:mm). 5\. Administrator reviews the list of bank withdrawals. |  |  |
| Alternative Flows: |  N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-200 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### **2.2.3.138 View List of Refund Requests**

| UC ID and Name | UC-138 \- View List of Refund Requests |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | The administrator clicks the "Refund Requests" tab in the Admin Finance dashboard. |  |  |
| Description: | Allows administrators to view the list of pending refund requests submitted by learners within the 14-day window. Shows purchase and request details, and provides actionable links and decision triggers. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The administrator account must have platform-wide financial management permissions**.** |  |  |
| Post-conditions: | **POST-1.** The administrator views the list of pending refund requests including the transaction ID, course title, student name, amount, purchase date, request date, and student refund reason. |  |  |
| Normal Flow: | **A. View Platform Funds Withdrawal History Successfully** 1\. Administrator accesses the Admin Finance dashboard page. 2\. The system retrieves the list of platform withdrawals from the database. 3\. Administrator clicks the "Manager Bank Withdrawals" tab. 4\. The system displays the withdrawals table containing the following columns:     \- Reference ID: Sequential withdrawal ID prefixed with \# (and the name of the initiating manager).     \- Amount: The withdrawn amount      \- Status: The colored status badge representing the withdrawal state (Paid, In Transit, Pending, Failed, Canceled).     \- Initiated Date: The timestamp when the withdrawal request was created (formatted as dd/MM/yyyy and HH:mm). 5\. Administrator reviews the list of bank withdrawals. |  |  |
| Alternative Flows: |  N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A  |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### 

	

###### **2.2.3.139 Approve Refund Request**

| UC ID and Name | UC-139 \- Approve Refund Request |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | Stripe |
| Trigger: | Administrator clicks the "Approve" button next to a pending refund request in the Refund Requests list. |  |  |
| Description: | Allows administrators to approve a learner's pending refund request. The system automatically performs a Stripe refund, reverses the instructor's payout (if already transferred), updates the database ledger status, and revokes the student's course access. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The administrator account must have platform-wide financial management permissions**. PRE-3.** The user must have at least one refund request record. |  |  |
| Post-conditions: | **POST-1.** The student's course access is successfully revoked. **POST-2.** The purchase amount is refunded to the student via Stripe Connect, and the payout reversal is initiated. **POST-3.** The transaction and payout record statuses are updated to be refunded in the database. |  |  |
| Normal Flow: | **A.View List of Refund Requests Successfully** 1.Admin accesses the admin finance dashboard page AdminFinance. 2.Admin selects the "Refund Requests" tab. 3.Admin clicks the "Approve" button on the pending refund request row. 4.The system displays the "Approve Refund" confirmation dialog containing a feedback input field. 5.Admin inputs optional feedback and clicks the "Approve & Refund" button. 6.The system reverses the instructor's earnings transfer via Stripe Connect. 7.The system refunds the transaction amount to the student via Stripe API. 8.The system updates the transaction status and instructor payout status to "refunded" and negates their amounts in the database. 9.The system revokes the student's enrollment access to the course. 10.The system sends a notification to the student about the approved refund. 11.The system broadcasts real-time SignalR notifications to update the dashboard balances. 12.The system closes the confirmation dialog and displays a success notification.  |  |  |
| Alternative Flows: |  **A.5 Click Cancel** Admin clicks the "Cancel" button on the confirmation dialog. The system closes the confirmation dialog. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Transaction Not Found** The system displays a transaction not found error message. Back to step 1\. **EX-5 Status is Not Pending** The system displays an error message indicating the transaction is not in a pending status. Back to step 2\. **EX-6 Transfer Reversal Failed** The system displays an error message regarding the failure to reverse the instructor's transfer. Back to step 3\. **EX-7 Stripe Refund Failed** The system displays an error message regarding the failure to process the refund on Stripe. Back to step 3\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-161,BR-201,BR-202,BR-203,BR-204 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### **2.2.3.140 Reject Refund Request**

| UC ID and Name | UC-140 \- Reject Refund Request |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A. |
| Trigger: | Admin clicks the "Reject" button on the pending refund request row. |  |  |
| Description: | Allows administrators to reject a student's pending refund request. The system reverts the transaction status to succeeded, records the administrative rejection note, retains the student's course access, and notifies the student. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The administrator account must have platform-wide financial management permissions**. PRE-3.** The user must have at least one refund request record. |  |  |
| Post-conditions: | **POST-1.** The transaction status is reverted to "succeeded" in the database. **POST-2.** The administrator's rejection note is saved in the database. **POST-3.** The student's course enrollment remains active in the database. **POST-4.** The student receives a notification indicating the rejection of the refund. |  |  |
| Normal Flow: | **A.Reject Refund Request Successfully** 1.Admin accesses the admin finance dashboard page AdminFinance. 2.Admin selects the "Refund Requests" tab. 3.Admin clicks the "Reject" button on the pending refund request row. 4.The system displays the "Reject Refund" confirmation dialog containing a required feedback input field. 5.Admin inputs the rejection reason and clicks the "Reject Request" button. 6.The system updates the transaction status to "succeeded" and saves the rejection note in the database. 7.The system sends a notification to the student about the rejected refund. 8.The system closes the confirmation dialog and displays a rejection success notification. |  |  |
| Alternative Flows: |  **A.5 Click Cancel** Admin clicks the "Reject Request" button on the confirmation dialog. The system closes the confirmation dialog. Back to step 3\. **A.5.1 Reject Reason Missing** Admin clicks the "Reject Request" button without entering a rejection reason. The system displays a validation error message: "You must enter a rejection reason\!". Back to step 5\.  |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Transaction Not Found** The system displays a transaction not found error message. Back to step 1\. **EX-5 Status is Not Pending** The system displays an error message indicating the transaction is not in a pending status. Back to step 2\. **EX-6 Transfer Reversal Failed** The system displays an error message regarding the failure to reverse the instructor's transfer. Back to step 3\. **EX-7 Stripe Refund Failed** The system displays an error message regarding the failure to process the refund on Stripe. Back to step 3\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-205, BR-206,BR-207,BR-208 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### **2.2.3.149 View transaction detail (For System)**

| UC ID and Name | UC-149 View transaction detail (For System) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A. |
| Trigger: | Admin clicks the "View" button next to a transaction row in the transaction list, or clicks the "Details" button in the pending refund requests list  |  |  |
| Description: | Allows administrators to view the detailed information of a specific transaction, including the buyer profile, course information, financial split figures, and any associated refund logs.  |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The administrator account must have platform-wide financial management permissions**. PRE-3.**. The user must have at least one transaction record. |  |  |
| Post-conditions: | **POST-1.** Admin views the detailed course information, buyer profile, financial split figures, and refund logs**.** |  |  |
| Normal Flow: | **A.View transaction detail successfully**  1.Admin accesses the admin finance dashboard page AdminFinance. 2.Admin clicks the "Details" button next to a transaction row. 3.The system queries the database for the transaction, order item, buyer, instructor payout, and refund extensions. 4.The system displays the Transaction Details page containing: Transaction ID, creation timestamp, and current status badge. Gross purchase amount and payment currency. Course title, thumbnail, and instructor name. Buyer's full name, email, and registration details. Financial split card showing the calculation: Stripe processing fee, net price, instructor payout, and platform share. Refund log panel (showing request date, reason, and admin feedback note) if a refund request exists.  |  |  |
| Alternative Flows: | N/A. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-2 Transaction Not Found** The system displays a transaction not found error message. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-182,BR-209,BR-210 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### **2.2.3.141  View transaction list (For System)**

| UC ID and Name | UC-141 \- View transaction list (For System) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Administrator accesses the Platform Finance Dashboard page (or clicks the "Transactions" tab within the dashboard). |  |  |
| Description: | Allows administrators to track all course purchase transactions across the entire platform. Provides search, filtering by status, sorting, and direct links to view the details of each transaction. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system. **PRE-2.** The administrator account must have platform-wide financial management permissions. |  |  |
| Post-conditions: | **POST-1.** The administrator views the platform-wide transaction history list including the Stripe Tx ID, course title, buyer, instructor, amount, date, and status. |  |  |
| Normal Flow: | **A. View Transaction History Successfully** 1.Administrator accesses the Admin Finance dashboard page. 2\. The system retrieves the list of course transactions from the database. 3\. The system displays the Transactions tab showing the transaction table with the following columns:     \- ID: Sequential transaction ID prefixed with \#.     \- Stripe Tx ID: The secure Stripe transaction reference ID.     \- Course: The title of the purchased course.     \- Purchased By: The name of the student who made the purchase.     \- Instructor: The name of the instructor teaching the course.     \- Amount: The purchase price.     \- Date: The transaction date and time (formatted as dd/MM/yyyy HH:mm).     \- Status: The colored badge representing the transaction state (Succeeded, Failed, Pending, Refund Pending, Refunded). 4\. Administrator reviews the list of transactions. |  |  |
| Alternative Flows: |  N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-182 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has stable internet connection. |  |  |

###### 

###### 

###### **2.2.3.142  Search transactions (For System)**

| UC ID and Name | UC-142 \- Search transactions (For System) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 07/06/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin enters search text in the transaction search input field and submits the filter form. |  |  |
| Description: | Allows authenticated Admin users to search the system-wide transaction history by course title, student (buyer) name, or instructor name. |  |  |
| Preconditions: | **PRE-1.** User must be successfully authenticated as an Admin. **PRE-2.** Admin is on the Financial Management dashboard under the Transaction History tab. |  |  |
| Post-conditions: | **POST-1**. The system displays only those transactions that match the search keyword. |  |  |
| Normal Flow: | **A. Search Transactions Successfully** 1\. Admin accesses the Financial Management page. 2\. Admin selects the "Transaction History" tab. 3\. Admin enters a search term (e.g. course title, student name, or instructor name) in the search input box. 4\. Admin clicks the "Filter" button or presses Enter. 5\. The system requests the transaction list matching the keyword from the server, while preserving active date period filters (year/month) and paging parameters. 6\. The server validates that the active user possesses an Admin role. 7\. The server queries the database for transactions matching the search term within the selected year and month. 8\. The server returns the matching results. 9\. The system displays the matching transactions in the table. |  |  |
| Alternative Flows: | **A.5 Clear search keyword** 1\. Admin clicks the "Clear Filters" button. 2\. The system resets the search keyword, status filter, and sorting order to their defaults. 3\. The system requests the transaction list for the active period from the server. 4\. Back to step 6\. **A.9 No matching transactions found** 1\. The server returns an empty list of transactions matching the keyword. 2\. The system displays a message indicating "No transactions matched your criteria". |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Unauthorized Access** 1\. The server detects that the user is not an Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.143  Filter transactions(For System)** 

| UC ID and Name | UC-143-Filter transactions(For System) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created:  | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin selects a status option from the status dropdown menu and clicks the "Filter" button inside the transaction filter form. |  |  |
| Description: | Allows authenticated Admin users to filter the system-wide transaction history by transaction status. |  |  |
| Preconditions: | PRE-1. User must be successfully authenticated as an Admin. PRE-2. Admin is on the Financial Management dashboard under the Transaction History tab. |  |  |
| Post-conditions: | POST-1. The system displays only those transactions in the list that match the selected filter criteria. |  |  |
| Normal Flow: | **A. Filter Transactions Successfully** 1.Admin accesses the Financial Management page. 2.Admin selects the "Transaction History" tab. 3.Admin selects a status option (e.g. "Succeeded") from the status dropdown menu. 4.Admin clicks the "Filter" button inside the transaction filter form. 5.The system constructs a request for the transaction list matching the selected status. 6.The system includes active search keywords, sorting order, and period (month and year) in the request. 7.The system sends the request to the server. 8.The server validates that the active user possesses an Admin role. 9.The server queries the database for transactions matching the status within the active year and month. 10.The server returns the filtered transaction results. 11.The system displays the filtered transactions in the table. |  |  |
| Alternative Flows: |  **A.5 Clear filters** 1\. Admin clicks the "Clear Filters" button inside the transaction filter form. 2\. The system resets the status filter, sorting order, and search keyword parameters to their defaults. 3\. The system requests the transaction list for the active period from the server. 4\. Back to step 8\. **A.9 No matching transactions found** 1\. The server returns an empty list of transactions matching the filters. 2\. The system displays a message indicating "No transactions matched your criteria". |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not an Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### 

###### **2.2.3.144  Sort transactions(For System)** 

| UC ID and Name | UC-144-Sort transactions(For System) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created:  | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin selects a sorting order from the sorting dropdown and submits the form. |  |  |
| Description: | Allows authenticated Admin users to sort the system-wide transaction history by date or amount. |  |  |
| Preconditions: | **PRE**\-1. User must be successfully authenticated as an Admin. **PRE**\-2. Admin is on the Financial Management dashboard under the Transaction History tab. |  |  |
| Post-conditions: | **POST**\-1. The system displays the transactions in the table matching the selected sorting order. |  |  |
| Normal Flow: | **A. Sort Transactions Successfully** 1\. Admin accesses the Financial Management page. 2\. Admin selects the "Transaction History" tab. 3\. Admin selects a sorting option (e.g. "Amount: High to Low") from the sorting dropdown menu. 4\. Admin clicks the "Filter" button. 5\. The system requests the transaction list sorted by the selected sorting option from the server, while preserving active search keywords, status filters, and date period filters. 6\. The server validates that the active user possesses an Admin role. 7\. The server queries the database, applying the sorting order to the retrieved transactions within the selected year and month. 8\. The server returns the sorted results. 9\. The system displays the sorted transactions in the table. |  |  |
| Alternative Flows: | **A.5 Clear filters** 1\. Admin clicks the "Clear Filters" button inside the transaction filter form. 2\. The system resets the sorting order to its default ("Date: Newest"), along with other filter criteria. 3\. The system requests the transaction list for the active period from the server. 4\. Back to step 6\.  **A.9 No transactions found** 1\. The server returns an empty list of transactions matching the filters. 2\. The system displays a message indicating "No transactions matched your criteria". |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Unauthorized Access** 1\. The server detects that the user is not an Admin. 2\. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has a stable internet connection. |  |  |

###### 

###### **2.2.3.145 Set Transfer Rate**

| UC ID and Name | UC-145: Set Transfer Rate |  |  |
| ----- | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A. |
| Trigger: | Admin clicks the "Save Rate" button on the settings card. |  |  |
| Description: | Allows administrators to update the percentage of course purchase revenue allocated to instructors. The new rate is stored in system configurations, applied to all future transactions, and broadcasted to onboarded instructors. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The system configuration repository is accessible. |  |  |
| Post-conditions: | **POST-1.** The updated transfer rate is saved in the database system configurations. **POST-2.** Active instructors with linked Stripe Connect accounts receive notifications regarding the updated split policy. **POST-3.** All transactions processed after the change calculate payouts using the new split rate. |  |  |
| Normal Flow | **A. Set Revenue Share Split Rate Successfully** 1.Admin accesses the admin finance dashboard page AdminFinance. 2.Admin locates the "Revenue Share Split Rate" setting card. 3.Admin enters the target percentage rate in the "Instructor Share (%)" input field. 4.Admin clicks the "Save Rate" button. 5.The system displays the "Confirm Rate Change" dialog. 6.Admin clicks the "Save Rate" confirmation button. 7.The system updates the transfer rate configuration key in the database. 8.The system retrieves the list of instructors with onboarded Stripe Connect accounts. 9.The system triggers background notifications about the policy update to the retrieved instructors. 10.The system refreshes the dashboard view displaying the newly saved rate.  |  |  |
| Alternative Flows: | **A.5 Click Cancel** Admin clicks the "Cancel" button on the confirmation dialog. The system closes the confirmation dialog. Back to step 3\. **A.5.1 Invalid Rate Entered** Admin enters a rate outside the permitted range or a non-numeric value in the input field. Admin clicks the "Save Rate" button. The system displays the "Invalid Split Rate" error dialog indicating that the rate must be between 30% and 95%. Admin clicks the "Understood" button. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-211, BR-212,BR-213 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### **2.2.3.146 Set Payout Schedule of Instructor**

| UC ID and Name | UC-146: Set Payout Schedule of Instructor |  |  |
| ----- | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A. |
| Trigger: | Admin clicks the "Save Schedule" button on the settings card. |  |  |
| Description: | Allows administrators to configure the days of the month when automatic bulk payouts are triggered for instructors. The new payout days are saved in the system configurations. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The system configuration repository is accessible. |  |  |
| Post-conditions: | **POST-1**. The updated payout schedule days are saved in the database system configurations**. POST-2.** The bulk payout scheduler is updated to trigger payouts on the configured days**.** |  |  |
| Normal Flow | **A. Set Payout Schedule Successfully** 1.Admin accesses the admin finance dashboard page AdminFinance. 2.Admin locates the "Auto Payout Scheduler" setting card. 3.Admin enters the comma-separated day(s) of the month (e.g., "15, 18") in the "Trigger Day(s)" input field. 4.Admin clicks the "Save Schedule" button. 5.The system displays the "Confirm Payout Schedule" dialog. 6.Admin clicks the "Save Schedule" confirmation button. 7.The system updates the payout days configuration key in the database. 8.The system refreshes the dashboard view displaying the newly saved trigger days.  |  |  |
| Alternative Flows: | **A.5 Click Cancel** Admin clicks the "Cancel" button on the confirmation dialog. The system closes the confirmation dialog. Back to step 3\. **A.5.1 Invalid Payout Days Entered** Admin enters an empty configuration, non-numeric values, or days outside the 15-20 range in the input field. Admin clicks the "Save Schedule" button. The system displays the "Invalid Payout Days" or "Empty Configuration" error dialog. Admin clicks the "Understood" button. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-214,BR-215 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### **2.2.3.147 Withdrawal Funds**

| UC ID and Name | UC-147: Withdraw Funds |  |  |
| ----- | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | Stripe  |
| Trigger: | Admin clicks the "Withdraw Now" button on the withdrawal settings card. |  |  |
| Description: | Allows administrators to manually transfer accumulated platform net profit from the Stripe available balance directly into the platform's corporate bank account. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  **PRE-2.** The platform's Stripe Connect available balance must be at least $0.50. **PRE-3:** Account successfully linked to Stripe's bank account. |  |  |
| Post-conditions: | **POST-1.** A Stripe Payout is successfully initiated to the corporate bank account. **POST-2.** A withdrawal record is added to the database with a pending status. **POST-3.** The dashboard balances are updated in real time via SignalR.. |  |  |
| Normal Flow | **A.Withdraw Funds Successfully** 1.Admin accesses the admin finance dashboard page AdminFinance. 2.Admin locates the "Withdraw Funds to Bank" settings card showing the available balance. 3.Admin enters the target payout amount in the "Amount" input field. 4.Admin enters an optional transfer explanation in the "Description" input field. 5.Admin clicks the "Withdraw Now" button. 6.The system displays the "Confirm Withdrawal" warning dialog. 7.Admin clicks the "Confirm & Initiate" button. 8.The system requests Stripe to create a payout to the platform's registered bank account. 9.Stripe processes the request and returns the payout transaction identifier. 10.The system creates a withdrawal log record with a pending status in the database. 11The system broadcasts real-time SignalR notifications to update active dashboard balances. 12.The system closes the confirmation dialog and displays a success notification.  |  |  |
| Alternative Flows: | **A.7 Click Cancel** Admin clicks the "Cancel" button on the confirmation dialog. The system closes the confirmation dialog. Back to step 3\. **A.7.1 Invalid Amount Entered** Admin enters a withdrawal value less than $0.50, a non-numeric value, or an amount exceeding the available balance. Admin clicks the "Withdraw Now" button. The system displays the "Invalid Withdrawal Amount" error dialog. Admin clicks the "Understood" button. Back to step 3\.  |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-216,BR-217 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 

###### 

###### **2.2.3.148 Filter financial dashboard by period**

| UC ID and Name | UC-148: Filter financial dashboard by period |  |  |
| ----- | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Admin | Secondary Actors: | None |
| Trigger: | Admin selects a month and/or year and clicks the period "Filter" button. |  |  |
| Description: | Allows authenticated Admin users to filter the financial dashboard data (including revenue overview cards, Stripe balance metrics, monthly performance chart, and transaction list) by selecting a specific month and year. |  |  |
| Preconditions: | **PRE-1.** The administrator must be successfully logged into the system.  |  |  |
| Post-conditions: | **POST-1.** The system displays only the financial metrics and list data on the dashboard that match the selected period (month and year). |  |  |
| Normal Flow | **A. Filter Financial Dashboard by Period Successfully** 1.Admin accesses the Financial Management page. 2.Admin selects a month from the month selector. 3.Admin selects a year from the year selector. 4.Admin clicks the "Filter" button next to the year selector. 5.The system constructs a request for the dashboard data for the selected period. 6.The system includes the active tab and active transaction filter parameters in the request. 7.The system sends the request to the server. 8.The server validates that the active user possesses an Admin role. 9.The server retrieves financial summaries, balance metrics, payout logs, bank withdrawals, and transaction records matching the selected year and month. 10.The server returns the retrieved financial dashboard data. 11.The system displays the updated financial dashboard data.  |  |  |
| Alternative Flows: | **A.9 No financial data found for the selected period** 1.The server returns empty data and zero values for the selected period. 2.The system displays the overview cards with zero amounts, an empty chart, and a message indicating "No transactions matched your criteria" in the transaction history table.. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-3 Unauthorized Access** The server detects that the user is not an Admin. The system redirects the user to the access denied error page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: |  Administrator has a stable internet connection. |  |  |

###### 