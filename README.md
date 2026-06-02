# 🎓 Shivakala Coaching Classes — Management System

> **Enterprise-grade coaching institute management platform** built with ASP.NET Core 8 MVC · EF Core · SQLite · Bootstrap 5 · whatsapp-web.js

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

### 2. Apply Migrations

```bash
cd src/Shivakala.Web
dotnet ef database update --project ../Shivakala.Infrastructure
```

### 3. Run the App

```bash
dotnet run --project src/Shivakala.Web
```

Open → `http://localhost:5000`  
Admin → `http://localhost:5000/admin`

### 4. Start WhatsApp Sidecar (optional)

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
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/shivakala.db"
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "changeme123"
  }
}
```

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
