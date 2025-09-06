import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RatingService {
  private api = `${environment.apiUrl}/ratings`;
  constructor(private http: HttpClient) {}
  rate(payload: { appointmentId: number; userId: number; doctorId: number; score: number; comment?: string }) {
    return this.http.post<{ message: string; average: number }>(this.api, payload);
  }
  getAverage(doctorId: number) { return this.http.get<{ doctorId: number; average: number }>(`${this.api}/doctor/${doctorId}/avg`); }
  getDoctorRatings(doctorId: number) { return this.http.get(`${this.api}/doctor/${doctorId}`); }
}
