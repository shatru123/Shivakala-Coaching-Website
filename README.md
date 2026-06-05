# 🎓 Shivakala Coaching Classes — Management System

> **Enterprise-grade coaching institute management platform** built with ASP.NET Core 8 MVC · EF Core · SQLite/PostgreSQL/SQL Server · Bootstrap 5 · whatsapp-web.js

---

## ✨ Features

| Module | Features |
|--------|----------|
| **Students** | Admission, profiles, photo upload, ID card, CSV export |
| **Teachers** | CRUD, photo, salary, subject allocation |
| **Batches** | Create classes, allocate students/subjects/teachers |
| **Attendance** | Daily mark sheet, subject-wise, monthly reports, % tracking |
| **Fees** | Collect fees, receipts (print), fee structure, pending dues |
| **Exams** | Schedule, enter marks, auto-rank, grade, publish results |
| **Homework** | Assign with attachment, view submissions |
| **Timetable** | Weekly grid, conflict detection, printable |
| **WhatsApp** | Free broadcast via QR scan — no paid API |
| **Notice Board** | Announcements, circulars |
| **Study Materials** | PDFs, notes, previous papers |
| **Audit Logs** | Every admin action logged |
| **Gallery / Testimonials** | Website content management |

---

## 🏗 Architecture

```
ShivakalaCoaching.sln
├── src/
│   ├── Shivakala.Core/           # Entities, Interfaces, Services (Domain)
│   ├── Shivakala.Infrastructure/ # EF Core, Repositories, Services (Data)
│   └── Shivakala.Web/            # ASP.NET Core MVC, Controllers, Views
└── whatsapp-sidecar/             # Node.js whatsapp-web.js HTTP bridge
```

---

## 🚀 Quick Start (Local)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 18+](https://nodejs.org/) (for WhatsApp sidecar)

### 1. Clone & Restore

```bash
git clone https://github.com/shatru123/Shivakala-Coaching-Website
cd Shivakala-Coaching-Website
dotnet restore
```

### 2. Choose Database Provider

For local SQLite:

```json
"Database": {
  "Provider": "Sqlite"
}
```

For PostgreSQL:

```json
"Database": {
  "Provider": "PostgreSql"
}
```

For SQL Server:

```json
"Database": {
  "Provider": "SqlServer"
}
```

For local development, keep the default as `Sqlite` unless you already have PostgreSQL or SQL Server running.

To spin up PostgreSQL locally with Docker Compose:

```bash
docker compose -f docker-compose.postgres.local.yml up -d
```

To spin up SQL Server locally with Docker Compose:

```bash
docker compose -f docker-compose.sqlserver.local.yml up -d
```

### 3. Apply Migrations

```bash
cd src/Shivakala.Web
dotnet ef database update --project ../Shivakala.Infrastructure
```

For PostgreSQL:

```bash
cd src/Shivakala.Web
dotnet ef database update --project ../Shivakala.PostgresMigrations -- --provider=PostgreSql
```

For SQL Server:

```bash
cd src/Shivakala.Web
dotnet ef database update --project ../Shivakala.SqlServerMigrations -- --provider=SqlServer
```

### 4. Run the App

```bash
dotnet run --project src/Shivakala.Web
```

Open → `http://localhost:5000`  
Admin → `http://localhost:5000/admin`

### Local PostgreSQL Verification

1. Start PostgreSQL:

```bash
docker compose -f docker-compose.postgres.local.yml up -d
```

2. Set `Database:Provider` to `PostgreSql` in `src/Shivakala.Web/appsettings.json`.

3. Apply PostgreSQL migrations:

```bash
dotnet ef database update --project src/Shivakala.PostgresMigrations --startup-project src/Shivakala.Web -- --provider=PostgreSql
```

4. Run the app:

```bash
dotnet run --project src/Shivakala.Web
```

5. Stop PostgreSQL when finished:

```bash
docker compose -f docker-compose.postgres.local.yml down
```

### Local SQL Server Verification

1. Start SQL Server:

```bash
docker compose -f docker-compose.sqlserver.local.yml up -d
```

2. Set `Database:Provider` to `SqlServer` in `src/Shivakala.Web/appsettings.json`.

3. Keep the default local SQL Server connection string:

```json
"SqlServer": "Server=localhost,14333;Database=shivakala;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
```

4. Apply SQL Server migrations:

```bash
dotnet ef database update --project src/Shivakala.SqlServerMigrations --startup-project src/Shivakala.Web -- --provider=SqlServer
```

5. Run the app:

```bash
dotnet run --project src/Shivakala.Web
```

6. Stop SQL Server when finished:

```bash
docker compose -f docker-compose.sqlserver.local.yml down
```

### 5. Start WhatsApp Sidecar (optional)

```bash
cd whatsapp-sidecar
npm install
npm start
```

Then go to **Admin → WhatsApp** and scan the QR code.

---

## 🐳 Docker Deployment

```bash
# Copy and edit environment config
cp docker-compose.yml docker-compose.prod.yml
# Edit credentials in docker-compose.prod.yml

docker compose -f docker-compose.prod.yml up -d --build
```

---

## ⚙️ Configuration

`appsettings.json` / environment variables:

```json
{
  "Database": {
    "Provider": "Sqlite"
  },
  "ConnectionStrings": {
    "Sqlite": "Data Source=App_Data/shivakala.db",
    "PostgreSql": "Host=localhost;Port=5432;Database=shivakala;Username=postgres;Password=strong-password",
    "SqlServer": "Server=YOUR_SQL_SERVER;Database=shivakala;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;TrustServerCertificate=True;"
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "changeme123"
  },
  "WhatsApp": {
    "BaseUrl": "http://localhost:3500",
    "ApiKey": ""
  }
}
```

For most production hosting providers, set:

- `Database__Provider=PostgreSql`
- `ConnectionStrings__PostgreSql=<your managed postgres connection string>`

Many hosts also provide a single `DATABASE_URL`. That now works too, as long as `Database__Provider=PostgreSql`.

For very small single-server deployments with file storage:

- `Database__Provider=Sqlite`
- `ConnectionStrings__Sqlite=Data Source=App_Data/shivakala.db`

For Windows shared hosting such as SmarterASP.NET:

- `Database__Provider=SqlServer`
- `ConnectionStrings__SqlServer=<your SmarterASP SQL Server connection string>`
- `WhatsApp__BaseUrl=https://wa.yourdomain.com`
- `WhatsApp__ApiKey=<shared secret between the MVC app and the Node sidecar>`

### SmarterASP.NET Deployment Notes
This repo now supports a split SmarterASP.NET deployment:

- the main MVC site runs on `shivkalaclasses.com`
- SQL Server stays on SmarterASP SQL Server
- the WhatsApp sidecar runs as a separate Node.js site such as `wa.shivkalaclasses.com`

See the full runbook in `SMARTERASP_DEPLOYMENT.md`.

For local Mac/Linux verification before deployment, use `docker-compose.sqlserver.local.yml` instead of LocalDB.

If you cannot run EF commands against the remote host directly, I can add a SQL migration bundle path next.

---

## 🔐 Security

- CSRF tokens on every form
- Parameterized EF Core queries (SQL injection–safe)
- BCrypt password hashing (AppUser)
- Role-based authorization attributes
- Audit log for every admin action
- Helmet headers via ASP.NET Core security middleware

---

## 📁 Folder Structure (new additions)

```
src/Shivakala.Core/Entities/
  AppUser.cs · Teacher.cs · Batch.cs · BatchSubject.cs
  StudentBatch.cs · Attendance.cs · TeacherAttendance.cs
  FeeStructure.cs · FeePayment.cs · Exam.cs · ExamResult.cs
  Homework.cs · HomeworkSubmission.cs · TimetableSlot.cs
  Notification.cs · AuditLog.cs · SyllabusItem.cs

src/Shivakala.Infrastructure/
  Repositories/ → all new repo implementations
  Services/     → AuditService · WhatsAppService

src/Shivakala.Web/Controllers/
  TeacherController · BatchController · AttendanceController
  FeeController · ExamController · HomeworkController
  TimetableController · WhatsAppController

src/Shivakala.Web/Views/
  Teacher/ · Batch/ · Attendance/ · Fee/ · Exam/
  Homework/ · Timetable/ · WhatsApp/

whatsapp-sidecar/
  server.js · package.json · Dockerfile · README.md
```

---

## 🗺 Roadmap

- [ ] Parent portal login
- [ ] SMS integration (free Textbelt/MSG91 trial)
- [ ] Online fee payment (Razorpay free tier)
- [ ] Student ID card PDF generation
- [ ] Progressive Web App (PWA)
- [ ] Dark mode

---

## 🧑‍💻 Developer

**Shatrughna** · Senior Engineer @ Ticketmaster  
GitHub: [@shatru123](https://github.com/shatru123)

---

*Built with ❤️ for SK Classes, Chikhali, Maharashtra*
