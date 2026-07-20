###### **2.2.3.39 Checkout**

| UC ID and Name | UC-39 \- Checkout |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 10/07/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | Stripe |
| Trigger: | The user navigates to the checkout page. |  |  |
| Description: | This use case facilitates the secure payment process for one or more courses using the Stripe payment gateway, resulting in course enrollment upon successful transaction. |  |  |
| Preconditions: | **PRE-1:** User must be authenticated. **PRE-2:** Cart must contain at least one course (or a single course selected for Direct Buy). |  |  |
| Post-conditions: | **POST-1**: Payment is successfully processed and recorded. **POST-2:** User is enrolled in the purchased courses. **POST-3:** Cart is cleared and payouts for instructors are scheduled. |  |  |
| Normal Flow: | **A. Process Checkout Successfully** 1\. The user reviews the shopping cart and clicks the "Checkout" button. 2\. The system calls the Stripe API to create a Payment Request and directly displays the secure card information form (Stripe elements) on the Checkout page instead of redirecting the user away from the website. 3\. Stripe displays the secure payment form containing the following fields: Contact Email (pre-filled with the user's email) Card Information (Card number with network logo, Expiration Date, CVC code) Cardholder Name Country/Region (default is the user's current location) . 4\. The users enters valid Stripe payment card information and clicks the checkout button. 5.Stripe processes the payment successfully; the system validates the transaction, records the order and enrollment, calculates/deducts gateway and commission fees, holds the instructor's payout, and sends a sales notification.  6\. The system will delete the student's shopping cart and display a successful payment notification. |  |  |
| Alternative Flows: | **A.2 Coupon Application.** 1.The system detects that a coupon code is attached to the checkout request and validates its eligibility (checks active status, expiration date, usage limit, and minimum order value). 2.If the coupon is valid, the system calculates the discount amount and applies the discounted prices to the checkout session line items. 3.If the coupon is invalid or expired, the system ignores the discount and prepares the checkout session using the original course prices. 4.Return to step A.2. **A.4 Incorrect Card Information / Payment Declined** 1\. The student enters incorrect card details or the payment is declined on the Stripe page. 2.Stripe displays an error message directly on the payment form (e.g., "Incorrect card number" or "Your card was declined"). 3\. Return to step A.5. **A.4 Payment Cancellation.** 1.The user cancels the payment on the secure Stripe payment page. 2.Stripe redirects the user back to the shopping cart page. 3.The system displays a payment cancellation notice and restores the cart contents. |  |  |
| Exceptions: | **EX-01: Payment Declined** 1\. Stripe reports a failed transaction. The system notifies the user and allows them to retry with a different payment method. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-282,BR-283,BR-284 |  |  |
| Other Information: | The system uses Stripe's "Checkout Sessions" for PCI compliance and security. |  |  |
| Assumptions: | The user has a valid payment method accepted by Stripe. |  |  |

