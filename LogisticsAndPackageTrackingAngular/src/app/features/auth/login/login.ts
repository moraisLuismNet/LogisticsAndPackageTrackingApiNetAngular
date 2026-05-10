import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';

@Component({
  selector: 'app-login',
  template: `
    <div class="min-h-[80vh] flex items-center justify-center p-4">
      <div class="w-full max-w-md">
        <div class="text-center mb-10">
          <img
            src="assets/LogisticsAndPackageTracking.png"
            alt="ShipTrack"
            class="w-32 h-32 mx-auto object-contain mb-6"
          />
          <h1 class="text-3xl font-bold text-gray-900">Welcome Back</h1>
          <p class="text-gray-500 mt-1">Sign in to your ShipTrack account</p>
        </div>

        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          @if (error()) {
            <div class="mb-6 p-4 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">
              {{ error() }}
            </div>
          }

          <form (ngSubmit)="onSubmit()" class="space-y-5">
            <app-input
              label="Email Address"
              type="email"
              placeholder="you@example.com"
              [(value)]="email"
            />
            <app-input
              label="Password"
              type="password"
              placeholder="Enter your password"
              [(value)]="password"
            />
            <div class="mt-8">
              <app-button
                label="Sign In"
                type="submit"
                variant="primary"
                [loading]="loading()"
                [disabled]="!email() || !password()"
                class="w-full"
              />
            </div>
          </form>

          <div class="mt-6 text-center">
            <p class="text-gray-500 text-sm">
              Don't have an account?
              <a routerLink="/register" class="text-indigo-600 hover:text-indigo-700 font-medium"
                >Create one</a
              >
            </p>
          </div>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonComponent, InputComponent],
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = signal('');
  password = signal('');
  loading = signal(false);
  error = signal('');

  constructor() {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/shipments']);
    }
  }

  onSubmit(): void {
    if (!this.email() || !this.password()) return;

    this.loading.set(true);
    this.error.set('');

    this.auth.login({ mail: this.email(), password: this.password() }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.auth.setToken(response.data.token);
          this.auth.setUser(response.data);
          this.router.navigate(['/shipments']);
        } else {
          this.error.set(response.message || 'Login failed');
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'An unexpected error occurred');
        this.loading.set(false);
      },
    });
  }
}
