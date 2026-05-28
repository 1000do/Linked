# TODO - Fix Issues

## Plan
- [x] 1. Xóa entity `MaterialPHash` không còn dùng + xóa dòng `DROP TABLE` trong `db_scripts/3.sql`
- [x] 2. Fix Cookie Secure flag: BE `AuthController.cs` (dùng `Request.IsHttps`)
- [x] 3. Fix Cookie Secure flag: FE `AccountController.cs` (dùng `Request.IsHttps`)
- [x] 4. Fix Cookie Secure flag: FE `ApiClient.cs` (dùng `_ctx.HttpContext?.Request.IsHttps`)
- [x] 5. Fix CORS hardcoded: BE `Program.cs` (dùng biến `allowedOrigins` từ config)
- [x] 6. Rename file `UserProfileService .cs` -> `UserProfileService.cs`

