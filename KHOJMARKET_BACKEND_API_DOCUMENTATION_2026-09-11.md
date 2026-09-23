# KhojMarket Backend API Documentation
Updated: 2026-09-11

This document describes the KhojMarket backend implemented so far, including authentication, OTP, requirements, images, verification, favorites, seller inquiries, seller wallet/credits, admin credit adjustments, status rules, testing flow, database tables, and the next planned backend modules.

---

# 1. Project Overview

KhojMarket is a Pakistan-focused local buyer/seller marketplace.

Main flow:

```text
Buyer registers/logs in
        ↓
Buyer verifies phone with OTP
        ↓
Buyer creates requirement
        ↓
Requirement = admin_review_required
        ↓
Admin / future AI Agent reviews
        ↓
verified / rejected
        ↓
verified + active requirement appears publicly
        ↓
Verified seller sends inquiry
        ↓
Buyer views / accepts / rejects inquiry
        ↓
Future: buyer completes deal and selects seller
        ↓
Buyer ↔ Seller reviews
```

Backend stack:

```text
ASP.NET Core
.NET 10
EF Core 10
SQL Server Express
JWT Bearer Authentication
Swagger / OpenAPI
```

Development database:

```text
Server: DESKTOP-POQI31N\SQLEXPRESS
Database: KhojMarketDb
```

---

# 2. Main Database Tables

Current main tables:

```text
Users
Requirements
RequirementFields
RequirementImages
PhoneOtps
RequirementFavorites
SellerInquiries
SellerWallets
CreditTransactions
__EFMigrationsHistory
```

Future planned tables:

```text
MarketplaceDeals
BuyerReviews
SellerReviews
```

---

# 3. User Roles

Supported roles:

```text
buyer
seller
admin
```

JWT contains the role claim.

If a user's role is changed directly in SQL, login again to receive a fresh JWT containing the updated role.

---

# 4. Authentication

## 4.1 Register

```http
POST /api/auth/register
```

Buyer example:

```json
{
  "name": "Test Buyer",
  "email": "buyer@test.com",
  "phone": "03001234567",
  "password": "Test@123",
  "role": "buyer"
}
```

Seller example:

```json
{
  "name": "Test Seller",
  "email": "seller@test.com",
  "phone": "03001234567",
  "password": "Test@123",
  "role": "seller"
}
```

Seller inquiry requires:

```text
Role = seller
SellerVerificationStatus = verified
```

## 4.2 Login

```http
POST /api/auth/login
```

Example:

```json
{
  "email": "buyer@test.com",
  "password": "Test@123",
  "role": "buyer"
}
```

Use the returned JWT in Swagger Authorize.

## 4.3 Current User

```http
GET /api/auth/me
```

Authentication required.

---

# 5. Phone OTP Verification

Phone OTP is account-level phone verification. It is separate from requirement verification.

Buyer must have `PhoneVerified = true` before creating a requirement.

## 5.1 Send OTP

```http
POST /api/auth/otp/send
```

Development behavior:
- 6-digit OTP generated
- code stored hashed in DB
- expiry approximately 5 minutes
- previous unused OTP invalidated
- development response may include `developmentOtp`
- real SMS provider is not connected yet

## 5.2 Verify OTP

```http
POST /api/auth/otp/verify
```

Success:

```text
Users.PhoneVerified = true
PhoneOtps.UsedAt = current UTC time
```

Maximum failed attempts: 5.

Pakistan phone normalization includes:

```text
03xxxxxxxxx → +923xxxxxxxxx
```

---

# 6. Requirement Creation

Buyer must be logged in, have role `buyer`, and have verified phone.

## 6.1 Create Requirement

```http
POST /api/requirements
Content-Type: application/json
```

Example:

```json
{
  "title": "Honda Civic 2022 required",
  "category": "Vehicles",
  "subCategory": "Cars",
  "requirementType": "product",
  "budgetMin": 3000000,
  "budgetMax": 4500000,
  "country": "Pakistan",
  "city": "Karachi",
  "area": "Gulzar-e-Hijri",
  "postalCode": "75330",
  "fields": [
    {
      "fieldKey": "modelYear",
      "label": "Model Year",
      "value": "2022",
      "isRequired": true,
      "sortOrder": 1
    },
    {
      "fieldKey": "condition",
      "label": "Condition",
      "value": "Used",
      "isRequired": true,
      "sortOrder": 2
    }
  ]
}
```

New requirement defaults:

```text
Status = admin_review_required
ActivityStatus = active
```

Current customer-facing reference example:

```text
KM-REQ-A18A99FA
```

---

# 7. Requirement Images

```http
POST /api/requirements/{requirementId}/images
Content-Type: multipart/form-data
```

Rules:

```text
Allowed: jpg, jpeg, png, webp
Max images: 20
Current max size: 5 MB each
```

Development storage:

```text
wwwroot/uploads/requirements/<ReferenceNo>/
```

Database stores file path/metadata, not Base64 image data.

---

# 8. Requirement Verification

New requirement starts as:

```text
admin_review_required
```

Buyer does not manually verify requirement.

## 8.1 Admin Approve

```http
POST /api/admin/requirements/{requirementId}/approve
```

Role: `admin`

Result:

```text
Status = verified
VerificationMethod = admin
VerifiedAt = now
AdminApprovedBy = admin user id
AdminApprovedAt = now
RejectionReason = null
```

## 8.2 Admin Reject

```http
POST /api/admin/requirements/{requirementId}/reject
```

Example body:

```json
{
  "reason": "Requirement details are incomplete."
}
```

Allowed transitions:

```text
admin_review_required → verified
admin_review_required → rejected
```

Current pending-review endpoint does not allow `verified → rejected`.

---

# 9. Requirement Read APIs

Pagination:

```text
Default Page = 1
Default PageSize = 10
Max PageSize = 50
```

## 9.1 Public Requirements

```http
GET /api/requirements/public
```

No authentication required.

Only returns:

```text
Status = verified
ActivityStatus = active
IsDeleted = false
```

Filters:

```text
page
pageSize
search
category
requirementType
country
city
```

Example:

```http
GET /api/requirements/public?page=1&pageSize=10&city=Karachi&requirementType=product
```

Card/list response includes:

```text
Id
ReferenceNo
Title
Category
SubCategory
RequirementType
Status
ActivityStatus
BudgetMin
BudgetMax
Country
City
Area
CoverImagePath
InquiryCount
CreatedAt
```

## 9.2 Buyer Own Requirements

```http
GET /api/requirements/mine
```

Role: `buyer`

Optional filters:

```text
status
activityStatus
page
pageSize
```

## 9.3 Admin Pending Requirements

```http
GET /api/admin/requirements/pending
```

Role: `admin`

Returns only pending, non-deleted requirements.

---

# 10. Requirement Full Details

```http
GET /api/requirements/{requirementId}
```

Returns full requirement including dynamic fields and images.

Access:

```text
Owner can view own requirement.
Public user can view public verified + active requirement.
```

---

# 11. Requirement Activity

Verification and activity are separate.

## 11.1 Mark Inactive

```http
PATCH /api/requirements/{requirementId}/activity
```

Example:

```json
{
  "activityStatus": "inactive",
  "reason": "Paused for now"
}
```

## 11.2 Mark Active

```json
{
  "activityStatus": "active",
  "reason": null
}
```

Completed requirement cannot be reactivated.

---

# 12. Complete Requirement

```http
PATCH /api/requirements/{requirementId}/complete
```

Current example:

```json
{
  "reason": "Found what I needed",
  "sellerUserId": null
}
```

Result:

```text
Status = completed
ActivityStatus = inactive
CompletedAt = now
CompletionReason = supplied reason
```

Future deal module will require seller selection when completion happens through KhojMarket.

---

# 13. Delete Requirement

Soft delete:

```http
DELETE /api/requirements/{requirementId}
```

Example:

```json
{
  "reason": "Created by mistake"
}
```

Result:

```text
IsDeleted = true
DeletedAt = now
DeletionReason = reason
ActivityStatus = inactive
```

---

# 14. Edit Requirement

```http
PUT /api/requirements/{requirementId}
```

Role: buyer owner.

Editable fields include title, category, subcategory, type, budget, location, postal code, and dynamic fields.

Important rule:

```text
verified requirement edited → admin_review_required
rejected requirement edited → admin_review_required
```

Completed requirement cannot be edited.

---

# 15. Favorites / Heart

Favorite means bookmark/save only.

## 15.1 Add Favorite

```http
POST /api/requirements/favorites/{requirementId}
```

Example response:

```json
{
  "requirementId": "436c5103-8304-4fbf-96d9-396f67f970a0",
  "isFavorite": true
}
```

## 15.2 Remove Favorite

```http
DELETE /api/requirements/favorites/{requirementId}
```

## 15.3 Get My Favorites

```http
GET /api/requirements/favorites
```

Example:

```json
[
  "436c5103-8304-4fbf-96d9-396f67f970a0"
]
```

Unique rule:

```text
UserId + RequirementId = unique
```

---

# 16. Seller Inquiry

Seller inquiry is implemented and tested.

Seller requirements:

```text
Role = seller
SellerVerificationStatus = verified
```

Target requirement must be:

```text
verified
active
not deleted
```

One seller can send only one inquiry per requirement.

## 16.1 Send Inquiry

```http
POST /api/inquiries/requirements/{requirementId}
```

Example:

```json
{
  "message": "I have this product available.",
  "offeredPrice": 350000
}
```

Successful response example:

```json
{
  "id": "dafcd01a-4410-45ca-aa2a-a2eccd3f9b8c",
  "requirementId": "436c5103-8304-4fbf-96d9-396f67f970a0",
  "requirementReferenceNo": "KM-REQ-A18A99FA",
  "requirementTitle": "Honda Civic 2022 required",
  "buyerUserId": "a5735900-6a17-4e7d-8ff0-898f5a751159",
  "sellerUserId": "689f66f1-2118-45ff-a30d-5e387e682569",
  "sellerName": "Test Seller",
  "sellerPhone": "03001234567",
  "sellerEmail": "seller@test.com",
  "message": "I have this product available.",
  "offeredPrice": 350000,
  "status": "sent",
  "createdAt": "2026-09-11T11:07:54.6899207",
  "viewedAt": null,
  "respondedAt": null
}
```

## 16.2 Buyer Gets Requirement Inquiries

```http
GET /api/inquiries/requirement/{requirementId}
```

Role: buyer owner.

## 16.3 Seller Gets Own Inquiries

```http
GET /api/inquiries/seller/mine
```

Role: seller.

## 16.4 Mark Viewed

```http
PATCH /api/inquiries/{inquiryId}/viewed
```

Transition:

```text
sent → viewed
```

## 16.5 Accept Inquiry

```http
PATCH /api/inquiries/{inquiryId}/accept
```

Transitions:

```text
sent → accepted
viewed → accepted
```

Accepted means buyer wants to proceed; it does not yet mean deal completed.

## 16.6 Reject Inquiry

```http
PATCH /api/inquiries/{inquiryId}/reject
```

Transitions:

```text
sent → rejected
viewed → rejected
```

---

# 17. Inquiry Count

Requirement list response includes `InquiryCount` so UI can show:

```text
6 inquiries received
```

---

# 18. Seller Wallet / Credits

Wallet stores current seller balance.

`CreditTransactions` stores all credit history.

Current configuration:

```json
"MarketplaceCredits": {
  "InitialSellerCredits": 100,
  "InquiryCost": 10
}
```

Example:

```text
100 → inquiry → 90
90 → inquiry → 80
```

Insufficient credits block inquiry.

## 18.1 Seller Wallet

```http
GET /api/seller/wallet
```

Role: seller.

Example:

```json
{
  "balance": 90,
  "inquiryCost": 10,
  "canSendInquiry": true
}
```

## 18.2 Seller Credit Transactions

```http
GET /api/seller/wallet/transactions
```

Transaction types:

```text
initial_balance
inquiry_deduction
admin_credit
admin_deduction
refund
```

Inquiry creation + credit deduction must be one DB transaction.

---

# 19. Admin Seller Wallet / Credits

## 19.1 Admin View Seller Wallet

```http
GET /api/admin/sellers/{sellerUserId}/wallet
```

Role: admin.

## 19.2 Admin View Seller Credit History

```http
GET /api/admin/sellers/{sellerUserId}/wallet/transactions
```

Role: admin.

## 19.3 Admin Adjust Credits

```http
POST /api/admin/sellers/{sellerUserId}/wallet/adjust
```

Add credits:

```json
{
  "amount": 200,
  "reason": "Promotional seller credits"
}
```

Deduct credits:

```json
{
  "amount": -50,
  "reason": "Manual admin correction"
}
```

Rules:

```text
Amount > 0 → admin_credit
Amount < 0 → admin_deduction
Amount = 0 → invalid
Final balance cannot go below 0
Reason required
```

Audit field:

```text
PerformedByUserId
```

---

# 20. Status Reference

Requirement statuses:

```text
admin_review_required
verified
rejected
completed
```

Requirement activity:

```text
active
inactive
```

Inquiry statuses:

```text
sent
viewed
accepted
rejected
```

Seller verification statuses:

```text
not_started
pending
verified
rejected
```

---

# 21. Development Seller Setup

Register seller:

```http
POST /api/auth/register
```

```json
{
  "name": "Test Seller",
  "email": "seller@test.com",
  "phone": "03001234567",
  "password": "Test@123",
  "role": "seller"
}
```

Development-only SQL verification:

```sql
UPDATE Users
SET SellerVerificationStatus = 'verified'
WHERE Email = 'seller@test.com';
```

Then login again for a fresh seller JWT.

---

# 22. End-to-End Test Flow

```text
1. Register buyer
2. Login buyer
3. Send OTP
4. Verify OTP
5. Create requirement
6. Upload images
7. Login admin
8. Approve requirement
9. Verify it appears in public API
10. Register seller
11. Verify seller for development
12. Login seller
13. Check wallet
14. Send inquiry
15. Confirm credit deduction
16. Login buyer
17. Get requirement inquiries
18. Mark inquiry viewed
19. Accept/reject inquiry
20. Check InquiryCount
21. Favorite/unfavorite
22. Test inactive/active
23. Test edit -> admin_review_required
24. Test soft delete
25. Admin test seller wallet
26. Admin add/deduct credits
```

---

# 23. HTTP Status Meaning

```text
200 OK             success
204 No Content     success without body
400 Bad Request    validation/business-rule error
401 Unauthorized   missing/invalid/expired JWT
403 Forbidden      authenticated but role/access denied
404 Not Found      resource missing
500 Server Error   unexpected backend/DI/runtime issue
```

---

# 24. Core Business Rules

```text
Buyer must verify phone before creating requirement.
PhoneVerified comes only from successful OTP.
AI requirement verification must not set PhoneVerified.
New requirement starts as admin_review_required.
Public requirement must be verified + active + not deleted.
Verified requirement edit requires re-review.
Inactive does not remove verification.
Delete is soft delete.
Seller must be verified before inquiry.
One seller inquiry per requirement.
Inquiry acceptance does not mean purchase completed.
Credits are admin-adjustable.
Every credit change creates transaction history.
Seller balance cannot go below zero.
Max requirement images = 20.
Images are stored outside SQL DB.
```

---

# 25. Planned Deal Flow

```text
Seller sends inquiry
        ↓
Buyer views inquiry
        ↓
Buyer accepts inquiry
        ↓
Buyer/seller communicate
        ↓
Product/service received
        ↓
Buyer clicks Mark Complete
        ↓
Buyer selects "Found through KhojMarket seller"
        ↓
Buyer selects final seller
        ↓
MarketplaceDeal created
        ↓
Requirement completed
        ↓
Buyer/Seller reviews unlocked
```

---

# 26. Planned Reviews

Review is allowed only after a completed deal.

```text
Inquiry sent     → no review
Inquiry viewed   → no review
Inquiry accepted → no review yet
Deal completed   → reviews allowed
```

Then:

```text
Buyer → Seller review
Seller → Buyer review
```

One review per side per completed deal.

Buyer rating aggregates across completed transactions.

Example:

```text
5, 5, 4, 5, 3
Average = 4.4
```

UI:

```text
Ali Ahmed
★ 4.4 (5 reviews)
```

---

# 27. Planned Requirement Card UI Data

Public requirement card target:

```text
♡ Favorite
Verified Requirement
PRODUCT / SERVICE
Title
Cover Image
Dynamic Details
Location
Budget
Buyer Rating
Inquiry Count
Active
```

Buyer profile actions:

```text
View Details
Edit
Active / Inactive
Mark Complete
Delete
```

---

# 28. Main Backend Services

```text
UserService
JwtService
OtpService
RequirementService
RequirementReadService
RequirementLifecycleService
RequirementVerificationService
RequirementFavoriteService
SellerInquiryService
SellerCreditService
```

---

# 29. Important DI Registrations

Before `builder.Build()`:

```csharp
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<RequirementService>();
builder.Services.AddScoped<RequirementReadService>();
builder.Services.AddScoped<RequirementLifecycleService>();
builder.Services.AddScoped<RequirementVerificationService>();
builder.Services.AddScoped<RequirementFavoriteService>();
builder.Services.AddScoped<SellerInquiryService>();
builder.Services.AddScoped<SellerCreditService>();
```

---

# 30. Important Middleware Order

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

# 31. Known Migrations / Schema Milestones

Known migration names used during development include:

```text
InitialCreate
AddUserPasswordHash
AddPhoneOtp
AddRequirementVerification
AddRequirementLifecycle
AddRequirementFavorites
AddSellerInquiries
AddSellerWalletAndCredits
AddCreditAdminAudit
```

Some final migration names may differ depending on which model changes were bundled together.

Check actual applied migrations in:

```text
__EFMigrationsHistory
```

---

# 32. Useful SQL Queries

```sql
SELECT * FROM Users;
SELECT * FROM Requirements;
SELECT * FROM RequirementFields;
SELECT * FROM RequirementImages;
SELECT * FROM RequirementFavorites;
SELECT * FROM SellerInquiries;
SELECT * FROM SellerWallets;
SELECT * FROM CreditTransactions;
```

Verified active requirements:

```sql
SELECT
    Id,
    ReferenceNo,
    Title,
    Status,
    ActivityStatus,
    IsDeleted
FROM Requirements
WHERE Status = 'verified'
  AND ActivityStatus = 'active'
  AND IsDeleted = 0;
```

---

# 33. Frontend / Backend Architecture

Correct:

```text
React / Browser
        ↓ HTTPS
ASP.NET Core API
        ↓
SQL Server
```

Wrong:

```text
React / Browser → SQL Server directly
```

Backend owns security, validation, JWT, OTP, admin approval, seller verification, credits, inquiries, reviews, and DB access.

---

# 34. Next Implementation Order

```text
1. Confirm seller wallet + admin credit APIs fully tested
2. MarketplaceDeal entity
3. Complete requirement with final seller selection
4. Buyer → Seller review
5. Seller → Buyer review
6. Aggregate ratings
7. Seller profile / buyer rating APIs
8. Frontend API integration
9. Real SMS provider
10. AI requirement verification
11. Production image storage
```

---

# 35. Source of Truth for New Chat

Use this file as the backend source of truth.

Recommended new-chat message:

```text
KhojMarket continue karo.
Attached backend API documentation ko source of truth samjho.
Existing flow break mat karna.
Next pending backend step se continue karo.
```
