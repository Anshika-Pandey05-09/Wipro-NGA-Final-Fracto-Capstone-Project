// import { Component } from '@angular/core';
// import { RouterModule, Router } from '@angular/router';
// import { CommonModule } from '@angular/common';
// import { AuthService } from '../../core/services/auth.service';

// @Component({
//   selector: 'app-navbar',
//   standalone: true,
//   imports: [CommonModule, RouterModule],
//   templateUrl: './navbar.component.html',
//   styleUrls: ['./navbar.component.scss']
// })
// export class NavbarComponent {
//   constructor(public auth: AuthService, private router: Router) {}



//   logout() {
//     this.auth.logout();
//     this.router.navigate(['/login']);
//   }
// }

import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit, OnDestroy {
  isLoggedIn = false;
  isAdmin = false;
  username = 'Account';

  private sub?: Subscription;

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    // initial
    this.refreshState();

    // react to role/login changes (if app doesn’t reload after login)
    this.sub = this.auth.role$.subscribe(() => this.refreshState());

    // also listen to storage in case login happened in another tab
    window.addEventListener('storage', this.onStorage);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    window.removeEventListener('storage', this.onStorage);
  }

  private onStorage = () => this.refreshState();

  private refreshState() {
    this.isLoggedIn = this.auth.isLoggedIn();
    this.isAdmin = this.auth.isAdmin();
    this.username = this.auth.getUsername();
  }

  logout() {
    this.auth.logout();
    this.refreshState();
    this.router.navigate(['/login']);
  }

  // helpers for template
  track(_i: number, item: any) { return item?.path || item; }
}
