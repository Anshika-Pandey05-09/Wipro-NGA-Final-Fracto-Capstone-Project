import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit(form?: NgForm) {
    this.error = '';
    if (form && form.invalid) {
      this.error = 'Please enter username and password.';
      return;
    }

    this.loading = true;
    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/doctors']);
      },
      error: (err: unknown) => {
        this.loading = false;
        this.error = this.extractError(err);
        console.error('Login error:', err);
      }
    });
  }

  private extractError(err: unknown): string {
    // Network/HTTP shape
    if (err instanceof HttpErrorResponse) {
      const data = err.error;

      // If backend returned plain text
      if (typeof data === 'string') return data;

      // ASP.NET Core ProblemDetails or custom object
      if (data && typeof data === 'object') {
        // Common fields
        if (data.detail) return data.detail;
        if (data.message) return data.message;
        if (data.title) return data.title;

        // ModelState errors: { errors: { Field: [ "msg1", "msg2" ] } }
        if (data.errors && typeof data.errors === 'object') {
          const messages: string[] = [];
          for (const key of Object.keys(data.errors)) {
            const arr = data.errors[key];
            if (Array.isArray(arr)) messages.push(...arr);
          }
          if (messages.length) return messages.join('\n');
        }
      }

      // Fallback using status text
      if (err.status === 0) return 'Unable to reach the server. Check your connection.';
      return err.statusText || `Request failed with status ${err.status}.`;
    }

    // Unknown shape (string, Error, etc.)
    if (typeof err === 'string') return err;
    if (err && typeof err === 'object' && 'toString' in err) return String(err);
    return 'Login failed.';
  }
}
