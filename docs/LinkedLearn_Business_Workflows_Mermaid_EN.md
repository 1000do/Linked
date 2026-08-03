# 📊 BUSINESS WORKFLOWS – LINKEDLEARN PLATFORM (MERMAID FLOWCHARTS)

This document contains the Mermaid Flowchart source code for the 6 core business workflows of the **LinkedLearn** platform. You can use this directly in GitHub Markdown, VS Code Mermaid Preview, or Notion.

---

## 👥 SYSTEM ROLE COLOR SPECIFICATION

| Actor | Description | Color Code |
| :--- | :--- | :--- |
| **Guest** | Unauthenticated Visitor | Slategray (`#64748b`) |
| **User** | Enrolled Student | Indigo (`#4338ca`) |
| **Instructor** | Course Creator & Educator | Teal (`#0f766e`) |
| **Staff** | Moderation & Support Agent | Amber (`#b45309`) |
| **Admin** | Super Administrator | Rose (`#be123c`) |
| **System** | Automated Engine / Webhook / AI | Cyan (`#0891b2`) |

---

## 1️⃣ WORKFLOW 1: INSTRUCTOR ONBOARDING & ACTIVATION

```mermaid
flowchart LR
    classDef guest fill:#f1f5f9,stroke:#64748b,stroke-width:2px,color:#0f172a
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef instructor fill:#ccfbf1,stroke:#0f766e,stroke-width:2px,color:#134e4a
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W1_1["1️⃣ Guest Registration<br/>• Email & Password<br/>• Create User Account"]:::guest --> W1_2["2️⃣ Submit Profile<br/>• Bio & Experience<br/>• Degrees & Certs"]:::user
    W1_2 --> W1_3["3️⃣ System Ingestion<br/>• Save Pending Profile<br/>• Dispatch Ticket"]:::system
    W1_3 --> W1_4["4️⃣ Staff/Admin Review<br/>• Verify Credentials<br/>• Evaluate Eligibility"]:::staff
    W1_4 --> W1_5{"5️⃣ Approve?"}:::decision
    
    W1_5 -- "Yes" --> W1_6["6️⃣ System Activation<br/>• Upgrade to Instructor<br/>• Email Stripe Link"]:::system
    W1_6 --> W1_7["7️⃣ Stripe Onboarding<br/>• Bank Account Info<br/>• Complete Payout Setup"]:::instructor

    W1_5 -- "No" --> W1_R["📄 Additional Info /<br/>❗ Reject + Reason"]:::reject
```

---

## 2️⃣ WORKFLOW 2: COURSE CREATION & MODERATION

```mermaid
flowchart LR
    classDef instructor fill:#ccfbf1,stroke:#0f766e,stroke-width:2px,color:#134e4a
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W2_1["1️⃣ Instructor Dashboard<br/>• Click Create Course"]:::instructor --> W2_2["2️⃣ Course Details<br/>• Title, Desc, Price<br/>• Upload Thumbnail"]:::instructor
    W2_2 --> W2_3["3️⃣ Lessons & Quizzes<br/>• Videos & Reading Docs<br/>• Compose Quiz Item"]:::instructor
    W2_3 --> W2_4["4️⃣ AI Content Scan<br/>• Scan Banned Words<br/>• Assign AI Risk Score"]:::system
    W2_4 --> W2_5["5️⃣ Staff/Admin Review<br/>• Review AI Results<br/>• Inspect Quality"]:::staff
    W2_5 --> W2_6{"6️⃣ Approve?"}:::decision

    W2_6 -- "Yes" --> W2_7["7️⃣ Publish Course<br/>• Status Published<br/>• Display Marketplace"]:::system
    W2_6 -- "No" --> W2_R["📄 Revision Request /<br/>❗ Reject + Reason"]:::reject
```

---

## 3️⃣ WORKFLOW 3: COURSE DISCOVERY & WISHLIST

```mermaid
flowchart LR
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b

    W3_1["1️⃣ User Login<br/>• Authenticate Account<br/>• Load Personal Feed"]:::user --> W3_2["2️⃣ Search & Filter<br/>• Keywords & Category<br/>• Filter Price & Hot"]:::user
    W3_2 --> W3_3["3️⃣ View Course Detail<br/>• Reviews & Syllabus<br/>• Watch Demo Video"]:::user
    W3_3 --> W3_4["4️⃣ View Instructor Profile<br/>• Bio & Credentials<br/>• View Other Courses"]:::user
    W3_4 --> W3_5["5️⃣ Add Wishlist<br/>• Click 💖 Save Course<br/>• Sync Across Devices"]:::user
```

---

## 4️⃣ WORKFLOW 4: CART, PAYMENT & REFUND

```mermaid
flowchart LR
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W4_1["1️⃣ Cart & Checkout<br/>• View Wishlist & Cart<br/>• Click Checkout"]:::user --> W4_2["2️⃣ Stripe Payment<br/>• Enter Coupon Code<br/>• Pay via Gateway"]:::user
    W4_2 --> W4_3["3️⃣ Transaction & Payout<br/>• Activate Enrollment<br/>• Record Payout Ledger"]:::system
    W4_3 --> W4_4["4️⃣ Request Refund<br/>• Within 14 Days<br/>• Submit Refund Reason"]:::user
    W4_4 --> W4_5["5️⃣ Staff/Admin Assessment<br/>• Check 14-Day Limit<br/>• Progress < 20%"]:::staff
    W4_5 --> W4_6{"6️⃣ Approve?"}:::decision

    W4_6 -- "Yes" --> W4_7["7️⃣ Stripe Refund<br/>• Refund via Stripe<br/>• Revoke Enrollment"]:::system
    W4_6 -- "No" --> W4_R["📄 Reject Refund<br/>• Send Ineligible Notice"]:::reject
```

---

## 5️⃣ WORKFLOW 5: LEARNING, QUIZ & REPORT MODERATION

```mermaid
flowchart LR
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W5_1["1️⃣ Watch Lessons<br/>• Stream Video Lessons<br/>• Track Progress %"]:::user --> W5_2["2️⃣ SignalR Realtime Chat<br/>• Lesson Chat Window<br/>• Q&A with Instructor"]:::user
    W5_2 --> W5_3["3️⃣ Take Quiz<br/>• Multiple-Choice Test<br/>• View Score & Answer"]:::user
    W5_3 --> W5_4["4️⃣ Review / Report<br/>• Rating & Comments<br/>• Or Report Violation"]:::user
    W5_4 --> W5_5["5️⃣ Staff/Admin Audit<br/>• Ingest Report Ticket<br/>• Audit Evidence"]:::staff
    W5_5 --> W5_6{"6️⃣ Approve?"}:::decision

    W5_6 -- "Yes" --> W5_7["7️⃣ Enforce Penalties<br/>• Hide/Delete Item<br/>• Suspend Offender"]:::system
    W5_6 -- "No" --> W5_R["📄 Dismiss Report<br/>• Close Ticket"]:::reject
```

---

## 6️⃣ WORKFLOW 6: SYSTEM ADMINISTRATION (BACK-OFFICE MANAGEMENT)

> 💡 **Note**: The Back-Office consists of 7 independent management modules running in parallel; process flow arrows (`-->`) are omitted.

```mermaid
flowchart LR
    classDef admin fill:#ffe4e6,stroke:#be123c,stroke-width:2px,color:#881337
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f

    M1["1️⃣ Instructor Management<br/>• Approve Applications<br/>• Verify Credentials"]:::staff
    M2["2️⃣ Account & RBAC<br/>• Ban / Unban Accounts<br/>• Manage Roles"]:::admin
    M3["3️⃣ AI Service Config<br/>• AI Risk Thresholds<br/>• Manage Banned Words"]:::admin
    M4["4️⃣ Course Approvals<br/>• Approve / Reject<br/>• Quality Supervision"]:::staff
    M5["5️⃣ Review & Chat Moderation<br/>• Moderate Reviews<br/>• Monitor SignalR Logs"]:::staff
    M6["6️⃣ Violation Report Center<br/>• Receive Reports<br/>• Enforce Penalties"]:::staff
    M7["7️⃣ Financial & Refunds<br/>• Gross Revenue Ledger<br/>• Approve Refund Requests"]:::admin
```
