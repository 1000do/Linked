###### **2.2.3.26 Verify Email**

| UC ID and Name | UC-26-Verify Email |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User | Secondary Actors: | N/A |
| Trigger: | User enters the OTP code received in email and clicks the "Verify" / "Submit" button on the OTP verification form. |  |  |
| Description: | Allows a User to verify their email address by submitting the One-Time Password (OTP) sent to their registered email. Upon validation, the system marks the account as verified and updates user session state. |  |  |
| Preconditions: | **PRE-1.** An OTP has been generated and sent to the User's email. **PRE-2.** The User's email is stored in TempData. |  |  |
| Post-conditions: | **POST-1.** The account's verification state is updated to true (IsVerified \= true) in the database. **POST-2.** The system sets the IsVerified cookie to "true" and redirects the User to their Profile page with a success message. |  |  |
| Normal Flow: | **A. Verify Email Successfully** 1\. User enters the OTP code received in email on the Verify OTP form. 2\. User clicks the Verify button. 3\. The system validates the OTP input form format. 4\. The system retrieves the user's email from TempData. 5\. The system submits a verification request with the email and OTP. 6\. The system consumes and validates the OTP code. 7\. The system retrieves the account by email and updates IsVerified \= true in the database. 8\. The system removes the verification flags from TempData. 9\. The system updates the frontend IsVerified cookie to "true". 10\. The system displays a verification success message and redirects the User to the Profile page. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-4 Email Session Expired** 1\. The system detects that the email in TempData is empty or null. 2\. The system redirects the User to the Login page. **EX-6 Invalid OTP Format** 1\. The system detects that the entered OTP format is invalid. 2\. The system displays validation error messages. 3\. Back to step 1\.  **EX-6.1 Invalid Request State** 1\. The system detects that the verification payload is invalid or malformed. 2\. The system displays a request error message. 3\. Back to step 1\.  **EX-7 Incorrect or Expired OTP** 1\. The system detects that the OTP is incorrect or has expired. 2\. The system keeps the email and verification flags in TempData. 3\. The system displays the error message "Incorrect or expired OTP". 4\. Back to step 1\.  **EX-9 Account Not Found** 1\. The system detects that the account associated with the email does not exist in the database. 2\. The system displays an error message. 3\. The system redirects the User to the Login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-226, BR-227 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User has stable internet connection. |  |  |

###### **2.2.3.27 Change Password**

| UC ID and Name | UC-27 \- Change Password |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User | Secondary Actors: | N/A |
| Trigger: | User enters current password, new password, and confirms the new password, then clicks the "Change Password" button. |  |  |
| Description: | Allows an authenticated User to update their password. The system verifies the current password, validates the new password, hashes it using BCrypt, saves it to the database, and forces a logout so the user must log in again with their new credentials. |  |  |
| Preconditions: | **PRE-1.** User must be logged in with a valid session (AccessToken). |  |  |
| Post-conditions: | **POST-1.** The user's account password hash is successfully updated in the database. **POST-2.** The user is logged out (all authentication cookies deleted) and redirected to the Login page with a success message. |  |  |
| Normal Flow: | **A. Change Password Successfully** 1\. User accesses the Change Password page. 2\. User enters current password, new password, and confirm new password. 3\. User clicks the Change Password button. 4\. The system validates the input formats. 5\. The system verifies that the User has a valid AccessToken cookie. 6\. The system validates that the new password matches the confirm new password. 7\. The system retrieves the user's account details and checks that it is not an OAuth-linked account (has a non-empty password hash). 8\. The system verifies that the current password matches the hashed password in the database. 9\. The system hashes the new password using BCrypt and updates the account's password hash field. 10\. The system saves the updated account details to the database. 11\. The system calls the logout endpoint to revoke tokens. 12\. The system deletes all local authentication cookies. 13\. The system redirects the User to the Login page with a success message. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-6 Passwords Do Not Match** 1\. The system detects that the new password and confirm new password fields do not match. 2\. The system displays the error message: "Password confirmation does not match." 3\. Back to step 2\. **EX-7 OAuth Account Modification Denied** 1\. The system detects that the account lacks a password hash (registered via Google OAuth). 2\. The system displays an error message indicating that password change is not supported for external accounts. 3\. Back to step 1\. **EX-8 Incorrect Current Password** 1\. The system detects that the entered current password does not match the stored hashed password. 2\. The system displays the error message: "Current password is incorrect." 3\. Back to step 2\. **EX-10 Database Update Failure** 1\. The system detects that saving the updated password hash failed (rows affected \<= 0). 2\. The system displays an error message. 3\. Back to step 2\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-228, BR-229 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User has stable internet connection. |  |  |

###### **2.2.3.28 Update Personal Info**

| UC ID and Name | UC-28 \- Update Personal Info |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User | Secondary Actors: | N/A |
| Trigger: | User click “Edit Profile” button and enters personal profile details and clicks the "Save Changes" button on the Edit Profile form. |  |  |
| Description: | Allows an authenticated User to update their personal details. The system processes validations, handles image upload, updates information within a transaction, and synchronizes frontend display cookies. |  |  |
| Preconditions: | **PRE-1.** User must be logged in with a valid session. |  |  |
| Post-conditions: | **POST-1.** The user profile information is updated in the database. **POST-2.** Local display cookies are synchronized, and the User is redirected to their Profile page with a success message. |  |  |
| Normal Flow: | **A. Update Personal Info Successfully** 1\. User enters personal info. 2\. User clicks the Save Changes button. 3\. The system validates the input formats. 4\. The system verifies that the User has a valid AccessToken cookie. 5\. The system extracts the user ID from the session. 6\. The system retrieves the User and Account records from the database. 7\. The system validates the email format (ends with "@gmail.com") and ensures it is not already taken (if modified). 8\. The system uploads the avatar image file (if provided) and retrieves the uploaded image URL. 9\. The system updates the User and Account entities and saves the changes in a database transaction. 10\. The system queries the updated user profile from the database. 11\. The system updates local display cookies (UserName, AvatarUrl, IsVerified) with the new details. 12\. The system redirects the User to the Profile view with a success message. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-5 No Access Token / Invalid Session** 1\. The system detects that the AccessToken cookie is missing or invalid. 2\. The system redirects the User to the Login page.  **EX-7.1 User Not Found** 1\. The system detects that the user profile does not exist. 2\. The system displays an error message. 3\. Back to step 1\.  **EX-7.2 Google Account Email Modification Blocked** 1\. The system detects that the user registered via Google OAuth and attempted to modify their email address. 2\. The system displays a block message indicating email changes are disabled for Google accounts. 3\. Back to step 1\.  **EX-7.3 Invalid Email Format** 1\. The system detects that the modified email does not end with "@gmail.com". 2\. The system displays an email format error message. 3\. Back to step 1\.  **EX-8 Avatar Upload Failure** 1\. The system detects that the avatar file upload failed. 2\. The system displays an image upload error message. 3\. Back to step 1\.  **EX-9 Email Already Taken** 1\. The system detects that the modified email is already in use by another account. 2\. The system rolls back the database transaction and displays an error message. 3\. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | BR-230, BR-231, BR-232  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User has stable internet connection. |  |  |

###### **2.2.3.29 View Profile**

| UC ID and Name | UC-29 \- View Profile |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc  | Date Created: | 11/6/2026 |
| Primary Actor: | User | Secondary Actors: | N/A |
| Trigger: | User accesses the profile page. |  |  |
| Description: | Allows an authenticated User to view their personal profile information, such as full name, username, email, phone number, bio, date of birth, and verification status. The system also synchronizes the client-side verification cookie during this request. |  |  |
| Preconditions: | **PRE-1.** User must be logged in with a valid session |  |  |
| Post-conditions: | **POST-1**. The user profile information is retrieved and displayed successfully. **POST-2**. The IsVerified cookie is synchronized with the latest account verification status. |  |  |
| Normal Flow: | **A. View Profile Successfully** 1\. User accesses the profile page. 2\. The system checks if the AccessToken cookie is present. 3\. The system parses the user ID from the session. 4\. The system queries the database to retrieve user details, including account navigation and instructor info. 5\. The system constructs the user profile response object. 6\. The system updates the local client IsVerified cookie with the actual status. 7\. The system renders and displays the user profile information. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-2 No Access Token / Invalid Session** 1\. The system detects that the AccessToken cookie is missing or invalid. 2\. The system redirects the User to the Login page.  **EX-4 User Not Found** 1\. The system detects that the user profile details do not exist in the database (retrieved entity is null). 2\. The system redirects the User to the Login page. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Very High |  |  |
| Business Rules: | BR-233  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | User has stable internet connection. |  |  |

