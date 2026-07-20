###### **2.2.3.89  View Profile (For Manager)** 

| UC ID and Name | UC-89 View Profile (For Manager) |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung  | Date Created: | 18/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A. |
| Trigger: | Manager accesses the profile page. |  |  |
| Description: | Allows managers (Super Admin or Staff Support) to view their profile details, including system account information, and update their account password through a secure modal. |  |  |
| Preconditions: | **PRE-1:** The manager must be logged in. |  |  |
| Post-conditions: | **POST-1**. The system displays the manager's profile, system account information, and status. |  |  |
| Normal Flow: | **A. View Manager Profile Successfully** A. View Profile Successfully Manager accesses the profile page. The system validates the active login session. The system requests the manager's profile data from the database. The system retrieves the manager's profile data. The system retrieves the account's active lockout history. The system displays the manager profile details and system information (profile picture, name, email, biography, phone number, creation date, and suspension details if active). |  |  |
| Alternative Flows: | N/A. |  |  |
| Exceptions: | **EX-01: Session Expired / Unauthorized Access** The system displays an authentication error message. The system redirects the user to the login page. **EX-3 Manager Profile Not Found** The system fails to retrieve the manager profile. The system displays an error message regarding profile retrieval failure. The system redirects the manager to the login page. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | Medium. |  |  |
| Business Rules: | BR-266,BR-267 |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

###### 

###### **2.2.3.90  Edit Profile (For Manager)**

| UC ID and Name | UC-90 Edit Profile (For Manager)  |  |  |
| ----: | :---- | :---- | :---- |
| Created By: | Pham Huu Hung | Date Created: | 18/05/2026 |
| Primary Actor: | Staff, Admin | Secondary Actors: | N/A. |
| Trigger: | Manager clicks the "Edit Profile" link on the profile page. |  |  |
| Description: | Allows managers (Super Admin or Staff Support) to edit their profile information, upload a profile picture, and save the updated details to the system.  |  |  |
| Preconditions: | **PRE-1:** The manager must be logged in. |  |  |
| Post-conditions: | **POST-1.** The manager's profile information is successfully updated in the database. **POST-2.** The manager's display cookies are updated to synchronize with the new data. **POST-3.** The manager is shown a success confirmation. |  |  |
| Normal Flow: | **A. Edit Manager Profile Successfully**   Manager clicks the "Edit Profile" link. The system retrieves and displays the manager's current profile details on the edit profile page. The manager updates the profile details in the form (optionally selecting a profile picture, and entering display name, full name, phone number, and biography).  The manager clicks the "Save Profile" button.   The system validates the input fields and displays a confirmation prompt.   The manager clicks the confirmation button on the prompt.   The system uploads the profile picture (if a new file was selected) and updates the profile details in the database. The system updates the cookies and layout dynamically, and displays a success confirmation message. |  |  |
| Alternative Flows: | **A.1 Cancel editing** The manager clicks the "Cancel" link or the "Back to Profile" link. The system redirects the manager back to the profile page. |  |  |
| Exceptions: | **EX-2 Session Invalid (Not Logged In)** The system displays an authentication error message.  The system redirects the user to the login page. **EX-3 Manager Profile Not Found** The system fails to retrieve the manager profile. The system displays an error message regarding profile retrieval failure.   The system redirects the manager to the login page**. EX-7 File Upload Service offline**   The system fails to upload the profile picture.   The system displays an error message regarding file upload failure.   Back to step 3\. |  |  |
| Priority: | High  |  |  |
| Frequency of Use: | Medium |  |  |
| Business Rules: | N/A |  |  |
| Other Information: | N/A |  |  |
| Assumptions: | N/A |  |  |

