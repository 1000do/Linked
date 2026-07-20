###### **2.2.3.21  Gift Courses**

| UC ID and Name | UC-17-Gift Courses |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung  | Date Created: | 13/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | The user clicks the "Gift this course" button on the course details page. |  |  |
| Description: | Allows the user to configure gift details—including the recipient's name, email, card theme, and a personal message with a live preview—before proceeding to the checkout page. |  |  |
| Preconditions: | **PRE-1**. The user must be authenticated. **PRE-2**. The target course must be retrievable from the system. |  |  |
| Post-conditions: | **POST-1**. The user is redirected to the Gift Checkout page with all gift details passed as parameters. |  |  |
| Normal Flow: | **A. Configure Gift Details Successfully** The user accesses the Gift Setup page. The system displays the gift setup form, the course summary card, and the live gift card preview. User fills in the Recipient's Email, Recipient's Name (Optional), Choose Card Theme, and Gift Message (Optional) fields. User clicks the "Proceed to Payment" button. The system validates all form inputs. The server queries the database to verify the recipient's enrollment status for the selected course. The system redirects the user to the Gift Checkout page. |  |  |
| Alternative Flows: | **A.5 Invalid form inputs** The system highlights the invalid fields and displays inline validation error messages. Back to step 3\. **A.6 Recipient already enrolled** The system displays a validation error on the recipient email field indicating the recipient already owns the course. Back to step 3\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** The system displays an authentication error message. The system redirects the user to the login page. **EX-2 Course Not Found** The system fails to retrieve course information from the backend. The system displays an error message. The system redirects the user to the home page. **EX-6 Recipient Already Enrolled** The system detects via real-time API check that the recipient already has an active enrollment in the course. The system highlights the recipient email field with an error message indicating the recipient already owns the course. The system disables the "Proceed to Payment" button. Back to step 4\. |  |  |
| Priority: | Medium |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-155 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | The system has a reliable email delivery service. |  |  |

###### **2.2.3.22 Pay for a gift**

| UC ID and Name | UC-18-Pay for a gift |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 13/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | N/A |
| Trigger: | 1\. System requests recipient details within the Gift course configuration workspace. |  |  |
| Description: | Allows the giver to enter and validate the recipient's personal identity details (full name and email address), ensuring accurate delivery coordinates for the automated redeem voucher. |  |  |
| Preconditions: | PRE-1. The user must be authenticated. PRE-2. The gift details must have been successfully configured and passed to the checkout page. |  |  |
| Post-conditions: | POST-1. Payment has been processed and successfully recorded. POST-2. A unique redemption code has been generated and sent to the recipient's Gmail address. POST- 3\. Payment schedule for the instructor has been planned. |  |  |
| Normal Flow: | **A. Pay for a Gift Successfully** The user accesses the Gift Checkout page. The system displays the billing contact email, the gift card details summary, the course information, the pricing details, and the payment card entry form. The user inputs the Cardholder Name, Card Number, Expiry Date, CVC, and Country or Region. The user clicks the "Pay Securely" button. The system validates that the Cardholder Name is entered. The system communicates with the Stripe Payment Gateway to process the card transaction. The Stripe Payment Gateway confirms the payment is successful. The system inserts the order, order item, transaction, and gift records in a single database transaction. The system calculates the Stripe transaction fee and the instructor's payout amount, then saves the payout record in the database. The system triggers the background tasks to send a gift card email with the redemption link to the recipient. The system detects the recipient email is not registered in the system and skips sending an in-app notification. The system triggers the background tasks to send a sales notification to the course instructor. The system redirects the user to the course list page. The system displays a checkout success message. |  |  |
| Alternative Flows: | **A.5 Missing cardholder name** The system highlights the Cardholder Name field and displays a validation error message. Back to step 3\. **A.7 Payment declined** The Stripe Payment Gateway declines the payment card. The system displays a payment error message. Back to step 3\. **A.11 Recipient email is registered** The system triggers the background task to send an in-app notification to the recipient. Back to step 12\. |  |  |
| Exceptions: | **EX-1 Session Invalid (Not Logged In)** The system displays an authentication error message. The system redirects the user to the login page. **EX-2 Gift Details Missing** The system detects that the required gift parameters (recipient email, etc.) are missing. The system displays an error message. The system redirects the user to the home page. **EX-10 Payment Intent Creation Failed** The system fails to create the payment intent on the backend checkout service. The system displays an error message. The system redirects the user back to the gift setup page. **EX-10.1 Email Delivery Failed** The system logs the email delivery failure. Back to step 11 |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: |    BR-156, |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

###### **2.2.3.23  Receive a gift**

| UC ID and Name | UC-19-Receive a gifts |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 13/05/2026 |
| Primary Actor: | User | Secondary Actors: | N/A |
| Trigger: | The user clicks the claim link from their email or in-app notification. |  |  |
| Description: | Allows the recipient to view the gift card and redeem the gifted course. If the user is not logged in, the system prompts them to log in or register. Once authenticated, the user clicks the claim button to enroll in the course and mark the gift as claimed, which also automatically cancels any pending refund requests for that gift.  |  |  |
| Preconditions: | PRE-1. The redemption token must be valid and exist in the system. PRE-2. The gift must not be already claimed or refunded. |  |  |
| Post-conditions: | POST-1. The user is enrolled in the gifted course with active status. POST-2. The gift record is marked as claimed and associated with the claiming user's ID. POST-3. Any pending refund request for the gift transaction is rejected |  |  |
| Normal Flow: | **A. Receive a Gift Successfully** The user clicks the redemption link in their email or in-app notification. The system validates the redemption token. The system verifies that the user is authenticated. The system displays the customized gift card preview, course information, and the "Claim Your Gift" button. The user clicks the "Claim Your Gift" button. The system verifies that the user does not already own the course. The system creates an active enrollment record for the user. The system updates the gift record as claimed and links it to the user's account in a single database transaction. The system rejects any pending refund request for this gift transaction. The system displays a claim success message and redirects the user to the course study page.  |  |  |
| Alternative Flows: | **A.3 User is not logged in** The system displays the gift card preview and hides the claim button, showing "Login" and "Register" buttons instead. User clicks the "Login" or "Register" button. The system redirects the user to the login/registration page, passing the gift token as a return URL parameter. User logs in or registers successfully. The system redirects the user back to the Gift Claim page. Back to step 4\. **A.6 Recipient already enrolled** The system detects that the claiming user is already enrolled in the course. The system displays an error message: "You already own this course in your library." The system disables the "Claim Your Gift" button |  |  |
| Exceptions: | **EX-2 Invalid or Expired Token**  The system detects that the redemption token does not exist. The system displays a token invalid error message. The system redirects the user to the Claim Error page. **EX-2.1 Refunded Gift** The system detects that the gift has been refunded. The system displays an error message indicating the gift is refunded. The system redirects the user to the Claim Error page. **EX-5 Gift Already Claimed** The system detects that the gift has already been claimed. The system displays a message indicating the gift is already claimed. The system disables the claiming options. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-157,BR-158,BR-159,BR-160 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | The message will be rendered as plain text or safe HTML in the email. |  |  |

