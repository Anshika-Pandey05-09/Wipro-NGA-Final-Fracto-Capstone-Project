import { Routes } from '@angular/router';
import { LoginComponent } from './pages/auth/login/login.component';
import { RegisterComponent } from './pages/auth/register/register.component';
import { DoctorListComponent } from './pages/doctors/doctor-list/doctor-list.component';
import { BookAppointmentComponent } from './pages/appointments/book-appointment/book-appointment.component';
import { MyAppointmentsComponent } from './pages/appointments/my-appointments/my-appointments.component';
import { ManageDoctorsComponent } from './pages/admin/manage-doctors/manage-doctors.component';
import { AdminAppointmentsComponent } from './pages/admin/admin-appointments/admin-appointments.component'; // ⬅ ensure this path
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { ManageUsersComponent } from './pages/admin/manage-user/manage-users.component';

export const routes: Routes = [
  { path: '', component: DoctorListComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'book/:id', component: BookAppointmentComponent, canActivate: [AuthGuard] },
  { path: 'my-appointments', component: MyAppointmentsComponent, canActivate: [AuthGuard] },
  { path: 'manage-doctors', component: ManageDoctorsComponent, canActivate: [AdminGuard] },
  { path: 'admin/appointments', component: AdminAppointmentsComponent, canActivate: [AdminGuard] }, //  NEW
  { path: 'admin/users', component: ManageUsersComponent, canActivate: [AdminGuard] },

  { path: '**', redirectTo: '' }
];
