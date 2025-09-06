import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentService, AppointmentVM } from '../../../core/services/appointment.service';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from 'environments/environment';
import { RatingService } from 'app/core/services/rating.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-appointments.component.html',
  styleUrls: ['./my-appointments.component.scss']
})
export class MyAppointmentsComponent implements OnInit {
  rows: AppointmentVM[] = [];
  loading = true;
  error = '';
  private readonly API_ROOT = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(
    private apptSvc: AppointmentService,
    private auth: AuthService,
    private ratingSvc: RatingService
  ) {}

  // rating UI state
  ratingFor: number | null = null;  // appointment id
  ratingValue = 0;
  ratingNote = '';

  ngOnInit(): void {
    this.reload();
  }

  private reload() {
    const uid = this.auth.getUserId();
    if (!uid) { this.error = 'Please login to view your appointments.'; this.loading = false; return; }
    this.loading = true;
    this.apptSvc.mine(uid).subscribe({
      next: (list) => { this.rows = list || []; this.loading = false; },
      error: () => { this.error = 'Failed to load your appointments'; this.loading = false; }
    });
  }

  imgUrl(p?: string | null): string {
    if (!p) return '/assets/default.png';
    if (/^https?:\/\//i.test(p)) return p;
    if (p.startsWith('/')) return `${this.API_ROOT}${p}`;
    return `${this.API_ROOT}/images/${p}`;
  }

  statusClass(s: string) {
    const k = (s || '').toLowerCase();
    if (k === 'pending') return 'badge bg-warning text-dark';
    if (k === 'booked') return 'badge bg-success';
    if (k === 'cancelled' || k === 'canceled') return 'badge bg-secondary';
    return 'badge bg-light text-dark';
  }

  // cancel
  canCancel(a: AppointmentVM) {
    return a.status === 'Pending' || a.status === 'Booked';
  }
  cancel(a: AppointmentVM) {
    const uid = this.auth.getUserId(); if (!uid) return;
    if (!confirm('Cancel this appointment?')) return;
    this.apptSvc.userCancel(a.id, uid).subscribe({
      next: () => this.reload(),
      error: () => alert('Cancel failed')
    });
  }

  // rating
  canRate(a: AppointmentVM) {
    const date = new Date(a.appointmentDate);
    const today = new Date(); today.setHours(0,0,0,0);
    return date <= today && a.status !== 'Cancelled';
  }
  startRate(a: AppointmentVM) {
    this.ratingFor = a.id;
    this.ratingValue = 5;
    this.ratingNote = '';
  }
  submitRate(a: AppointmentVM) {
    const uid = this.auth.getUserId(); if (!uid) return;
    if (!this.ratingValue) { alert('Please choose 1-5'); return; }
    this.ratingSvc.rate({
      appointmentId: a.id,
      userId: uid,
      doctorId: a.doctorId,
      score: this.ratingValue,
      comment: this.ratingNote || undefined
    }).subscribe({
      next: () => { this.ratingFor = null; this.reload(); },
      error: (err) => alert(err?.error?.message || 'Rating failed')
    });
  }
  cancelRate() { this.ratingFor = null; }

  trackById = (_: number, a: AppointmentVM) => a.id;
}
