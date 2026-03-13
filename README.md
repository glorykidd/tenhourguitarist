# TenHourGuitarist — Blazor Server Migration Plan

## Overview

This document describes the complete migration of TenHourGuitarist from ASP.NET WebForms (.NET Framework 4.8) to a modern .NET 10 Blazor Server application. The old application had significant technical debt: WebForms code-behind pattern, raw ADO.NET SQL, MD5 password hashing, hardcoded API keys, no unit tests, and tightly coupled YAF forum integration. The new application is a ground-up rebuild with a fresh database schema.

**Status: Code complete. Builds with 0 errors, 0 warnings.**

---

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET / ASP.NET Core | 10.0 |
| UI Framework | Blazor Server (Interactive Server) | 10.0 |
| Component Library | MudBlazor | 8.15.0 |
| Public Site CSS | Bootstrap 5 (CDN) | 5.3 |
| Database | SQLite + EF Core | 10.0.2 |
| Authentication | ASP.NET Identity | 10.0.2 |
| Payments | Stripe (Checkout Sessions + Webhooks) | 50.3.0 |
| Email | MailKit via AWS SES SMTP | 4.14.1 |
| Rich Text Editor | TinyMCE.Blazor | 2.2.1 |
| Image Processing | SixLabors.ImageSharp | 3.1.12 |
| Logging | Serilog | 10.0.0 |
| Hosting | Self-hosted IIS (Windows) or cross-platform | — |

---

## Project Structure

```
TenHourGuitarist.Blazor/
├── TenHourGuitarist.Blazor.sln
├── .github/workflows/
│   ├── thg-blazor-dev.yaml          # Build-only on develop
│   ├── thg-blazor-stage.yaml        # Deploy to stage IIS
│   └── thg-blazor-prod.yaml         # Deploy to production IIS
└── src/TenHourGuitarist/
    ├── TenHourGuitarist.csproj
    ├── Program.cs                     # All service registrations, middleware
    ├── appsettings.json               # Base configuration
    ├── appsettings.Development.json   # Dev overrides
    ├── appsettings.Production.json    # Prod overrides
    ├── web.config                     # IIS configuration
    │
    ├── Data/
    │   ├── ApplicationDbContext.cs     # IdentityDbContext<ApplicationUser>, 13 DbSets
    │   ├── SeedData.cs                # Role + admin account seeding
    │   ├── Entities/                  # 15 entity classes + enums
    │   └── Configurations/            # 14 IEntityTypeConfiguration classes
    │
    ├── Services/
    │   ├── Interfaces/                # 14 service interfaces
    │   └── [14 implementations]       # CourseService, StripeService, etc.
    │
    ├── Controllers/
    │   └── StripeWebhookController.cs # POST /api/stripe/webhook
    │
    ├── Components/
    │   ├── App.razor                  # HTML shell, CSS/JS refs, render mode
    │   ├── Routes.razor               # AuthorizeRouteView + RedirectToLogin
    │   ├── _Imports.razor             # Global usings
    │   ├── Layout/                    # MainLayout, AdminLayout, NavMenu, etc.
    │   ├── Shared/                    # 9 reusable components
    │   └── Pages/                     # 45+ routable pages
    │       ├── Home/                  # Homepage
    │       ├── About/                 # About page
    │       ├── Contact/               # Contact form
    │       ├── Courses/               # Course catalog + detail
    │       ├── Lessons/               # Lesson viewer + free lessons
    │       ├── Blog/                  # Blog list + post
    │       ├── Podcasts/              # Podcast list + detail
    │       ├── Resources/             # Resource list + detail
    │       ├── Subscription/          # Packages, checkout success/cancel
    │       ├── Account/               # Login, register, profile, history
    │       └── Admin/                 # 24 admin CRUD pages
    │
    ├── EmailTemplates/                # 5 HTML email templates
    └── wwwroot/
        ├── css/site.css
        ├── js/site.js
        └── uploads/                   # User-uploaded content
```

### File Counts

| Category | Count |
|----------|-------|
| C# files (.cs) | 66 |
| Razor files (.razor) | 68 |
| Email templates (.html) | 5 |
| CI/CD workflows (.yaml) | 3 |
| **Total source files** | **142** |

---

## Data Model

### Entity Relationship Diagram

```
ApplicationUser (IdentityUser)
  ├── 1:N → Course (as Instructor)
  ├── 1:N → BlogPost (as Author)
  ├── 1:N → Podcast (as Author)
  ├── 1:N → FreeResource (as Author)
  ├── 1:N → LessonHistory
  ├── 1:N → Subscription
  └── 1:N → Order

Course ──1:N──→ Lesson ──1:N──→ LessonHistory
BlogCategory (self-referencing ParentId) ──1:N──→ BlogPost
Package ──1:N──→ Subscription
Package ──1:N──→ Order
```

### Entities (15 classes)

**ApplicationUser** — extends `IdentityUser`
- `DisplayName` (string, required, max 100)
- `ProfileImagePath` (string?, max 500)
- `CreatedAt` (DateTime, default datetime('now'))
- `IsDeleted` (bool, default false)
- Navigation: Courses, BlogPosts, Podcasts, FreeResources, LessonHistories, Subscriptions, Orders

**Course**
- `Id`, `Name`, `Slug` (unique index), `Description`, `CoverImagePath`, `TrailerUrl` (Vimeo)
- `Level` (enum: Beginner/Intermediate/Advanced, stored as string)
- `Price` (decimal 18,2), `ReleaseDate`, `DisplayOrder`, `IsFree`, `IsPublished`
- `InstructorId` (FK → ApplicationUser, restrict delete)
- Navigation: Instructor, Lessons (cascade delete)

**Lesson**
- `Id`, `CourseId` (FK → Course, cascade delete), `Name`, `Slug` (unique index)
- `Description`, `Subject`, `CoverImagePath`, `VideoUrl`, `FreeVideoUrl`
- `DurationMinutes`, `DurationSeconds`, `DisplayOrder`, `CreatedAt`
- Navigation: Course, LessonHistories

**LessonHistory**
- `Id`, `UserId` (FK), `LessonId` (FK), `CourseId` (FK), `ViewedAt`
- All FKs use restrict delete

**BlogPost**
- `Id`, `Slug` (unique), `Title`, `Content` (HTML), `FeaturedImagePath`
- `CategoryId` (FK → BlogCategory, restrict), `AuthorId` (FK → User, restrict)
- `PublishedDate`, `IsPublished`

**BlogCategory**
- `Id`, `Title`, `Slug` (unique), `Description`
- `ParentId` (self-referencing FK, restrict delete)
- Navigation: Parent, Children, BlogPosts

**Podcast**
- `Id`, `Name`, `Slug` (unique), `Description`, `AudioFileUrl`
- `AuthorId` (FK → User, restrict), `PublishedDate`, `IsPublished`

**FreeResource**
- `Id`, `Name`, `Slug` (unique), `Description`, `CoverImagePath`, `VideoUrl`
- `AuthorId` (FK → User, restrict), `CreatedAt`

**Package**
- `Id`, `Title`, `Subtitle`, `Description`, `Amount` (decimal 18,2)
- `DurationUnit` (enum: Month/Year), `DurationCount`, `Features` (pipe-delimited)
- `Slug` (unique), `StripePriceId`, `IsActive`, `IsDeleted` (soft delete)

**Subscription**
- `Id`, `UserId` (FK, restrict), `PackageId` (FK, restrict)
- `StripeSubscriptionId`, `Status` (enum: Active/Cancelled/Expired/PastDue)
- `StartDate`, `EndDate`

**Order**
- `Id`, `OrderRefId` (Guid, unique, auto-generated UUID)
- `UserId` (FK, restrict), `PackageId` (FK, restrict)
- `StripeSessionId`, `StripePaymentIntentId`
- `PaymentStatus` (enum: Pending/Success/Failed/Cancelled)
- `Amount` (decimal 18,2), `CreatedAt`, `PaymentDate`

**Subject** — `Id`, `Name`, `Description`

**ContactMessage** — `Id`, `Name`, `Email`, `Phone`, `Subject`, `Message`, `CreatedAt`

**SiteVisit** — `Id`, `Date` (DateOnly, unique index), `Count`

### EF Core Configuration

Each entity has a dedicated `IEntityTypeConfiguration<T>` class in `Data/Configurations/`. All configurations are auto-discovered via `ApplyConfigurationsFromAssembly`. Key patterns:
- Enums stored as strings (max 50 chars) for readability
- `datetime('now')` SQLite defaults for timestamps
- SQLite `randomblob()` UUID generation for Order.OrderRefId
- Unique indexes on all Slug fields
- Restrict delete on all user FKs (prevent cascade through Identity tables)
- Cascade delete only for Course → Lesson

---

## Authentication & Authorization

### Identity Configuration

```
Password:       8+ chars, uppercase, lowercase, digit, special character
Lockout:        15 minutes after 5 failed attempts
Email:          Confirmed email required
Cookie:         5-hour expiration with sliding renewal, HTTPS only
Login path:     /account/login
```

### Roles and Access Matrix

| Role | Public Site | Admin Dashboard |
|------|-------------|-----------------|
| **Admin** | Full access | Full access to all admin pages |
| **Instructor** | Full access | Courses + Lessons management only |
| **Subscriber** | Premium lesson videos, viewing history | No admin access |
| **Member** | Free content only | No admin access |

### Authorization Policies

| Policy | Requirement | Used By |
|--------|-------------|---------|
| `AdminOnly` | Role: Admin | Dashboard, Blog, Podcasts, Resources, Packages, Orders, Subjects, Users |
| `InstructorOrAdmin` | Role: Admin or Instructor | Courses, Lessons, Account Settings |
| `SubscriberAccess` | Role: Admin, Instructor, or Subscriber | Premium lesson access |
| `Authenticated` | Any authenticated user | Profile, History, Purchases |

### Subscription Gating

`LessonViewer.razor` checks `ISubscriptionService.HasActiveSubscriptionAsync(userId)` before rendering the Vimeo embed. Courses where `IsFree == true` bypass this check automatically.

### Seed Data

On startup, `SeedData.InitializeAsync` seeds:
- 4 roles: Admin, Instructor, Member, Subscriber
- Default admin: `admin@tenhourguitarist.com` / `Admin@123456`

---

## Stripe Payment Integration

### Architecture

Pre-configured Products/Prices in the Stripe Dashboard. Each `Package` entity stores a `StripePriceId`. Webhook-driven payment confirmation (not redirect-based).

### Checkout Flow

```
1. User clicks Subscribe on /subscribe/packages
2. StripeService.CreateCheckoutSessionAsync():
   - Creates Order record (status: Pending) in database
   - Creates Stripe Checkout Session (mode: subscription)
   - Returns Stripe checkout URL
3. User redirected to Stripe hosted checkout page
4. User completes payment
5. Stripe redirects to /subscribe/success (confirmation display)
6. Stripe fires checkout.session.completed webhook
7. POST /api/stripe/webhook → StripeWebhookController
8. StripeService.HandleWebhookAsync():
   - Updates Order to PaymentStatus.Success
   - Creates Subscription record (status: Active)
   - Assigns "Subscriber" role to user
   - Sends OrderConfirmation email
```

### Webhook Events Handled

| Event | Action |
|-------|--------|
| `checkout.session.completed` | Create subscription, update order, assign role, send email |
| `customer.subscription.updated` | Update subscription status and end date |
| `customer.subscription.deleted` | Mark subscription cancelled/expired, send cancellation email |
| `invoice.payment_failed` | Mark subscription as PastDue |

### Stripe.net v50 API Notes

The project uses Stripe.net v50.3.0 (API version 2025-03-31.basil). Key API differences from older versions:
- `Subscription.CurrentPeriodEnd` → `subscription.Items.Data[0].CurrentPeriodEnd`
- `Invoice.SubscriptionId` → `invoice.Parent.SubscriptionDetails.SubscriptionId`
- `Session.SubscriptionId` and `Session.PaymentIntentId` remain unchanged

---

## Service Layer

### 14 Services

| Service | Responsibility |
|---------|---------------|
| **CourseService** | CRUD, search with level filter, pagination |
| **LessonService** | CRUD, free lessons list, view recording, user history |
| **BlogService** | Post CRUD + category CRUD, pagination, author/category includes |
| **PodcastService** | CRUD with published filter, pagination |
| **FreeResourceService** | CRUD with pagination |
| **PackageService** | CRUD with soft delete (IsDeleted flag) |
| **SubscriptionService** | Active subscription check, Stripe ID lookup, status updates |
| **OrderService** | CRUD, lookups by OrderRefId/StripeSessionId/UserId |
| **SubjectService** | Simple CRUD |
| **ContactService** | Create + list with pagination |
| **AnalyticsService** | Visit upsert, dashboard metrics (users, subscribers, courses, revenue) |
| **FileStorageService** | Image upload (5MB, auto-resize >1200px) + audio upload (50MB), GUID filenames |
| **EmailService** | MailKit SMTP, HTML template loading with `{{placeholder}}` replacement |
| **StripeService** | Checkout Session creation, webhook handling for 4 event types |

All services are registered as `Scoped` in `Program.cs` and injected via interfaces.

---

## Component Architecture

### Two Layouts

**MainLayout.razor** (public site)
- Bootstrap 5 navbar via `NavMenu.razor`
- `FooterComponent.razor` with quick links
- MudBlazor providers (MudDialogProvider, MudSnackbarProvider, MudThemeProvider)

**AdminLayout.razor** (admin dashboard)
- MudBlazor `MudLayout` with `MudDrawer` sidebar and `MudAppBar`
- `AdminSidebar.razor` with role-conditional menu items
- `[Authorize]` attribute — requires authentication

### 9 Shared Components

| Component | Purpose |
|-----------|---------|
| `CourseCard.razor` | MudCard with course image, name, level chip, description |
| `VimeoPlayer.razor` | Responsive 16:9 iframe for Vimeo embeds |
| `AudioPlayer.razor` | HTML5 `<audio>` element with MIME type detection |
| `ImageUpload.razor` | MudFileUpload with preview, 5MB validation, server upload |
| `RichTextEditor.razor` | TinyMCE.Blazor wrapper with toolbar config |
| `ConfirmDialog.razor` | MudDialog for delete confirmations |
| `LoadingSpinner.razor` | MudProgressCircular with optional message |
| `StatusAlert.razor` | MudAlert with closeable option |
| `RedirectToLogin.razor` | Captures returnUrl, navigates to /account/login |

### Page Routes (69 routes across 45+ pages)

**Public (10 pages, 16 routes)**
| Route | Page | Description |
|-------|------|-------------|
| `/` | Home | Hero, featured courses grid |
| `/about` | About | Mission, features, approach |
| `/contact` | Contact | Contact form → IContactService |
| `/courses` | CourseList | Search, level filter, pagination |
| `/courses/{Slug}` | CourseDetail | Course info, trailer, lesson sidebar |
| `/courses/{CourseSlug}/lessons/{LessonSlug}` | LessonViewer | Subscription-gated video player |
| `/free-lessons` | FreeLessons | Free lesson card grid |
| `/blog` | BlogList | Paginated post list with excerpts |
| `/blog/{Slug}` | BlogPost | Full post with HTML content |
| `/podcasts` | PodcastList | Paginated list with inline audio |
| `/podcasts/{Slug}` | PodcastDetail | Full podcast with audio player |
| `/resources` | ResourceList | Paginated resource grid |
| `/resources/{Slug}` | ResourceDetail | Resource with optional video |
| `/subscribe/packages` | Packages | Pricing cards, Stripe checkout |
| `/subscribe/success` | CheckoutSuccess | Payment confirmation |
| `/subscribe/cancel` | CheckoutCancel | Payment cancelled |

**Account (9 pages)**
| Route | Page | Auth | Description |
|-------|------|------|-------------|
| `/account/login` | Login | No | Email/password + remember me |
| `/account/register` | Register | No | DisplayName, email, password |
| `/account/forgot-password` | ForgotPassword | No | Email enumeration safe |
| `/account/reset-password` | ResetPassword | No | Token-based password reset |
| `/account/verify-email` | VerifyEmail | No | Email confirmation via token |
| `/account/profile` | Profile | Yes | Edit profile, image, password |
| `/account/history` | ViewingHistory | Yes | Paginated lesson history |
| `/account/purchases` | PurchaseHistory | Yes | Subscriptions + orders |
| `/account/logout` | Logout | Yes | Sign out + redirect |

**Admin (24 pages, 43 routes)**
| Route(s) | Page | Policy |
|-----------|------|--------|
| `/admin` | Dashboard | AdminOnly |
| `/admin/account` | AccountAdmin | InstructorOrAdmin |
| `/admin/courses`, `/admin/courses/new`, `/admin/courses/{Id:int}` | Courses CRUD | InstructorOrAdmin |
| `/admin/lessons`, `/admin/lessons/new`, `/admin/lessons/{Id:int}` | Lessons CRUD | InstructorOrAdmin |
| `/admin/blog`, `/admin/blog/new`, `/admin/blog/{Id:int}` | Blog CRUD | AdminOnly |
| `/admin/blog/categories`, `.../new`, `.../{Id:int}` | Categories CRUD | AdminOnly |
| `/admin/podcasts`, `/admin/podcasts/new`, `/admin/podcasts/{Id:int}` | Podcasts CRUD | AdminOnly |
| `/admin/resources`, `/admin/resources/new`, `/admin/resources/{Id:int}` | Resources CRUD | AdminOnly |
| `/admin/packages`, `/admin/packages/new`, `/admin/packages/{Id:int}` | Packages CRUD | AdminOnly |
| `/admin/orders`, `/admin/orders/{Id:int}` | Orders (read-only) | AdminOnly |
| `/admin/subjects`, `/admin/subjects/new`, `/admin/subjects/{Id:int}` | Subjects CRUD | AdminOnly |
| `/admin/users`, `/admin/users/{UserId}` | Users management | AdminOnly |
| `/admin/instructors`, `/admin/instructors/new`, `/admin/instructors/{UserId}` | Instructor management | AdminOnly |

### Admin CRUD Pattern

All admin pages follow a consistent pattern:
- **List pages**: `MudDataGrid` with server-side data, search/filter, Edit/Delete action buttons
- **Form pages**: `MudForm` with validation; `[Parameter] int? Id` determines create vs edit
- **Delete**: `ConfirmDialog` via `IDialogService`, success/error via `ISnackbar`
- **Slug generation**: Auto-generated from Name/Title field
- **Images**: `ImageUpload.razor` with subfolder parameter

---

## Email System

### Templates (5 HTML files in `/EmailTemplates/`)

| Template | Placeholders | Purpose |
|----------|-------------|---------|
| `VerifyEmail.html` | `{{UserName}}`, `{{VerifyUrl}}`, `{{Year}}` | Account email confirmation |
| `ResetPassword.html` | `{{UserName}}`, `{{ResetUrl}}`, `{{Year}}` | Password reset link |
| `OrderConfirmation.html` | `{{UserName}}`, `{{PackageName}}`, `{{Amount}}`, `{{OrderRefId}}`, `{{Year}}` | Subscription purchase receipt |
| `OrderCancellation.html` | `{{UserName}}`, `{{PackageName}}`, `{{OrderRefId}}`, `{{Year}}` | Subscription cancelled |
| `ContactNotification.html` | `{{FromName}}`, `{{FromEmail}}`, `{{Subject}}`, `{{Message}}`, `{{Year}}` | Admin notification of contact form |

All templates use inline CSS, table-based layout, branded header, and responsive 600px max-width.

### SMTP Configuration

```json
{
  "Email": {
    "Host": "email-smtp.us-east-1.amazonaws.com",
    "Port": 587,
    "Username": "<AWS SES SMTP username>",
    "Password": "<AWS SES SMTP password>",
    "FromAddress": "noreply@tenhourguitarist.com",
    "FromName": "TenHourGuitarist"
  }
}
```

---

## File Upload System

- **Storage**: `wwwroot/uploads/{subfolder}/` with GUID filenames
- **Images**: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` — max 5MB, auto-resize if width >1200px via ImageSharp
- **Audio**: `.mp3`, `.wav`, `.ogg`, `.m4a` — max 50MB
- **IIS limit**: `web.config` sets `maxAllowedContentLength` to 50MB
- **Subfolders**: courses, lessons, blog, podcasts, resources, profiles

---

## CI/CD Pipeline

Three GitHub Actions workflows using a self-hosted Windows runner:

| Workflow | Branch | Action |
|----------|--------|--------|
| `thg-blazor-dev.yaml` | develop | Build only (Debug config) |
| `thg-blazor-stage.yaml` | stage | Build → Backup → Stop IIS → Deploy → Start IIS → GitHub Release |
| `thg-blazor-prod.yaml` | master | Build → Backup → Stop IIS → Deploy → Start IIS → GitHub Release |

### IIS Requirements

- ASP.NET Core Hosting Bundle (.NET 10)
- WebSocket Protocol enabled (required for Blazor Server SignalR)
- `web.config` configures: AspNetCoreModuleV2, in-process hosting, 50MB request limit

---

## Configuration

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TenHourGuitarist.db"
  },
  "Serilog": { ... },
  "Stripe": {
    "SecretKey": "",
    "WebhookSecret": "",
    "PublishableKey": ""
  },
  "Email": {
    "Host": "", "Port": 587,
    "Username": "", "Password": "",
    "FromAddress": "", "FromName": ""
  },
  "SiteUrl": "https://tenhourguitarist.com"
}
```

The SQLite database file is created automatically in the project directory. No external database server is required.

### Environment-Specific Overrides

- **Development**: `TenHourGuitarist_Dev.db`, Debug logging, `https://localhost:5001` SiteUrl
- **Production**: `TenHourGuitarist.db`, Warning-level logging only

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (for database management)

```bash
# Install the EF Core CLI tool (if not already installed)
dotnet tool install --global dotnet-ef
```

### 1. Clone and Restore

```bash
git clone <repository-url>
cd tenhourguitarist/src/TenHourGuitarist
dotnet restore
```

### 2. Create the Database

The application uses **SQLite** — no external database server is needed. The database file is created automatically in the project directory.

```bash
# Navigate to the project directory
cd src/TenHourGuitarist

# Apply the migration to create the database
dotnet ef database update
```

This creates a `TenHourGuitarist_Dev.db` file (in Development) with all 14+ tables (plus ASP.NET Identity tables) and seeds the 4 roles + default admin account on first run.

The connection strings are pre-configured:
- **Development**: `Data Source=TenHourGuitarist_Dev.db` (in `appsettings.Development.json`)
- **Production**: `Data Source=TenHourGuitarist.db` (in `appsettings.json`)

### 3. Run the Application

```bash
cd src/TenHourGuitarist

# Run in development mode
dotnet run

# Or with hot reload
dotnet watch
```

The app will be available at `http://localhost:5280` (or whatever port is configured in `Properties/launchSettings.json`).

### 4. Log In as Admin

After the first run, the seed data creates a default admin account:
- **Email**: `admin@tenhourguitarist.com`
- **Password**: `Admin@123456`

> **Important**: Change the default admin password immediately after first login via `/admin/account`.

---

## Additional Configuration

### 5. Configure Stripe (Payments)

### Configure Stripe

1. **Create Products and Prices** in the [Stripe Dashboard](https://dashboard.stripe.com/products):
   - Create subscription products (e.g., "Monthly Plan", "Annual Plan")
   - Copy each Price ID (e.g., `price_1234...`)

2. **Get API Keys** from [Stripe Dashboard → Developers → API Keys](https://dashboard.stripe.com/apikeys)

3. **Set up Webhook Endpoint** in [Stripe Dashboard → Developers → Webhooks](https://dashboard.stripe.com/webhooks):
   - Endpoint URL: `https://yourdomain.com/api/stripe/webhook`
   - Events to listen for:
     - `checkout.session.completed`
     - `customer.subscription.updated`
     - `customer.subscription.deleted`
     - `invoice.payment_failed`
   - Copy the Webhook Signing Secret

4. **Update appsettings.json**:
   ```json
   {
     "Stripe": {
       "SecretKey": "sk_live_...",
       "WebhookSecret": "whsec_...",
       "PublishableKey": "pk_live_..."
     }
   }
   ```

5. **Create Package records** in the admin dashboard (`/admin/packages`) with the `StripePriceId` field matching your Stripe Price IDs.

### 6. Configure Email (AWS SES)

1. **Verify your domain** in AWS SES
2. **Create SMTP credentials** in AWS SES Console
3. **Update appsettings.json**:
   ```json
   {
     "Email": {
       "Host": "email-smtp.us-east-1.amazonaws.com",
       "Port": 587,
       "Username": "YOUR_SES_SMTP_USERNAME",
       "Password": "YOUR_SES_SMTP_PASSWORD",
       "FromAddress": "noreply@tenhourguitarist.com",
       "FromName": "TenHourGuitarist"
     }
   }
   ```

### 7. Configure Vimeo

No API integration needed — lessons use direct Vimeo embed URLs. When creating lessons in the admin:
- Set `VideoUrl` to the Vimeo URL (e.g., `https://vimeo.com/123456789`)
- Set `FreeVideoUrl` for preview clips accessible without subscription
- Ensure videos are set to "Hide from Vimeo" + "Allow embedding on specific domains" in Vimeo settings

### 8. Deploy to IIS

**Prerequisites on the server:**
- .NET 10 Hosting Bundle installed
- IIS WebSocket Protocol feature enabled
- IIS site configured with HTTPS binding

**Manual deployment:**
```bash
dotnet publish src/TenHourGuitarist/TenHourGuitarist.csproj -c Release -o ./publish
```

Copy the `publish/` folder contents to your IIS site directory.

**Automated deployment:**
Push to the `stage` or `master` branch — the GitHub Actions workflow handles build, backup, and deployment automatically.

### 9. Create Upload Directories (Production)

On the server, ensure these directories exist under `wwwroot/uploads/`:
```
wwwroot/uploads/courses/
wwwroot/uploads/lessons/
wwwroot/uploads/blog/
wwwroot/uploads/podcasts/
wwwroot/uploads/resources/
wwwroot/uploads/profiles/
```

The `FileStorageService` creates subdirectories automatically, but the IIS app pool identity needs write permissions to `wwwroot/uploads/`.

### 10. Populate Initial Content

Using the admin dashboard:
1. **Create Subjects** (`/admin/subjects`) — lesson topic categories
2. **Create Courses** (`/admin/courses`) — with cover images and Vimeo trailer URLs
3. **Create Lessons** (`/admin/lessons`) — with Vimeo video URLs and display order
4. **Create Packages** (`/admin/packages`) — with Stripe Price IDs
5. **Create Blog Posts** (`/admin/blog`) — with categories and featured images
6. **Create Podcasts** (`/admin/podcasts`) — with audio file URLs
7. **Create Free Resources** (`/admin/resources`) — with optional video URLs

### 11. Additional Hardening (Recommended)

- [ ] Move secrets out of `appsettings.json` into environment variables or a secrets manager
- [ ] Add CSP headers (Content Security Policy) for XSS protection
- [ ] Add rate limiting on the contact form and login endpoints
- [ ] Add reCAPTCHA to the contact form and registration
- [ ] Set up Serilog file sink or Seq/Application Insights for production logging
- [ ] Configure HTTPS redirect and HSTS in IIS
- [ ] Set up database backups (copy the `.db` file on a schedule)
- [ ] Add health check endpoint for monitoring
- [ ] Write integration tests for critical flows (registration, subscription, webhook)
- [ ] Review and customize the Bootstrap/MudBlazor theme colors

### 12. Data Migration (If Needed)

The plan specifies a fresh schema with no migration from the old database. If you later need to migrate existing data:
1. Export old data from the legacy database
2. Transform the data to match the new schema (different column names, proper types, new FKs)
3. Import into the SQLite database using a custom migration tool or scripts
4. Re-hash passwords (the old app used MD5; new app uses ASP.NET Identity's PBKDF2)
   - Users from the old system will need to use "Forgot Password" to set new passwords

---

## Verification Checklist

- [x] `dotnet build` succeeds with 0 warnings
- [x] `dotnet ef database update` creates SQLite schema; all tables verified
- [ ] Register a new user → verify email → login → see member content → logout
- [ ] Browse packages → Stripe checkout (test mode) → webhook fires → subscriber role assigned → premium lessons accessible
- [ ] Login as Admin → create/edit/delete a course with image upload → verify on public site
- [ ] Login as Instructor → verify only Courses/Lessons visible in admin sidebar
- [ ] Publish to IIS → verify WebSocket connection (Blazor Server) → verify static files served
- [ ] Test email delivery for all 5 templates
- [ ] Test file uploads (images and audio) at maximum sizes
- [ ] Verify mobile responsiveness on public site pages
