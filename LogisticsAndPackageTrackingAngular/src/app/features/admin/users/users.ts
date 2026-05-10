import { Component, signal, inject, OnInit } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { CardComponent } from '../../../shared/components/card/card';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner';
import { AuthResponse } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-users',
  template: `
    <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="space-y-6">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">Users</h2>
          <p class="text-gray-500 mt-1">Manage registered users</p>
        </div>

        <app-card>
          @if (loading()) {
            <app-loading-spinner />
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full">
                <thead>
                  <tr class="text-left text-sm text-gray-500 border-b border-gray-200">
                    <th class="pb-3 font-medium">Name</th>
                    <th class="pb-3 font-medium">Email</th>
                    <th class="pb-3 font-medium">Contact</th>
                    <th class="pb-3 font-medium">Address</th>
                    <th class="pb-3 font-medium">Role</th>
                  </tr>
                </thead>
                <tbody>
                  @for (user of users(); track user.mail) {
                    <tr class="border-b border-gray-100 hover:bg-gray-50 transition-colors">
                      <td class="py-4 text-sm font-medium text-gray-900">{{ user.fullName }}</td>
                      <td class="py-4 text-sm text-gray-700">{{ user.mail }}</td>
                      <td class="py-4 text-sm text-gray-500">{{ user.mail }}</td>
                      <td class="py-4 text-sm text-gray-500 max-w-[200px] truncate">{{ user.address }}</td>
                      <td class="py-4">
                        <span
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [class.bg-indigo-100]="user.role === 'Admin'"
                          [class.text-indigo-800]="user.role === 'Admin'"
                          [class.bg-gray-100]="user.role !== 'Admin'"
                          [class.text-gray-700]="user.role !== 'Admin'"
                        >
                          {{ user.role || 'Customer' }}
                        </span>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            <div class="mt-4 text-sm text-gray-500">
              Showing {{ users().length }} users
            </div>
          }
        </app-card>
      </div>
    </div>
  `,
  standalone: true,
  imports: [CardComponent, LoadingSpinnerComponent],
})
export class AdminUsersComponent implements OnInit {
  private authService = inject(AuthService);

  loading = signal(true);
  users = signal<AuthResponse[]>([]);

  ngOnInit(): void {
    this.authService.getUsers().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.users.set(response.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
