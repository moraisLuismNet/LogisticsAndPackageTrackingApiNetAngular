import { Component, signal, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SafeHtmlPipe } from '../../pipes/safe-html.pipe';

@Component({
  selector: 'app-layout',
  template: `
    <div class="flex h-screen overflow-hidden">
      <!-- Sidebar -->
      <aside class="w-64 glass-strong flex flex-col animate-slide-in">
        <!-- Logo -->
        <div class="p-6 border-b border-slate-700/50">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-brand to-purple-600 flex items-center justify-center glow">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/>
              </svg>
            </div>
            <div>
              <h1 class="text-lg font-bold text-textMain">ShipTrack</h1>
              <p class="text-xs text-textMuted">Logistics Platform</p>
            </div>
          </div>
        </div>

        <!-- Navigation -->
        <nav class="flex-1 p-4 space-y-1">
          @for (item of navItems(); track item.path) {
            <a
              [routerLink]="item.path"
              routerLinkActive="bg-brand/20 text-brand border-brand/50"
              [routerLinkActiveOptions]="{ exact: item.exact }"
              class="flex items-center gap-3 px-4 py-3 rounded-xl text-textMuted hover:bg-slate-700/30 hover:text-textMain transition-all duration-200 border border-transparent"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" [innerHTML]="item.icon | safeHtml"></svg>
              <span class="font-medium">{{ item.label }}</span>
            </a>
          }
        </nav>

        <!-- User Section -->
        <div class="p-4 border-t border-slate-700/50">
          <div class="flex items-center gap-3 px-4 py-3">
            <div class="w-8 h-8 rounded-full bg-gradient-to-br from-brand to-purple-600 flex items-center justify-center text-sm font-bold">
              {{ userInitials() }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-textMain truncate">{{ userName() }}</p>
              <div class="flex items-center gap-2">
                <p class="text-xs text-textMuted truncate">{{ userEmail() }}</p>
                @if (isAdmin()) {
                  <span class="text-[10px] px-1.5 py-0.5 rounded bg-purple-500/20 text-purple-300 border border-purple-500/30 font-semibold">ADMIN</span>
                }
              </div>
            </div>
            <button (click)="logout()" class="text-textMuted hover:text-red-400 transition-colors" title="Logout">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
              </svg>
            </button>
          </div>
        </div>
      </aside>

      <!-- Main Content -->
      <main class="flex-1 overflow-y-auto">
        <!-- Top Bar -->
        <header class="sticky top-0 z-10 glass-strong px-6 py-4">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <h2 class="text-xl font-semibold text-textMain">{{ pageTitle() }}</h2>
              @if (isAdmin()) {
                <span class="text-xs px-2 py-1 rounded-lg bg-purple-500/20 text-purple-300 border border-purple-500/30 font-semibold">Admin Panel</span>
              }
            </div>
            <div class="flex items-center gap-4">
              <button class="p-2 rounded-lg text-textMuted hover:text-textMain hover:bg-slate-700/30 transition-all">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
                </svg>
              </button>
            </div>
          </div>
        </header>

        <!-- Page Content -->
        <div class="p-6 animate-fade-in">
          <router-outlet></router-outlet>
        </div>
      </main>
    </div>
  `,
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, SafeHtmlPipe]
})
export class LayoutComponent {
  auth = inject(AuthService);

  navItems = signal([
    { path: '/dashboard', label: 'Dashboard', exact: true, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>' },
    { path: '/shipments', label: 'Shipments', exact: false, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/>' },
    { path: '/tracking', label: 'Track Package', exact: true, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7"/>' }
  ]);

  userName = signal('');
  userEmail = signal('');
  userInitials = signal('?');
  isAdmin = signal(false);

  pageTitle = signal('Dashboard');

  constructor() {
    this.updateUserInfo();
  }

  updateUserInfo(): void {
    const user = this.auth.getUser();
    if (user?.fullName) {
      this.userName.set(user.fullName);
      this.userEmail.set(user.email);
      this.userInitials.set(user.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2));
      this.isAdmin.set(user.role === 'Admin');
    }
  }

  logout(): void {
    this.auth.logout();
    window.location.href = '/login';
  }
}
