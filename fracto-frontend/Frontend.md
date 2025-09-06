
# Fracto Frontend

This README describes the frontend part of the Fracto project (Angular). It assumes an Angular application that consumes the backend API.

## Prerequisites
- Node.js (LTS)
- npm or yarn
- Angular CLI (optional, recommended)

## Setup
1. Clone or copy project frontend folder.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Configure environment files (e.g. `src/environments/environment.ts`) to point to backend API:
   ```ts
   export const environment = {
     production: false,
     apiBaseUrl: 'https://localhost:5001/api'
   };
   ```
4. Run the dev server:
   ```bash
   ng serve
   ```
   Open `http://localhost:4200` in your browser.

## Structure (suggested)
- `src/app/components` - UI components (header, footer, doctor-list, appointment-form, etc.)
- `src/app/pages` - Page containers (home, doctors, profile, admin-dashboard)
- `src/app/services` - Services for API calls (auth.service.ts, doctor.service.ts, appointment.service.ts)
- `src/app/models` - TypeScript interfaces for API models
- `src/assets` - static assets (images, icons)

## Authentication
- Use JWT tokens from backend.
- Store token in memory or secure cookie (avoid localStorage for security reasons if possible).
- Add an `AuthInterceptor` to attach `Authorization: Bearer <token>` header to API calls.

## Recommended Libraries
- Angular Material or Bootstrap for UI components
- ngx-toastr for notifications
- @auth0/angular-jwt for token utilities

## Deployment
- Build production bundle:
  ```bash
  ng build --prod
  ```
- Serve the `dist/` folder from any static file server (NGINX, Apache) or integrate with ASP.NET Core static files.
