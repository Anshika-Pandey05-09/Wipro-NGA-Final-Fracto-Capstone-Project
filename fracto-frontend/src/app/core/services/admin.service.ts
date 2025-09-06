import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private api = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  /** Example admin endpoints; adjust to your API routes */
  getUsers(params?: Record<string, any>): Observable<any[]> {
    const cleaned: Record<string, string> = {};
    Object.entries(params || {}).forEach(([k, v]) => {
      if (v !== null && v !== undefined && `${v}` !== '') cleaned[k] = `${v}`;
    });
    const httpParams = new HttpParams({ fromObject: cleaned });
    return this.http.get<any[]>(`${this.api}/users`, { params: httpParams });
  }

  deleteUser(id: number) {
    return this.http.delete(`${this.api}/users/${id}`);
  }

  approveQuestion(id: number) {
    return this.http.post(`${this.api}/questions/${id}/approve`, {});
  }

  rejectQuestion(id: number) {
    return this.http.post(`${this.api}/questions/${id}/reject`, {});
  }
}
