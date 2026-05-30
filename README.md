# Shivakala Coaching Classes

Production-ready ASP.NET Core MVC `.NET 8` website for `Shivakala Coaching Classes` with clean architecture, SQLite, Entity Framework Core, Bootstrap 5, Razor views, Marathi and English localization, student registration, and enquiry management.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core 8
- SQLite
- Razor Views
- Bootstrap 5
- Font Awesome
- `.resx` localization

## Solution Structure

```text
ShivakalaCoaching.sln

src/
 ├── Shivakala.Web
 ├── Shivakala.Core
 └── Shivakala.Infrastructure
```

## Features

- Responsive home page with hero banner, stats, courses, faculty, testimonials, and CTA
- About, Courses, Registration, Enquiry, and Contact pages
- Marathi and English language switcher
- Dark and light theme toggle
- SQLite database with EF Core migrations
- Registration and enquiry forms with client-side and server-side validation
- Sticky navigation, glassmorphism cards, gradients, and smooth reveal animations
- WhatsApp floating action button
- Error handling and logging
- Admin dashboard for registrations and enquiries

## Database

SQLite database file:

```text
src/Shivakala.Web/App_Data/shivakala.db
```

Current tables:

- `Students`
- `Enquiries`
- `Courses`

## Getting Started

### Prerequisites

- .NET SDK `8.0.421` or compatible .NET 8 SDK

### Run Locally

```bash
dotnet restore ShivakalaCoaching.sln
dotnet build ShivakalaCoaching.sln
dotnet run --project src/Shivakala.Web
```

Open:

```text
https://localhost:5001
or
http://localhost:5000
```

## Migrations

Local EF Core tool is configured through `.config/dotnet-tools.json`.

Create a migration:

```bash
dotnet dotnet-ef migrations add MigrationName --project src/Shivakala.Infrastructure --startup-project src/Shivakala.Web --context ShivakalaDbContext --output-dir Data/Migrations
```

Apply migrations:

```bash
dotnet dotnet-ef database update --project src/Shivakala.Infrastructure --startup-project src/Shivakala.Web --context ShivakalaDbContext
```

## Localization

Shared resources are stored in:

```text
src/Shivakala.Web/Resources/
```

Supported cultures:

- `en`
- `mr`

## Admin Access

Admin pages:

- `/Admin/Login`
- `/Admin`
- `/Admin/Registrations`
- `/Admin/Enquiries`

Default credentials:

```text
Username: admin
Password: P@$$w0rd
```

## Production Notes

- Update institute contact details, map query, and WhatsApp number in the web layer before deployment if needed.
- Replace placeholder admissions email and phone number with final production values.
- Configure reverse proxy, HTTPS certificate, and environment-specific logging in your hosting environment.

## Default Pages

- `/`
- `/about`
- `/courses`
- `/registration`
- `/enquiry`
- `/contact`

## Commit

Target commit message:

```text
Initial production-ready Shivakala Coaching website
```
