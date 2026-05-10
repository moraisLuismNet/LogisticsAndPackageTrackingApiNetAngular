import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  template: `
    <nav class="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-20">
          <div class="flex items-center gap-8">
            <a routerLink="/" class="flex items-center gap-2.5">
              <img
                src="assets/LogisticsAndPackageTracking.png"
                alt="ShipTrack"
                class="w-16 h-16 object-contain"
              />
              <span class="text-xl font-bold text-gray-900">ShipTrack</span>
            </a>
            <div class="hidden md:flex items-center gap-1">
              <a
                routerLink="/shipments"
                routerLinkActive="text-indigo-600 bg-indigo-50"
                [routerLinkActiveOptions]="{ exact: false }"
                class="px-3 py-2 rounded-lg text-sm font-medium text-gray-500 hover:text-gray-700 hover:bg-gray-50 transition-all"
              >
                Shipments
              </a>
              @if (auth.isAdmin()) {
                <a
                  routerLink="/admin/users"
                  routerLinkActive="text-indigo-600 bg-indigo-50"
                  class="px-3 py-2 rounded-lg text-sm font-medium text-gray-500 hover:text-gray-700 hover:bg-gray-50 transition-all"
                >
                  Users
                </a>
              }
            </div>
          </div>
          <div class="flex items-center gap-3">
            @if (auth.isAuthenticated()) {
              <span class="text-sm text-gray-500 hidden sm:block">{{ auth.getUser()?.email }}</span>
              @if (auth.isAdmin()) {
                <span
                  class="text-[10px] px-1.5 py-0.5 rounded bg-purple-100 text-purple-700 font-semibold"
                  >ADMIN</span
                >
              }
              <button
                (click)="logout()"
                class="px-3 py-1.5 rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 transition-all"
              >
                Logout
              </button>
            } @else {
              <a
                routerLink="/login"
                class="px-3 py-1.5 rounded-lg text-sm font-medium text-gray-500 hover:text-gray-700 hover:bg-gray-50 transition-all"
                >Login</a
              >
              <a
                routerLink="/register"
                class="px-4 py-1.5 rounded-lg text-sm font-medium bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm transition-all"
                >Register</a
              >
            }
          </div>
        </div>
      </div>
    </nav>
  `,
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
})
export class NavbarComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
