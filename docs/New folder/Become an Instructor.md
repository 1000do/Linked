###### **2.2.3.54 Become an Instructor**

| UC ID and Name | UC-54 \- Become an Instructor |  |  |
| ----: | :---- | ----: | :---- |
| Created By: | Pham Huu Hung | Date Created: | 16/05/2026 |
| Primary Actor: | User | Secondary Actors: | N/A |
| Trigger: | 1\. User clicks the "Become an Instructor" or "Apply to Teach" button in their profile settings or portal navigation. |  |  |
| Description: | Allows a registered Learner to submit an application (biography, skills, CV/ID documents) to join the platform's instructor team. Once submitted, the application awaits administrative review. |  |  |
| Preconditions: | PRE-01: Users must be successfully authenticated (logged in). PRE-02. Users must not already possess an approved or active Instructor account profile. |  |  |
| Post-conditions: | POST-1: The user's application is successfully submitted and set to "Pending Approval". POST-2: If approved, the user successfully links their bank information via Stripe Connect Express Onboarding (Stripe status changes to "Active"). POST-3: The user's role is upgraded to "instructor" in the system and cookies. |  |  |
| Normal Flow: | **A. Submit Instructor Application Successfully** 1.The user clicks the "Become an Instructor" navigation link. 2\. The system checks the active session and verifies that the user's email is verified. 3\. The system displays the instructor application form containing: Professional Title  Areas of Expertise  LinkedIn Profile  YouTube Channel Facebook Profile  Stripe Connect Country Document  Document Upload  4\. The user fills out the required professional details, selects their Stripe country, and uploads a valid document file. 5\. The user clicks the "Submit Application" button.. 6\. The system performs form validation checks (verifying required fields and checking that the uploaded documents meet the platform's size, format, and quantity constraints). 7\. If form data is valid, the system dispatches the payload to the backend, uploads the files to secure storage, and registers the application in the database with status set to Pending. 8\. The system displays a success message: "Your application has been submitted successfully\! Please wait for admin approval." and redirects the user to the Application Status page. |  |  |
| Alternative Flows: | **A.2 Email Not Verified** 1.The system detects that the user's email is not verified. 2\. The system blocks the application form, displays an "Email Not Verified" warning banner, and provides a "Verify Email Now" action button.. 3\. The user is redirected to the Email Verification page. 4\. The system updates the existing application log in the Database to \`Pending\` instead of creating a duplicate row. **A.4 Resubmitting a Rejected Application.** 1.The system detects a prior application marked as Rejected. 2.The system displays a warning banner showing the previous rejection feedback, pre-fills the form fields with the old inputs, and shows a link to the existing document file. 3.The user adjusts their details, uploads a new document file (optional if keeping the existing one), and clicks "Resubmit Application". 4.The system updates the existing application record in the database directly to Pending instead of creating a duplicate row. 5.Return to step A.8. |  |  |
| Exceptions: | **EX-01: Session Invalid (Not Logged In)** 1\. Display the message: "Please login to submit an application". 2\. Redirect to the login page. **EX-3 Form Validation Failed** 1\. The user leaves required fields empty, or uploads a document file with an invalid extension or a file size exceeding 5MB. 2\. The system halts submission, displays validation errors inline, and allows the user to re-enter". |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: |  BR-174, BR-175, BR-176,BR-177  |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | The file storage service and backend API validation rules are active and properly configured. |  |  |

###### **2.2.3.55  Setup payout**

| UC ID and Name | UC-55-Setup payout |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 16/05/2026 |
| Primary Actor: | User,Instructor | Secondary Actors: | Stripe |
| Trigger: | Instructors click the "Setup Payout" or "Payout Settings" button in their instructor dashboard workspace. |  |  |
| Description: | Allows approved instructors to link their financial profiles with Stripe Connect or enter manual banking credentials to receive earned course purchase commissions. |  |  |
| Preconditions: | PRE-01:. User must be successfully authenticated (logged in). PRE-02. The user's account must be approved as an Instructor by the system.PRE-03. The user's account must be approved as an Instructor by the system. |  |  |
| Post-conditions: | POST-01:The instructor's bank account is successfully linked, and the payout status is displayed as "Active" |  |  |
| Normal Flow: | **A. Link Payout with Stripe Connect Successfully** 1\. Instructor accesses the Application Status page. 2\. The system retrieves the instructor's application status from the database. 3\. The system displays the approved application status along with the "Setup Payout" button. 4\. Instructor clicks the "Setup Payout" button. 5\. The system redirects the instructor to the Stripe onboarding page. 6\. On the Stripe onboarding page, the instructor completes the verification and banking setup:     \- 6a. The instructor enters their contact details and inputs the OTP verification code.     \- 6b.Instructor enters basic personal details.     \- 6c. Instructor enters receiving bank details.     \- 6d instructor reviews details and clicks the "Agree and submit" button. 7\. Stripe redirects the instructor back to the system's platform. 8\. The system displays a success message indicating that Stripe setup is completed. 9\. The system updates the instructor's payout status to active in the database. 10\. The system displays the active payout status along with the "Access Stripe Dashboard" button |  |  |
| Alternative Flows: | **A.6  User Cancels Onboarding Mid-process** 1\. The user cancels the onboarding process on the Stripe page. 2\. The system redirects the instructor back to the platform. 3\. The system displays a warning message indicating that Stripe setup is not completed. 4\. The system displays the inactive payout status along with the "Setup Payout" button. 5\. Back to step 1\. |  |  |
| Exceptions: | **EX-01 Session Invalid (Not Logged In)** 1\. Display the message: "Please login to view payout settings". 2\. Redirect to the login page. **EX-4 Stripe Onboarding Connection Failed** 1\. The system detects a connection failure with the payment gateway. 2\. The system displays an error message regarding connection failure. 3\. Back to step 3\. |  |  |
| Priority: | High |  |  |
| Frequency of Use: | High |  |  |
| Business Rules: | BR-178 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | Instructor has stable internet connection. Stripe is working properly. |  |  |

