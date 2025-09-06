import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';
import { Doctor } from '../../models/doctor';

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private api = `${environment.apiUrl}/doctors`;

  constructor(private http: HttpClient) {}

  getAll(params?: Record<string, any>): Observable<Doctor[]> {
    const cleaned: Record<string, string> = {};
    Object.entries(params || {}).forEach(([k, v]) => {
      if (v !== null && v !== undefined && `${v}` !== '') cleaned[k] = `${v}`;
    });
    const opts = Object.keys(cleaned).length
      ? { params: new HttpParams({ fromObject: cleaned }) }
      : {};
    return this.http.get<Doctor[]>(this.api, opts);
  }

  search(query: Record<string, any>): Observable<Doctor[]> { return this.getAll(query); }

  getById(id: number): Observable<Doctor> { return this.http.get<Doctor>(`${this.api}/${id}`); }

  add(doctor: Doctor) { return this.http.post(this.api, doctor); }

  update(id: number, doctor: Doctor) { return this.http.put(`${this.api}/${id}`, doctor); }

  delete(id: number) { return this.http.delete(`${this.api}/${id}`); }

  getTimeslots(id: number, date: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.api}/${id}/timeslots`, {
      params: new HttpParams({ fromObject: { date } })
    });
  }

  getSpecializations(): Observable<Array<{ id: number | string; name: string }>> {
    return this.http.get<Array<{ id: number | string; name: string }>>(
      `${environment.apiUrl}/specializations`
    );
  }

  /** Upload actual image to /api/images/upload (Admin only) */
  uploadProfileImage(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ path: string; fileName: string }>(
      `${environment.apiUrl}/images/upload`,
      form
    );
  }


  available(query: Record<string, any>) {
  return this.http.get<Doctor[]>(`${this.api}/available`, {
    params: new HttpParams({ fromObject: Object.fromEntries(
      Object.entries(query || {}).filter(([_, v]) => v !== null && v !== undefined && `${v}` !== '')
    ) })
  });
}

}
