import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

import { DoctorService } from '../../../core/services/doctor.service';
import { Doctor } from 'app/models/doctor';
import { environment } from 'environments/environment';

@Component({
  selector: 'app-doctor-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './doctor-list.component.html',
  styleUrls: ['./doctor-list.component.scss']
})
export class DoctorListComponent implements OnInit {
  f!: FormGroup;

  doctors: Doctor[] = [];
  specializations: Array<{ id: number | string; name: string }> = [];
  specializationsMap = new Map<number | string, string>();

  ratingOptions = [1, 2, 3, 4, 4.5, 5];

  loading = true;
  error = '';

  /** e.g. http://localhost:5002 from http://localhost:5002/api */
  private readonly API_ROOT = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(
    private fb: FormBuilder,
    private doctorService: DoctorService
  ) {}

  ngOnInit() {
    this.f = this.fb.group({
      q: [''],
      city: [''],
      specializationId: [''],
      date: [''],
      minRating: [''],
      availableOnly: [false],
    });

    // Load dropdowns then search
    this.loadSpecializations(() => this.onSearch());

    // Debounced live search
    this.f.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => this.onSearch());
  }

  private loadSpecializations(after?: () => void) {
    this.doctorService.getSpecializations().subscribe({
      next: (res: any[]) => {
        this.specializations = (res ?? []).map(x => ({
          id: x.id ?? x.specializationId ?? x.value ?? x.Id ?? x.SpecializationId,
          name: x.name ?? x.specializationName ?? x.label ?? x.Name ?? x.SpecializationName
        })).filter(x => x.id != null && x.name);

        this.specializationsMap.clear();
        for (const s of this.specializations) this.specializationsMap.set(s.id, s.name);

        after?.();
      },
      error: () => after?.()
    });
  }

  onSearch() {
    const raw = this.f.getRawValue();

    const query: Record<string, any> = {};
    if (raw.q) query['q'] = String(raw.q).trim();                    // client-side
    if (raw.city) query['city'] = String(raw.city).trim();           // server supports
    if (raw.specializationId) query['specializationId'] = raw.specializationId; // server supports
    if (raw.date) query['date'] = raw.date;                          // client-side
    if (raw.minRating) query['minRating'] = Number(raw.minRating);   // server supports
    if (raw.availableOnly) query['availableOnly'] = true;            // client-side

    this.loading = true;
    this.error = '';

    this.doctorService.search(query).subscribe({
      next: (res: Doctor[]) => {
        let data = (res ?? []).map(d => ({
          ...d,
          startTime: this.fromApiTime(d.startTime),
          endTime: this.fromApiTime(d.endTime),
          profileImagePath: d.profileImagePath || 'default.png'
        }));

        // The API ignores q/date/availableOnly → apply client-side for those.
        const needClientSide =
          !!query['q'] || !!query['date'] || !!query['availableOnly'];
        if (needClientSide) data = this.applyClientFilters(data, query);

        this.doctors = data;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error(err);
        this.error = 'Could not load doctors';
        this.loading = false;
      }
    });
  }

  onReset() {
    this.f.reset({
      q: '',
      city: '',
      specializationId: '',
      date: '',
      minRating: '',
      availableOnly: false
    });
  }

  // ---------- UI helpers ----------
  trackById = (_: number, d: Doctor) => d.id!;

  specName(id: number | string): string {
    return this.specializationsMap.get(id) || 'General';
  }

  /** Build a displayable image URL from a stored path */
  imgUrl(p?: string): string {
    if (!p) return '/assets/default.png';
    if (/^https?:\/\//i.test(p)) return p;           // already absolute
    if (p.startsWith('/')) return `${this.API_ROOT}${p}`; // e.g. "/uploads/abc.png"
    return `${this.API_ROOT}/images/${p}`;           // e.g. "default.png"
  }

  onImgError(event: Event) {
    (event.target as HTMLImageElement).src = '/assets/default.png';
  }

  private fromApiTime(hhmmss?: string) {
    const m = /^(\d{2}:\d{2})/.exec(hhmmss || '');
    return m ? m[1] : (hhmmss || '');
  }

  /** Client-side safety net for filters UI supports but API ignores */
  private applyClientFilters(items: Doctor[], q: Record<string, any>): Doctor[] {
    return items.filter(d => {
      if (q['q']) {
        const needle = String(q['q']).toLowerCase();
        if (!(`${d.name}`.toLowerCase().includes(needle))) return false;
      }
      if (q['city'] && d.city?.toLowerCase() !== String(q['city']).toLowerCase()) return false;
      if (q['specializationId'] && Number(d.specializationId) !== Number(q['specializationId'])) return false;
      if (q['minRating'] && Number(d.rating) < Number(q['minRating'])) return false;

      if (q['date']) {
        if (!d.startTime || !d.endTime) return false;
      }

      if (q['availableOnly']) {
        if (!d.slotDurationMinutes || !d.startTime || !d.endTime) return false;
      }
      return true;
    });
  }
}
