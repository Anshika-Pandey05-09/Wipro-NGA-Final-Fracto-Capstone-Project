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
