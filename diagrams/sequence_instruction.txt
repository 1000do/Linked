# Sequence Diagram Design Instructions

Guidelines and standards for creating consistent, high-quality, and structurally sound sequence diagrams.

---

## 1. Participants

### General List of Participants
Depending on the specific use case, a diagram may include some or all of the following:
*   **Views:** `.cshtml` views (Client/UI)
*   **ViewModels & DTOs:** Data transfer and view representation objects
*   **Controllers:** Frontend Controllers & Backend/API Controllers
*   **Services:** Business logic layers
*   **Repositories & Data Layers:** Repository classes & `AppDbContext`
*   **Entities & Storage:** Domain Entities & the Database

### Modeling Rules for Participants
*   **Visual Representation:** All participants except actors (which use the stick figure) and Databases(which use the cylinder figure) must be drawn as rectangles. 
*   **Implementations Only:** Focus exclusively on concrete implementations; ignore interfaces.
*   **Instance Syntax:** If a participant represents a specific instance object, prefix its name with a colon `:` (e.g., `:CourseService`). Otherwise, write the class/type name directly.

---

## 2. Combined Fragments

*   **Self-Call Triggers:** Before drawing an `opt` or `alt` combined fragment, draw a self-call message describing the *purpose* or *condition* of the fragment.
*   **Extended Alt Blocks:** An `alt` fragment can contain more than two blocks (e.g., to handle switch cases or `if`, `else if`, ..., `else` control structures).
*   **Loop Conditions:** A `loop` fragment must have an explicit condition specified to define when the loop continues.

---

## 3. Arrow Styles

Use distinct arrow styles to represent different interaction types:
*   `->` (Solid Arrow): Synchronous calls, requests, or redirects.
*   `-->>` (Dashed/Dotted Arrow): Returns, responses, thrown exceptions, or messages displayed to the actor.

---

## 4. Lifeline Activation & Deactivation

Managing the active lifecycle of participants is critical for readability.

### Basic Rules
*   **Activation (`++`):** Activate a participant's lifeline upon the first call, request, or redirect it receives.
*   **Deactivation (`--`):** Deactivate a lifeline immediately upon a return statement, an exception throw, or a message displayed to the actor.
*   **Re-activation:** Subsequent requests to a deactivated lifeline will activate it again.
*   **Completeness:** Ensure every lifeline (except the actor's) is properly deactivated by the time it reaches its final return message.

### Self-Call Activations
*   **Triggers for `opt` or `alt`:** If a self-call triggers a conditional fragment, instantly deactivate the lifeline before entering the fragment:
    ```plantuml
    fe_ctrl->fe_ctrl++: Validate ModelState
    fe_ctrl--
    group opt [Invalid ModelState]
        fe_ctrl-->>view: Return status code 400 
        view-->>act: Display error message
    end
    ```
*   **Auxiliary Calls:** If a self-call is not part of the main execution flow under review, activate and instantly deactivate it:
    ```plantuml
    sv -> sv++: Call CheckInstructorRights()
    sv--
    ```
*   **Main Execution Details:** If the self-call represents the main execution flow where details are being expanded (e.g., calling an internal helper method whose steps are detailed), keep it active until the return statement of that method.

### Trailing Lifeline Deactivation in Alt Fragments
If the diagram ends with an `alt` fragment or nested `alt` fragments, only the final branch of the outermost `alt` fragment is responsible for deactivating the trailing lifelines along its return path.

#### Example: Final branch deactivation (`else condition_3`)
```plantuml
alt condition_1
    loop for each userId in targetUserIds
        sv->hub++: Call Clients.User(userId).SendAsync("ReceiveNotification")
        hub-->>sv: Broadcast notification via SignalR
        hub--
    end
    sv-->>ctrl: Return targetUserIds.Count
    ctrl-->>view: Return status code 200 (Ok)
    view-->>act: Display "Sent Successfully" message
else condition_2
    ' ... intermediate logic ...
else condition_3
    sv-->>ctrl--: Return 0
    ctrl-->>view--: Return status code 400 (BadRequest)
    view-->>act--: Display "Send failed" message
end
@enduml
```

---

## 5. Flow Logic & Modeling Guidelines

*   **Form Validation:** Always represent client-side form validation in the UI/view first. Only depict server-side ModelState validation (`Validate ModelState`) if it is explicitly written in the code.
*   **Component Omission:**
    *   Omit the frontend controller if the execution flow does not require or use it.
    *   Omit ViewModels and DTOs if they are handled automatically by the framework and not explicitly manipulated in the custom code.
*   **Class/Layer Simplification:** Keep focus on the main execution path. If a method calls other methods within the same class, represent this simply as a self-call.
*   **Database Changes (Insert, Update, Delete):**
    *   Assume that crucial `SaveChangesAsync()` repository calls return the number of affected rows (`numberOfRowsAffected`) to the service.
    *   Use this to enable branching with the condition `numberOfRowsAffected > 0`.
    *   Within the successful branch (`numberOfRowsAffected > 0`), detail the rest of the normal execution path.
    *   Explicitly state or report this assumption in your documentation.
*   **Distinct Return Paths:** Return paths of `alt` branches must be separated all the way from their entry point to the final exit point/actor. Even if they output similar descriptions like "Return status code" or "Display message", they represent distinct logical paths and must be treated as such.
