import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, AppUser } from '../../../core/services/user.service';

@Component({
  selector: 'app-manage-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-users.component.html'
})
export class ManageUsersComponent implements OnInit {
  users: AppUser[] = [];
  model: AppUser = { username: '', email: '', role: 'User', password: '' };
  editingId: number | null = null;
  loading = true;
  error = '';
  success = '';

  constructor(private svc: UserService) {}
  ngOnInit() { this.reload(); }

  reload() {
    this.loading = true; this.error = ''; this.success = '';
    this.svc.list().subscribe({
      next: (u) => { this.users = u || []; this.loading = false; },
      error: (e) => { this.error = e?.error || 'Failed to load'; this.loading = false; }
    });
  }

  add() {
    this.error = ''; this.success = '';
    if (!this.model.username?.trim() || !this.model.email?.trim() || !this.model.password?.trim()) {
      this.error = 'Username, Email and Password are required.'; return;
    }
    this.svc.create(this.model).subscribe({
      next: () => { this.success = 'User created'; this.reset(); this.reload(); },
      error: (e) => this.error = e?.error || 'Create failed'
    });
  }

  edit(u: AppUser) {
    this.editingId = u.id!;
    this.model = { username: u.username, email: u.email, role: u.role, password: '' };
  }

  save() {
    if (!this.editingId) return;
    this.error = ''; this.success = '';
    if (!this.model.username?.trim() || !this.model.email?.trim()) {
      this.error = 'Username and Email are required.'; return;
    }
    this.svc.update(this.editingId, this.model).subscribe({
      next: () => { this.success = 'User updated'; this.reset(); this.reload(); },
      error: (e) => this.error = e?.error || 'Update failed'
    });
  }

  remove(id?: number) {
    if (!id) return;
    if (!confirm('Delete user?')) return;
    this.svc.remove(id).subscribe({
      next: () => this.reload(),
      error: (e) => this.error = e?.error || 'Delete failed'
    });
  }

  reset() {
    this.editingId = null;
    this.model = { username: '', email: '', role: 'User', password: '' };
  }
}
