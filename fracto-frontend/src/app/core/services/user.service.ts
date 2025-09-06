import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

export interface AppUser {
  id?: number;
  username: string;
  email: string;
  role: 'User' | 'Admin';
  password?: string; // only for create or change
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private api = `${environment.apiUrl}/admin/users`; // <-- matches controller

  constructor(private http: HttpClient) {}

  list(): Observable<AppUser[]> {
    return this.http.get<AppUser[]>(this.api);
  }

  create(u: AppUser) {
    // backend Create requires: username, email, password, role
    const payload = {
      username: (u.username || '').trim(),
      email: (u.email || '').trim(),
      password: u.password || '',
      role: u.role || 'User'
    };
    return this.http.post(this.api, payload);
  }

  update(id: number, u: AppUser) {
    // backend Update accepts username/email/role and optional password (omit if blank)
    const payload: any = {
      username: (u.username || '').trim(),
      email: (u.email || '').trim(),
      role: u.role || 'User',
    };
    if (u.password && u.password.trim().length > 0) payload.password = u.password.trim();
    return this.http.put(`${this.api}/${id}`, payload);
  }

  remove(id: number) {
    return this.http.delete(`${this.api}/${id}`);
  }
}
