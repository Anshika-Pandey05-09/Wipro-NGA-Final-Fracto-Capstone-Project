import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorService } from '../../../core/services/doctor.service';
import { Doctor } from 'app/models/doctor';
import { environment } from 'environments/environment';

type SpecItem = { id: number | string; name: string };

@Component({
  selector: 'app-manage-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-doctors.component.html',
  styleUrls: ['./manage-doctors.component.scss']
})
export class ManageDoctorsComponent implements OnInit {
  doctors: Doctor[] = [];
  specializations: SpecItem[] = [];
  loading = true;
  error = '';

  filterText = '';
  filteredDoctors: Doctor[] = [];

  model: Doctor & { imageFile?: File | null } = {
    name: '',
    city: '',
    specializationId: 1,
    rating: 4.0,
    startTime: '09:00',
    endTime: '17:00',
    slotDurationMinutes: 30,
    profileImagePath: 'default.png'
  };
  editingDoctorId: number | null = null;
  previewUrl: string | null = null;

  private readonly API_ROOT = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(private doctorService: DoctorService) {}

  ngOnInit() {
    this.doctorService.getSpecializations().subscribe({
      next: (list: any[]) => {
        this.specializations = (list ?? []).map(s => ({
          id: s.id ?? s.specializationId ?? s.value ?? s.Id ?? s.SpecializationId,
          name: s.name ?? s.specializationName ?? s.label ?? s.Name ?? s.SpecializationName
        })).filter(x => x.id != null && x.name);
      }
    });
    this.load();
  }

  load() {
    this.loading = true;
    this.doctorService.getAll().subscribe({
      next: (res: Doctor[]) => {
        this.doctors = (res ?? []).map(d => ({
          ...d,
          startTime: this.fromApiTime(d.startTime),
          endTime: this.fromApiTime(d.endTime),
          profileImagePath: d.profileImagePath || 'default.png'
        }));
        this.filteredDoctors = this.doctors.slice();
        this.loading = false;
      },
      error: (err: any) => { console.error(err); this.error = 'Load failed'; this.loading = false; }
    });
  }

  private toApiTime(hhmm: string) { return /^\d{2}:\d{2}$/.test(hhmm) ? `${hhmm}:00` : hhmm; }
  private fromApiTime(hhmmss?: string) { const m = /^(\d{2}:\d{2})/.exec(hhmmss || ''); return m ? m[1] : (hhmmss || ''); }

  trackById = (_: number, d: Doctor) => d.id!;
  filter(list: Doctor[]): Doctor[] {
    const q = (this.filterText || '').trim().toLowerCase();
    if (!q) return list;
    return list.filter(d => `${d.name}`.toLowerCase().includes(q) || `${d.city}`.toLowerCase().includes(q));
  }
  specName(id: number | string): string { return this.specializations.find(s => `${s.id}` === `${id}`)?.name || 'General'; }
  imgUrl(p?: string): string {
    if (!p) return '/assets/default.png';
    if (/^https?:\/\//i.test(p)) return p;
    if (p.startsWith('/')) return `${this.API_ROOT}${p}`;
    return `${this.API_ROOT}/images/${p}`;
  }
  onImgError(ev: Event) { (ev.target as HTMLImageElement).src = '/assets/default.png'; }

  onFileChange(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0] || null;
    this.model.imageFile = file || null;
    this.previewUrl = file ? URL.createObjectURL(file) :
      (this.model.profileImagePath ? this.imgUrl(this.model.profileImagePath) : null);
  }

  add() {
    const finalize = (profileImagePath: string) => {
      const payload: Doctor = this.preparePayload(profileImagePath);
      this.doctorService.add(payload).subscribe({
        next: () => { this.resetForm(); this.load(); },
        error: (err: any) => { console.error(err); alert(this.readError(err, 'Add failed')); }
      });
    };

    if (this.model.imageFile) {
      this.doctorService.uploadProfileImage(this.model.imageFile).subscribe({
        next: res => finalize(res?.path || 'default.png'),
        error: () => finalize('default.png')
      });
    } else {
      finalize(this.model.profileImagePath || 'default.png');
    }
  }

  edit(d: Doctor) {
    this.editingDoctorId = d.id ?? null;
    this.model = { ...d, imageFile: null };
    this.previewUrl = d.profileImagePath ? this.imgUrl(d.profileImagePath) : null;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  update() {
    if (!this.editingDoctorId) return;

    const finalize = (profileImagePath: string) => {
      const payload: Doctor = this.preparePayload(profileImagePath);
      this.doctorService.update(this.editingDoctorId!, payload).subscribe({
        next: () => { this.resetForm(); this.editingDoctorId = null; this.load(); },
        error: (err: any) => { console.error(err); alert(this.readError(err, 'Update failed')); }
      });
    };

    if (this.model.imageFile) {
      this.doctorService.uploadProfileImage(this.model.imageFile).subscribe({
        next: res => finalize(res?.path || this.model.profileImagePath || 'default.png'),
        error: () => finalize(this.model.profileImagePath || 'default.png')
      });
    } else {
      finalize(this.model.profileImagePath || 'default.png');
    }
  }

  deleteDoctor(id?: number) {
    if (!id || !confirm('Delete this doctor?')) return;
    this.doctorService.delete(id).subscribe({
      next: () => this.load(),
      error: (err: any) => { console.error(err); alert(this.readError(err, 'Delete failed')); }
    });
  }

  cancelEdit() { this.editingDoctorId = null; this.resetForm(); }

  resetForm() {
    this.model = {
      name: '', city: '', specializationId: 1, rating: 4.0,
      startTime: '09:00', endTime: '17:00', slotDurationMinutes: 30,
      profileImagePath: 'default.png', imageFile: null
    };
    this.previewUrl = null;
  }

  private preparePayload(profileImagePath?: string): Doctor {
    const path = (profileImagePath || this.model.profileImagePath || 'default.png').trim();
    return {
      id: this.editingDoctorId || undefined,
      name: (this.model.name || '').trim(),
      city: (this.model.city || '').trim(),
      specializationId: Number(this.model.specializationId),
      rating: Number(this.model.rating),
      startTime: this.toApiTime(this.model.startTime),
      endTime: this.toApiTime(this.model.endTime),
      slotDurationMinutes: Number(this.model.slotDurationMinutes),
      profileImagePath: path
    };
  }

  private readError(err: any, fallback: string) {
    return err?.error?.title || err?.error?.message || err?.statusText || fallback;
  }
}
