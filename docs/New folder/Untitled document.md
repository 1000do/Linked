###### **2.2.3.80  View balance** 

| UC ID and Name | UC-80 View balance |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | 1\. Instructors click the "Revenue & Payouts" link in their dashboard navigation. |  |  |
| Description: | Allows instructors to track their financial metrics and cash flows filtered by month/year, including gross sales revenue, refunded amount, actual net earnings, pending escrow holdings, Stripe wallet balances, and successful withdrawals. |  |  |
| Preconditions: | **PRE-1**: The instructor must be logged in. **PRE-2**: The user's account must be active as an approved Instructor with a linked Stripe Connect account. |  |  |
| Post-conditions: | **POST-1:** The instructor views all financial balance statistics and the corresponding transaction revenue chart for the selected period. |  |  |
| Normal Flow: | **A. View Earnings Dashboard Successfully** 1\. Instructor accesses the Earnings Dashboard page. 2\. The system retrieves the instructor's financial balance statistics and transaction data from the database. 3\. The system displays the Earnings Dashboard page header. 4\. The system displays the automated payout scheduler banner. 5\. The system displays the detailed financial cards (Gross Revenue, Total Refunded, Net Earnings, Pending Transfer, Stripe Wallet, Withdrawn). 6\. The system displays the Transaction Revenue Chart. |  |  |
| Alternative Flows: | **A.5.1 Filter Balances by Month and Year** 1\. Instructor selects a specific Month and Year from the period filter dropdowns. 2\. The instructor clicks the "Filter" button. 3\. The system requests updated financial statistics and chart data for the selected period from the database. 4\. The system retrieves the filtered financial statistics and chart data. 5\. The system displays the updated cards and chart. 6\. Back to step 5\. **A.5.2 Sync Balances Manually from Stripe** 1\. Instructor clicks the "Sync from Stripe" button. 2\. The system requests Stripe to synchronize the latest wallet and payout status data. 3\. Stripe returns the synchronized data. 4\. The system updates the wallet and payout status in the database. 5\. The system displays a success message indicating successful synchronization. 6\. Back to step 5\. |  |  |
| Exceptions: | **EX-01: Session Expired / Unauthorized Access** 1\. The instructor's session is expired, or they attempt to access /Instructor/Payouts without logging in. 2\. The system redirects the instructor to the Account Login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-179,BR-180,BR-181  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

###### 

###### **2.2.3.81 View Instructor's Transaction History**

| UC ID and Name |  UC-81-View Instructor's Transaction History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | 1\. Instructors click the "Revenue & Payouts" link in their dashboard navigation. |  |  |
| Description: | Displays a detailed list of course purchases made by learners for courses owned by the active instructor. Provides search, filtering, sorting, and direct links to view the financial breakdown of each transaction. |  |  |
| Preconditions: | **PRE-1**: The instructor must be logged in. **PRE-2**:  The user's account must be active as an approved Instructor with a linked Stripe Connect account. . |  |  |
| Post-conditions: | **POST-1:** The instructor views the history of course purchase transactions including the actual Net Earnings and transaction statuses. |  |  |
| Normal Flow: | **A. View Transaction History Successfully** 1\. The user navigates to the Earning . 2\. The system displays and defaults to the "Learner Purchases" tab. 3\. The system displays the total count of transaction records (e.g., "Total: 1 records"). 4\. The system displays the course purchase transaction table with the following columns:    \- Date: The date and time of the transaction (formatted as \`dd/MM\` and \`HH:mm\`).    \- Course / Purchaser: The title of the purchased course, the full name of the buyer, and the Transaction ID prefixed with \`\#\`.    \- Net Earnings: The actual amount received by the instructor for the sale (after platform revenue share deductions).    \- Status: The colored status badge of the transaction (Completed \- Green, Pending \- Amber, Failed \- Red, Refunded \- Light Red, Refund Pending \- Orange).    \- Actions: A button with an eyeball (Visibility) icon to view the invoice details. 5\. The instructor reviews the list of transactions. |  |  |
| Alternative Flows: | **A.2 Empty History Display** 1\. The system detects that the instructor has no sales. 2\. The system displays an empty state screen with the message: "No transactions yet. Revenue data will appear here as soon as students purchase your courses." 3\. The profit chart displays a flat line showing zero profits. **A.3 Filtering Balances by Month and Year** 1\. The instructor selects a specific month and year from the period filter dropdown menu. 2\. The instructor clicks the \*"Filter"\*\* button. 3\. The page reloads and displays the balance corresponding to the selected period. 4\. Return step A4  |  |  |
| Exceptions: | **EX-01: Session Expired / Unauthorized Access** 1\. The instructor's session is expired, or they attempt to access /Transaction/Instructor without logging in. 2\. The system redirects the instructor to /Account/Login. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-182, BR-183 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

###### **2.2.3.82 View payout history**

| UC ID and Name | UC-82-View payout history |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | 1\. Instructors click the "Payout History" (or "Payouts") tab in their dashboard. |  |  |
| Description: | Allows instructors to track their historical and pending payout transfers, including sequential numbering, course sale references, net payout amounts, and real-time Stripe settlement statuses. |  |  |
| Preconditions: | **PRE-1**: The instructor must be logged in. **PRE-2:** The instructor must have a valid session cookie . |  |  |
| Post-conditions: | **POST-1**: The platform displays the list of payout records corresponding to course sales.. |  |  |
| Normal Flow: | **A. View Payout History List Successfully** 1\. The instructor accesses the Payouts / Earnings tab in the dashboard. 2\. The system retrieves and displays a paginated list of credited payout records with the following columns:        \- No.: Sequential number of the payout record .        \- Course Title: Name of the sold course, with details showing the gross sales split .        \- Net Payout: The actual earnings credited. Positive earnings show in green text .        \- Status: Visual status badge of the transfer:          \- To Bank (Paid): Paid out to the instructor's bank account           \- In Transit: Payout is currently being processed by Stripe Connect.          \- Stripe Wallet: Settled to the instructor's Stripe balance, ready to be swept to the bank.          \- Unpaid (Pending): Awaiting scheduled trigger.  3\. The Instructor can filter payouts by Keyword (course title, Stripe references) and Status (Success, In Progress, Failed), or sort them by date and amount. |  |  |
| Alternative Flows: | **A.3 Filter & Search Payouts**     1\. The instructor inputs a keyword in the search box or selects a Status.     2\. The instructor clicks the "Filter" button.     3\. The system refreshes the list to show matching payout items.   **A.3 Displaying Refunded/Deducted Payouts (Negative Money Flow)**   1\. If a learner request for a refund has been approved, the system generates a negative entry (deduction) on the instructor's ledger.    2\. The system displays this record in the payout list with:   \- Net Payout: The negative value highlighted in red text      \- Status'': A red status badge showing ''Refunded''.    **A.3 Stripe Transfer Fails**     1\. During bulk payout execution, if Stripe Connect encounters a transfer failure (e.g. due to verification requirements):     2\. The system marks the payout record as failed and displays a red status badge showing ''Failed''.  |  |  |
| Exceptions: | **EX-01: Session Expired / Unauthorized Access** 1\. The instructor's session is expired, or they attempt to access /Instructor/Payouts without logging in. 2\. The system redirects the instructor to the Account Login page.  |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: |  BR-184,BR-185 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

###### 

###### **2.2.3.83 Sort Instructor's Transaction History**

| UC ID and Name | UC-83-Sort Instructor's Transaction History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | Instructor selects a sorting option and clicks the "Filter" button on the Revenue & Payouts page. |  |  |
| Description: | Allows an approved instructor to sort their learner purchases transaction history by date or amount to easily audit and analyze their course sales revenues. |  |  |
| Preconditions: | **PRE-1.** Instructors must be successfully authenticated. **PRE-2.** Instructor's account status must be approved (ApprovalStatus \= "Approved"). |  |  |
| Post-conditions: | **POST-1**. The transaction list is sorted and displayed according to the selected sorting option. **POST-2**. Active search keywords, status filters, and period filters are preserved. |  |  |
| Normal Flow: | **A. Sort Transaction History Successfully** 1\. Instructor accesses the instructor transaction page. 2\. Instructor selects the "Learner Purchases" tab. 3\. Instructor clicks the sorting dropdown. 4\. Instructor selects a sorting option. 5\. Instructor clicks the "Filter" button. 6\. The system requests the sorted transaction history from the server. 7\. The server queries the transactions from the database sorted by the selected criteria. 8\. The system displays the sorted list of transactions to the instructor. |  |  |
| Alternative Flows: | **A.5 Reset sort** 1\. Instructor clicks the "Reset" link. 2\. The system resets the sorting dropdown to the default option. 3\. The system requests the default transaction history from the server. 4\. The server queries the default transactions from the database. 5\. The system displays the default list of transactions. 6\. Back to step 3\. **A.7 No transactions found** 1\. The system detects that there are no transaction history records matching the filter criteria. 2\. The system displays a "No purchases recorded for this period." message. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-9 Database query failure** 1\. The server fails to query the transaction data from the database. 2\. The system displays an error message regarding query failure. 3\. Back to step 3\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-186,BR-187  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. |  |  |

###### 

###### **2.2.3.84  Filter Instructor's Transaction History**

| UC ID and Name | UC-84-Filter Instructor's Transaction History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | Instructor enters or selects filter parameters and clicks the "Filter" button. |  |  |
| Description: | Allows an instructor to search and filter their transaction history list ("Learner Purchases" tab) by name/purchaser, status, month, and year. |  |  |
| Preconditions: | **PRE-1**. The instructor is successfully authenticated. **PRE-2**. The instructor is on the instructor transaction history page. |  |  |
| Post-conditions: | **POST-1**. The system displays the list of transactions matching the specified filter criteria. |  |  |
| Normal Flow: | **A. Filter Transaction History Successfully** 1\. Instructor accesses the instructor transaction page. 2\. Instructor selects the "Learner Purchases" tab. 3\. Instructor enters a search query in the search field. 4\. Instructor selects a filter status from the status dropdown. 5\. Instructor selects a transaction status from the period dropdowns. 6\. Instructor clicks the "Filter" button. 7\. The system validates the filter inputs. 8\. The system requests the transaction history filtered by the search query, status, month, and year from the server. 9\. The server queries the matching transactions from the database. 10\. The system displays the filtered list of transactions to the instructor. |  |  |
| Alternative Flows: | **A.6  Reset filter** 1\. Instructor clicks the "Reset" link. 2\. The system resets the search query, status dropdown, month, and year to their default values. 3\. The system requests the default transaction history from the server. 4\. The server queries the default transactions from the database. 5\. The system displays the default list of transactions. 6\. Back to step 3\. **A.9 No matching results found** 1\. The server returns an empty list of transactions. 2\. The system displays a message indicating that no purchases were recorded for this period. 3\. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Low |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-188,BR-189 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. |  |  |

###### 

###### **2.2.3.85  Filter payout History**

| UC ID and Name | UC-85-Filter payout History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | Instructor enters or selects filter parameters and clicks the "Filter" button. |  |  |
| Description: | Allows an instructor to search and filter their payout history list ("Payout History" tab) by course title, Stripe ID, status, month, and year. |  |  |
| Preconditions: | **PRE-1.** The instructor is successfully authenticated. **PRE-2.** The instructor is on the instructor transaction history page. |  |  |
| Post-conditions: | **POST-1**. The system displays the list of payouts matching the specified filter criteria. |  |  |
| Normal Flow: | **A. Filter Payout History Successfully** 1\. Instructor accesses the instructor transaction page. 2\. Instructor selects the "Payout History" tab. 3\. Instructor enters a search query in the payout search field. 4\. Instructor selects a filter status from the payout status dropdown. 5\. Instructor selects a transaction status from the period dropdowns. 6\. Instructor clicks the "Filter" button. 7\. The system validates the filter inputs. 8\. The system requests the payout history filtered by the search query, status, month, and year from the server. 9\. The server queries the matching payouts from the database. 10\. The system displays the filtered list of payouts to the instructor. |  |  |
| Alternative Flows: | **A.6 Reset filter** 1\. Instructor clicks the "Reset" link. 2\. The system resets the payout search query, payout status dropdown, month, and year to their default values. 3\. The system requests the default payout history from the server. 4\. The server queries the default payouts from the database. 5\. The system displays the default list of payouts. 6\. Back to step 3\.  **A.9 No matching results found** 1\. The server returns an empty list of payouts. 2\. The system displays a message indicating that no payouts were recorded for this period. 3\. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Low |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-188,BR-189 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. |  |  |

###### 

###### **2.2.3.86  Sort payout History**

| UC ID and Name | UC-86 Sort payout History |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A. |
| Trigger: | Instructor selects a sorting option and clicks the "Filter" button. |  |  |
| Description: | Allows an instructor to sort their payout history list ("Payout History" tab) based on date or payout amount. |  |  |
| Preconditions: | **PRE-1.** The instructor is successfully authenticated. **PRE-2.** The instructor is on the instructor transaction history page. |  |  |
| Post-conditions: | **POST-1**. The system displays the list of payouts sorted according to the selected sorting option. |  |  |
| Normal Flow: | **A. Sort Payout History Successfully** 1\. Instructor accesses the instructor transaction page. 2\. Instructor selects the "Payout History" tab. 3\. Instructor clicks the sorting dropdown. 4\. Instructor selects a sorting option. 5\. Instructor clicks the "Filter" button. 6\. The system requests the sorted payout history from the server. 7\. The server queries the payouts from the database sorted by the selected criteria. 8\. The system displays the sorted list of payouts to the instructor. |  |  |
| Alternative Flows: | **A.5  Reset sort** 1\. Instructor clicks the "Reset" link. 2\. The system resets the sorting dropdown to the default option. 3\. The system requests the default payout history from the server. 4\. The server queries the default payouts from the database. 5\. The system displays the default list of payouts. 6\. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Low |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-189,BR-190 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. |  |  |

###### 

###### **2.2.3.88  View Instructor's Transaction History Detail**

| UC ID and Name | UC-88-View Instructor's Transaction History Detail |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A |
| Trigger: | Instructor clicks the "View" button next to a transaction row in the Learner Purchases list of the Earnings dashboard. |  |  |
| Description: | Allows instructors to view the comprehensive financial ledger breakdown, coupon usage, Stripe gateway references, purchaser info, and refund dispute details for a specific transaction. |  |  |
| Preconditions: | **PRE-01:** The instructor must be logged in. **PRE-02:** The transaction record must belong to a course owned by the logged-in instructor. **PRE-03:** There is at least one transaction in the record. |  |  |
| Post-conditions: | **POST-01:** The instructor views the full transaction breakdown and split details. |  |  |
| Normal Flow: | **A. View  Instructor's Transaction History Detail Successfully** 1\. Instructor accesses the Earnings dashboard page. 2\. The system retrieves the list of course transactions from the database. 3\. The system displays the list of transactions under the Learner Purchases tab. 4\. Instructor clicks the "View" button next to a transaction row. 5\. The system requests transaction details from the database based on the selected transaction ID. 6\. The system retrieves the transaction details from the database. 7\. The system displays the transaction details page containing the Transaction Overview, Product Details, Purchaser Details, Stripe secure references, and Split Summary Card. |  |  |
| Alternative Flows: |  **A.7.1 Refund Request Pending Banner** 1\. The system detects that the transaction has a refund pending status. 2\. The system displays a top warning banner showing the refund request date and the student's stated refund reason. 3\. Back to step 7\. **A.7.2 Refund Approved Banner** 1\. The system detects that the transaction has a refunded status. 2\. The system displays a top alert banner showing the student's refund reason and the administrator's settlement note. 3\. The system displays the ledger values with negative deductions highlighted in red in the Split Summary Card. 4\. Back to step 7\. **A.7.3 Refund Request Rejected Banner** 1\. The system detects that the transaction has a completed status and a refund rejection note is present. 2\. The system displays a top informational banner showing the student's original refund reason and the administrator's rejection note. 3\. Back to step 7\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-5 Transaction Not Found** 1\. The system detects that the transaction ID does not exist in the database. 2\. The system displays a warning message regarding transaction not found. 3\. The system redirects the instructor to the Earnings dashboard page. **EX-5.2 Unauthorized Transaction Access** 1\. The system detects that the transaction record does not belong to a course owned by the logged-in instructor. 2\. The system displays an authorization error message. 3\. The system redirects the instructor to the Earnings dashboard page |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-191,BR-192,BR-193 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. Stripe is working properly. |  |  |

###### 

###### **2.2.3.87  View Instructor's Course Revenue**

| UC ID and Name | UC-87: View Instructor's Course Revenue |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Instructor | Secondary Actors: | N/A |
| Trigger: | The instructor clicks the "Course Revenues" tab in the Earnings dashboard. |  |  |
| Description: | Allows instructors to view the sales breakdown and revenue statistics for each of their courses, including monthly revenue, yearly revenue, lifetime revenue, sales quantity, and growth trends. |  |  |
| Preconditions: | **PRE-01:** The instructor must be logged in. **PRE-02.** The user's account must be active as an approved Instructor with linked Stripe settings. |  |  |
| Post-conditions: | **POST-1.** The instructor views the course-by-course revenue list and performance growth data for the selected period. |  |  |
| Normal Flow: | **A. View Instructor's Course Revenue Successfully** 1\. Instructor accesses the Earnings dashboard page. 2\. The system retrieves the list of owned courses along with their corresponding sales quantity, monthly revenue, previous month's revenue, yearly revenue, and lifetime revenue from the database. 3\. The system displays the Earnings dashboard. 4\. Instructor clicks the "Course Revenues" tab. 5\. The system displays the course revenue table containing: Course Title, Sales Quantity, Monthly Revenue, Yearly Revenue, and Lifetime Revenue. |  |  |
| Alternative Flows: |  **A.7.1 Refund Request Pending Banner** 1\. The system detects that the transaction has a refund pending status. 2\. The system displays a top warning banner showing the refund request date and the student's stated refund reason. 3\. Back to step 7\. **A.7.2 Refund Approved Banner** 1\. The system detects that the transaction has a refunded status. 2\. The system displays a top alert banner showing the student's refund reason and the administrator's settlement note. 3\. The system displays the ledger values with negative deductions highlighted in red in the Split Summary Card. 4\. Back to step 7\. **A.7.3 Refund Request Rejected Banner** 1\. The system detects that the transaction has a completed status and a refund rejection note is present. 2\. The system displays a top informational banner showing the student's original refund reason and the administrator's rejection note.   3\. Back to step 7\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. **EX-5 Transaction Not Found** 1\. The system detects that the transaction ID does not exist in the database. 2\. The system displays a warning message regarding transactions not found. 3\. The system redirects the instructor to the Earnings dashboard page. **EX-5.2 Unauthorized Transaction Access** 1\. The system detects that the transaction record does not belong to a course owned by the logged-in instructor. 2\. The system displays an authorization error message. 3\. The system redirects the instructor to the Earnings dashboard page |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-194,BR-195,BR-196 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. Stripe is working properly. |  |  |

###### 