###### **2.2.3.1  Register**

| UC ID and Name | UC-01-Register |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 	11/6/2026 |
| Primary Actor: | Guest | Secondary Actors: | N/A |
| Trigger: | Guest accesses the registration page, enters details, and clicks the "Register" button. |  |  |
| Description: | Allows a Guest to create a new user account on the platform by providing an email, username, full name, password, and password confirmation. Upon successful creation, the account is set to active but unverified, and the Guest is prompted to verify their email. |  |  |
| Preconditions: | **PRE-1.** Guest must not be currently logged into the system. |  |  |
| Post-conditions: | **POST-1.** A new Account and User record are successfully saved to the database. **POST-2.** The system redirects the Guest to the Verification Recommendation screen. |  |  |
| Normal Flow: | **A. Register Account Successfully** 1\. Guest accesses the registration page. 2\. Guest enters registration details: Email, Username, Full Name, Password, and Confirm Password. 3\. Guest clicks the Register button. 4\. The system validates that all fields are filled, passwords match, and the email is a valid Gmail address. 5\. The system checks if the email is already in use in the database. 6\. The system checks if the username is already in use in the database. 7\. The system hashes the password using BCrypt. 8\. The system instantiates a new Account and User entity. 9\. The system saves the Account and User entities to the database. 10\. The system redirects the Guest to the Verification Recommendation page displaying their registered email. |  |  |
| Alternative Flows: | **A.4.1 Passwords Do Not Match** 1\. The system detects that the Password and Confirm Password fields do not match. 2\. The system displays a validation error message. 3\. Back to step 2\.  **A.4.2 Non-Gmail Email Address** 1\. The system detects that the email address does not end with "@gmail.com". 2\. The system displays an error message: "Only Gmail registration is supported." / "Email must be a @gmail.com address". 3\. Back to step 2\.  **A.10 Skip Verification** 1\. Guest clicks the Skip to Login link on the Verification Recommendation page. 2\. The system redirects the Guest to the Login page. 3\. Back to step 1\. |  |  |
| Exceptions: | **EX-6 Username Already Exists** 1\. The system detects that the username is already registered. 2\. The system displays an error message indicating the username is already in use. 3\. Back to step 2\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-222, BR-223, BR-224, BR-225 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Guest has stable internet connection. |  |  |

###### **.2.3.24 Login**

| UC ID and Name | UC-24 \- Login |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User, Instructor, Staff, Admin | Secondary Actors: | N/A |
| Trigger: | Actor enters Username/Email and Password, and clicks the "Sign In" button on the Login page. |  |  |
| Description: | Allows any registered actor (User, Instructor, Staff, Admin) to authenticate using local credentials. The system verifies the credentials, checks for suspension, creates authentication tokens, sets browser session cookies, and redirects the actor based on their role. |  |  |
| Preconditions: | **PRE-1.** Actor is on the Login page and not currently authenticated. |  |  |
| Post-conditions: | **POST-1.** Authentication tokens (AccessToken, RefreshToken) and session cookies are successfully appended to the actor's browser. **POST-2.** The system redirects the actor to the home page or admin dashboard depending on their role. |  |  |
| Normal Flow: | **A. Login Successfully** 1\. Actor enters their Username or Email and Password on the Login page. 2\. Actor clicks the Sign In button. 3\. The system validates the input format. 4\. The system queries the database to retrieve the account by username or email. 5\. The system verifies that the password matches the stored hashed password using BCrypt. 6\. The system checks if the account has an active lockout (suspension). 7\. The system updates the last login timestamp for the account in the database. 8\. The system detects the account's role (User, Instructor, Staff, or Admin). 9\. The system generates a JWT AccessToken and RefreshToken, and saves the RefreshToken to the database. 10\. The system appends HttpOnly cookies (AccessToken, RefreshToken) and client-readable cookies (UserName, AvatarUrl, UserRole, UserId, IsVerified) to the browser response. 11\. The system redirects the Actor based on their role (User or Instructor to Homepage; Admin or Staff to Admin dashboard). |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-5 Invalid Credentials** 1\. The system detects that the account does not exist or the password is incorrect. 2\. The system displays the error message: "Incorrect username/email or password." 3\. Back to step 1\.  **EX-6 Account Suspended** 1\. The system detects that the account is suspended (has an active lockout). 2\. The system displays a suspension error message indicating the lockout end date/time. 3\. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Very High |  |  |
| Business Rules: | BR-240, BR-241, BR-242, BR-243 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. |  |  |

###### **2.2.3.25 Login with Google**

| UC ID and Name | UC-25 \- Login with Google |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen N. Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User, Instructor | Secondary Actors: | Google Service |
| Trigger: | Actor clicks the "Sign in with Google" button on the Login page. |  |  |
| Description: | Allows a User or Instructor to log in using their Google credentials via OAuth 2.0. If the email is logging in for the first time, a new account is registered automatically with default user privileges, auto-verified, and no password hash. Suspended accounts are blocked. |  |  |
| Preconditions: | **PRE-1.** Actor is on the Login page and not currently authenticated. |  |  |
| Post-conditions: | **POST-1.** Authentication tokens and session cookies are set on the actor's browser. **POST-2.** If new, a verified Account and User record are saved to the database. **POST-3.** The system redirects the actor to the home page depending on their role. |  |  |
| Normal Flow: | **A. Login with Google Successfully** 1\. Actor clicks the “Sign in with Google” button. 2\. The system triggers Google Identity authentication and opens the OAuth popup. 3\. The Google Service returns the verified IdToken credential. 4\. The system validates the Google IdToken structure and signature. 5\. The system extracts the user profile: Email, Name, and Picture URL. 6\. The system queries the database to find the account by email. 7\. \[Existing User\] The system retrieves the account role. 8\. The system verifies that the account does not have an active lockout. 9\. The system generates a JWT AccessToken and RefreshToken. 10\. The system appends HttpOnly cookies (AccessToken, RefreshToken) and client-readable cookies (UserName, AvatarUrl, UserRole, UserId, IsVerified \= true). 11\. The system redirects the Actor based on their role. |  |  |
| Alternative Flows: | **A.6a First Time Login (New User)** 1\. The system detects that the email does not exist in the database. 2\. The system generates a unique username from the email prefix (appending numbers if the username is already taken). 3\. The system instantiates a new Account and User entity. 4\. The system saves the Account and User entities to the database. 5\. Back to step 7\. |  |  |
| Exceptions: | **EX-4 Token Validation Failed** 1\. The system detects that the IdToken is invalid, malformed, or has expired. 2\. The system displays an authentication error message. 3\. Back to step 1\.  **EX-8 Account Suspended** 1\. The system detects that the account is suspended (has an active lockout). 2\. The system throws an UnauthorizedAccessException and displays a suspension warning indicating the lockout expiry date/time. 3\. Back to step 1\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | Very High |  |  |
| Business Rules: | BR-238, BR-239, BR-240 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. Google Identity Service is working properly. |  |  |

###### 

###### 

###### **2.2.3.47  Forget Password**

| UC ID and Name | UC-47-Forget Password |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User, Instructor | Secondary Actors: | Google Service |
| Trigger: | Actor accesses the Forgot Password page, enters their email, and clicks the "Submit" button. |  |  |
| Description: | Allows an unauthenticated actor (User or Instructor) who has forgotten their password to perform a secure password reset. The system verifies the email, generates a 6-digit OTP code, sends it to the email, validates the entered OTP, and saves the new BCrypt-hashed password to the database |  |  |
| Preconditions: | **PRE-1**. Actor must not be currently authenticated in the system. |  |  |
| Post-conditions: | **POST-1.** A 6-digit OTP code is generated and emailed to the actor. **POST-2**. The account's password hash is successfully updated in the database and the OTP is consumed. **POST-3**. The system redirects the actor to the Login page with a success message. |  |  |
| Normal Flow: | **A. Reset Password Successfully** 1\. Actor accesses the Forgot Password page. 2\. Actor enters their registered Email address and clicks the Submit button. 3\. The system queries the database to find the account by email. 4\. The system validates that the account email exists, is verified, and is not linked to Google OAuth. 5\. The system generates a 6-digit OTP code, stores it in the database with a 2-minute expiration, and sends it via email. 6\. The system redirects the Actor to the Verify OTP page. 7\. Actor enters the OTP code received in email and clicks the Verify button. 8\. The system validates the OTP state in the database. 9\. The system redirects the Actor to the Reset Password page. 10\. Actor enters a new password and clicks the Reset button. 11\. The system consumes (validates and deletes) the OTP code record. 12\. The system hashes the new password using BCrypt and updates the password hash in the database. 13\. The system redirects the Actor to the Login page with a "Password changed successfully" message. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | **EX-4 Email Not Found** 1\. The system detects that the email does not exist in the database. 2\. The system displays the error message: "Email not found". 3\. Back to step 2\.  **EX-4.1 Google Account Password Reset Denied** 1\. The system detects that the account is linked to Google OAuth. 2\. The system displays the error message: "This account uses Google login". 3\. Back to step 2\.  **EX-4.2 Email Not Verified** 1\. The system detects that the account email has not been verified. 2\. The system displays the error message: "Email is not verified". 3\. Back to step 2\.  **EX-8 Incorrect or Expired OTP** 1\. The system detects that the entered OTP is incorrect or has expired. 2\. The system displays the error message: "Incorrect or expired OTP". 3\. Back to step 7\. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | Low |  |  |
| Business Rules: | BR-226, BR-228, BR-247 . |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. |  |  |

###### **2.2.3.53  Logout**

| UC ID and Name | UC-53-Logout |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Nguyen Nguyen Ngoc | Date Created: | 11/6/2026 |
| Primary Actor: | User, Instructor, Staff, Admin | Secondary Actors: | N/A  |
| Trigger: | Actor clicks the "Logout" button. |  |  |
| Description: | Allows an authenticated actor (User, Instructor, Staff, or Admin) to safely terminate their session. The system invalidates the refresh token in the database, removes all local browser session cookies, and redirects the actor to the homepage in an unauthenticated state. |  |  |
| Preconditions: | **PRE-1.** Actor must be currently logged in and authenticated. |  |  |
| Post-conditions: | **POST-1**. The actor's refresh token is revoked in the database. **POST-2**. All local browser authentication and session cookies are deleted. **POST-3**. The system redirects the actor to the homepage. |  |  |
| Normal Flow: | **A. Logout Successfully** 1\. Actor clicks the Logout button. 2\. The system sends a log out request to the frontend controller. 3\. The frontend controller sends a POST request to the backend log out API with the active AccessToken. 4\. The backend controller extracts the account ID from the token claims. 5\. The system revokes the active RefreshToken for the account in the database. 6\. The system clears backend API cookies. 7\. The system deletes all frontend cookies. 8\. The system redirects the Actor to the Homepage. 9\. The system displays the homepage in an unauthenticated state showing Login and Register buttons. |  |  |
| Alternative Flows: | N/A |  |  |
| Exceptions: | N/A |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-248  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Actor has stable internet connection. |  |  |

###### 