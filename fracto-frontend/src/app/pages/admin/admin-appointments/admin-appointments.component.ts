import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { environment } from 'environments/environment';
import { AppointmentService, AppointmentVM } from '../../../core/services/appointment.service';

@Component({
  selector: 'app-admin-appointments',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-appointments.component.html',
  styleUrls: ['./admin-appointments.component.scss']
})
export class AdminAppointmentsComponent implements OnInit {
  f!: FormGroup;
  rows: AppointmentVM[] = [];
  loading = true;
  error = '';
  total = 0;

  private readonly API_ROOT = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(private fb: FormBuilder, private apptSvc: AppointmentService) {}

  ngOnInit(): void {
    this.f = this.fb.group({
      q: [''],
      city: [''],
      status: [''],
      dateFrom: [''],
      dateTo: ['']
    });

    this.fetch();

    this.f.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => this.fetch());
  }

  fetch() {
    this.loading = true; this.error = '';
    const { city, status, dateFrom, dateTo, q } = this.f.getRawValue();

    this.apptSvc.adminList({ city, status, dateFrom, dateTo }).subscribe({
      next: (list) => {
        const needle = (q || '').toString().trim().toLowerCase();
        const filtered = needle
          ? (list || []).filter(x =>
              (x.doctorName || '').toLowerCase().includes(needle) ||
              (x.patientName || '').toLowerCase().includes(needle))
          : (list || []);
        this.rows = filtered;
        this.total = filtered.length;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error(err);
        this.error = err?.error?.message || err?.statusText || 'Failed to load appointments';
        this.loading = false;
      }
    });
  }

  approve(a: AppointmentVM) {
    if (a.status !== 'Pending') { alert('Only Pending can be approved.'); return; }
    this.apptSvc.approve(a.id).subscribe({
      next: () => this.fetch(),
      error: (err) => alert(err?.error?.message || 'Approve failed')
    });
  }

  cancel(a: AppointmentVM) {
    if (!(a.status === 'Pending' || a.status === 'Booked')) { alert('Only Pending/Booked can be cancelled.'); return; }
    if (!confirm('Cancel this appointment?')) return;
    this.apptSvc.cancel(a.id).subscribe({
      next: () => this.fetch(),
      error: (err) => alert(err?.error?.message || 'Cancel failed')
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
    if (k === 'booked') return 'badge bg-primary';
    if (k === 'cancelled' || k === 'canceled') return 'badge bg-secondary';
    return 'badge bg-light text-dark';
  }

  trackById = (_: number, a: AppointmentVM) => a.id;
}
