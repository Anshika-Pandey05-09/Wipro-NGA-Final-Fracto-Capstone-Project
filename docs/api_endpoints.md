
# API Endpoints - Fracto

Base URL: `http://{host}/api`

## Authentication
- `POST /api/auth/register` - Register new user. Body: { username, email, password }
- `POST /api/auth/login` - Login. Body: { email, password } -> returns JWT token

## Users
- `GET /api/users` - [Admin] List users
- `GET /api/users/{id}` - Get user by id
- `POST /api/users` - [Admin] Create user
- `PUT /api/users/{id}` - [Admin] Update user
- `DELETE /api/users/{id}` - [Admin] Delete user

## Doctors
- `GET /api/doctors` - List doctors (with optional ?city=&specializationId=)
- `GET /api/doctors/{id}` - Get doctor details (including schedule)
- `POST /api/doctors` - [Admin] Create doctor
- `PUT /api/doctors/{id}` - [Admin] Update doctor
- `DELETE /api/doctors/{id}` - [Admin] Delete doctor

## Specializations
- `GET /api/specializations` - List specializations
- `POST /api/specializations` - [Admin] Create specialization
- `PUT /api/specializations/{id}` - [Admin] Update specialization
- `DELETE /api/specializations/{id}` - [Admin] Delete specialization

## Appointments
- `GET /api/appointments` - List appointments (admin can see all; user sees own)
- `GET /api/appointments/{id}` - Get appointment details
- `POST /api/appointments` - Book appointment. Body: { userId, doctorId, appointmentDate, timeSlot }
- `PUT /api/appointments/{id}` - Update appointment (status changes, reschedule)
- `DELETE /api/appointments/{id}` - Cancel appointment

## Ratings
- `GET /api/ratings?doctorId={id}` - Get ratings for a doctor
- `POST /api/ratings` - Create rating after appointment. Body: { appointmentId, doctorId, userId, score, comment }
- `GET /api/ratings/{id}` - Get rating by id

## Notes
- All POST/PUT/DELETE endpoints that modify data require authentication with a valid JWT in `Authorization: Bearer <token>` header.
- Admin-only endpoints require role `Admin` in the JWT claims.
