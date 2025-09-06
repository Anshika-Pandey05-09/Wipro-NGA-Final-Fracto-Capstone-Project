
# User Guide - Fracto

This guide explains how users and admins interact with the Fracto application.

## Roles
- **User**: Browse doctors, book appointments, view/cancel appointments, rate doctors after appointments.
- **Admin**: Manage doctors, specializations, view all appointments, manage users (CRUD).

## Typical User Flows

### 1. Browse Doctors
- Open the app and navigate to the "Doctors" page.
- Use filters by city and specialization.
- Click a doctor to view details (profile, available slots, rating).

### 2. Book Appointment
- Select a date and available timeslot shown by the doctor.
- Confirm booking — appointment record created with status `Pending` or `Confirmed` depending on business logic.
- Receive notification on successful booking (email or in-app).

### 3. View/Cancel Appointments
- Navigate to "My Appointments".
- Cancel available appointments if allowed (e.g., before cutoff time).
- After cancellation, appointment `Status` changes to `Cancelled`.

### 4. Rate Doctor
- After completed appointment, user can rate the doctor (1-5) and leave a comment.
- Ratings are stored in `Ratings` table linked to appointment and doctor.

### 5. Admin Tasks
- Login as admin and access Admin Dashboard.
- Manage doctors (add/edit/delete), including setting working hours and slot durations.
- Manage specializations and users.

## Error Handling & Validation
- Client-side form validations for inputs (required fields, email format, date checks).
- Backend returns meaningful HTTP status codes and error messages for display.
