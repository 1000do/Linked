# Sync `5.sql` and `AppDbContext.cs`

The SQL script has drifted from the EF Core model. This plan covers every delta between the two files and describes the exact changes needed.

## Proposed Changes

### [MODIFY] [5.sql](file:///c:/Users/anhkc/Desktop/Linked/db_scripts/5.sql)

---

### 1. Replace `material_embeddings` with `text_embeddings` + `media_embeddings`

The DbContext maps two separate embedding tables, but the SQL only has a single `material_embeddings` table.

**SQL (current — lines 234–239):**
```sql
CREATE TABLE material_embeddings (
    embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    embedding vector(768),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**DbContext ([text_embeddings](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs#L1043-L1067) + [media_embeddings](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs#L1069-L1093)):**

**Replace with:**
```sql
CREATE TABLE text_embeddings (
    text_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    text_embedding vector(768),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE media_embeddings (
    media_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    media_embedding vector(512),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```
---
### [MODIFY] [AppDbContext.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs)
---

### 2. Split `transaction_exts` into `transactions`

The DbContext maps `RefundReason`, `RefundAdminNote`, `RefundRequestedAt` directly on the [Transaction entity](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/Transaction.cs#L36-L40) and [maps them in transactions config](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs#L898-L902). There is no `TransactionExt` entity or DbSet.

**Current SQL has two objects:**
- `transactions` table (lines 341–354)
- `transaction_exts` table (lines 356–361)

**Change:**
- Add columns `refund_reason TEXT`, `refund_admin_note TEXT`, `refund_requested_at TIMESTAMP` to the `transactions` table
- Remove the `transaction_exts` table entirely
- Add `DROP TABLE IF EXISTS transaction_exts CASCADE;` to the drop section

---

### 3. Replace 3 report tables with single `user_reports`

The DbContext has a single [UserReport entity](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/UserReport.cs) mapped to table `user_reports` with a generic `target_id` + `chat_id` FK. The SQL has 3 separate tables: `course_reports`, `course_review_reports`, `lesson_review_reports`.

**Remove (lines 456–496):**
- `course_reports`
- `course_review_reports`
- `lesson_review_reports`

**Replace with ([DbContext config](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs#L937-L983)):**
```sql
CREATE TABLE user_reports (
    report_id SERIAL PRIMARY KEY,
    reporter_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    target_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    resolver_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    reason VARCHAR(255),
    description TEXT,
    user_reports_status VARCHAR(50),
    resolution_note TEXT,
    resolved_at TIMESTAMP,
    chat_id INT REFERENCES chats(chat_id) ON DELETE SET NULL,
    access_granted_until TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

### 4. Remove `log_status` from moderation log tables

The 3 moderation log entities ([MessageModerationLog](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/MessageModerationLog.cs), [CourseReviewModerationLog](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/CourseReviewModerationLog.cs), [LessonReviewModerationLog](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/LessonReviewModerationLog.cs)) do **not** have a `LogStatus` property, and the DbContext does not map one. The SQL includes `log_status VARCHAR(50)` in all three.

**Remove `log_status` column from:**
- `message_moderation_logs` (line 566)
- `course_review_moderation_logs` (line 578)
- `lesson_review_moderation_logs` (line 590)

---

### 5. Update DROP TABLE section (top of file)

| Action | Table |
|--------|-------|
| **Remove** drop for | `material_embeddings` |
| **Remove** drop for | `course_reports` (doesn't exist in header, but the table itself is removed) |
| **Add** drop for | `text_embeddings` |
| **Add** drop for | `media_embeddings` |
| **Add** drop for | `transaction_exts` |
| **Add** drop for | `lockouts` |
| **Add** drop for | `enrollment_progress` |
| **Add** drop for | `instructor_payouts` |
| **Add** drop for | `chat_participants` |
| **Remove** drop for | `course_review_reports` (doesn't exist in header either) |
| **Remove** drop for | `lesson_review_reports` (doesn't exist in header either) |

> [!NOTE]
> The current drop section already references `user_reports` (line 9) but the table doesn't exist in the CREATE section. After this change it will be consistent. Drops for `course_reports`, `course_review_reports`, `lesson_review_reports` should be added to the drop section so old deployments get cleaned up.

---

### 6. Update views that reference removed tables

`view_course_stats`, `view_lesson_stats`, `view_user_stats`, `view_order_stats`, `view_instructor_stats` — these don't reference any of the changed tables, so **no view changes needed**.

---

### Summary of all changes

| # | What | Type |
|---|------|------|
| 1 | `material_embeddings` → `text_embeddings` + `media_embeddings` | Rename + Add table |
| 2 | Merge `transaction_exts` columns into `transactions`, delete `transaction_exts` | Merge + Delete table |
| 3 | 3 report tables → single `user_reports` | Replace tables |
| 4 | Remove `log_status` from 3 moderation log tables | Remove columns |
| 5 | Update DROP TABLE section at top of file | Housekeeping |

## Verification Plan

### Manual Verification
- Visually diff every `CREATE TABLE` in the updated SQL against its corresponding `modelBuilder.Entity<>` block to confirm column names, types, defaults, FK constraints, and ON DELETE behaviors all match.
- Confirm every `DbSet<T>` in the DbContext has a corresponding `CREATE TABLE` in the SQL.
- Confirm no extra tables exist in the SQL that don't have a corresponding entity.
