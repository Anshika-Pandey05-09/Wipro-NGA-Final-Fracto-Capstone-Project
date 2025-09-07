import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  username = '';
  email = '';
  password = '';
  error = '';
  success = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';
    this.success = '';

    this.auth.register({
      username: this.username,
      email: this.email,
      password: this.password
    }).subscribe({
      next: (res: any) => {
        // backend returns { message: "Registration successful" }
        this.success = res?.message || 'Registration successful. You can login now.';
        setTimeout(() => this.router.navigate(['/login']), 1200);
      },
      error: (err) => {
        console.error('Registration error:', err);

        if (err.error?.message) {
          this.error = err.error.message;   // backend-sent message
        } else if (typeof err.error === 'string') {
          this.error = err.error;          // plain string
        } else {
          this.error = 'Registration failed. Please try again.';
        }
      }
    });
  }
}
