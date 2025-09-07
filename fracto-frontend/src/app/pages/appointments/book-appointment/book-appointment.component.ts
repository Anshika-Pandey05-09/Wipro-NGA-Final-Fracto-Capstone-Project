import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { DoctorService } from '../../../core/services/doctor.service';
import { AppointmentService } from '../../../core/services/appointment.service';
import { Doctor } from 'app/models/doctor';
import { environment } from 'environments/environment'; // add

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './book-appointment.component.html',
  styleUrls: ['./book-appointment.component.scss']
})
export class BookAppointmentComponent implements OnInit {
  doctorId = 0;
  doctor: Doctor | null = null;

  specializationsMap = new Map<number | string, string>();

  date = '';
  minDate = '';
  slots: string[] = [];
  selectedSlot = '';

  loadingDoctor = true;
  loadingSlots = false;
  message = '';
  error = '';
  isLoggedIn = false;

  // Build root once: http://localhost:5002 from http://localhost:5002/api
  private readonly API_ROOT = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(
    private route: ActivatedRoute,
    private doctorService: DoctorService,
    private apptService: AppointmentService,
    private router: Router
  ) {}

  ngOnInit() {
    this.isLoggedIn = !!(localStorage.getItem('userId') || sessionStorage.getItem('dc_token'));

    const today = new Date();
    const pad = (n: number) => (n < 10 ? '0' + n : '' + n);
    this.minDate = `${today.getFullYear()}-${pad(today.getMonth() + 1)}-${pad(today.getDate())}`;
    this.date = this.minDate;

    this.doctorId = Number(this.route.snapshot.paramMap.get('id') || 0);
    if (!this.doctorId) {
      this.error = 'Invalid doctor id';
      this.loadingDoctor = false;
      return;
    }

    this.loadingDoctor = true;

    this.doctorService.getSpecializations().subscribe({
      next: (list: any[]) => {
        (list ?? []).forEach(s => {
          const id = s.id ?? s.specializationId ?? s.value ?? s.Id ?? s.SpecializationId;
          const name = s.name ?? s.specializationName ?? s.label ?? s.Name ?? s.SpecializationName;
          if (id != null && name) this.specializationsMap.set(id, name);
        });
      },
      error: () => {}
    });

    this.doctorService.getById(this.doctorId).subscribe({
      next: (d) => {
        this.doctor = d;
        this.loadingDoctor = false;
        this.loadSlots(true);
      },
      error: () => {
        this.error = 'Doctor not found';
        this.loadingDoctor = false;
      }
    });
  }

  //  Build correct URL for images in uploads/* or images/* (or bare file name)
  imgUrl(p?: string | null): string {
    const fallback = `${this.API_ROOT}/images/default.png`;
    if (!p) return fallback;

    // Already absolute?
    if (/^https?:\/\//i.test(p)) return p;

    // Normalize: remove leading slash, add folder if missing
    let clean = p.trim().replace(/^\/+/, '');
    if (!clean.includes('/')) {
      // bare filename like "abc.jpg" → assume images/*
      clean = `images/${clean}`;
    }
    return `${this.API_ROOT}/${clean}`;
  }

  onImgError(event: Event) {
    (event.target as HTMLImageElement).src = `${this.API_ROOT}/images/default.png`;
  }

  specName(): string {
    if (!this.doctor) return '';
    return this.specializationsMap.get(this.doctor.specializationId) || 'General';
  }

  onDateChange() {
    this.selectedSlot = '';
    this.loadSlots();
  }

  loadSlots(silent = false) {
    this.message = '';
    this.error = '';
    if (!this.date) { this.error = 'Please pick a date'; return; }

    if (!silent) this.loadingSlots = true;
    this.doctorService.getTimeslots(this.doctorId, this.date).subscribe({
      next: (s: string[]) => {
        this.slots = Array.isArray(s) ? s : [];
        this.loadingSlots = false;
      },
      error: () => {
        this.slots = [];
        this.loadingSlots = false;
        this.error = 'Could not load time slots';
      }
    });
  }

  book() {
    this.error = '';
    this.message = '';

    if (!this.isLoggedIn) {
      this.error = 'You must be logged in to book';
      return;
    }
    if (!this.date) {
      this.error = 'Please pick a date';
      return;
    }
    if (!this.selectedSlot) {
      this.error = 'Please select a time slot';
      return;
    }

    const userId = Number(localStorage.getItem('userId') || '0');
    if (!userId) {
      this.error = 'You must be logged in to book';
      return;
    }

    const payload = {
      userId,
      doctorId: this.doctorId,
      appointmentDate: this.date,
      timeSlot: this.selectedSlot
    };

    this.apptService.book(payload).subscribe({
      next: () => {
        this.message = 'Appointment booked successfully 🎉';
        setTimeout(() => this.router.navigate(['/my-appointments']), 1000);
      },
      error: (err) => {
        console.error(err);
        this.error = (err?.error && typeof err.error === 'string')
          ? err.error
          : 'Booking failed. Please try again.';
      }
    });
  }
}
