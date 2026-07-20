###### **2.2.3.43 View Transaction List**

| UC ID and Name | UC-43 \- View Transaction List |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 14/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | Users access their purchase history page. |  |  |
| Description: | Allows an authenticated student to view a paginated list of their purchase transactions, search by course title, filter by payment status, sort by date/price, navigate details, request a refund for eligible purchases within 14 days, and submit a billing support ticket. |  |  |
| Preconditions: | **PRE-1.** User must be successfully authenticated. |  |  |
| Post-conditions: | **POST-1.** The user's transaction history is retrieved and displayed. **POST-2.** The user can navigate to the details of individual transactions, request refunds, or submit support tickets. |  |  |
| Normal Flow: | **A. View Transaction History Successfully** 1\. The user accesses the purchase history page. 2\. The system validates the user's session. 3\. The system sends a request to retrieve the user's transaction history. 4\. The system retrieves the transaction records from the database. 5\. The system displays the list of transactions with transaction details. 6\. The system displays the total purchases metric. |  |  |
| Alternative Flows: | **A.1.1 Empty transaction list** 1\. The system detects that the user has no transaction history records. 2\. The system displays an empty history card with a "No transactions found" message. 3\. The system displays the "Explore Courses" button. 4\. User clicks the "Explore Courses" button. 5\. The system redirects the user to the home page. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-161,BR-162,BR-163,BR-173 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | 1\. User has stable internet connection. 2\. Stripe payment gateway integration is functional. |  |  |

###### **2.2.3.40 Search Transaction History**

| UC ID and Name | UC-40 \- Search Transaction History |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 14/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | User enters a keyword in the search input field of the purchase history page. |  |  |
| Description: | Allows the user to search their purchase/transaction history by course title. The search is submitted to the server via a GET request which reloads the page with the filtered list. |  |  |
| Preconditions: | **PRE-1**. The User must be successfully authenticated. **PRE-2**. The User must have accessed their purchase history page. |  |  |
| Post-conditions: | **POST-1**. The transaction list is filtered and displays only transactions matching the search keyword. |  |  |
| Normal Flow: | **A. Search Transaction History Successfully** 1\. The user enters a search keyword in the input field with the placeholder "Search by course title...". 2\. The user clicks the "Apply" button or presses the ‘’Enter’’ key. 3\. The page reloads and displays the list of transactions containing the searched keyword in the course title. 4\. The search input field retains the entered keyword, and a "Reset" button appears next to the filter controls. |  |  |
| Alternative Flows: | **A.2.1 Reset Search** 1\. The user clicks the "Reset" button. 2\. The page reloads. 3\. The search input field is cleared, the "Reset" button disappears, and the system displays the full, unfiltered transaction history list. **A.3 No Matching Results Found** 1\. The user performs a search, but the keyword does not match any purchased courses. 2\. The system displays an empty state screen with the title: \`"No transactions found"\` and description: \`"You have not made any purchases yet, or no transactions match your criteria."\` 3\. The system displays an "Explore Courses"button redirecting the user back to the course marketplace. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system detects the user's session has expired. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-164,BR-165 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User has a stable internet connection. |  |  |

###### **.2.3.45 Request Refund** 

| UC ID and Name | UC-45 \- Request Refund |  |  |
| ----- | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 14/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | The user clicks the "Refund Request" button next to a successful transaction in their Purchase History list.  |  |  |
| Description: | Allows an authenticated student to request a refund for a course purchased within a 14-day window. The system automatically checks strict eligibility metrics (learning progress, past course refunds, recent refund frequency, and account flags). If any auto-rejection rule is violated, the request is immediately declined with an abstract reason; otherwise, it is submitted to the Admin for manual review. |  |  |
| Preconditions: | PRE-1. The user must be successfully authenticated. PRE-2. The target transaction status must be successful, and not already refunded or pending refund. PRE-3. The transaction must have occurred within the last 14 days. PRE-4. The transaction must belong to the logged-in user. |  |  |
| Post-conditions: | POST-1. The transaction status is updated to refund pending in the database, and the request is queued for Admin review. POST-2. If the request is automatically rejected, the transaction status remains unchanged, and the auto-rejection decision is logged in the database. |  |  |
| Normal Flow: | **A. Submit Refund Request Successfully** 1\. User accesses the Purchase History page. 2\. User clicks the "Refund Request" button next to the eligible transaction. 3\. The system displays a confirmation modal requesting the user to enter a reason for the refund. 4\. User enters a reason in the text field. 5\. User clicks the "Submit Request" button. 6\. The system requests the backend database to submit the refund request. 7\. The system validates the transaction ownership. 8\. The system queries the database to evaluate the refund eligibility metrics. 9\. The system updates the transaction status to refund pending in the database. 10\. The system creates a new pending refund request record in the database. 11\. The system displays a success message indicating the refund request is under review. 12\. The system reloads the page to display the updated transaction status |  |  |
| Alternative Flows: | **A.8.1 Auto-Rejection Triggered** 1\. The system detects that at least one auto-rejection rule is violated . 2\. The system keeps the transaction status unchanged. 3\. The system saves the auto-rejection decision and abstract reason into the database logs. 4\. The system displays a rejection message showing the corresponding abstract reason (see BR-06). 5\. Back to step 1\. |  |  |
| Exceptions: | **EX-01: Session Invalid (Not Logged In)** 1\. The system detects that the user session has expired or is invalid. 2\. The system redirects the user to the Login page.. **EX-5: Missing Refund Reason** 1\. The user attempts to submit the refund request with an empty reason field. 2\. The system blocks the submission and displays a validation alert: \- "You must enter a refund reason\! |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Low |  |  |
|  Business Rules: | BR-166, BR-167,BR-168,BR-169.BR-170,BR-171,BR-172 |  |  |
| Other Information: | Once an Admin manually approves a pending refund, the system initiates the Stripe refund process, reverses the instructor's payout, and revokes the student's course access. |  |  |
| Assumptions: | User has stable internet connection. Course total durations and student progress metrics are calculated accurately by the platform's tracking database. |  |  |

###### **2.2.3.46 View Transaction Detail**

| UC ID and Name | UC-46 \- View Transaction Detail  |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 14/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | 1\. The user clicks the "View Details" (chevron icon) button next to a transaction record in the Purchase History list.  |  |  |
| Description: | Provides a detailed invoice receipt of a specific course purchase transaction, displaying detailed financial breakdowns, purchaser information, and payment gateway metadata. |  |  |
| Preconditions: | PRE-01: Users must be successfully authenticated (logged in). PRE-02: Users must have placed at least one transaction.  PRE-03. The specified transaction must exist and be associated with the Customer's account. |  |  |
| Post-conditions: | POST \-01:The System successfully displays the full receipt details of the selected transaction on the Transaction Detail page. . |  |  |
| Normal Flow: | **A. View Transaction Detail Successfully** 1\. User clicks the "View Detail" button next to a transaction record. 2\. The system checks the active session, queries the database, and retrieves transaction data. 3\.The user views the detailed transaction receipt containing: Transaction details: Transaction ID, Payment Date/Time, and Status. Buyer details: Name and Email. Course details: Course Title, Instructor Name, and Course Thumbnail. Price details: Subtotal, Discount Amount, applied Coupon Code, and final Total. |  |  |
| Alternative Flows: | **A.3 Refunded Transaction** 1\. The system detects that the transaction status is "refunded". 2\. The system displays a refund notification banner showing the user's refund reason and the admin's approval note**.** 3\. The system displays the transaction amount as a negative value and updates the status badge to "Refunded". |  |  |
| Exceptions: | **EX-02: Session Invalid (Not Logged In)** 1\. Display the message: "Please login to view transaction history". 2\. Redirect to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-173 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | The transaction data displayed is pulled from the system's database and reflects secure online payment details cleared by Stripe. |  |  |

