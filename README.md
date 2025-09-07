# FRACTO — Doctor Appointment & Ratings Platform

A full‑stack capstone project for discovering doctors, booking appointments with *slot‑uniqueness, and posting **post‑visit ratings. Built with **Angular (SPA)* on the frontend and *ASP.NET Core Web API + EF Core (SQL Server)* on the backend. No SignalR used.

## Table of Contents
- [Features](#features)
- [Architecture](#architecture)
- [ER Diagram](#er-diagram)
- [Tech Stack](#tech-stack)
- [Folder Structure](#folder-structure)
- [Getting Started](#getting-started)
  - [Backend Setup (.NET)](#backend-setup-net)
  - [Frontend Setup (Angular)](#frontend-setup-angular)
- [Environment Configuration](#environment-configuration)
- [Database & Migrations](#database--migrations)
- [API Quickstart](#api-quickstart)
- [Testing](#testing)
- [Deployment](#deployment)
- [Screenshots](#screenshots)
- [DFD (Optional Docs)](#dfd-optional-docs)
- [Security Notes](#security-notes)
- [Contributing](#contributing)
- [License](#license)

---

## Features
- 🔐 *JWT Authentication* for Patients and Admins
- 👨‍⚕️ *Doctor Catalogue* with specialization, city, timings, slot duration
- 📅 *Appointments* with *unique slot* constraint; statuses: Booked → Approved / Cancelled
- ⭐ *Ratings* (patients can rate after appointment); doctor card shows averages
- 🖼️ *Profile Images* stored under /wwwroot/uploads and served via /uploads/{file}
- 🧭 *Responsive UI* with guards & HTTP interceptor
- 🧪 *Automated tests* (xUnit + Moq and Jasmine/Karma)



*Layers*
- Angular SPA → calls HTTPS JSON to Web API
- ASP.NET Core MVC + Web API → Controllers: Auth, Users, Doctors, Appointments, Ratings, Specializations, Images
- EF Core DbContext → SQL Server
- Static file storage: /wwwroot/uploads (profile images)

## ER Diagram
Mermaid version (drop into GitHub/VS Code previews or Mermaid Live Editor):

mermaid
erDiagram
  USERS ||--o{ APPOINTMENTS : "books"
  USERS ||--o{ RATINGS     : "writes"
  DOCTORS ||--o{ APPOINTMENTS : "receives"
  DOCTORS ||--o{ RATINGS      : "has"
  DOCTORS ||--o{ DOCTOR_SPECIALIZATIONS : "is"
  SPECIALIZATIONS ||--o{ DOCTOR_SPECIALIZATIONS : "categorizes"

  USERS {
    int      Id PK
    string   Username  "UNIQUE"
    string   Email     "UNIQUE"
    string   PasswordHash
    string   Role      "Patient|Admin"
    datetime CreatedAt
  }

  DOCTORS {
    int      Id PK
    string   Name
    string   City
    string   Specialization  "denormalized label (optional)"
    int      SlotDurationMinutes
    string   ImagePath
    datetime CreatedAt
  }

  SPECIALIZATIONS {
    int    Id PK
    string Name UNIQUE
  }

  DOCTOR_SPECIALIZATIONS {
    int DoctorId FK
    int SpecializationId FK
  }

  APPOINTMENTS {
    int      Id PK
    int      DoctorId FK
    int      UserId   FK
    date     AppointmentDate
    string   TimeSlot      "e.g., 10:00-10:20"
    tinyint  Status        "0=Booked,1=Approved,2=Cancelled"
    datetime CreatedAt
  }

  RATINGS {
    int      Id PK
    int      DoctorId FK
    int      UserId   FK
    tinyint  Score        "1..5"
    string   Comment
    datetime CreatedAt
  }


> SQL unique constraint to prevent double‑booking:
sql
CREATE UNIQUE INDEX IX_Appointments_Doctor_Date_TimeSlot
ON Appointments (DoctorId, AppointmentDate, TimeSlot);


## Tech Stack
- *Frontend:* Angular (standalone components), Router, HttpClient, Interceptor, Guards
- *Backend:* ASP.NET Core MVC + Web API (.NET 9), *EF Core* (Code‑First)
- *Database:* SQL Server
- *Auth:* JWT (Token Service)
- *Testing:* xUnit + Moq; Jasmine/Karma
- *Docs:* Mermaid + PNG/SVG diagrams

## Folder Structure

fracto/
├─ fracto-frontend/                # Angular app
│  ├─ src/
│  └─ ...
├─ Fracto.Api/                     # ASP.NET Core Web API
│  ├─ Controllers/
│  ├─ Models/
│  ├─ Data/                        # DbContext, Migrations
│  ├─ wwwroot/uploads/             # profile images
│  └─ appsettings.*.json
├─ docs/
│  └─ architecture/FRACTO_System_Architecture.png
└─ README.md


## Getting Started

### Backend Setup (.NET)
Prereqs: *.NET SDK 9, **SQL Server* (or local DB), *EF Core Tools* (dotnet tool update --global dotnet-ef)

bash
cd Fracto.Api

# 1) Set connection string in appsettings.Development.json
#   "ConnectionStrings": { "DefaultConnection": "Server=.;Database=FractoDb;Trusted_Connection=True;TrustServerCertificate=True" }

# 2) Apply migrations (create DB)
dotnet ef database update

# 3) Run API
dotnet run
# Swagger at http://localhost:5002/swagger when running locally


### Frontend Setup (Angular)
Prereqs: *Node 20+, **npm, **Angular CLI* (npm i -g @angular/cli)

bash
cd fracto-frontend

# 1) Install deps
npm install

# 2) Set API base URL in src/environments/environment.development.ts
# export const environment = { production: false, apiUrl: 'http://localhost:5002/api' };

# 3) Run dev server
ng serve -o


## Environment Configuration
- *Backend:* appsettings.Development.json
  - ConnectionStrings:DefaultConnection
  - Jwt:Issuer, Jwt:Audience, Jwt:Key
  - Upload:Root (optional; default wwwroot/uploads)
- *Frontend:* environment*.ts
  - apiUrl must point to your running API base

## Database & Migrations
Create a new migration after model changes:
bash
cd Fracto.Api
dotnet ef migrations add <NameOfChange>
dotnet ef database update


Optional seed (example):
csharp
// On startup
if (!ctx.Specializations.Any()) {
    ctx.Specializations.AddRange(new Specialization { Name="Dermatology" }, new Specialization { Name="Dentist" });
    ctx.SaveChanges();
}


## API Quickstart
bash
# Register
curl -X POST http://localhost:5002/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","email":"a@a.com","password":"P@ssw0rd!"}'

# Login -> returns JWT
curl -X POST http://localhost:5002/api/auth/login -H "Content-Type: application/json" \
  -d '{"email":"a@a.com","password":"P@ssw0rd!"}'

# List doctors
curl http://localhost:5002/api/doctors

# Book appointment (Bearer token required)
curl -X POST http://localhost:5002/api/appointments \
  -H "Authorization: Bearer <token>" -H "Content-Type: application/json" \
  -d '{"doctorId":1,"date":"2025-09-10","timeSlot":"10:00-10:20"}'


## Testing
*Backend*
bash
dotnet test


*Frontend*
bash
cd fracto-frontend
ng test
# Lint
ng lint


## Deployment
- Build Angular → upload static assets to host/CDN or serve via reverse proxy
- Publish API:
bash
cd Fracto.Api
dotnet publish -c Release -o out
# Deploy /out to IIS / Azure App Service / Docker image

- Set ASPNETCORE_ENVIRONMENT=Production, real DB connection string & JWT secrets
- Configure *CORS* to allow your frontend origin

## Screenshots
Place screenshots under docs/screenshots/ and embed them here:
md
![Login](docs/screenshots/login.png)
![Doctor List](docs/screenshots/doctor-list.png)


## DFD (Optional Docs)
Add your generated DFDs (PNG/SVG) under docs/dfd/ and link them from the report/presentation.

## Security Notes
- Store *JWT key* in user secrets/KeyVault, not in Git
- Validate uploads (size/MIME); serve via /uploads/{file} with safe filenames
- Use short‑lived access tokens; protect Admin endpoints with policies

## Contributing
PRs welcome. Please run tests and linters before submitting.

## License
MIT (or your institution’s standard). Update this section as needed.