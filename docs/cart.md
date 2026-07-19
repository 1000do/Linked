###### **2.2.3.37 Add to cart**

| UC ID and Name | UC-34 \- Add to cart |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Truong Hung | Date Created: | 10/07/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | User or Instructor clicks the "Add to Cart" button on a course details card or page. |  |  |
| Description: | This use case allows an authenticated user to add one or more courses to their shopping cart to purchase them together in a single transaction. |  |  |
| Preconditions: | **PRE-1.** User or Instructor must be successfully authenticated. **PRE-2.** User or Instructor must have a verified email status. |  |  |
| Post-conditions: | **POST-1.** The course is successfully added to the user's shopping cart in the database. **POST-2.** The system redirects the User or Instructor to the Cart Summary page, or displays a success toast notification on the current page. |  |  |
| Normal Flow: | **A. Add Course to Cart Successfully** 1\. User or Instructor accesses a course detail page. 2\. User or Instructor clicks the "Add to Cart" button. 3\. The system checks for a valid authenticated session. 4\. The system requests the backend to add the course to the user's shopping cart. 5\. The server validates that the active user's email address is verified. 6\. The server checks if the course is already in the user's cart, already purchased, or if the user is the instructor of the course. 7\. The server adds the course record to the user's shopping cart in the database. 8\. The server returns a success response. 9\. The system displays a success message indicating the course was added to the cart. 10\. The system redirects the User or Instructor to the Cart page. |  |  |
| Alternative Flows: | **A.2 Add to Cart**  1\. User or Instructor clicks the "Add to Cart" button configured. 2\. The system requests the backend to add the course to the cart. 3\. Back to step 5 of the Normal Flow. 4\. The server returns a success response. 5\. The system displays a success toast notification on the current page and updates the cart item count badge in the header without redirecting. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** 1\. The system redirects the User or Instructor to the login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-41,BR-42,BR-43 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User or Instructor has stable internet connection. |  |  |

###### **2.2.3.38  View Cart items**

| UC ID and Name | UC-37 \- View Cart items |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Truong Hung | Date Created: | 14/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | 1\. User clicks the "Shopping Cart" icon in the navigation bar header. |  |  |
| Description: | Displays the list of courses added to the active user's shopping cart, calculates the pricing summary (subtotal, coupon discounts, final payable amount), and lists available checkout coupon options. |  |  |
| Preconditions: | PRE-1:User must be successfully authenticated (logged in). |  |  |
| Post-conditions: | POST-1: The user views the detailed list of cart items and the finalized calculated total amount. |  |  |
| Normal Flow: | **A. View Cart Successfully** A. View Cart Successfully 1\. The user clicks the "Cart" icon on the navigation header. 2\. The system verifies the active session and navigates the user to the Shopping Cart page. 3\. The system displays the detailed list of courses (title, instructor, thumbnail, price) currently in the cart. 4\. The system calculates and displays the Subtotal, applied Discount Amount (if any), and the final Payable Total. 5\. The user views the detailed shopping cart workspace and reviews their items. |  |  |
| Alternative Flows: | **A.4 Active Coupon Applied**  1\. The system detects that a coupon code was previously applied to this order. 2\. The system recalculates the discount and updates the final Payable Total . 3\. Return to step A.5. |  |  |
| Exceptions: | **EX-01: Unauthorized / Session Expired**  1\. If the session has expired, the user is redirected to the login page before the cart can be viewed.  **EX-02: Empty Cart** 1\. At step 3, if there are no courses currently in the cart: The system displays the empty notice: "Your cart is empty" with the description "Explore interesting courses and add them to your cart\!" and an "Explore Courses" button. **EX-03: Invalid Coupon in Cart**  1\. At step 3, if any applied coupon is detected as invalid, expired, or doesn't meet minimum requirements: The system automatically removes that invalid coupon code from the user's coupon cookie map.    \- The system recalculates the pricing summary and updates the cart display.    \- The system displays the specific error message: "This coupon has expired or run out of usage." or "This coupon does not exist." 2\. Return to step A.4. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-35 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

###### **2.2.3.40 Delete cart items**

| UC ID and Name | UC-35 \- Delete cart items |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Truong Hung | Date Created: | 10/07/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | User or Instructor clicks the delete button next to a course item on the Cart page. |  |  |
| Description: | Allows authenticated Users or Instructors to remove a course from their shopping cart. |  |  |
| Preconditions: | **PRE-1.** User or Instructor must be successfully authenticated. **PRE-2.** User or Instructor has at least one course item in their shopping cart. **PRE-3**. User or Instructor is on the Cart page. |  |  |
| Post-conditions: | **POST-1.** The course item is permanently removed from the user's shopping cart in the database. **POST-2.** The system recalculates the cart summary prices and refreshes the Cart page. |  |  |
| Normal Flow: | **A. Remove Course from Cart Successfully** 1\. User or Instructor accesses the Cart page. 2\. User or Instructor clicks the delete icon button next to the target course item. 3\. The system requests the backend to remove the course from the user's shopping cart. 4\. The server validates that the active User or Instructor session is valid. 5\. The server deletes the course record from the user's shopping cart in the database. 6\. The server returns a success response. 7\. The system displays a success notification indicating the course was removed. 8\. The system recalculates the cart summary prices and re-renders the Cart page. |  |  |
| Alternative Flows: | **EX-1 Session Invalid (Not Logged In)** 1\. The system redirects the User or Instructor to the login page.  **EX-6.1 Course not in cart** 1\. The server detects that the course is not present in the user's shopping cart. 2\. The system displays an error message: "Cannot remove course from cart." |  |  |
| Exceptions: | **EX-01: Unauthorized** 1\. If the session has expired, the user is redirected to the login page before the deletion can occur. **EX-1.2: Course Not Found**  1\. If the target course is not found in the user's cart:  error message: "Course not found in the cart."  2\. Return to step A.1. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-44 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User or Instructor has stable internet connection. |  |  |

###### 

