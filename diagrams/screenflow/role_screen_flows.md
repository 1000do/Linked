# Role-Based Screen Flow Diagrams

These diagrams illustrate the logical journey flows of the system partitioned into three core views. Nodes are grouped into Subgraphs to prevent spaghetti arrows and clearly outline feature domains.

## 1. Learner & Instructor Journey (Guest -> User -> Instructor)

```mermaid
graph LR
    classDef page fill:#DAE8FC,stroke:#333,stroke-width:2px,color:#000;
    classDef popup fill:#FFF2CC,stroke:#333,stroke-width:2px,color:#000;

    subgraph Course_Marketplace["Course Marketplace"]
        S1["Home"]
        P2(["Global Support Chat Widget"])
        S2["Course Collection"]
        S3["Course Details"]
        P5(["Course Video Preview Modal"])
        P6(["Course Report Submission Modal"])
        P7(["Course Review Report Submission Modal"])
        S9["User Profile"]
        S10["Edit User Profile"]
        S13["Change Password"]
        S14["Cart"]
        P19(["Coupon Application Modal"])
        S15["Checkout"]
        S16["My Courses"]
        S17["Course Learning"]
        P23(["Lesson Review Report Submission Modal"])
        S18["Wishlist"]
        S19["User Chat"]
        S22["Personal Notifications"]
        P29(["Personal Notification Details Modal"])
        S23["Purchase History"]
        P31(["Refund Request Modal"])
        S24["Invoice Details"]
        S31["Instructor Public Profile"]
        S46["Gift Card Setup"]
        S47["Gift Checkout"]
        S48["Gift Claim"]
    end

    subgraph Authentication["Authentication"]
        S4["Login"]
        S5["Register"]
        S6["Forgot Password"]
        S7["Reset Password"]
        S8["Verify OTP"]
        S50["Email Verification Recommendation"]
    end

    subgraph Instructor_Space["Instructor Space"]
        S20["Instructor Chat"]
        S25["Instructor Transaction Details"]
        S27["Instructor Application Form"]
        S28["Instructor Application Status"]
        S29["Instructor Notifications"]
        P38(["Instructor Notification Details Modal"])
        S32["Instructor Dashboard"]
        S33["Course Editor Space"]
        P42(["Lesson Addition Modal"])
        P43(["Coupon Selection Modal"])
        S34["Course Creation Form"]
        P45(["Coupon Selection Modal"])
        S35["Course Recycle Bin"]
        S36["Instructor Earnings"]
    end

    %% Apply Classes
    class S1 page;
    class P2 popup;
    class S2 page;
    class S3 page;
    class P5 popup;
    class P6 popup;
    class P7 popup;
    class S4 page;
    class S5 page;
    class S6 page;
    class S7 page;
    class S8 page;
    class S9 page;
    class S10 page;
    class S13 page;
    class S14 page;
    class P19 popup;
    class S15 page;
    class S16 page;
    class S17 page;
    class P23 popup;
    class S18 page;
    class S19 page;
    class S20 page;
    class S22 page;
    class P29 popup;
    class S23 page;
    class P31 popup;
    class S24 page;
    class S25 page;
    class S27 page;
    class S28 page;
    class S29 page;
    class P38 popup;
    class S31 page;
    class S32 page;
    class S33 page;
    class P42 popup;
    class P43 popup;
    class S34 page;
    class P45 popup;
    class S35 page;
    class S36 page;
    class S46 page;
    class S47 page;
    class S48 page;
    class S50 page;

    %% Screen Navigations
    S1 -->|"Click Login"| S4
    S1 -->|"Click Register"| S5
    S4 -->|"Click Forgot Password"| S6
    S6 -->|"Submit Email"| S8
    S5 -->|"Submit Details"| S50
    S50 -->|"Click Login"| S4
    S50 -->|"Click Verify Now"| S8
    S8 -->|"Email Verified"| S9
    S8 -->|"OTP Verified"| S7
    S7 -->|"Password Reset Successfully"| S4
    S9 -->|"Click Verify Email"| S8
    S1 -->|"Search Courses"| S2
    S2 -->|"View Course"| S3
    S1 -->|"Click Featured Course"| S3
    S14 -->|"Proceed to Checkout"| S15
    S1 -->|"View Cart"| S14
    S1 -->|"Click My Courses icon on Header"| S16
    S1 -->|"Click Wishlist icon on Header"| S18
    S15 -->|"Payment Success"| S16
    S23 -->|"View Invoice"| S24
    S16 -->|"Go to Learning"| S17
    S3 -->|"Click Go to Learning"| S17
    S4 -->|"Login Success (Learner/Instructor)"| S1
    S1 -->|"Click Profile Avatar"| S9
    S9 -->|"Edit Profile"| S10
    S9 -->|"Change Password"| S13
    S1 -->|"View Alerts"| S22
    S1 -->|"Open Messages"| S19
    S3 -->|"View Instructor Info"| S31
    S9 -->|"Click Purchase History"| S23
    S3 -->|"Gift this Course"| S46
    S46 -->|"Proceed to Pay"| S47
    S48 -->|"Click Login"| S4
    S48 -->|"Click Register"| S5
    S1 -->|"Apply to Teach"| S27
    S27 -->|"Submit Application"| S28
    S1 -->|"Switch to Instructor"| S32
    S32 -->|"Edit Courses"| S33
    S32 -->|"Create Course"| S34
    S32 -->|"View Deleted Courses"| S35
    S32 -->|"View Revenue"| S36
    S36 -->|"View Payouts"| S25
    S32 -->|"Manage Student Chats"| S20
    S32 -->|"View Instructor Alerts"| S29
    S32 -->|"View Application Status"| S28

    %% Popup Triggers
    S1 -.->|"Click floating chat icon"| P2
    S3 -.->|"Click preview video"| P5
    S3 -.->|"Click Report Course"| P6
    S3 -.->|"Click Report Review"| P7
    S14 -.->|"Click Apply Coupon"| P19
    S17 -.->|"Click Report Lesson Review"| P23
    S22 -.->|"Click Notification Item"| P29
    S23 -.->|"Click Request Refund"| P31
    S29 -.->|"Click Notification Item"| P38
    S33 -.->|"Click Add Lesson"| P42
    S33 -.->|"Click Select Coupon"| P43
    S34 -.->|"Click Select Coupon"| P45
```

## 2. Internal Operations (Staff & Admin Shared)

```mermaid
graph LR
    classDef page fill:#DAE8FC,stroke:#333,stroke-width:2px,color:#000;
    classDef popup fill:#FFF2CC,stroke:#333,stroke-width:2px,color:#000;

    subgraph Content_Moderation["Content Moderation"]
        S3["Course Details"]
        P5(["Course Video Preview Modal"])
        S17["Course Learning"]
        S37["Instructor Application Management"]
        P49(["Instructor Application Approval Modal"])
        P50(["Instructor Application Rejection Modal"])
        S38["Instructor Application Details"]
        P52(["Instructor Application Approval Modal"])
        P53(["Instructor Application Rejection Modal"])
        S39["Course Moderation"]
        P55(["Course Approval Modal"])
        P56(["Course Rejection Modal"])
        P57(["Course Flagging Modal"])
        S40["Report Center"]
        P59(["Report Details Modal"])
        P60(["Report Resolution Modal"])
    end

    subgraph Management_Core["Management Core"]
        S11["Manager Profile"]
        S12["Edit Manager Profile"]
        S21["System Chat"]
        S41["System Notification Management"]
        P62(["Notification Creation Modal"])
        P63(["System Notification Details Modal"])
    end

    %% Apply Classes
    class S3 page;
    class P5 popup;
    class S11 page;
    class S12 page;
    class S17 page;
    class S21 page;
    class S37 page;
    class P49 popup;
    class P50 popup;
    class S38 page;
    class P52 popup;
    class P53 popup;
    class S39 page;
    class P55 popup;
    class P56 popup;
    class P57 popup;
    class S40 page;
    class P59 popup;
    class P60 popup;
    class S41 page;
    class P62 popup;
    class P63 popup;

    %% Screen Navigations
    S3 -->|"Click Preview course"| S17
    S41 -->|"Manage Chat"| S21
    S41 -->|"Manage Instructor Application"| S37
    S41 -->|"Manage Course Moderation"| S39
    S41 -->|"Manage Reports"| S40
    S39 -->|"Click Preview icon"| S3
    S37 -->|"View Details"| S38
    S41 -->|"Click Profile icon"| S11
    S11 -->|"Click Edit Profile"| S12

    %% Popup Triggers
    S3 -.->|"Click preview video"| P5
    S37 -.->|"Click Approve"| P49
    S37 -.->|"Click Reject"| P50
    S38 -.->|"Click Approve"| P52
    S38 -.->|"Click Reject"| P53
    S39 -.->|"Click Approve Course"| P55
    S39 -.->|"Click Reject Course"| P56
    S39 -.->|"Click Flag Course"| P57
    S40 -.->|"Click View Report"| P59
    S40 -.->|"Click Resolve Report"| P60
    S41 -.->|"Click Compose Alert"| P62
    S41 -.->|"Click View Alert"| P63
```

## 3. Executive Administration (Admin Only)

```mermaid
graph LR
    classDef page fill:#DAE8FC,stroke:#333,stroke-width:2px,color:#000;
    classDef popup fill:#FFF2CC,stroke:#333,stroke-width:2px,color:#000;

    subgraph System_Finance["System Finance"]
        S26["System Transaction Details"]
        S44["System Finance Management"]
        P70(["Refund Request Approval Modal"])
        P71(["Refund Request Rejection Modal"])
    end

    subgraph Accounts_And_Services["Accounts & Services"]
        S42["Account Management"]
        P65(["Staff Account Addition Modal"])
        P66(["Staff Account Modification Modal"])
        P67(["Staff Account Flagging Modal"])
        S43["Account Details"]
        S49["AI Service Management"]
        P79(["AI Model Addition Modal"])
        P80(["AI Model Modification Modal"])
        P81(["AI Model Details Modal"])
    end

    subgraph Promotions["Promotions"]
        S45["Coupon Management"]
        P73(["Coupon Creation Modal"])
        P74(["Coupon Modification Modal"])
    end

    %% Apply Classes
    class S26 page;
    class S42 page;
    class P65 popup;
    class P66 popup;
    class P67 popup;
    class S43 page;
    class S44 page;
    class P70 popup;
    class P71 popup;
    class S45 page;
    class P73 popup;
    class P74 popup;
    class S49 page;
    class P79 popup;
    class P80 popup;
    class P81 popup;

    %% Screen Navigations
    S44 -->|"View Transactions"| S26
    S44 -->|"Manage Account"| S42
    S44 -->|"Manage AI Service"| S49
    S44 -->|"Manage Coupon"| S45
    S42 -->|"View Staff Info"| S43

    %% Popup Triggers
    S42 -.->|"Click Add Account"| P65
    S42 -.->|"Click Edit Account"| P66
    S42 -.->|"Click Flag Account"| P67
    S44 -.->|"Click Approve Refund"| P70
    S44 -.->|"Click Reject Refund"| P71
    S45 -.->|"Click Create Promotion"| P73
    S45 -.->|"Click Edit Promotion"| P74
    S49 -.->|"Click Add Model"| P79
    S49 -.->|"Click Edit Model"| P80
    S49 -.->|"Click View Model Data"| P81
```

