// // import { Injectable } from '@angular/core';
// // import { HttpClient } from '@angular/common/http';
// // import { environment } from 'environments/environment';
// // import { Appointment } from '../../models/appointment';
// // import { Observable } from 'rxjs';

// // @Injectable({ providedIn: 'root' })
// // export class AppointmentService {
// //   private api = `${environment.apiUrl}/appointments`;

// //   constructor(private http: HttpClient) {}

// //   book(appt: Appointment) {
// //     return this.http.post(this.api, appt);
// //   }

// //   cancel(id: number) {
// //     return this.http.delete(`${this.api}/${id}`);
// //   }

// //   byUser(userId: number): Observable<Appointment[]> {
// //     return this.http.get<Appointment[]>(`${this.api}/user/${userId}`);
// //   }
// // }

// import { Injectable } from '@angular/core';
// import { HttpClient } from '@angular/common/http';
// import { environment } from 'environments/environment';
// import { Observable } from 'rxjs';

// // App-level models (adjust if you already have these elsewhere)
// export interface Appointment {
//   id?: number;
//   userId: number;
//   doctorId: number;
//   appointmentDate: string; // ISO date string e.g. '2025-09-04'
//   timeSlot: string;        // e.g. '10:00-10:30'
//   status?: 'Booked' | 'Pending' | 'Approved' | 'Cancelled' | string;
// }

// export interface AdminAppointment {
//   id: number;
//   userId: number;
//   doctorId: number;
//   appointmentDate: string;
//   timeSlot: string;
//   status: 'Booked' | 'Pending' | 'Approved' | 'Cancelled' | string;
// }

// @Injectable({ providedIn: 'root' })
// export class AppointmentService {
//   private api = `${environment.apiUrl}/appointments`;

//   constructor(private http: HttpClient) {}

//   /** User/Admin: book an appointment */
//   book(appt: Appointment): Observable<{ id: number; status: string }> {
//     return this.http.post<{ id: number; status: string }>(this.api, appt);
//   }

//   /** User/Admin: cancel appointment (matches PUT /appointments/cancel/{id}) */
//   cancel(id: number): Observable<void> {
//     return this.http.put<void>(`${this.api}/cancel/${id}`, {});
//   }

//   /** User/Admin: list appointments for a user */
//   byUser(userId: number): Observable<Appointment[]> {
//     return this.http.get<Appointment[]>(`${this.api}/user/${userId}`);
//   }

//   // -------- Admin endpoints --------

//   /** Admin: get all appointments */
//   getAll(): Observable<AdminAppointment[]> {
//     return this.http.get<AdminAppointment[]>(`${this.api}`);
//   }

//   /** Admin: get pending appointments (requires GET /appointments/pending on backend) */
//   getPending(): Observable<AdminAppointment[]> {
//     return this.http.get<AdminAppointment[]>(`${this.api}/pending`);
//   }

//   /** Admin: approve an appointment */
//   approve(id: number): Observable<void> {
//     return this.http.put<void>(`${this.api}/approve/${id}`, {});
//   }
// }

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';

export interface AppointmentVM {
  id: number;
  doctorId: number;
  doctorName: string;
  doctorProfileImagePath?: string;
  city: string;
  patientName: string;
  appointmentDate: string; // ISO date from server
  timeSlot: string;
  status: 'Pending' | 'Booked' | 'Cancelled' | string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private api = `${environment.apiUrl}/appointments`;

  constructor(private http: HttpClient) {}

  // user books
  book(payload: { userId: number; doctorId: number; appointmentDate: string; timeSlot: string }) {
    return this.http.post<AppointmentVM>(this.api, payload);
  }

  // admin list
  adminList(filters?: { status?: string; city?: string; dateFrom?: string; dateTo?: string }): Observable<AppointmentVM[]> {
    const cleaned: Record<string, string> = {};
    Object.entries(filters || {}).forEach(([k, v]) => { if (v) cleaned[k] = String(v); });
    const params = new HttpParams({ fromObject: cleaned });
    return this.http.get<AppointmentVM[]>(`${this.api}/admin`, { params });
  }

  // user list
  mine(userId: number): Observable<AppointmentVM[]> {
    return this.http.get<AppointmentVM[]>(`${this.api}/user/${userId}`);
  }

  // admin actions
  approve(id: number) { return this.http.post<AppointmentVM>(`${this.api}/${id}/approve`, {}); }
  cancel(id: number) { return this.http.post<AppointmentVM>(`${this.api}/${id}/cancel`, {}); }

   userCancel(id: number, userId: number) {
    return this.http.post(`${this.api}/${id}/user-cancel`, { userId });
  }
}
