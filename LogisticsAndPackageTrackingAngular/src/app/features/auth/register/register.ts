import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';

@Component({
  selector: 'app-register',
  template: `
    <div class="min-h-[80vh] flex items-center justify-center p-4">
      <div class="w-full max-w-md">
        <div class="text-center mb-10">
          <img
            src="assets/LogisticsAndPackageTracking.png"
            alt="ShipTrack"
            class="w-32 h-32 mx-auto object-contain mb-6"
          />
          <h1 class="text-3xl font-bold text-gray-900">Create Account</h1>
          <p class="text-gray-500 mt-1">Get started with ShipTrack</p>
        </div>

        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          @if (error()) {
            <div class="mb-6 p-4 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">
              {{ error() }}
            </div>
          }

          @if (success()) {
            <div
              class="mb-6 p-4 rounded-lg bg-green-50 border border-green-200 text-green-700 text-sm"
            >
              {{ success() }}
            </div>
          }

          <form (ngSubmit)="onSubmit()" class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <app-input label="First Name" placeholder="John" [(value)]="firstName" />
              <app-input label="Last Name" placeholder="Doe" [(value)]="lastName" />
            </div>
            <app-input label="Mail" placeholder="client@example.com" [(value)]="mail" />
            <app-input label="Address" placeholder="123 Main St, City" [(value)]="address" />
            <app-input
              label="Password"
              type="password"
              placeholder="Min. 6 characters"
              [(value)]="password"
            />
            <div class="mt-8">
              <app-button
                label="Create Account"
                type="submit"
                variant="primary"
                [loading]="loading()"
                [disabled]="!firstName() || !lastName() || !password() || !mail() || !address()"
                class="w-full"
              />
            </div>
          </form>

          <div class="mt-6 text-center">
            <p class="text-gray-500 text-sm">
              Already have an account?
              <a routerLink="/login" class="text-indigo-600 hover:text-indigo-700 font-medium"
                >Sign in</a
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
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  firstName = signal('');
  lastName = signal('');
  mail = signal('');
  address = signal('');
  password = signal('');
  loading = signal(false);
  error = signal('');
  success = signal('');

  constructor() {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    if (
      !this.firstName() ||
      !this.lastName() ||
      !this.password() ||
      !this.mail() ||
      !this.address()
    )
      return;

    this.loading.set(true);
    this.error.set('');
    this.success.set('');

    this.auth
      .register({
        firstName: this.firstName(),
        lastName: this.lastName(),
        email: this.mail(), // Usamos mail como email
        password: this.password(),
        mail: this.mail(),
        address: this.address(),
      })
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.success.set('Account created successfully! Redirecting to login...');
            setTimeout(() => this.router.navigate(['/login']), 2000);
          } else {
            this.error.set(response.message || 'Registration failed');
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
