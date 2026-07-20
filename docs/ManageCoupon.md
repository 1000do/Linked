###### **2.2.3.150.  View Coupons List**

| UC ID and Name | UC-150- View Coupons List |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc   | Date Created: | 10/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin accesses the "Coupons & Offers" page. |  |  |
| Description: | Allows an Admin to browse a list of all coupons in the system. The Admin can search by code or ID, filter by active status or type, and sort the list client-side. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. |  |  |
| Post-conditions: | **POST-1.** The coupon list is successfully retrieved and rendered on the frontend. |  |  |
| Normal Flow: | **A. View Coupons List Successfully** 1\. Admin accesses the Coupon List page. 2\. The system retrieves the list of coupons sorted by CouponId descending. 3\. The system maps the coupon list to the coupon response details (ID, Code, Type, Value, Min Order Value, Start Date, End Date, Usage Limit, Used Count, Active Status). 4\. The system renders and displays the Coupon List table. |  |  |
| Alternative Flows: | **A.1 Filter Coupons** 1\. Admin selects active status filter or coupon type filter (Fixed/Percentage). 2\. The system retrieves the matched coupon list from the database. 3\. Back to step 4\. **A.2 Search Coupons** 1\. Admin enters search keyword (Coupon Code). 2\. The system retrieves the matching coupon list from the database. 3\. Back to step 4\. **A.3 Sort Coupons**  1\. Admin clicks on a table header column (Date Create, Expiring Soon, Most Popular ) to sort. 2\. The system sorts the cached coupon list in memory (ascending or descending order). 3\. The system rerenders and displays the sorted table. 4\. Back to step 4\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Very High |  |  |
| Business Rules: |  BR-262  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has a stable internet connection. |  |  |

###### **2.2.3.151 Add Coupons**

| UC ID and Name | UC-151-Add Coupons |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc | Date Created:  | 10/06/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Create Coupon" button on the coupon list page. |  |  |
| Description: | Allows an Admin to create a new coupon in the coupon warehouse by specifying a unique code, coupon type, discount value, minimum order value, validity dates, and usage limit. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. |  |  |
| Post-conditions: | **POST-1**. The new coupon is successfully saved to the database. **POST-2**. The system renders the updated Coupon List / Success message. |  |  |
| Normal Flow: | **A. Add Coupon Successfully** 1\. Admin accesses the Coupon List view. 2\. Admin clicks the Add Coupon button. 3\. The system displays the coupon creation form. 4\. Admin enters the coupon details: Code, Type, Value, Min Order Value, Start Date, End Date, and Limit. 5\. Admin clicks the Save button. 6\. The system normalizes the coupon type and validates form inputs. 7\. The system checks if the coupon code is unique in the database. 8\. The system instantiates a new Coupon entity with Used Count \= 0 and Is Active \= true. 9\. The system saves the coupon details to the database. 10\. The system displays a success message and renders the updated Coupon List. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-6 Validation Failure** 1\. The system detects invalid coupon parameters (e.g., Value \<= 0, Value \> 100 for percentage, Start Date \> End Date, or Limit \< 1). 2\. The system displays a validation error message explaining the failure. 3\. Back to step 4\.  **EX-7 Coupon Code Already Exists** 1\. The system detects that the coupon code already exists in the database. 2\. The system displays an error message: "Coupon code already exists". 3\. Back to step 4\.  **EX-9 Database Save Failure** 1\. The system detects that saving the coupon details failed (rows affected \<= 0). 2\. The system displays an error message regarding database save failure. 3\. Back to step 4\. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-261  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has a stable internet connection. |  |  |

###### **2.2.3.152.  Edit Coupons**

| UC ID and Name | UC-152- Edit Coupons |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc   | Date Created: | 10/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Edit" button on a coupon in the Actions row of the coupon list. |  |  |
| Description: | Allows an Admin to update properties of an existing coupon (End Date, Usage Limit, or Active Status). Core parameters such as Coupon Code, Coupon Type, and Discount Value are locked to preserve transactional history. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2.** The target coupon must exist in the system. |  |  |
| Post-conditions: | **POST-1**. The updated coupon properties are successfully saved to the database. **POST-2**. The system renders the updated Coupon List / Success message. |  |  |
| Normal Flow: | **A. Edit Coupon Successfully** 1\. Admin accesses the Coupon List page. 2\. Admin clicks the Edit button on a specific coupon. 3\. The system displays the coupon edit form with current coupon details (Coupon Code, Coupon Type, and Discount Value are read-only). 4\. Admin modifies the allowed fields: End Date, Usage Limit, or Active Status. 5\. Admin clicks the Save button. 6\. The system retrieves the target coupon by ID. 7\. The system validates the modified fields against business rules. 8\. The system updates the allowed properties of the Coupon entity. 9\. The system saves the changes to the database. 10\. The system displays a success message and renders the updated Coupon List. |  |  |
| Alternative Flows: |  N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-6 Coupon Not Found** 1\. The system detects that the target coupon ID does not exist in the database. 2\. The system displays a "Coupon not found" error message. 3\. Back to step 1\.  **EX-7 Invalid Input Data** 1\. The system detects invalid update parameters (e.g., End Date is before the coupon's Start Date, or the new Usage Limit is less than the current Used Count). 2\. The system displays a validation error message explaining the failure. 3\. Back to step 4\.  **EX-9 Database Save Failure** 1\. The system detects that saving the updated coupon details failed (rows affected \<= 0). 2\. The system displays an error message regarding database save failure. 3\. Back to step 4\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: |  Medium |  |  |
| Business Rules: | BR-263  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

 

###### **2.2.3.153.  Delete Coupons**

| UC ID and Name | UC-153- Delete Coupons |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 10/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks the "Delete" button on a coupon in the Actions row of the coupon list. |  |  |
| Description: | Allows an Admin to soft-delete (disable) a coupon. Instead of physically removing the record from the database, the coupon is marked inactive and its end date is moved to the past to preserve historical data for accounting/auditing. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2**. The target coupon must exist in the system. |  |  |
| Post-conditions: | **POST-1**. The coupon is successfully soft-deleted (marked inactive) in the database. **POST-2.** The system renders the updated Coupon List / Success message. |  |  |
| Normal Flow: | **A. Delete Coupon Successfully** 1\. Admin accesses the Coupon List page. 2\. Admin clicks the Delete button on a specific coupon. 3\. The system retrieves the target coupon by ID. 4\. The system updates the coupon status to inactive (IsActive \= false) and shifts the End Date to yesterday (DateTime.Now.AddDays(-1)). 5\. The system saves the changes to the database. 6\. The system displays a success message and renders the updated Coupon List. |  |  |
| Alternative Flows: |  N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page.  **EX-3 Coupon Not Found** 1\. The system detects that the target coupon ID does not exist in the database. 2\. The system displays a "Coupon not found" error message. 3\. Back to step 1\.  **EX-5 Database Save Failure** 1\. The system detects that saving the changes failed (rows affected \<= 0). 2\. The system displays an error message regarding database save failure. 3\. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: |  Low |  |  |
| Business Rules: | BR-264  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### **2.2.3.154. Search Coupons**

| UC ID and Name | UC-154-Search Coupons |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc | Date Created: | 10/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin enters a search keyword in the search input field. |  |  |
| Description: | Allows an Admin to filter the coupon list by searching for a specific keyword matching the Coupon Code. |  |  |
| Preconditions: | **PRE-1.** Admin must be logged in with Admin role privileges. **PRE-2**. Admin is currently on the Coupon List page. |  |  |
| Post-conditions: | **POST-1**. The filtered coupon list matching the search keyword is successfully retrieved and rendered on the frontend. |  |  |
| Normal Flow: | **A. Search Coupons Successfully** 1\. Admin enters a search keyword in the search input field. 2\. The system captures the keyword and triggers a query request. 3\. The system queries the database for coupons matching the search keyword (by Coupon Code). 4\. The system maps the matched coupon entities to response objects. 5\. The system renders and displays the filtered Coupon List table. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: |  High |  |  |
| Business Rules: | BR-262  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

###### 

###### **2.2.3.155. Sort Coupons**

| UC ID and Name | UC-155-Sort Coupons |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc | Date Created: | 10/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin clicks on a table header column in the Coupon List page. |  |  |
| Description: | Allows an Admin to sort the displayed coupon list ascending or descending based on selected columns (e.g., Date Create, Discount Value, Expiring Soon, Most Popular) entirely client-side. |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2.** Admin is currently on the Coupon List page with coupon data displayed. |  |  |
| Post-conditions: | **POST-1**. The coupon list table is rerendered and sorted based on the selected column and direction. |  |  |
| Normal Flow: | **A. Sort Coupons Successfully** 1\. Admin clicks on a table header column (e.g., Date Create, Discount Value,...). 2\. The system identifies the column and toggles the sorting direction. 3\. The system sorts the cached coupon list in memory. 4\. The system rerenders and displays the sorted Coupon List table. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: |  High |  |  |
| Business Rules: | BR-262  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection |  |  |

###### **2.2.3.156. Filter Coupons**

| UC ID and Name | UC-156-Filter Coupons |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc | Date Created: | 10/6/2026 |
| Primary Actor: | Admin | Secondary Actors: | N/A |
| Trigger: | Admin selects active status filter or coupon type filter. |  |  |
| Description: | Allows an Admin to filter the displayed coupon list by selecting active status (Active/Paused) or coupon type (Fixed/Percentage). |  |  |
| Preconditions: | **PRE-1**. Admin must be logged in with Admin role privileges. **PRE-2.** Admin is currently on the Coupon List page. |  |  |
| Post-conditions: | **POST-1**. The filtered coupon list matching the criteria is successfully retrieved and rendered on the frontend. |  |  |
| Normal Flow: | **A. Filter Coupons Successfully** 1\. Admin selects the active status filter or coupon type filter. 2\. The system captures the selected filter parameters and triggers a query request. 3\. The system queries the database for coupons matching the filter criteria (IsActive or CouponType). 4\. The system maps the matched coupon entities to response objects. 5\. The system renders and displays the filtered Coupon List table.e. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system displays an authentication error message. 2\. The system redirects the user to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: |  High |  |  |
| Business Rules: | BR-265  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Admin has stable internet connection. |  |  |

