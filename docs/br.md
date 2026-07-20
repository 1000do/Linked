## **3.Business Rule**

| ID | Rule Definition |
| ----- | ----- |
| BR-1 | **Universal Form Data Validation:** All submitted form fields must pass strict data type, format, length, and value constraints (e.g., report descriptions must be at least 20 characters, and confidence thresholds must be decimals between 0.01 and 1.0) before the system processes the submission. |
| BR-2 | **Unique Reasons:** The user cannot select the same report reason more than once in a single submission. |
| BR-3 | **Reason Selection:** A valid violation reason (Spam, Offensive, Fake, Other) must be selected before submission. |
| BR-4 | **Duplicate Report Prevention:** A user cannot report the same review/course with the same reason if their previous report is still pending. |
| BR-5 | **Stripe Link Limits:** Instructors without an active Stripe Connect profile may publish up to 2 courses with a maximum of 30 minutes of video content. |
| BR-6 | **Free Course Video Ceiling:** Courses priced at 0 are capped at 60 minutes of video content. Paid courses require an active Stripe Connect configuration. |
| BR-7 | **Content Re-activation:** On resubmission, the system automatically resets any previously rejected lesson and material statuses back to active for admin review. |
| BR-8 | **Locked Out Instructor Rights:** Instructors with an active lockout status are prohibited from submitting courses for review or changing course status. |
| BR-9 | **Default View â€” Course Reports:** The Report Center always opens with the Course Reports tab and the "Pending" status filter selected by default. |
| BR-10 | **Access Control â€” Report Center:** Only authenticated Staff and Admin actors are permitted to access the Report Center. |
| BR-11 | **Status Badges:** Report statuses (Pending, Resolved, Rejected, Escalated) are visually distinguished using specific status badges. |
| BR-12 | **Action Availability:** The "Moderate" action button is available only for reports in Pending, Under Review, or Escalated status. All other statuses display an "Info" button instead. |
| BR-13 | **Staff Escalation Limitation:** Staff actors cannot moderate Escalated reports. Only Admins may moderate reports in the Escalated status. |
| BR-14 | **Default View â€” Course Reviews Subtab:** When the Review Reports tab is accessed, the system defaults to the "Course Reviews" subtab with the "Pending" status filter applied. |
| BR-15 | **Adaptive Moderation/Detail Popup UI:** The moderation and detail popups dynamically adjust titles, fields, labels, and action terminologies (e.g., "Flag Course" vs "Remove content") depending on whether a course or a review is being viewed/moderated. |
| BR-16 | **Resolution Visibility:** Resolution details (Moderator, Resolved At, Resolution Note) are visible only for reports with a resolved or rejected status. |
| BR-17 | **Independent Status Filters:** The status filter in the Course Reports tab maintains its own independent state from the status filter in the Review Reports tab. |
| BR-18 | **Subtab Filter Retention:** When switching between the "Course Reviews" and "Lesson Reviews" subtabs, the currently active status filter is retained and applied to the newly selected subtype. |
| BR-19 | **Universal Empty State Display:** Whenever a list retrieval operation yields no records (either due to an empty database or active filter criteria), the system displays a user-friendly empty state message instead of a blank table or list container. |
| BR-20 | **Escalation Restriction:** Staff may select the "Escalate to Senior Admin" option. Admins do not see this option, as they are the highest escalation tier. |
| BR-21 | **Action Limitations:** The "Flag Course / Remove content" checkbox is enabled exclusively when the Resolution Status is set to "Accept Report". Changing the status automatically clears and disables the checkbox. |
| BR-22 | **Course Strike System:** When a course report is accepted with the "Flag Course" toggle checked: Strike 1â€“2 sends a warning to the instructor; Strike 3 permanently archives the course and applies a 30-day severe instructor lockout. |
| BR-23 | **Review Penalty System:** When a review report is accepted with "Remove content" checked: Strike 1 sends a warning; Strike 2 applies a 7-day moderate review lockout; Strike 3 applies a 30-day severe full account ban and terminates active sessions. |
| BR-24 | **Implicit Refresh:** A successful moderation action automatically triggers a silent refresh of the current report list and overall statistics. |
| BR-25 | **Admin Authorization for AI Services:** Only authenticated Admin actors are authorized to access and perform actions within the AI Service and Model Management module (including adding/updating models, toggling status, viewing details/list, configuring thresholds, and viewing activity logs). |
| BR-26 | **List Sorting and Ordering:** Administrative lists (such as the AI model list and activity logs) are sorted in descending order of their latest timestamp (last modified date for models, creation date for logs), ensuring that the most recent entries are displayed first. |
| BR-27 | **Pre-populated Data:** Model details displayed in the popup are sourced from data embedded in the page during the initial render; no additional server request is made on click. |
| BR-28 | **Universal List Pagination:** All list retrieval operations displaying tabular or grid data must display a limited number of entries per page with pagination controls. |
| BR-29 | **Default Subtab â€” Activity Log:** The course moderation category is displayed by default when the logs tab is opened. |
| BR-30 | **Status Default:** The model status defaults to active upon creation. |
| BR-31 | **Status Toggle:** Toggling an active model sets its status to inactive; toggling an inactive model restores it to active. |
| BR-32 | **Read Status Sync:** Viewing a notification's details modal automatically marks it as read in the database and updates the UI state. |
| BR-33 | **Ownership Constraint:** A user or instructor can only mark their own received notifications as read. |
| BR-34 | **Confirmation Prompt:** Deleting a notification requires confirmation via a dialog to prevent accidental deletion. |
| BR-35 | **Cart in list:** Cart item prices must reflect the current database prices including any active platform discounts. |
| BR-36 | **Default View:** By default, all active conversations are retrieved and displayed in "Recently Active" sorting order (newest last message at the top). |
| BR-37 | **Online Status Indicator:** A green dot is displayed on the partner's avatar if the partner is currently connected to the chat hub. |
| BR-38 | **Sender Peer Constraint:** Staff and Admin users cannot send notifications to themselves. Staff members are blocked from sending notifications to other Staff or Admin users. |
| BR-39 | **Recipient Selection Required:** If the target group is "Specific User", at least one valid email tag must be specified |
| BR-40 | **Auto Read State Synchronization:** If a Staff user views a notification specifically addressed to them that is unread, the system must trigger a status transition to "Read". Admins or other Staff users viewing the notification do not trigger this transition. |
| BR-41 | **Verified Email Constraint:** Only Actors with a verified email address are allowed to add items to their cart. |
| BR-42 | **Purchase and Ownership Restrictions:** An Actor cannot add their own courses (where they are the instructor) or already |
| BR-43 | **Item Uniqueness:** A course can only be added to an Actor's cart once. |
| BR-44 | **Price Recalculation:** Deleting an item must immediately trigger a recalculation of the cart's subtotal, discount, and grand total, and update any applied coupons. |
| BR-45 | **Personal Cleared History:** Clearing chat history only sets the cleared timestamp for the active participant. The conversation history remains intact and visible to other participants. |
| BR-46 | **Real-Time Synchronization:** Typing state, online status, and message transmission are updated instantly via SignalR callbacks. |
| BR-47 | **Role promotion:** Approving an application triggers a role promotion for the target user from "User" to "Instructor". |
| BR-48 | **Rejection Reason Required:** A non-empty text justification is mandatory before an instructor application can be rejected. |
| BR-49 | **Public Visibility:** The course list displays only courses that are currently published. Draft, pending, rejected, archived, or locked courses are excluded from the public list. |
| BR-50 | **Wishlist Sync:** Wishlist heart icons reflect the current wishlist status of the logged-in User in real-time, displaying a filled red heart for wishlisted courses and an unfilled icon otherwise. |
| BR-51 | **Cart Status Integration:** If a logged-in User has already purchased/enrolled in a course, the "Add to cart" button is replaced by a checkmark icon to prevent double purchases. |
| BR-52 | **Debounced Live Search:** Real-time keyword filtering is debounced by 350ms to minimize API calls during continuous typing. |
| BR-53 | **View Mode Retention:** User layout preference (Grid vs. List view) is stored in the browser's local storage and persists across sessions. |
| BR-54 | **State Persistence:** Active filter parameters are stored in the browser's local storage and are automatically applied when the user returns to the page without URL parameters. |
| BR-55 | **Curriculum Locking:** Only the first material of the first lesson of the first section is available for preview to non-enrolled actors. All subsequent materials are visually locked (showing a lock icon) and cannot be accessed from the details page. |
| BR-56 | **Unpublished Course Access Restrictions:** Courses with statuses other than "published" (e.g., draft, pending, archived) are strictly hidden from the public and can only be viewed by the original instructor, system administrators, or students who were previously enrolled. |
| BR-57 | **Review Visibility:** Course reviews and rating statistics are publicly visible to all actors. |
| BR-58 | **Wishlist State:** The wishlist toggle option is only displayed to Users. Instructors viewing their own course and system administrators cannot add courses to their wishlist. |
| BR-59 | **Course Reporting:** The option to report a course is only visible to enrolled students. It is hidden from the course owner, administrators, and for courses that are pending or already permanently archived with 3 or more flags. |
| BR-60 | **Review Eligibility:** The ability to write and submit a course review is strictly limited to authenticated Users who are enrolled in the course and have completed 100% of the lessons. |
| BR-61 | **Debounced Query Execution:** Real-time keyword filtering is debounced by 350ms to minimize API calls during continuous typing. |
| BR-62 | **Keyword Match Scope:** Search query checks against course titles, instructor names, short descriptions, and categories. |
| BR-63 | **Published Status Only:** Only courses with "published" status are returned in the search results. |
| BR-64 | **Filter Combinability:** Multiple filters (category, price, rating, keyword) can be applied simultaneously, narrowing the results conjunctively (AND operation). |
| BR-65 | **Rating Filter Bounds:** Rating filters select courses with average rating greater than or equal to the selected threshold (e.g., \>= 4.5, \>= 4.0, \>= 3.5). |
| BR-66 | **Price Filter Bounds:** Price filter options classify courses as "Free" (Price \== 0\) or "Paid" (Price \> 0). |
| BR-67 | **Newest Ordering:** Courses are sorted by their creation date in descending order. |
| BR-68 | **Highest Rated Ordering:** Courses are sorted by their average rating in descending order. |
| BR-69 | **Price Low-to-High Ordering:** Courses are sorted by their price in ascending order. |
| BR-70 | **Price High-to-Low Ordering:** Courses are sorted by their price in descending order. |
| BR-71 | **Sort Order Persistence:** The selected sorting parameter is saved in the browser's local storage and persists across page reloads. |
| BR-72 | **Course Visibility:** Only courses with status "Published" are displayed on the public profile. Draft, pending, rejected, or archived courses are strictly excluded. |
| BR-73 | **Profile Access:** A public profile is only accessible if the instructor's account application status is "Approved". |
| BR-74 | **Social Links:** Social media links ("LinkedIn", "YouTube", "Facebook") are only rendered as clickable buttons/links if the instructor has provided the respective URLs. |
| BR-75 | **Anonymous Access:** Anyone (including Guests) can view public course reviews and statistics. |
| BR-76 | **Review Masking (Removed Reviews):** If a review has been removed by the moderator (status "violating"), the content is replaced with a warning notification: "This review was removed by a moderator for violating community standards." and the rating stars are set to 0\. If a review has been self-deleted by the user, it is omitted from the course details list. |
| BR-77 | **Paging Size:** The reviews list on the details page is paginated with a limit of 5 reviews per page. |
| BR-78 | **General vs Lesson Badges:** General course-level reviews are flagged with a "General review" badge, while lesson-level reviews (originally submitted in the player) are flagged with the corresponding lesson title. |
| BR-79 | **Progress Calculation:** Course progress percentage is calculated as (Completed Materials / Total Materials) \* 100, rounded to 1 decimal place. |
| BR-80 | **Video Completion Trigger:** A video material is marked as completed only when the video playback reaches the end ("onended" event). |
| BR-81 | **Document/Text Completion Trigger:** A text document (".txt") is marked as completed immediately upon viewing. Other downloadable resources are marked as completed when the User clicks the download button. |
| BR-82 | **Course Completion State:** When all learning materials in a course are marked as completed, the system automatically sets the enrollment status IsCompleted \= true and updates the completion date in the database. |
| BR-83 | **Instructor Bypass:** Course owners/instructors can access the Course Player ("Learn" page) for preview, and their progress is treated as 100% completed virtual progress without writing completions to the database. |
| BR-84 | **Dynamic Button Update:** For free courses, when an un-enrolled User clicks "Enroll Now", the page successfully processes the enrollment and dynamically updates the button to "Go to learning" without a full page reload. |
| BR-85 | **Unpublished Course Warning:** If the course is not in the published state, a warning banner is displayed at the top of the course player, but access remains permitted for owners and managers. |
| BR-86 | **Protected Media:** Video lectures are served with playback controls only; direct download option is disabled (utilizing "controlsList" attribute set to "nodownload"). |
| BR-87 | **Completed Badge Display:** A completed course card displays a distinct completed overlay badge with the text "Completed" and a "check\_circle" icon. |
| BR-88 | **Progress Bar Color Coding:** The progress bar is colored emerald green if the course is 100% completed, and the primary color (teal) otherwise. |
| BR-89 | **Dynamic Actions based on Completion:** The action button text is set to "Review" with a "replay" icon for completed courses, and "Continue learning" with a "play\_arrow" icon for incomplete courses. |
| BR-90 | **No Self-Enrollment:** Instructors are blocked from enrolling in their own courses (both free and paid). They bypass enrollment checks when accessing the player for their own courses. |
| BR-91 | **Double Purchase Prevention:** The system blocks purchase/checkout of courses that the user is already enrolled in, prompting them to remove it from their cart. |
| BR-92 | **Stripe Integration:** Paid course enrollment is completed asynchronously or upon return redirect validation after Stripe gateway processes the successful payment. |
| BR-93 | **Completion Check for Course Review:** Non-owner students must have 100% course progress (completed all materials) to write a review on the Course Details page. |
| BR-94 | **Minimal Progress for Lesson Review:** Non-owner students must have completed at least 1 lesson/material to write a review on the Course Player page. |
| BR-95 | **Rating Restrictions:** Star ratings must be between 1 and 5 stars (inclusive). |
| BR-96 | **Account Restrictions (Lockout):** Users with active comment/review restrictions are blocked from submitting new reviews. The system checks this validation at submission time. |
| BR-97 | **Instructor Notification:** Upon successful submission, a notification is sent to the course instructor with a link pointing directly to the review card. |
| BR-98 | **Self-Ownership Only:** An actor can only delete reviews they have authored. |
| BR-99 | **Moderation Block:** If a review has an active pending moderation report, deletion is blocked until the moderation report is resolved. |
| BR-100 | **Soft Delete:** Deletion is implemented as a soft-delete (setting \`IsRemoved \= true\` and updating status to "removed" in the database). It does not permanently delete the record. |
| BR-101 | **Rating Statistics Recalculation:** Upon successful deletion, the overall rating average and counts are dynamically recalculated to exclude the deleted review. |
| BR-102 | **Title Length Constraints:** The course title must be between 5 and 100 characters in length. |
| BR-103 | **Text Constraints:** What You Will Learn and Requirements descriptions must each be at least 20 characters in length (excluding HTML tags). |
| BR-104 | **Stripe Payout Connectivity:** Instructors must connect and activate their Stripe accounts to set premium prices. If Stripe is disconnected or inactive, the course price is locked to 0.00 (Free). |
| BR-105 | **Voucher Validation:** Platform sponsored discount vouchers cannot be applied to free courses. |
| BR-106 | **Draft Initial State:** All newly created courses are saved with a status of "Draft" and must go through moderation before being published. |
| BR-107 | **Status Transition Restrictions:** An instructor can only set a course to status 'pending' (moderation), 'archived' (hidden), or 'published' (only when unarchiving from status 'archived'). |
| BR-108 | **Permanently Discontinued Block:** A course that is 'archived' and has 3 or more flags is considered permanently discontinued due to policy violations and cannot have its status changed or be edited. |
| BR-109 | **Active Enrollment Preservation:** Archiving a course does not delete it or impact active student enrollments; active students enrolled in the course continue to have full access to its contents on their dashboards. |
| BR-110 | **Auto-Purge Policy:** Learning materials placed in the trash bin are automatically and permanently deleted after 30 days by a background cleanup service. |
| BR-111 | **Cloud Storage Virtual Directory:** Moving a file to the trash bin renames the file prefix on Cloudinary to \`trash/\` and updates its status to "removed" in the database, hiding it from the public display while retaining the database record. |
| BR-112 | **Video Swap Constraint:** Restoring a video material to a lesson that already contains an active video will automatically move the current active video to the trash bin, replacing it with the restored video. |
| BR-113 | **Discontinued & Pending Locking:** Learning materials cannot be restored or permanently deleted if the parent course is pending review or is permanently discontinued due to policy violations. |
| BR-114 | **Unpublish Trigger:** Restoring a learning material to a published course will automatically update the course status back to "Draft" to force re-moderation of the updated curriculum. |
| BR-115 | **Draft Demotion Rule:** If the parent course is currently "Published", restoring any curriculum material will automatically update the course status to "Draft" and clear the moderation feedback, requiring the instructor to re-submit it for review. |
| BR-116 | **Rights & Mod Lockouts:** Restoration is locked if the instructor is under active suspension/lockout, or if the parent course is pending review, or if the parent course is permanently discontinued due to 3 or more flags. |
| BR-117 | **Cloud Storage Synchronization:** Restoring a file dynamically renames it in Cloudinary to remove the \`trash/\` prefix, syncing the physical path with the database status. |
| BR-118 | **Irreversible Deletion:** Permanent deletion removes both the database record and the asset hosted on cloud storage (Cloudinary) and cannot be recovered or restored. |
| BR-119 | **Cloud Storage Prefix:** The file is deleted from the virtual trash path (prefix \`trash/\`) in cloud storage using its \`CloudPublicId\`. |
| BR-120 | **Exclusion of Drafts:** Courses with status "Draft" are never retrieved or displayed in the moderation queues. |
| BR-121 | **Urgency Level Calculation:** For pending courses, the urgency color is calculated based on creation date: Red / "High" urgency: pending \> 72 hours. Amber / "Medium" urgency: pending \> 24 hours. Slate / "Normal" urgency: pending \<= 24 hours. |
| BR-122 | **Threat Levels:** Threat levels correspond to AI check feedback: Critical Threat (FlaggedOrRejected): course hits critical policy violations. High Threat (ManualAudit): course needs manual audit. Clean (Approved): course is approved by automated check. Pending / None: default. |
| BR-123 | **Infraction Flags:** A course tracks up to 3 flags. If flag count reaches 3, status is changed to "Archived" and course status maps to "Permanently Locked". |
| BR-124 | **Search Criteria:** Search keywords are matched (case-insensitively, substring match) against: Course Title Instructor Name (Full Name) |
| BR-125 | **Preservation of Filters:** Searching preserves all other active filters (Category, Status, Sort Option). |
| BR-126 | **Debounce Time:** To prevent excessive server requests, the search execution is debounced by 300 milliseconds from the last keystroke. |
| BR-127 | **Sorting Options:** "Threat Level (High to Low)" ("threat\_desc"): Sorts descending by threat level (Critical Threat \> High Threat \> Clean \> None), then ascending by creation date. This is the default sort. "Threat Level (Low to High)" ("threat\_asc"): Sorts ascending by threat level, then ascending by creation date. "Oldest (Priority)" ("oldest"): Sorts ascending by creation/submission date (oldest first). "Newest" ("newest"): Sorts descending by creation/submission date (newest first). |
| BR-128 | **Category Filter:** Matches the Category ID or Name against the course's category. If "All Categories" is selected, no category filtering is applied. |
| BR-129 | **Preservation of Other Criteria:** Filtering preserves active search keywords and sorting options. |
| BR-130 | **Re-approval Rule:** If the course status is rejected, archived, or permanently locked, the action button is labeled "Re-approve" but follows the same approval logic. |
| BR-131 | **Notification Content:** The approval notification sent to the instructor includes the optional feedback entered by the actor. |
| BR-132 | **Cache Eviction:** The Redis cache for the course details must be evicted immediately upon approval to ensure users see the updated content. |
| BR-133 | **Violation Element Tagging:** Course-level targets correspond to: title ("course.title"), description ("course.description"), thumbnail ("course.thumbnail"), what you will learn ("course.what\_you\_will\_learn"), requirements ("course.requirements"). Lesson-level targets correspond to "lesson.title". Material-level targets correspond to "file". |
| BR-134 | **Guest Login Prompt:** System prompts Guest users to log in when clicking on interactive elements on the homepage. |
| BR-135 | **Combined Feedback:** The course's "ModerationFeedback" is updated by combining all tagged element reasons (e.g. "\[@Title\] Reason" or "\[Lesson: Title\] Reason" separated by newlines). |
| BR-136 | **Notification Link:** The rejection notification sent to the instructor contains a link redirecting to the course editor "/InstructorCourse/Editor?id={courseId}". |
| BR-137 | **Flag Count Limit:** A course can have a maximum of 3 flags. Flag count cannot exceed 3\. |
| BR-138 | **Strike Warnings: Strike 1 (1st Flag):** Sends notification "Course Violation Reminder (1st Time)". The course is temporarily hidden. Instructor is reminded to review and comply. **Strike 2 (2nd Flag):** Sends notification "Severe Violation Warning (2nd Time)". Course remains hidden. Warning that 3rd flag leads to permanent lock. **Strike 3 (3rd Flag):** Sends notice "Permanent Course Discontinuation Notice (3rd Time)". Status changes to "archived" (Permanently Locked). Instructor is blocked from editing, and no new enrollments are allowed. |
| BR-139 | **Debounce Optimization:** Auto-saving uses a 1.5-second debounce delay to prevent database spamming during rapid typing.  |
| BR-140 | **Draft Reversion:** To ensure content integrity, any edits made to an already "published" course will automatically degrade its status back to "draft", requiring a new moderation review submission and clearing previous moderation feedback.  |
| BR-141 | **Content Integrity Hashing:** Textual details and the thumbnail file are hashed using MD5 and stored as a unique fingerprint to prevent duplication and support anti-plagiarism tracking.  |
| BR-142 | **Lesson Limit for Unlinked Stripe Accounts:** Instructors who have not linked an active Stripe Connect profile are only allowed to create up to 5 lessons per course. |
| BR-143 | **Lesson Title Uniqueness:** Lesson titles must be unique within a course (case-insensitive). |
| BR-144 | **Pending Review Modification Lock:** Learning materials and lessons cannot be added, edited, or removed if the course is currently in 'Pending' review status. |
| BR-145 | **Syllabus Change Reversion:** Adding or removing lessons and materials, or editing their content, in a 'Published' course automatically changes the course status back to 'Draft' and clears any existing moderation feedback. |
| BR-146 | **Lesson Soft Deletion:** Deleting a lesson soft-deletes the lesson record and all its associated materials (IsRemoved \= true) in the database, and moves physical cloud files to the trash. |
| BR-147 | **Supplementary Document Limits:** Instructors who have not linked an active Stripe Connect profile are only allowed to attach up to 2 supplementary documents per lesson. |
| BR-148 | **Material Soft Deletion:** Removing a material does not permanently delete the file; the physical file is moved to the Cloud Storage Service's trash folder and its database status is updated to 'Removed'. |
| BR-149 | **Active Student Deletion Restriction:** Courses with active student enrollments cannot be deleted and must be archived instead. |
| BR-150 | **Course Soft Deletion:** Deleting a course soft-deletes both the course and all its associated lessons and materials (IsRemoved \= true) in the database, rather than permanently destroying the records. |
| BR-151 | **Instructor Dashboard Access Restriction:** Only fully approved instructors are allowed to access the Instructor Dashboard; other states trigger redirects to application status or creation workflows. |
| BR-152 | **Course Status Badging:** The system visually distinguishes courses on the instructor dashboard using specific badges based on their current status (e.g., Published, Pending Review, Draft, Rejected, Archived, Removed, Permanently Locked). |
| BR-153 | **Contextual Dashboard Actions:** Course management actions on the instructor dashboard are determined dynamically based on course status; permanently locked or removed courses have severely restricted action sets. |
| BR-154 | **Instructor Data Isolation: Instructors** can only view statistics, revenue, and courses associated with their own account. |
| BR-155 | **Enrollment Pre-check:** The system  checks recipient enrollment as the user types in the email field (debounced). The "Proceed to Payment" button is disabled until the check confirms the recipient is not enrolled. |
| BR-156 | **Gift Price Restrictions:** Gift purchases are always made at the course's original price. Promotional coupons and discount codes are not applicable to gift purchases. |
| BR-157 | **Claim and Refund Mutual Exclusion:** Once a gift is successfully claimed by the recipient, any pending refund request for that gift transaction is automatically rejected. |
| BR-158 | **No Cart Modification:** Gifting is treated as a direct purchase and does not clear or modify the items in the user's active shopping cart. |
| BR-159 | **No Double Enrollment:** A user cannot claim a gift course if they already possess an active enrollment in that course |
| BR-160 | **Unclaimed Refund Restriction:** Gifts can only be claimed if they have not been refunded. A refund reverses the redemption token validity. |
| BR-161 | **14-Day Escrow Policy:** Refund requests are only permitted for successful transactions (Success status) within a 14-day escrow window from the purchase date. Once this window expires, the refund request button is disabled/hidden. |
| BR-162 | **Escrow Hold:** Instructor accounts are not credited with the course revenue until the 14-day refund window expires, ensuring funds are available for refund payouts. |
| BR-163 |  **Pagination Limit:** Transaction history results are paginated with a default of 10 items per page |
| BR-164 | **Enhanced Search Component** : The search input field must retain the entered keyword after the page reloads so the user knows which filter is currently active |
| BR-165 | **Course Title Matching:**The search is performed as a case-insensitive substring match on the Course Title |
| BR-166 | **Auto-Rejection \- Account Flag:** A refund request is automatically rejected if the user account currently has 3 or more warning flags (warning count \>= 3). |
| BR-167 | **Auto-Rejection \- Refund Frequency** A refund request is automatically rejected if the user has requested 3 or more refunds within the last 14 days.  |
| BR-168 | **Auto-Rejection \- Repeated Course Refund** A refund request is automatically rejected if the user has previously requested and received a successful refund for the exact same course.  |
| BR-169 | **Auto-Rejection \- Progress for Short Courses (\< 4h)** For courses with a total duration of less than 4 hours, a refund request is automatically rejected if the student's learning progress exceeds 15%. |
| BR-170 | **BR-05:** Auto-Rejection \- Progress for Long Courses (\>= 4h) For courses with a total duration of 4 hours or more, a refund request is automatically rejected if the student's completed video watch time exceeds 1 hour.  |
| BR-171 |  **Abstract Reason Confidentiality** All auto-rejection notifications must strictly display abstract descriptions for the reject reason. It is forbidden to expose specific numeric criteria (such as exact percentage values, exact watch hours, number of flags, or refund counts) to the user. The abstract descriptions displayed are: \- Account flags limit: "your account having multiple flags" \- Refund frequency limit: "having requested too many refunds within the refund period" \- Repeated refund: "previous refund history for this course" \- Progress limit for short course: "learning progress exceeding the limit for short courses" \- Watch time limit for long course: "video watch time exceeding the limit allowed for long courses" |
| BR-172 | **Refund Eligibility Window** A refund request must be submitted within 14 days (336 hours) from the original transaction timestamp.  |
| BR-173 | **Negative Refund Balance** :Display If the transaction status is marked as refunded, must display the Gross Amount and Total as negative values to represent the credited refund. |
| BR-174 |  **Strict Application Idempotency:**  A learner account can only have one active application record. If a previous request was rejected, any subsequent submission will overwrite the old record inline to keep the database clean and prevent duplicates. |
| BR-175 | **File Attachment Restrictions**   \- The user can upload a maximum of 3 document/certificate files.   \- Uploaded credential documents are strictly restricted to \`.pdf\`, \`.jpg\`, \`.jpeg\`, and \`.png\` extensions.   \- The maximum file upload size is 5MB per file to protect server resources. |
| BR-176 | **Email Verification Requirement**   Learners must have their email verified before they are permitted to submit or access the instructor application form. |
| BR-177 | **Instructor Role Activation :**The user's role is upgraded to "instructor" in cookies and database sessions only after Stripe Connect onboarding is verified as "Active" |
| BR-178 |  **Direct Edit Lockout :**Once the bank account is successfully linked and status is "Active", the system hides the "Setup Payout" button. Instructors are locked out from editing bank details directly on the platform and must click the "Access Stripe Dashboard" button to edit directly on Stripe's secure portal. |
| BR-179 | **Dynamic Balances Calculation:**All balance cards must dynamically recalculate based on the selected month and year filter. |
| BR-180 |  **Default Currency Display :**All balances, amounts, and chart labels default to USD (prefixed with the $ symbol). |
| BR-181 | **Financial Card Definitions & Formulas The financial metrics are defined and calculated as follows: \-** Gross Revenue: Total sales revenue generated from course purchases. \- Total Refunded: Total deductions from learner refund requests (prefixed with a red minus \- symbol). \- Net Earnings: Actual revenue remaining after subtracting refunded amounts (Net Earnings \= Gross Revenue \- Total Refunded). \- Pending Transfer: Funds currently locked in the 14-day escrow protection window or in transit. \- Stripe Wallet: Funds successfully moved to the instructor's Connected Stripe Wallet (Ready for Bank Transfer). \- Withdrawn: Total funds successfully transferred to the instructor's bank account. |
| BR-182 | **Negative Payout Display for Refunded Status:** If a transaction status is "refunded", the system must display as a negative value prefixed with a minus \`-\` sign in red (e.g., \`- $15.29\`). |
| BR-183 | **Transaction Access Control :**Instructors can only view details of transactions related to courses they own. Attempts to access other instructors' transaction details will be blocked and redirected. |
| BR-184 |  **Expected Payout Date Calculation :**The expected payout date (\`PayoutDate\` field in the database) is automatically calculated at the time of purchase as the configured payout day of the next month (e.g., the 15th of the next month) relative to the transaction date. This reflects the platform's monthly billing cycle and the 14-day escrow window. |
| BR-185 | **Sequential Numbering (No.)**The first column in the payout history table must display sequential numbers (No., e.g., \`\#1\`, \`\#2\`, \`\#3\`) instead of the escrow clearance date, avoiding confusion for the use |
| BR-186 | **Preservation of Filters:** Selecting a sorting option and submitting the query must preserve all other active search keywords, status filters, and calendar month/year period selections. |
| BR-187 |  **Default Sort Order:**If no sorting option is explicitly selected, the list defaults to "Date: Newest" order. |
| BR-188 |  **Default Period:**  If no month and year are specified, the system defaults to the current UTC calendar month and year. |
| BR-189 | **Pagination Reset:**  Modifying any filter criteria and applying it resets the transaction pagination back to page 1\. |
| BR-190 |   **Default Sorting**: The payout list is sorted by date newest to oldest by default if no sorting option is explicitly selected. |
| BR-191 | **Locked Currency and Split Rates :**All split rates (Transfer Rate) and conversion rates are locked at the time of purchase to guarantee stable revenues and prevent exchange rate fluctuations |
| BR-192 | **Stripe Fee Calculation :**Stripe processing fee is calculated exactly as GrossAmount \* 0.029 \+ 0.30 (capped at the gross amount to avoid negative ledger balances). |
| BR-193 | **Split Summary Ledger Formulas** The Split Summary Card calculates and displays values according to the following formulas: \- Gross Amount \= Original Course Price \- Coupon Discount (if coupon is applied). \- Net Course Revenue to Split \= Gross Amount \- Stripe Processing Fee. \- Platform Net Profit \= Net Course Revenue to Split \* (100% \- Transfer Rate %). \- Instructor Net Income \= Net Course Revenue to Split \* Transfer Rate %. |
| BR-194 | **Instructor Status Constraint :**Only active instructors whose registration application has been reviewed and approved by the platform administrator can access the course revenue dashboard. |
| BR-195 | **Period-Based Revenue Calculations:**Monthly, yearly, and lifetime revenues are dynamically calculated and loaded based on the active month and year filter selected. |
| BR-196 | **Month-over-Month Growth Calculation :**Growth percentage is calculated using the formula: Growth % \= ((Current Month Revenue \- Previous Month Revenue) / Previous Month Revenue) \* 100\. \- If the previous month's revenue is zero and the current month's revenue is greater than zero, it is labeled and displayed as "New Sales".   |
| BR-197 | **Course Revenue Calculations :** \- Gross Revenue \= Total sales revenue generated from all course purchases on the platform. \- Total Refunded \= Total deductions from refund requests. \- Net Earnings \= Gross Revenue \- Total Refunded. |
| BR-198 |  **Splits and Distributions Calculations:** \- Paid to Instructors \= Total funds paid out/transferred to instructors' Stripe accounts or banks. \- Escrow Funds \= Funds currently locked in the 14-day escrow protection window. \- Platform Net Profit \= Net platform share cut after splits (Net Course Revenue to Split \* (100% \- Instructor Share %)). |
| BR-199 | **Stripe Balances Calculations:** \- Stripe Balance Available Now \= Available balance in the platform's Stripe Connect balance. \- Stripe Balance In Transit \= Pending or incoming Stripe funds. \- Total Stripe Assets \= Stripe Balance Available Now \+ Stripe Balance In Transit. |
| BR-200 | **Withdrawal Representation :**The withdrawal amount is displayed as a deduction (colored in red, e.g. $32.28 representing the debit from the Stripe balance). |
| BR-201 | **Full Customer Refund:** The customer is refunded 100% of the original purchase amount via the Stripe Refund API. |
| BR-202 | **Amount Negation:** The transaction Amount and the instructor's PayoutAmount are updated to negative numbers in the database to maintain double-entry accounting integrity. |
| BR-203 | **Access Revocation:** The student's EnrollmentStatus is changed to revoked, immediately disabling course access. |
| BR-204 | **Claw Back Instructor Earnings (Transfer Reversal):**  The system reverses the exact payout amount originally transferred to the instructor's Connected Account: PayoutAmount \= Round((GrossAmount \- StripeFee) \* (TransferRate / 100), 2\) where: StripeFee \= Min(Round(GrossAmount \* 0.029 \+ 0.30, 2), GrossAmount)  |
| BR-205 | **Rejection Reason Required:**  Unlike approval, rejecting a refund request strictly requires a non-empty administrative explanation to provide clarity to the student.  |
| BR-206 | **Status Restored to Succeeded:** The transaction state returns to succeed as if the refund request was never initiated. |
| BR-207 | **No Financial Changes:** No funds are transferred, reversed, or refunded via Stripe Connect. The platform commissions and instructor earnings remain intact. |
| BR-208 | **Uninterrupted Learning Access:** The student's course enrollment status (EnrollmentStatus) remains active, ensuring they continue to have full access to learning materials. |
| BR-209 | **Refund Actions Visibility:** The "Approve Refund" and "Reject" buttons are conditionally rendered on the details page only if the transaction status is exactly refund\_pending. |
| BR-210 | **Refund Info Retrieval:**  If a refund request was ever made, the student's submitted refund reason and the request date are fetched from TransactionExt and displayed. If rejected, the Admin's rejection note is also shown. |
| BR-211 | **Permitted Splitting Range:**  The split rate must be a numeric value between 30.00% and 95.00% (inclusive).  |
| BR-212 | **Non-Retroactivity:**  Changing the configuration only applies to transactions created after the change. Existing payouts and past transaction records preserve their original split rate values. |
| BR-213 | **Instructor Policy Notifications:** The system sends an automated real-time notification to all active instructors who have connected Stripe accounts. Re-notification is bypassed for non-Stripe linked accounts.  |
| BR-214 | **Permitted Payout Days Range:** The trigger days must be integers between 15 and 20 (inclusive), formatted as a single number or a comma-separated list of numbers (e.g., 15 or 15, 18). |
| BR-215 | **Scheduler Automation:**  The automated bulk payout scheduler queries this setting and automatically initiates payouts to instructors' Connect accounts on the configured days of the month. |
| BR-216 | **Minimum Withdrawal Limit:** Platform withdrawals must be at least $0.50 (USD). |
| BR-217 | **Maximum Available Balance:** The maximum amount allowed to be withdrawn cannot exceed the platform's current Stripe Available balance. Cents are rounded to two decimal places. |
| BR-218 | **Widget Home-only Visibility:** The floating chat widget is visible and accessible only on the Home page for logged-in Users. |
| BR-219 | **Support Ticket Queueing:** Support requests submitted via the mini-chat widget create a pending request queue entry and prevent further messages until a staff member accepts the request. |
| BR-220 | **Support Request Assignment:** Pending support tickets are visible only to Staff members. When a Staff member clicks \`"Accept Request"\`, the ticket is assigned to them and removed from the public pending list. Admin members do not receive or view pending support requests from users.  |
| BR-221 | **Support Request Assignment:** Pending support tickets are visible only to Staff members. When a Staff member clicks \`"Accept Request"\`, the ticket is assigned to them and removed from the public pending list. Admin members do not receive or view pending support requests from users.  |
| BR-222 | **Gmail Exclusive:** Only emails ending with "@gmail.com" are permitted for registration. |
| BR-223 | **Uniqueness:** Email and username checks are case-insensitive and must be unique across the platform. |
| BR-224 | **Security:** Passwords must be hashed using BCrypt before database storage. |
| BR-225 | **Default Account Status:** Registered accounts are initialized with status "Active", AuthProvider "Local", and verification flag set to false. |
| BR-226 | **OTP Life Cycle:** The OTP code must be consumed upon validation (one-time use only) and expires within 2 minutes of generation. |
| BR-227 | **Session Cookie Sync:** The system must update the IsVerified cookie to "true" immediately on successful verification to dynamically unlock shopping and checkout permissions without requiring a re-login. |
| BR-228 | **External Account Prevention:** Users who registered using third-party auth (Google OAuth) are prohibited from changing password as they do not have a password hash stored locally. |
| BR-229 | **Forced Logout:** On a successful password change, the system must immediately revoke the refresh token, delete client-side cookies, and force the User to authenticate again to prevent active hijacked sessions. |
| BR-230 | **Google Email Lock:** Email modification is strictly locked for Google OAuth accounts to prevent authorization mismatches. |
| BR-231 | **Transactional Integrity:** Profile modifications spanning User and Account database tables must be executed within a single transaction. If any part of the query fails (e.g. duplicate email checks), the transaction rolls back completely. |
| BR-232 | **Session Synchronization:** Client-side display cookies (such as UserName and AvatarUrl) must be immediately updated on success to ensure the page headers dynamically reflect the changes. |
| BR-233 | **Verification Syncing:** During the profile retrieval process, the system must dynamically read the database verification flag and update the client-side IsVerified cookie to ensure consistent page behavior and menu permissions.  |
| BR-234 | **Uniqueness Constraint:** A specific course can be added to a User's wishlist only once. |
| BR-235 | **AJAX Toggle:** The add action is handled asynchronously via AJAX (ToggleAjax) so the UI changes state instantly without a page reload.  |
| BR-236  | **Default Statistics:** Average rating and total students are aggregated dynamically. If statistics are missing for a course, a default rating of 4.5 is displayed.  |
| BR-237  | **Permanent Deletion:** Unlike coupons or profiles, wishlist items are physically hard-deleted from the database when removed, as they do not require transaction history preservation.  |
| BR-238  | **Unique Username Generation:** For new Google sign-ups, the username is extracted from the email prefix. If the username is already taken, the system appends a numeric counter sequentially (e.g. johndoe1, johndoe2) until it is unique.  |
| BR-239  | **Auto-Verification:** Accounts registered via Google OAuth are flagged as verified (IsVerified \= true) immediately upon creation because Google has verified the email ownership. Null Password Hash: Google OAuth accounts are initialized with no password hash (PasswordHash \= null), bypassing local password authentication.  |
| BR-240  | **Suspension Block:** If an active lockout exists for the account, the system denies login and prints the suspension lockout end date in the error message.  |
| BR-241  | **Credential Flexibility:** The login identifier can be either the account's username or email address (case-insensitive).  |
| BR-242  | **BCrypt Verification:** Local password validation must be processed using BCrypt.Verify. |
| BR-243  | **Role-Based Redirection:** Authenticated Admin and Staff actors are redirected to the admin dashboard, while Users and Instructors are redirected to the homepage.  |
| BR-244  | **Validity Criteria:** To be applied, a coupon must be active, within its validity dates (StartDate \<= Now \<= EndDate), have remaining uses (UsedCount \< UsageLimit), and the cart subtotal must meet the MinOrderValue.  |
| BR-245  | **Discount Logic:** Percentage/Fixed discount calculation and Session Cookie Storage: Applied coupons are written to local cookies to maintain the discount status across browsing activities until checkout.  |
| BR-246  | **Cookie Invalidation:** The coupon code must be completely deleted from local session cookies. Price Reversion: Removing a coupon must immediately restore the cart's discount value to zero.  |
| BR-247  | **Verified Email Requirement:** Only accounts that have completed email verification are permitted to use the forget password reset feature.  |
| BR-248  | **Token Invalidation/Cookie Clearance:** Upon logout, the backend must immediately nullify/revoke the stored RefreshToken and clear all client-side cookies.  |
| BR-249  | **Required Credentials:** Email, Password, and Display Name are mandatory fields that cannot be empty.  |
| BR-250  | **Auto-Verification:** Since the account is directly created by the Admin, the new Staff account is initialized as verified (IsVerified \= true) and active (AccountStatus \= "Active") by default.  |
| BR-251  | **Role Scope Restriction:** This update endpoint is restricted solely to accounts carrying the "staff" role.  |
| BR-252  | **Ban/Lockout Logic:** Banning adds a 100-year lockout record; Unbanning deletes all active lockout entries to restore login access.  |
| BR-253  | **Super Admin Protection:** The Super Admin account ("admin" role) is protected and cannot be banned or locked out.  |
| BR-254  | **Role-Specific Data Aggregation:** System displays data based on specific roles (Students, Instructors, Managers/Staff) and detects lockouts if the account has been banned.  |
| BR-255  | **Search Criteria:** Search keywords are matched (case-insensitive substring) against the account's Email, Phone Number, Manager Display Name, or User Full Name.  |
| BR-256  | **Search Criteria:** Search keywords are matched (case-insensitive substring) against the account's Email, Phone Number, Manager Display Name, or User Full Name.  |
| BR-257  | **Default Pagination:** The page size defaults to 10 accounts per page if not specified otherwise.  |
| BR-258  | **Role Filters:** Supported role filters include: "user", "instructor", "staff", "registered\_staff", or "all" roles.  |
| BR-259  | **Flag Thresholds and Status Progression:** 1st/2nd flag (warning); 3rd flag (banned) triggers a 30-day lockout.  |
| BR-260  | **Non-Blocking Notifications:** A notification dispatch error must not block or roll back the flagging transaction. |
| BR-261 | **Unique:** Code, Coupon Types, Discount Value Limits, Validity Dates, and Usage Limit constraints for new coupons.  |
| BR-262  | **Sorting/Search:** Default sorting by ID, client-side in-memory sorting, and case-insensitive CouponCode matching. |
| BR-263 | **Locked Fields:** Code, Type, Value, and MinOrderValue are read-only. End Date and Usage Limit have specific update constraints. |
| BR-264  | **Soft-Delete Only:** Hard deletion is forbidden. Expiration Enforcement: End Date updated to the previous day to ensure the coupon becomes unusable. |
| BR-265  | **Filter Parameters:** Filtering by IsActive state, CouponType, or both combined in a single database query. |
| BR-266 | Lockout Status Monitoring: The profile displays any active suspension records (start and end times) for the manager account, keeping administrative users informed of active lockout configurations. |
| BR-267 | Role-based Labeling: The interface shows "Super Admin" if the manager's role is "ADMIN", and "Staff Support" otherwise. |
| BR-268 | **Unique Quiz Title:** The quiz title must be unique per instructor. The system checks this upon quiz creation and updates. |
| BR-269 | **Minimum Total Questions:** The total questions limit for a quiz must be greater than 0. |
| BR-270 | **Distribution Integrity:** The sum of question counts distributed across lessons must exactly equal the quiz's configured TotalQuestions. |
| BR-271 | **Question Bank Sufficiency:** For each lesson distribution, the number of requested questions must not exceed the actual number of questions available in the Question Bank for that specific lesson. |
| BR-272 | **Attempt Lockdown:** An instructor cannot update the settings of a quiz, delete a quiz, or remove a quiz from a course if there is currently an active (unsubmitted) attempt by any student. |
| BR-273 | **Enrolled Course Protection:** A quiz cannot be deleted if it is assigned to any course that has enrolled students. |
| BR-274 | **Pending Course Lockdown:** Quizzes cannot be added to, removed from, or toggled hidden in a course while the course status is "Pending". |
| BR-275 | **Auto Draft Fallback:** Adding or removing a quiz from a "Published" course will automatically revert the course status back to "Draft". |
| BR-276 | **Quiz Generation Logic:** When a student starts a quiz, the system randomly draws questions from the Question Bank based on the quiz's QuizLessonDistribution. If the distribution falls short, it supplements remaining questions randomly from the entire course's question pool. |
| BR-277 | **Score Calculation:** The attempt score is calculated as (Correct Answers / Total Questions) * 100, rounded to the nearest integer. |
| BR-278 | **Passing Condition:** A student attempt is marked as "Passed" if the calculated score is greater than or equal to the quiz's PassingScore configuration. |
| BR-279 | **Minimum Options:** A question must have at least 2 options. |
| BR-280 | **Minimum Correct Options:** A question must have at least 1 correct option. |
| BR-281 | **Instructor Course Ownership:** An instructor can only add, edit, delete, or view questions for courses they own. |

## 