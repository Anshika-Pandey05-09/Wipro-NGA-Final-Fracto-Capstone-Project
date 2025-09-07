import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { tap } from 'rxjs/operators';
import { Observable, BehaviorSubject } from 'rxjs';

interface LoginResponse {
  token: string;
  id: number;
  username: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = `${environment.apiUrl}/auth`;

  //  Emits whenever role/login changes
  private roleSubject = new BehaviorSubject<string | null>(this.currentRole());
  role$ = this.roleSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(data: { username: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.api}/login`, data).pipe(
      tap(res => {
        if (res?.token) {
          localStorage.setItem('token', res.token);
          localStorage.setItem('role', res.role);
          localStorage.setItem('username', res.username);
          localStorage.setItem('userId', String(res.id));
          this.roleSubject.next(res.role || null); // notify navbar
        }
      })
    );
  }

  register(data: any) {
    return this.http.post(`${this.api}/register`, data);
  }

  logout() {
    localStorage.clear();
    this.roleSubject.next(null); // notify navbar
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  isAdmin(): boolean {
    return (localStorage.getItem('role') || '').toLowerCase() === 'admin';
  }

  getUsername(): string {
    return localStorage.getItem('username') || 'Account';
  }

  getUserId(): number | null {
    const id = localStorage.getItem('userId');
    return id ? +id : null;
  }

  private currentRole(): string | null {
    return localStorage.getItem('role');
  }
}
