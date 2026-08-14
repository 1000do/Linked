# Non-Functional Requirements (NFR) - Definition & Specifications


## 5.1 EXTERNAL INTERFACE REQUIREMENTS

### 5.1.1 User Interfaces

**UI-1: Responsive Design — Viewport Support**
- **Definition**: The web application provides responsive layouts for viewport widths from **360 px to 1920 px** without horizontal scrolling or loss of access to primary navigation controls.
- **Acceptance Criteria**: All primary navigation controls remain visible and functional across the entire viewport range.

**UI-2: Consistent Navigation Patterns**
- **Definition**: The application maintains consistent navigation and interaction patterns across all major pages for authenticated users.
- **Acceptance Criteria**: Navigation layout, button placement, and interaction flows are identical across authenticated sections.

**UI-3: Keyboard Accessibility & Focus Management**
- **Definition**: All keyboard-accessible interactive controls provide a visible focus state and are reachable using standard keyboard navigation.
- **Acceptance Criteria**: Tab key navigates through all interactive elements; focus is always visible; keyboard-only users can complete all tasks.

---

### 5.1.2 Software Interfaces

**SI-1: RESTful API Design**
- **Definition**: The MVC frontend communicates with the Web API using **RESTful HTTP/HTTPS endpoints** and standard HTTP methods (GET, POST, PUT, DELETE, PATCH).
- **Acceptance Criteria**: All API endpoints follow REST conventions; responses return appropriate HTTP status codes (200, 201, 400, 401, 403, 404, 500, etc.).

**SI-2: JSON Encoding & Character Set**
- **Definition**: REST API requests and responses use **UTF-8 encoded JSON** unless another content type is explicitly required.
- **Acceptance Criteria**: All responses include header `Content-Type: application/json; charset=utf-8`; special characters properly encoded.

**SI-3: Error Response Format & Security**
- **Definition**: API errors return an appropriate HTTP status code and a **structured error response without exposing stack traces, database errors, credentials, or internal implementation details**.
- **Acceptance Criteria**: Error responses use consistent format; no `.cs` filenames, SQL statements, connection strings, or exception stack traces visible to client.

**SI-4: Real-Time Communication via SignalR**
- **Definition**: Real-time communication uses **ASP.NET Core SignalR**.
- **Acceptance Criteria**: WebSocket connections establish successfully; bi-directional messaging works for notifications, chat, and moderation updates.

**SI-5: AI Service Timeout Configuration**
- **Definition**: The Web API communicates with the Python AI Moderation Service through defined service endpoints.
- **Acceptance Criteria**: Request timeout configured; service responds within expected timeframe or fails gracefully.

**SI-6: Protected Credentials Storage**
- **Definition**: External service credentials are stored **exclusively in protected server-side configuration** and never exposed to client-side code.
- **Acceptance Criteria**: No credentials in HTML source, JavaScript bundles, or frontend configuration files; all secrets in environment variables or secure vaults.

---

## 5.2 QUALITY ATTRIBUTES

### 5.2.1 Performance Requirements

**PER-1: API Response Time — 95th Percentile**
- **Definition**: At least **95% of standard Web API requests** receive an HTTP response within **500 ms** under the defined normal workload (100 concurrent users, 10+ minute test).
- **Acceptance Criteria**: Median response time ≤ 500ms; 95th percentile ≤ 500ms across representative operations (course browse, search, detail, profile access, cart, dashboard).

**PER-2: API Response Time — 99th Percentile**
- **Definition**: At least **99% of standard Web API requests** receive an HTTP response within **2 seconds** under normal workload.
- **Acceptance Criteria**: 99th percentile latency ≤ 2000ms; no outliers exceeding 3 seconds.

**PER-3: Frontend Page Load (LCP)**
- **Definition**: At least **95% of standard MVC page loads** achieve a Largest Contentful Paint (LCP) of **2.5 seconds or less** under normal workload.
- **Acceptance Criteria**: LCP metric measured via browser dev tools or performance APIs; consistent across major pages.

**PER-4: SignalR Message Delivery Latency**
- **Definition**: At least **95% of messages** transmitted through an established SignalR connection are delivered to the intended recipient within **300 ms** under normal network conditions.
- **Acceptance Criteria**: Message latency (publish to receipt) ≤ 300ms for 95%+ of notifications under load.

**PER-5: Real-Time Notification Delivery**
- **Definition**: At least **95% of real-time notifications** are delivered to connected clients within **300 ms** of server-side dispatch.
- **Acceptance Criteria**: Notification delivery latency ≤ 300ms for 95%+ of recipients; measured across 10+ concurrent connections.

**PER-6: Asynchronous AI Operations**
- **Definition**: AI moderation operations expected to require more than **2 seconds** execute **asynchronously** and do not block the originating HTTP request.
- **Acceptance Criteria**: AI moderation job submitted and immediately returns response; actual processing happens in background queue.

**PER-7: Asynchronous Job Acknowledgement**
- **Definition**: An asynchronous AI submission returns an acknowledgement or job identifier within **1 second** for at least **95% of requests**.
- **Acceptance Criteria**: Async job submission returns 201 Accepted with job ID within 1 second; 95%+ of requests meet this.

**PER-8: Concurrent AI Operations Support**
- **Definition**: Standard API performance requirements remain satisfied while up to **10 AI moderation jobs** are processed concurrently.
- **Acceptance Criteria**: PER-1 and PER-2 still met when 10 AI jobs running in parallel.

---

### 5.2.2 Load and Concurrency Requirements

**LOAD-1: Concurrent Authenticated Users**
- **Definition**: The Web API supports at least **100 concurrent authenticated users** executing representative marketplace operations while satisfying performance requirements (PER-1, PER-2).
- **Acceptance Criteria**: 100 simultaneous connections; 0 connection errors; response times remain within PER-1/PER-2 thresholds.

**LOAD-2: Simultaneous SignalR Connections**
- **Definition**: The system supports at least **50 simultaneous SignalR connections** while maintaining **300 ms** message-delivery requirement (PER-4).
- **Acceptance Criteria**: 50 WebSocket connections established; all receive notifications within 300ms.

**LOAD-3: Concurrent AI Moderation Jobs**
- **Definition**: The system supports at least **10 concurrent AI moderation jobs** without causing standard API requests to exceed performance thresholds.
- **Acceptance Criteria**: 10 AI jobs running simultaneously; API still meets PER-1, PER-2.

**LOAD-4: Database Connection Pool**
- **Definition**: The database supports at least **100 concurrent application connections** during normal load without connection failures.
- **Acceptance Criteria**: Connection pool size ≥ 100; no "connection pool exhaustion" errors under load.

**LOAD-5: Request Success Rate Under Load**
- **Definition**: The system maintains successful request processing for at least **95% of requests** during normal-load test.
- **Acceptance Criteria**: (2xx + 3xx responses) / Total requests ≥ 95%; errors are transient (4xx/5xx < 5%).

**LOAD-Definition: Normal Workload Characteristics**
- **Concurrent Users**: 100
- **Test Duration**: Minimum 10 minutes
- **Representative Operations**:
  - Course browsing and search
  - Course detail retrieval
  - User profile access
  - Shopping cart operations
  - Dashboard access
  - Other typical read/write operations

---

### 5.2.3 Stress and Recovery Requirements

**STR-1: Stress Load Operation**
- **Definition**: The system remains **operational** when subjected to **2x normal concurrent workload** (200 concurrent users) for a minimum of **10 minutes**.
- **Acceptance Criteria**: Process remains running; maintains functionality; response times degrade gracefully; error rate < 5%.

**STR-2: Data Integrity Under Stress**
- **Definition**: Under 200-user stress workload, the system does not terminate unexpectedly, corrupt persistent data, or require manual process termination.
- **Acceptance Criteria**: No unrecovered exceptions; database integrity verified after stress test; no orphaned records.

**STR-3: Graceful Failure Under Overload**
- **Definition**: When system capacity is exceeded, the application fails gracefully by returning controlled HTTP errors such as **429 Too Many Requests** or **503 Service Unavailable**, rather than crashing.
- **Acceptance Criteria**: Overloaded endpoints return 429/503; process continues; no "500 Internal Server Error" from overload.

**STR-4: Post-Stress Recovery**
- **Definition**: Following stress test termination, the system returns to normal operating conditions within **5 minutes** without database restoration or manual intervention.
- **Acceptance Criteria**: After load stops, API responds normally (< 500ms) within 5 minutes; no manual recovery needed.

---

### 5.2.4 Availability Requirements

**AVL-1: Health Check Mechanism**
- **Definition**: The Web API exposes a **health-check endpoint** that identifies the availability of required dependencies (PostgreSQL, Redis, AI Moderation Service).
- **Acceptance Criteria**: `GET /api/health` returns 200 OK with JSON showing status of each dependency; identifies failures.

**AVL-2: Non-Critical Dependency Resilience**
- **Definition**: Failure of a **non-critical external dependency** does not cause the Web API process to terminate.
- **Acceptance Criteria**: If Cloudinary/external service fails, API continues; graceful error returned to client; process stays alive.

**AVL-3: Dependency Timeout Management**
- **Definition**: The system returns a **controlled error response within 30 seconds** when an external dependency becomes unavailable and no retry/fallback succeeds.
- **Acceptance Criteria**: External service timeouts ≤ 30 seconds; client receives error message; API doesn't hang.

**AVL-4: Critical Failure Logging**
- **Definition**: The application logs all **detected critical service failures within 5 seconds** of detection.
- **Acceptance Criteria**: Log entry created ≤ 5 seconds after failure detected; log entry includes timestamp, failure type, and context.

---

### 5.2.5 Recovery and Fault Tolerance Requirements

**REC-1: Database Backup Frequency**
- **Definition**: PostgreSQL is backed up at least **once every 24 hours**.
- **Acceptance Criteria**: Automated backup runs on defined schedule; backup file created daily.

**REC-2: Backup Restoration Time**
- **Definition**: The latest valid PostgreSQL backup is **restorable within 4 hours** following simulated database loss.
- **Acceptance Criteria**: Full database restore from backup completes in ≤ 4 hours.

**REC-3: AI Moderation Failure Handling**
- **Definition**: If the AI Moderation Service becomes unavailable, uploaded content remains intact and enters a **retry or manual-review state within 30 seconds**.
- **Acceptance Criteria**: Failed moderation job transitions to retry queue or manual review queue within 30 seconds.

**REC-4: AI Moderation Retry Policy**
- **Definition**: The AI Moderation Service automatically **retries failed moderation jobs no more than 3 times** before transferring to manual-review workflow.
- **Acceptance Criteria**: Job retry count ≤ 3; after 3 failures, job status = "manual_review".

**REC-5: API Recovery from Service Crash**
- **Definition**: Recovery of the Web API from a simulated service crash completes within **5 minutes** without database restoration.
- **Acceptance Criteria**: After container restart, API accessible and responsive within 5 minutes.

**REC-6: Idempotent Operations**
- **Definition**: Duplicate recovery or retry operations do not create duplicate orders, enrollments, transactions, or other critical records.
- **Acceptance Criteria**: Retrying same operation twice produces identical result; only one record created.

---

### 5.2.6 Security - Authentication & Authorization

**SEC-1: Server-Side Authentication Enforcement**
- **Definition**: The system enforces **server-side authentication and authorization** for all protected API endpoints.
- **Acceptance Criteria**: All protected endpoints require valid JWT token; unauthenticated requests return 401 Unauthorized.

**SEC-2: Role-Based Authorization**
- **Definition**: Role-based authorization supports exactly the following authenticated application roles:
  - User
  - Instructor
  - Staff
  - Admin
- **Acceptance Criteria**: Each endpoint validates user role; only authorized roles granted access.

**SEC-3: Forbidden Resource Access Response**
- **Definition**: An authenticated user lacking permission to access a protected resource receives **403 Forbidden** and does not receive protected data.
- **Acceptance Criteria**: Unauthorized role/user returns 403; no resource data leaked.

**SEC-4: Unauthenticated Request Response**
- **Definition**: Requests without valid authentication credentials receive **401 Unauthorized** for protected endpoints.
- **Acceptance Criteria**: Missing or invalid token returns 401; error message doesn't reveal endpoint logic.

**SEC-5: Token Validation**
- **Definition**: Expired, malformed, revoked, or cryptographically invalid authentication tokens are **rejected**.
- **Acceptance Criteria**: Expired tokens return 401; tampered tokens rejected; revoked tokens detected.

---

### 5.2.6 Security - Material-Based Authorization

**SEC-6: Server-Side Material Authorization**
- **Definition**: Authorization checks for educational materials are **enforced server-side** based on the authenticated user's relationship to the associated course.
- **Acceptance Criteria**: User cannot access course material without enrollment; check performed server-side.

**SEC-7: Identifier Manipulation Prevention**
- **Definition**: A user cannot access protected educational material solely by **modifying a material ID, lesson ID, course ID, or URL**.
- **Acceptance Criteria**: Direct material ID requests are authorized; changing URL parameter returns 403.

**SEC-8: Consistent Authorization Enforcement**
- **Definition**: Direct requests for protected materials **enforce the same authorization rules** as UI-initiated requests.
- **Acceptance Criteria**: API endpoint `/api/lessons/materials/1/stream` and web UI both enforce identical authorization.

**SEC-9: Information Disclosure Prevention**
- **Definition**: Authorization failure for protected material does not reveal **whether an unauthorized material ID exists**.
- **Acceptance Criteria**: Unauthorized user gets same error (403) for both non-existent and inaccessible materials.

**SEC-10: Instructor Material Ownership**
- **Definition**: **Instructors** can only modify educational materials for courses for which they have ownership or management permission.
- **Acceptance Criteria**: Instructor A (course owner) can modify lessons; Instructor B cannot.

---

### 5.2.6 Security - Input Validation & Injection Protection

**SEC-11: Parameterized Queries**
- **Definition**: All user-controlled input used in database operations uses **parameterized queries or ORM-generated parameterization**.
- **Acceptance Criteria**: No string concatenation in SQL; all queries use parameterized statements or EF Core LINQ.

**SEC-12: Input Validation**
- **Definition**: User-controlled input is validated for **expected type, length, format, and permitted value ranges** before processing.
- **Acceptance Criteria**: Invalid inputs rejected with 400 Bad Request; validation enforced at API boundary.

**SEC-13: Sensitive Data Exclusion from Responses**
- **Definition**: The application does not return **SQL statements, database connection information, stack traces, or internal exception details** in production responses.
- **Acceptance Criteria**: Error responses contain user-friendly messages only; no technical details exposed.

---

### 5.2.7 Security - File Upload

**FILE-1: File Type Validation — Dual Check**
- **Definition**: Uploaded files are validated using **both declared file type and server-side file-content/type detection**.
- **Acceptance Criteria**: File extension and MIME type checked; malicious extensions rejected.

**FILE-2: File Size Validation**
- **Definition**: Files exceeding the configured maximum upload size are **rejected before being persisted**.
- **Acceptance Criteria**: Oversized files rejected with 400 Bad Request; no partial files stored.

**FILE-3: Filename Sanitization**
- **Definition**: Uploaded filenames are **sanitized or replaced with server-generated identifiers** before storage.
- **Acceptance Criteria**: User-supplied filenames not stored; system-generated IDs used.

**FILE-4: Storage Directory Isolation**
- **Definition**: The application **prevents uploaded files from being written outside the designated storage directory**.
- **Acceptance Criteria**: Path traversal attacks fail; files isolated in storage bucket.

**FILE-5: Executable File Rejection**
- **Definition**: Executable or otherwise prohibited file types are **rejected regardless of filename extension**.
- **Acceptance Criteria**: `.exe`, `.bat`, `.sh`, `.dll` files rejected; extension spoofing prevented.

**FILE-6: Non-Executable File Serving**
- **Definition**: Uploaded files are **not directly executable by the web server**.
- **Acceptance Criteria**: Files served as static assets (not interpreted); no code execution.

**FILE-7: Failed Upload Cleanup**
- **Definition**: **Failed or rejected uploads do not leave partially uploaded files** in permanent storage.
- **Acceptance Criteria**: Aborted uploads cleaned up automatically; no orphaned files.

**FILE-8: Protected File Authorization**
- **Definition**: Access to protected uploaded educational materials **requires server-side authorization** before file is returned or URL is generated.
- **Acceptance Criteria**: Unauthorized users cannot access download URLs; file URLs expire or require re-auth.

---

### 5.2.8 Security - XSS Protection

**XSS-1: HTML Context Encoding**
- **Definition**: User-generated text displayed in HTML pages is **contextually encoded** before rendering.
- **Acceptance Criteria**: HTML special characters encoded (`<` → `&lt;`, `>` → `&gt;`); scripts appear as text.

**XSS-2: Context-Aware Sanitization**
- **Definition**: User-controlled input is not inserted into **HTML, JavaScript, CSS, or URL contexts** without appropriate encoding/sanitization.
- **Acceptance Criteria**: Event handlers stripped; style injection prevented; URL parameters escaped.

**XSS-3: Stored XSS Neutralization**
- **Definition**: The application rejects or safely **neutralizes stored XSS payloads** submitted through user-controlled fields.
- **Acceptance Criteria**: `<script>alert(1)</script>` payload stored but displayed as plain text (not executed).

**XSS-4: API Response Serialization**
- **Definition**: API responses do **not contain executable HTML or JavaScript** generated from untrusted input.
- **Acceptance Criteria**: JSON responses contain escaped/sanitized text; no script tags in responses.

---

### 5.2.9 Security - CSRF Protection

**CSRF-1: Cookie-Based CSRF Protection**
- **Definition**: State-changing requests authenticated through **cookies implement CSRF protection**.
- **Acceptance Criteria**: POST/PUT/DELETE requests require CSRF token; token validated server-side.

**CSRF-2: Invalid Token Rejection**
- **Definition**: Requests containing an **invalid or missing CSRF token are rejected** with **4xx** response.
- **Acceptance Criteria**: Missing token returns 400; invalid token returns 400; request not processed.

**CSRF-3: Comprehensive State-Change Protection**
- **Definition**: CSRF protection is **applied to all applicable state-changing operations** including account updates, course modifications, admin actions, and all POST/PUT/PATCH/DELETE requests.
- **Acceptance Criteria**: All mutation endpoints protected; no exceptions.

**CSRF-4: SameSite Cookie Policy**
- **Definition**: Authentication cookies use an **appropriate SameSite policy** (Strict/Lax).
- **Acceptance Criteria**: `Set-Cookie` headers include `SameSite=Strict` or `SameSite=Lax`.

---

### 5.2.10 Security - Rate Limiting & Abuse Protection

**RATE-1: Authentication Endpoint Rate Limiting**
- **Definition**: Publicly accessible authentication endpoints are rate-limited to **10 requests per minute per client IP**.
- **Acceptance Criteria**: After 10 requests/min from same IP, further requests return 429 Too Many Requests.

**RATE-2: OTP Request Rate Limiting**
- **Definition**: OTP or verification-code requests are limited to **5 requests per 15 minutes** per account and IP.
- **Acceptance Criteria**: 6th OTP request within 15 min returns 429; counter reset after 15 min window.

**RATE-3: Password Reset Rate Limiting**
- **Definition**: Password-reset or account-recovery requests are limited to **5 requests per 15 minutes** per account and IP.
- **Acceptance Criteria**: 6th reset request within 15 min returns 429.

**RATE-4: Public Search Rate Limiting**
- **Definition**: Public search endpoints support a configurable rate limit of at least **60 requests per minute** per client IP.
- **Acceptance Criteria**: 61st request/min from same IP returns 429.

**RATE-5: Rate Limit Response Code**
- **Definition**: When rate limit is exceeded, API returns **429 Too Many Requests** and does **not process** the rejected request.
- **Acceptance Criteria**: Rejected requests return 429; operation not executed; state not changed.

**RATE-6: Rate Limit Error Message Security**
- **Definition**: Rate-limited endpoints do **not reveal sensitive account information** through error message differences or response timing.
- **Acceptance Criteria**: Same error message for "account exists" and "rate limited"; no enumeration possible.

---

### 5.2.12 Security - Privacy & Data Protection

**PR-1: PII Access Control**
- **Definition**: Access to **personally identifiable information** is restricted according to user role and resource authorization.
- **Acceptance Criteria**: User A cannot access User B's email, phone, or personal data.

**PR-2: Minimal PII Exposure**
- **Definition**: API responses expose **only the personal information required** for the requested operation.
- **Acceptance Criteria**: Profile endpoint returns: name, avatar (excludes: payment methods, login history, IP logs).

**PR-3: Sensitive Data Logging Prohibition**
- **Definition**: Passwords, authentication tokens, payment credentials, and API secrets are **not included in application logs**.
- **Acceptance Criteria**: Log grep for passwords/tokens returns 0 results.

**PR-4: PII Logging Restriction**
- **Definition**: Personally identifiable information is **not logged unless explicitly required** for security, auditing, or diagnosis.
- **Acceptance Criteria**: PII logged only for audit trails (e.g., account ID in login attempt, not password).

**PR-5: Encrypted Data Transmission**
- **Definition**: Personal data transmitted between client and server uses **HTTPS with TLS 1.2 or higher**.
- **Acceptance Criteria**: Production deployment enforces HTTPS; TLS version ≥ 1.2.

**PR-6: User Profile Data Isolation**
- **Definition**: Users cannot retrieve another user's **private profile information** by modifying account ID in request.
- **Acceptance Criteria**: User A tries to GET `/api/profile/[User-B-ID]` → 403 Forbidden.

**PR-7: Protected Information Access Control**
- **Definition**: Protected chat messages, reports, and personal information are **only accessible to authorized users and administrative roles**.
- **Acceptance Criteria**: Non-moderator cannot access pending reports; chat messages visible only to participants.

**PR-8: Payment Card Non-Storage**
- **Definition**: **Payment card information is not stored** in the PostgreSQL database.
- **Acceptance Criteria**: No credit card numbers, CVV, or full PAN in database; Stripe handles all card processing.

---

## SUMMARY TABLE

| Section | ID | Requirement | Type |
|---|---|---|---|
| External Interface | UI-1 to UI-3 | User Interface (3 reqs) | Assumed Met |
| External Interface | SI-1 to SI-6 | Software Interfaces (6 reqs) | Testable |
| Performance | PER-1 to PER-8 | Performance (8 reqs) | Testable |
| Load & Concurrency | LOAD-1 to LOAD-5 | Load & Concurrency (5 reqs) | Testable |
| Stress | STR-1 to STR-4 | Stress & Recovery (4 reqs) | Testable |
| Availability | AVL-1 to AVL-4 | Availability (4 reqs) | Testable |
| Recovery | REC-1 to REC-6 | Recovery & Fault Tolerance (6 reqs) | Mixed |
| Security: Auth | SEC-1 to SEC-10 | Authentication & Authorization (10 reqs) | Testable |
| Security: Injection | SEC-11 to SEC-13 | Input Validation (3 reqs) | Testable |
| Security: Files | FILE-1 to FILE-8 | File Upload (8 reqs) | Testable |
| Security: XSS | XSS-1 to XSS-4 | XSS Protection (4 reqs) | Testable |
| Security: CSRF | CSRF-1 to CSRF-4 | CSRF Protection (4 reqs) | Testable |
| Security: Rate Limit | RATE-1 to RATE-6 | Rate Limiting (6 reqs) | Testable |
| Privacy | PR-1 to PR-8 | Privacy (8 reqs) | Testable |
| **TOTAL** | | **79 Requirements** | **77 Testable** |

---

**End of NFR Definitions Document**