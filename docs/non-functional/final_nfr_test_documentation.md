# Non-Functional Requirements (NFR) Final Test Documentation

## 1. Introduction & Scope
**Objective:** Document the testing strategy, tools, and test results for the 79 active Non-Functional Requirements (77 testable) defined for the LinkedLearn platform.
**Scope:** `CourseMarketplaceBE` (.NET 8 Web API), `CourseMarketplaceFE` (.NET 8 MVC frontend), and `AIModeration` (FastAPI Python Service).
**Test Levels:** 
- **Integration Testing:** API boundary checks (Auth, Security, File rules).
- **System / Performance Testing:** Validating system resilience and latency under load.
- **Manual Verification:** Security payload inspection and UI visual validation.

---

## 2. Required Tools & Environment
### Required Tools

*Table 1. Required Tools*

| Purpose | Tool | Vendor/In-house | Version |
|---|---|---|---|
| Generating load, stress, and WebSocket connections | k6 | Grafana (Open Source) | Latest |
| Crafting API requests, payload injection, validation | Postman | Postman | Latest |
| Executing Chaos Engineering | Docker CLI / Desktop | Docker | Latest |
| Measuring page load, accessibility, responsiveness | Chrome DevTools | Google | Latest |
| Manual code inspection | Visual Studio / IDE | Microsoft | Latest |

### Required Environment

*Table 2. Required Environment*

| Purpose | Tool | Provider | Version |
|---|---|---|---|
| Application Framework | .NET 8.0 / FastAPI | Microsoft / Open Source | 8.0 / Latest |
| Local staging environment orchestration | docker-compose | Docker | Latest |
| Primary database with anonymized dataset | PostgreSQL | Open Source | Latest |
| Caching and rate-limit tracking | Redis | Open Source | Latest |

---

## 3. Testing Strategy & Methodology

### 3.1 Security, Validation, & Code Review (Postman & IDE)
- **Target NFRs:** SEC, XSS, CSRF, FILE, RATE, PR, SI
- **Method:** Use Postman to fire requests at high-risk endpoints (Login, Uploads, Course mutations). Test invalid JWTs, missing CSRF tokens, oversized `.exe` payloads, and trigger rate limits. 
- **Manual XSS:** Inject `<script>` payloads via Postman and verify backend sanitization.
- **Manual Code Review:** Inspect repositories to confirm EF Core LINQ parameterization (SEC-11) and review global error middleware to ensure no stack traces or passwords are logged (SI-3, PR-3).

### 3.2 Frontend UI & Visual Testing (Chrome DevTools)
- **Target NFRs:** UI-1 to UI-3, PER-3
- **Method:** Resize the Chrome window from 360px to 1920px. Use the `Tab` key exclusively to navigate the dashboard. Use the DevTools "Performance" tab to measure initial page load times.

### 3.3 Performance, Load, and Stress (k6)
- **Target NFRs:** PER, LOAD, STR
- **Method:** Execute a `k6` script (`script.js`) simulating 100 concurrent users looping through `GET /api/courses` and `GET /api/courses/{id}`. Ramp up to 200+ concurrent users to observe graceful degradation (429/503 responses).

### 3.4 Availability & Recovery (Docker CLI)
- **Target NFRs:** AVL, REC
- **Method:** While a low-intensity `k6` load runs, use `docker pause <db-container>` and `docker kill <backend-container>` to simulate sudden infrastructure loss and measure API recovery times upon container restart.

---

## 4. Test Cases

### User Interface
- **TC-01**
  - **Acceptance Criteria**: All primary navigation controls remain visible and functional across the entire viewport range.
  - **Test Procedure**: Verify Responsive 360px - 1920px using Chrome DevTools.
  - **Tool / Method**: Chrome DevTools
- **TC-02**
  - **Acceptance Criteria**: Navigation layout, button placement, and interaction flows are identical across authenticated sections.
  - **Test Procedure**: Verify Consistent Navigation using Visual Review.
  - **Tool / Method**: Visual Review
- **TC-03**
  - **Acceptance Criteria**: Tab key navigates through all interactive elements; focus is always visible; keyboard-only users can complete all tasks.
  - **Test Procedure**: Verify Keyboard Accessibility using Manual (Tab Key).
  - **Tool / Method**: Manual (Tab Key)


### Software Interface
- **TC-04**
  - **Acceptance Criteria**: All API endpoints follow REST conventions; responses return appropriate HTTP status codes (200, 201, 400, 401, 403, 404, 500, etc.).
  - **Test Procedure**: Verify RESTful HTTP Methods using Postman.
  - **Tool / Method**: Postman
- **TC-05**
  - **Acceptance Criteria**: All responses include header `Content-Type: application/json; charset=utf-8`; special characters properly encoded.
  - **Test Procedure**: Verify UTF-8 JSON Encoding using Postman.
  - **Tool / Method**: Postman
- **TC-06**
  - **Acceptance Criteria**: Error responses use consistent format; no `.cs` filenames, SQL statements, connection strings, or exception stack traces visible to client.
  - **Test Procedure**: Verify Secure Error Format using Postman.
  - **Tool / Method**: Postman
- **TC-07**
  - **Acceptance Criteria**: WebSocket connections establish successfully; bi-directional messaging works for notifications, chat, and moderation updates.
  - **Test Procedure**: Verify SignalR Connectivity using k6 WebSocket.
  - **Tool / Method**: k6 WebSocket
- **TC-08**
  - **Acceptance Criteria**: Request timeout configured; service responds within expected timeframe or fails gracefully.
  - **Test Procedure**: Verify AI Timeout Config using IDE Review.
  - **Tool / Method**: IDE Review
- **TC-09**
  - **Acceptance Criteria**: No credentials in HTML source, JavaScript bundles, or frontend configuration files; all secrets in environment variables or secure vaults.
  - **Test Procedure**: Verify Secure Config Storage using IDE Review.
  - **Tool / Method**: IDE Review


### Performance
- **TC-10**
  - **Acceptance Criteria**: Median response time ≤ 500ms; 95th percentile ≤ 500ms across representative operations (course browse, search, detail, profile access, cart, dashboard).
  - **Test Procedure**: Verify Response times for 95% of requests ≤ 500ms using k6 (100 concurrent users, 10m).
  - **Tool / Method**: k6 (100 concurrent users, 10m)
- **TC-11**
  - **Acceptance Criteria**: 99th percentile latency ≤ 2000ms; no outliers exceeding 3 seconds.
  - **Test Procedure**: Verify Response times for 99% of requests ≤ 2s using k6 (100 concurrent users, 10m).
  - **Tool / Method**: k6 (100 concurrent users, 10m)
- **TC-12**
  - **Acceptance Criteria**: LCP metric measured via browser dev tools or performance APIs; consistent across major pages.
  - **Test Procedure**: Verify Frontend Page Load Time ≤ 2.5s using Chrome DevTools.
  - **Tool / Method**: Chrome DevTools
- **TC-13**
  - **Acceptance Criteria**: Message latency (publish to receipt) ≤ 300ms for 95%+ of notifications under load.
  - **Test Procedure**: Verify SignalR Latency ≤ 300ms using k6 WebSocket.
  - **Tool / Method**: k6 WebSocket
- **TC-14**
  - **Acceptance Criteria**: Notification delivery latency ≤ 300ms for 95%+ of recipients; measured across 10+ concurrent connections.
  - **Test Procedure**: Verify Real-Time Notification using k6 WebSocket.
  - **Tool / Method**: k6 WebSocket
- **TC-15**
  - **Acceptance Criteria**: AI moderation job submitted and immediately returns response; actual processing happens in background queue.
  - **Test Procedure**: Verify Async AI Execution using Postman.
  - **Tool / Method**: Postman
- **TC-16**
  - **Acceptance Criteria**: Async job submission returns 201 Accepted with job ID within 1 second; 95%+ of requests meet this.
  - **Test Procedure**: Verify Async Job Ack (202) using Postman.
  - **Tool / Method**: Postman
- **TC-17**
  - **Acceptance Criteria**: PER-1 and PER-2 still met when 10 AI jobs running in parallel.
  - **Test Procedure**: Verify Concurrent AI Jobs using k6 + Postman.
  - **Tool / Method**: k6 + Postman


### Load & Concurency
- **TC-18**
  - **Acceptance Criteria**: 100 simultaneous connections; 0 connection errors; response times remain within PER-1/PER-2 thresholds.
  - **Test Procedure**: Verify 100 Concurrent Users using k6 (100 concurrent users).
  - **Tool / Method**: k6 (100 concurrent users)
- **TC-19**
  - **Acceptance Criteria**: 50 WebSocket connections established; all receive notifications within 300ms.
  - **Test Procedure**: Verify 50 SignalR Connections using k6 WebSocket.
  - **Tool / Method**: k6 WebSocket
- **TC-20**
  - **Acceptance Criteria**: 10 AI jobs running simultaneously; API still meets PER-1, PER-2.
  - **Test Procedure**: Verify 10 Concurrent AI Jobs using Python / Postman.
  - **Tool / Method**: Python / Postman
- **TC-21**
  - **Acceptance Criteria**: Connection pool size ≥ 100; no "connection pool exhaustion" errors under load.
  - **Test Procedure**: Verify 100 DB Connections using pgAdmin.
  - **Tool / Method**: pgAdmin
- **TC-22**
  - **Acceptance Criteria**: (2xx + 3xx responses) / Total requests ≥ 95%; errors are transient (4xx/5xx < 5%).
  - **Test Procedure**: Verify 95% Request Success using k6 (10m).
  - **Tool / Method**: k6 (10m)


### Stress
- **TC-23**
  - **Acceptance Criteria**: Process remains running; maintains functionality; response times degrade gracefully; error rate < 5%.
  - **Test Procedure**: Verify 200 Concurrent User Stress Load using k6 (200 concurrent users, 10m).
  - **Tool / Method**: k6 (200 concurrent users, 10m)
- **TC-24**
  - **Acceptance Criteria**: No unrecovered exceptions; database integrity verified after stress test; no orphaned records.
  - **Test Procedure**: Verify Data Integrity Under Stress using Manual DB Check.
  - **Tool / Method**: Manual DB Check
- **TC-25**
  - **Acceptance Criteria**: Overloaded endpoints return 429/503; process continues; no "500 Internal Server Error" from overload.
  - **Test Procedure**: Verify Graceful Overload Failure using k6 (400+ concurrent users).
  - **Tool / Method**: k6 (400+ concurrent users)
- **TC-26**
  - **Acceptance Criteria**: After load stops, API responds normally (< 500ms) within 5 minutes; no manual recovery needed.
  - **Test Procedure**: Verify Post-Stress Recovery using k6 (Post-stress).
  - **Tool / Method**: k6 (Post-stress)


### Availability
- **TC-27**
  - **Acceptance Criteria**: `GET /api/health` returns 200 OK with JSON showing status of each dependency; identifies failures.
  - **Test Procedure**: Verify Health Check Endpoint using Postman.
  - **Tool / Method**: Postman
- **TC-28**
  - **Acceptance Criteria**: If Cloudinary/external service fails, API continues; graceful error returned to client; process stays alive.
  - **Test Procedure**: Verify Non-Critical Resilience using Docker CLI.
  - **Tool / Method**: Docker CLI
- **TC-29**
  - **Acceptance Criteria**: External service timeouts ≤ 30 seconds; client receives error message; API doesn't hang.
  - **Test Procedure**: Verify Dependency Timeouts using Docker CLI.
  - **Tool / Method**: Docker CLI
- **TC-30**
  - **Acceptance Criteria**: Log entry created ≤ 5 seconds after failure detected; log entry includes timestamp, failure type, and context.
  - **Test Procedure**: Verify Critical Failure Logging using Docker Logs.
  - **Tool / Method**: Docker Logs


### Recovery
- **TC-31**
  - **Acceptance Criteria**: Automated backup runs on defined schedule; backup file created daily.
  - **Test Procedure**: Verify DB Backup Frequency using Cron check.
  - **Tool / Method**: Cron check
- **TC-32**
  - **Acceptance Criteria**: Full database restore from backup completes in ≤ 4 hours.
  - **Test Procedure**: Verify DB Restoration Time using CLI.
  - **Tool / Method**: CLI
- **TC-33**
  - **Acceptance Criteria**: Failed moderation job transitions to retry queue or manual review queue within 30 seconds.
  - **Test Procedure**: Verify AI Failure Handling using Docker CLI.
  - **Tool / Method**: Docker CLI
- **TC-34**
  - **Acceptance Criteria**: Job retry count ≤ 3; after 3 failures, job status = "manual_review".
  - **Test Procedure**: Verify AI Retry Policy using CLI logs.
  - **Tool / Method**: CLI logs
- **TC-35**
  - **Acceptance Criteria**: After container restart, API accessible and responsive within 5 minutes.
  - **Test Procedure**: Verify API Crash Recovery using Docker CLI.
  - **Tool / Method**: Docker CLI
- **TC-36**
  - **Acceptance Criteria**: Retrying same operation twice produces identical result; only one record created.
  - **Test Procedure**: Verify Idempotent Operations using Postman.
  - **Tool / Method**: Postman


### Authentication & Authorization
- **TC-37**
  - **Acceptance Criteria**: All protected endpoints require valid JWT token; unauthenticated requests return 401 Unauthorized.
  - **Test Procedure**: Verify Auth Enforcement using Postman.
  - **Tool / Method**: Postman
- **TC-38**
  - **Acceptance Criteria**: Each endpoint validates user role; only authorized roles granted access.
  - **Test Procedure**: Verify Role-Based Auth using Postman.
  - **Tool / Method**: Postman
- **TC-39**
  - **Acceptance Criteria**: Unauthorized role/user returns 403; no resource data leaked.
  - **Test Procedure**: Verify Forbidden Resource (403) using Postman.
  - **Tool / Method**: Postman
- **TC-40**
  - **Acceptance Criteria**: Missing or invalid token returns 401; error message doesn't reveal endpoint logic.
  - **Test Procedure**: Verify Unauthenticated (401) using Postman.
  - **Tool / Method**: Postman
- **TC-41**
  - **Acceptance Criteria**: Expired tokens return 401; tampered tokens rejected; revoked tokens detected.
  - **Test Procedure**: Verify Token Validation using Postman.
  - **Tool / Method**: Postman

### Material-Based Authorization
- **TC-42**
  - **Acceptance Criteria**: User cannot access course material without enrollment; check performed server-side.
  - **Test Procedure**: Verify Material Auth Server-Side using Postman.
  - **Tool / Method**: Postman
- **TC-43**
  - **Acceptance Criteria**: Direct material ID requests are authorized; changing URL parameter returns 403.
  - **Test Procedure**: Verify Prevent ID Manipulation using Postman.
  - **Tool / Method**: Postman
- **TC-44**
  - **Acceptance Criteria**: API endpoint `/api/lessons/materials/1/stream` and web UI both enforce identical authorization.
  - **Test Procedure**: Verify Consistent Auth using Postman.
  - **Tool / Method**: Postman
- **TC-45**
  - **Acceptance Criteria**: Unauthorized user gets same error (403) for both non-existent and inaccessible materials.
  - **Test Procedure**: Verify Info Disclosure Prevent using Postman.
  - **Tool / Method**: Postman
- **TC-46**
  - **Acceptance Criteria**: Instructor A (course owner) can modify lessons; Instructor B cannot.
  - **Test Procedure**: Verify Instructor Ownership using Postman.
  - **Tool / Method**: Postman

### Input Validation & Injection Protection
- **TC-47**
  - **Acceptance Criteria**: No string concatenation in SQL; all queries use parameterized statements or EF Core LINQ.
  - **Test Procedure**: Verify Parameterized Queries using IDE Review.
  - **Tool / Method**: IDE Review
- **TC-48**
  - **Acceptance Criteria**: Invalid inputs rejected with 400 Bad Request; validation enforced at API boundary.
  - **Test Procedure**: Verify Input Validation (400) using Postman.
  - **Tool / Method**: Postman
- **TC-49**
  - **Acceptance Criteria**: Error responses contain user-friendly messages only; no technical details exposed.
  - **Test Procedure**: Verify Sens. Data Exclusion using Postman.
  - **Tool / Method**: Postman


### Privacy & Data Protection
- **TC-50**
  - **Acceptance Criteria**: User A cannot access User B's email, phone, or personal data.
  - **Test Procedure**: Verify PII Access Control using Postman.
  - **Tool / Method**: Postman
- **TC-51**
  - **Acceptance Criteria**: Profile endpoint returns: name, avatar (excludes: payment methods, login history, IP logs).
  - **Test Procedure**: Verify Minimal PII Exposure using Postman.
  - **Tool / Method**: Postman
- **TC-52**
  - **Acceptance Criteria**: Log grep for passwords/tokens returns 0 results.
  - **Test Procedure**: Verify Zero Sensitive Logging using IDE Search.
  - **Tool / Method**: IDE Search
- **TC-53**
  - **Acceptance Criteria**: PII logged only for audit trails (e.g., account ID in login attempt, not password).
  - **Test Procedure**: Verify PII Logging Restriction using Log Review.
  - **Tool / Method**: Log Review
- **TC-54**
  - **Acceptance Criteria**: Production deployment enforces HTTPS; TLS version ≥ 1.2.
  - **Test Procedure**: Verify Encrypted Transmission using Browser.
  - **Tool / Method**: Browser
- **TC-55**
  - **Acceptance Criteria**: User A tries to GET `/api/profile/[User-B-ID]` → 403 Forbidden.
  - **Test Procedure**: Verify User Data Isolation using Postman.
  - **Tool / Method**: Postman
- **TC-56**
  - **Acceptance Criteria**: Non-moderator cannot access pending reports; chat messages visible only to participants.
  - **Test Procedure**: Verify Protected Info Control using Postman.
  - **Tool / Method**: Postman
- **TC-57**
  - **Acceptance Criteria**: No credit card numbers, CVV, or full PAN in database; Stripe handles all card processing.
  - **Test Procedure**: Verify Payment Non-Storage using DB Query.
  - **Tool / Method**: DB Query


### File-Upload Security
- **TC-58**
  - **Acceptance Criteria**: File extension and MIME type checked; malicious extensions rejected.
  - **Test Procedure**: Verify Dual File Type Check using Postman.
  - **Tool / Method**: Postman
- **TC-59**
  - **Acceptance Criteria**: Oversized files rejected with 400 Bad Request; no partial files stored.
  - **Test Procedure**: Verify File Size Validation using Postman.
  - **Tool / Method**: Postman
- **TC-60**
  - **Acceptance Criteria**: User-supplied filenames not stored; system-generated IDs used.
  - **Test Procedure**: Verify Filename Sanitization using Postman.
  - **Tool / Method**: Postman
- **TC-61**
  - **Acceptance Criteria**: Path traversal attacks fail; files isolated in storage bucket.
  - **Test Procedure**: Verify Dir Isolation using Postman.
  - **Tool / Method**: Postman
- **TC-62**
  - **Acceptance Criteria**: `.exe`, `.bat`, `.sh`, `.dll` files rejected; extension spoofing prevented.
  - **Test Procedure**: Verify Executable Rejection using Postman.
  - **Tool / Method**: Postman
- **TC-63**
  - **Acceptance Criteria**: Files served as static assets (not interpreted); no code execution.
  - **Test Procedure**: Verify Non-Executable Serve using Browser.
  - **Tool / Method**: Browser
- **TC-64**
  - **Acceptance Criteria**: Aborted uploads cleaned up automatically; no orphaned files.
  - **Test Procedure**: Verify Upload Cleanup using Cloudinary.
  - **Tool / Method**: Cloudinary
- **TC-65**
  - **Acceptance Criteria**: Unauthorized users cannot access download URLs; file URLs expire or require re-auth.
  - **Test Procedure**: Verify Protected File Auth using Postman.
  - **Tool / Method**: Postman


### XSS Protection
- **TC-66**
  - **Acceptance Criteria**: HTML special characters encoded (`<` → `&lt;`, `>` → `&gt;`); scripts appear as text.
  - **Test Procedure**: Verify HTML Context Encoding using Postman.
  - **Tool / Method**: Postman
- **TC-67**
  - **Acceptance Criteria**: Event handlers stripped; style injection prevented; URL parameters escaped.
  - **Test Procedure**: Verify Context Sanitization using Postman.
  - **Tool / Method**: Postman
- **TC-68**
  - **Acceptance Criteria**: `<script>alert(1)</script>` payload stored but displayed as plain text (not executed).
  - **Test Procedure**: Verify Stored XSS Neutralized using Postman.
  - **Tool / Method**: Postman
- **TC-69**
  - **Acceptance Criteria**: JSON responses contain escaped/sanitized text; no script tags in responses.
  - **Test Procedure**: Verify API Serialization using Postman.
  - **Tool / Method**: Postman


### CSRF Protection
- **TC-70**
  - **Acceptance Criteria**: POST/PUT/DELETE requests require CSRF token; token validated server-side.
  - **Test Procedure**: Verify Cookie CSRF Protect using Postman.
  - **Tool / Method**: Postman
- **TC-71**
  - **Acceptance Criteria**: Missing token returns 400; invalid token returns 400; request not processed.
  - **Test Procedure**: Verify Invalid Token Reject using Postman.
  - **Tool / Method**: Postman
- **TC-72**
  - **Acceptance Criteria**: All mutation endpoints protected; no exceptions.
  - **Test Procedure**: Verify State-Change Protect using IDE Review.
  - **Tool / Method**: IDE Review
- **TC-73**
  - **Acceptance Criteria**: `Set-Cookie` headers include `SameSite=Strict` or `SameSite=Lax`.
  - **Test Procedure**: Verify SameSite Cookie using Browser.
  - **Tool / Method**: Browser


### Rate Limiting
- **TC-74**
  - **Acceptance Criteria**: After 10 requests/min from same IP, further requests return 429 Too Many Requests.
  - **Test Procedure**: Verify Auth Rate Limiting using Postman.
  - **Tool / Method**: Postman
- **TC-75**
  - **Acceptance Criteria**: 6th OTP request within 15 min returns 429; counter reset after 15 min window.
  - **Test Procedure**: Verify OTP Rate Limiting using Postman.
  - **Tool / Method**: Postman
- **TC-76**
  - **Acceptance Criteria**: 6th reset request within 15 min returns 429.
  - **Test Procedure**: Verify Password Reset Limit using Postman.
  - **Tool / Method**: Postman
- **TC-77**
  - **Acceptance Criteria**: 61st request/min from same IP returns 429.
  - **Test Procedure**: Verify Public Search Limit using Postman.
  - **Tool / Method**: Postman
- **TC-78**
  - **Acceptance Criteria**: Rejected requests return 429; operation not executed; state not changed.
  - **Test Procedure**: Verify Rate Limit 429 Code using Postman.
  - **Tool / Method**: Postman
- **TC-79**
  - **Acceptance Criteria**: Same error message for "account exists" and "rate limited"; no enumeration possible.
  - **Test Procedure**: Verify Rate Limit MSG Security using Postman.
  - **Tool / Method**: Postman


## 5. Test Results

### User Interface
- **TC-01**: **Passed**: Layout stacks correctly at 360px; navigation shifts to hamburger menu seamlessly.
- **TC-02**: **Passed**: Header/footer and sidebar behaviors match across Admin and User roles.
- **TC-03**: **Passed**: All interactive elements receive `focus` ring. Forms submit via Enter.

### Software Interface
- **TC-04**: **Passed**: Endpoints correctly map GET, POST, PUT, DELETE. Returns standard 200/201/204.
- **TC-05**: **Passed**: `Content-Type: application/json; charset=utf-8` present on all Web API responses.
- **TC-06**: **Passed**: Forced exception via invalid ID returned `{"success":false,"message":"Error"}`. No stack trace.
- **TC-07**: **Passed**: Bi-directional Hub connected successfully; messages echoed back.
- **TC-08**: **Passed**: Confirmed `HttpClient.Timeout` properly configured for AI backend.
- **TC-09**: **Passed**: DB connections and JWT keys stored exclusively in `appsettings.json` / Environment Vars.

### Performance
- **TC-10**: **Passed**: Median: `112ms`, 95% of requests completed under `245ms` across 15,000 requests.
- **TC-11**: **Passed**: 99% of requests completed under `512ms`. No outliers exceeded 2s.
- **TC-12**: **Passed**: Measured initial page load time on `/Course/Learn` at `1.8s` (Network throttling: Fast 4G).
- **TC-13**: **Passed**: Ping-Pong message latency for 95% of messages measured at `85ms`.
- **TC-14**: **Passed**: Event dispatch to client receipt latency for 95% of messages measured at `92ms`.
- **TC-15**: **Passed**: `POST /api/course/moderate` returned instantly while backend processed data.
- **TC-16**: **Passed**: Returned `202 Accepted` with valid GUID `JobId` in `120ms`.
- **TC-17**: **Passed**: Spammed 10 AI Jobs. Regular API response time for 95% of requests remained at `252ms` (meets PER-1).

### Load & Concurency
- **TC-18**: **Passed**: Zero connection resets. 0% HTTP 500s.
- **TC-19**: **Passed**: 50 persistent connections maintained without dropping.
- **TC-20**: **Passed**: AI service successfully handled 10 concurrent requests; queues managed effectively.
- **TC-21**: **Passed**: DB pool maxed at 82 active connections under 100 concurrent user load. No exhaustion errors.
- **TC-22**: **Passed**: 100% success rate (15,000 / 15,000 requests returned 200 OK).

### Stress
- **TC-23**: **Passed**: Latency degraded slightly (95% of requests completed under `850ms`), but system did not crash. Error rate 0%.
- **TC-24**: **Passed**: Examined orders/enrollments generated during STR-1; zero orphaned records.
- **TC-25**: **Passed**: Endpoints began returning `429 Too Many Requests`. Process remained stable.
- **TC-26**: **Passed**: Once concurrent users dropped to 50, latency immediately returned to `~150ms`.

### Availability
- **TC-27**: **Passed**: `GET /api/health` returned 200 OK with `{"db":"healthy", "redis":"healthy"}`.
- **TC-28**: **Passed**: Paused Redis. Cart operations failed gracefully with UI alert; core system stayed up.
- **TC-29**: **Passed**: Paused AI Backend. Moderation submission returned controlled error in exactly 30s.
- **TC-30**: **Passed**: DB disconnection logged immediately (`[Error] Npgsql.NpgsqlException`) with context.

### Recovery
- **TC-31**: **Passed**: Verified `crontab` config orchestrating daily `pg_dump`.
- **TC-32**: **Passed**: Restored 500MB test dump to a fresh container in 45 seconds (well under 4 hrs).
- **TC-33**: **Passed**: Paused AI Backend. Submissions transitioned to "Pending Retry" in queue.
- **TC-34**: **Passed**: Verified logs indicating 3 failed retry attempts before marking "manual_review".
- **TC-35**: **Passed**: `docker kill linked-backend-1`. Container auto-restarted and was healthy in 8 seconds.
- **TC-36**: **Passed**: Fired identical payment confirmation webhook twice; only one enrollment created.

### Authentication & Authorization
- **TC-37**: **Passed**: Request to `/api/user/profile` without token -> `401 Unauthorized`.
- **TC-38**: **Passed**: Request to admin endpoint with 'user' token -> `403 Forbidden`.
- **TC-39**: **Passed**: Instructor attempting to edit another instructor's course -> `403 Forbidden`.
- **TC-40**: **Passed**: Verified standard 401 response without leaking backend logic.
- **TC-41**: **Passed**: Modified 1 character in JWT signature -> `401 Unauthorized`.

### Material-Based Authorization
- **TC-42**: **Passed**: Direct API call to material without course enrollment -> `403 Forbidden`.
- **TC-43**: **Passed**: Changed `lessonId=1` to `lessonId=2` (unowned) -> `403 Forbidden`.
- **TC-44**: **Passed**: Verified identical auth requirements on both web controller and API controller.
- **TC-45**: **Passed**: Requesting non-existent protected material returned 403, identical to unauthorized access.
- **TC-46**: **Passed**: Instructor B attempt to delete Instructor A's lesson -> `403 Forbidden`.

### Input Validation & Injection Protection
- **TC-47**: **Passed**: Confirmed 100% usage of EF Core LINQ which auto-parameterizes all inputs.
- **TC-48**: **Passed**: Sent `price: -10` -> `400 Bad Request` via FluentValidation.
- **TC-49**: **Passed**: Caused DB foreign key error; API returned generic 500 without Postgres exception details.

### Privacy & Data Protection
- **TC-50**: **Passed**: User B cannot fetch `/api/user/UserA_ID/profile`. Returns 403.
- **TC-51**: **Passed**: Public profile API only returns Name, Avatar, and Bio.
- **TC-52**: **Passed**: Audited GlobalExceptionMiddleware. No passwords/secrets mapped to log outputs.
- **TC-53**: **Passed**: Audit trail only logs `UserId` for login failures, not the attempted email/password.
- **TC-54**: **Passed**: Ngrok/Production environment strictly enforces HTTPS (TLS 1.3).
- **TC-55**: **Passed**: Replaced JWT User ID with target ID; signature validation failed immediately.
- **TC-56**: **Passed**: Only Admin role can query `/api/report` endpoints.
- **TC-57**: **Passed**: Searched DB tables. No columns exist for Credit Card PAN or CVV.

### File-Upload Security
- **TC-58**: **Passed**: Renamed `.png` to `.txt`; rejected due to MIME/Extension mismatch.
- **TC-59**: **Passed**: Uploaded 15MB file; rejected by controller size limits (`400 Bad Request`).
- **TC-60**: **Passed**: Uploaded `drop_table.png`. File saved to Cloudinary under generic GUID identifier.
- **TC-61**: **Passed**: Attempted `../../../etc/passwd` path traversal in filename; sanitized completely.
- **TC-62**: **Passed**: Uploaded `malware.exe` renamed to `image.png`. Magic byte check rejected it.
- **TC-63**: **Passed**: Uploaded files are served by Cloudinary CDN safely as static assets.
- **TC-64**: **Passed**: Verified orphaned/failed multipart uploads do not persist in DB.
- **TC-65**: **Passed**: Direct URL to protected material enforces JWT check before proxying byte stream.

### XSS Protection
- **TC-66**: **Passed**: Saved review with `<script>`. Rendered safely in UI as `&lt;script&gt;`.
- **TC-67**: **Passed**: Injected `javascript:alert()` into URL fields; backend validation rejected payload.
- **TC-68**: **Passed**: Payload saved to DB, but successfully neutralized by MVC Razor HTML encoding upon render.
- **TC-69**: **Passed**: JSON payload serialized standardly; special characters properly escaped by `System.Text.Json`.

### CSRF Protection
- **TC-70**: **Passed**: `[AutoValidateAntiforgeryToken]` enforces validation on all POST/PUT/DELETEs.
- **TC-71**: **Passed**: Sent manipulated `RequestVerificationToken` -> `400 Bad Request`.
- **TC-72**: **Passed**: Verified global setup applies CSRF checks to all mutation actions.
- **TC-73**: **Passed**: Authenticated cookies issued with `SameSite=Strict`.

### Rate Limiting
- **TC-74**: **Passed**: 11th login attempt within 1 minute from same IP -> `429 Too Many Requests`.
- **TC-75**: **Passed**: Verified fixed window allows exactly 5 verification requests / 15 mins.
- **TC-76**: **Passed**: Verified fixed window allows exactly 5 reset requests / 15 mins.
- **TC-77**: **Passed**: 61st search query within 1 minute from same IP -> `429 Too Many Requests`.
- **TC-78**: **Passed**: Verified system returns HTTP 429 status code accurately.
- **TC-79**: **Passed**: Rate limit messages do not leak existence of specific user emails/accounts.

---
**End of Document**
