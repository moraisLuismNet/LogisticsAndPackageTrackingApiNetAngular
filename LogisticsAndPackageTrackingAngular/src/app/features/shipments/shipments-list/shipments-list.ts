import { Component, signal, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { AuthService } from '../../../core/services/auth.service';
import { CardComponent } from '../../../shared/components/card/card';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge';
import { Shipment, ShipmentStatus, AuthResponse } from '../../../core/models/api.models';

@Component({
  selector: 'app-shipments-list',
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="space-y-6">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h2 class="text-2xl font-bold text-gray-900">Shipments</h2>
            <p class="text-gray-500">Manage and monitor all your shipments</p>
          </div>
          @if (auth.isAdmin()) {
            <button
              (click)="toggleCreateForm()"
              class="px-5 py-2.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-medium shadow-sm transition-all"
            >
              + New Shipment
            </button>
          }
        </div>

        <!-- Search and Filter -->
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1 relative">
            <svg
              class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
            <input
              [(ngModel)]="searchTerm"
              placeholder="Search by tracking number, email, or receiver..."
              class="w-full pl-12 pr-4 py-2.5 rounded-lg bg-white border border-gray-300 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all"
            />
          </div>
          <select
            [(ngModel)]="statusFilter"
            class="px-4 py-2.5 rounded-lg bg-white border border-gray-300 text-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all"
          >
            <option value="">All Statuses</option>
            @for (status of allStatuses; track status) {
              <option [value]="status">{{ formatStatus(status) }}</option>
            }
          </select>
        </div>

        <!-- Create Shipment Form -->
        @if (showCreateForm) {
          <app-card>
            <h3 class="text-lg font-semibold text-gray-900 mb-4">Create New Shipment</h3>
            <form (ngSubmit)="createShipment()" class="space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1.5">Select User</label>
                  <select
                    (change)="onUserSelect($event)"
                    class="w-full px-4 py-2.5 rounded-lg bg-white border border-gray-300 text-gray-900 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all"
                  >
                    <option value="">-- Select a User --</option>
                    @for (user of users(); track user.mail) {
                      <option [value]="user.mail">{{ user.fullName }} ({{ user.mail }})</option>
                    }
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1.5">Weight (kg)</label>
                  <input
                    type="number"
                    step="0.1"
                    [(ngModel)]="newShipment.weight"
                    name="weight"
                    placeholder="2.5"
                    class="w-full px-4 py-2.5 rounded-lg bg-white border border-gray-300 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all"
                  />
                </div>
              </div>

              @if (selectedUser()) {
                <div
                  class="p-4 rounded-xl bg-indigo-50 border border-indigo-100 grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in slide-in-from-top-2 duration-300"
                >
                  <div>
                    <p class="text-xs font-semibold text-indigo-400 uppercase tracking-wider mb-1">
                      Receiver Name
                    </p>
                    <p class="text-sm font-medium text-indigo-900">
                      {{ selectedUser()?.fullName }}
                    </p>
                  </div>
                  <div>
                    <p class="text-xs font-semibold text-indigo-400 uppercase tracking-wider mb-1">
                      Client Email
                    </p>
                    <p class="text-sm font-medium text-indigo-900">{{ selectedUser()?.mail }}</p>
                  </div>
                  <div class="md:col-span-2">
                    <p class="text-xs font-semibold text-indigo-400 uppercase tracking-wider mb-1">
                      Destination Address
                    </p>
                    <p class="text-sm font-medium text-indigo-900">
                      {{ selectedUser()?.address || 'No address provided' }}
                    </p>
                  </div>
                </div>
              }

              @if (createError()) {
                <div class="p-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">
                  {{ createError() }}
                </div>
              }
              <div class="flex gap-3">
                <button
                  type="submit"
                  [disabled]="createLoading() || !selectedUser()"
                  class="px-5 py-2.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-medium shadow-sm transition-all disabled:opacity-50"
                >
                  @if (createLoading()) {
                    Creating...
                  } @else {
                    Create Shipment
                  }
                </button>
                <button
                  type="button"
                  (click)="showCreateForm = false"
                  class="px-5 py-2.5 rounded-lg bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium transition-all"
                >
                  Cancel
                </button>
              </div>
            </form>
          </app-card>
        }

        <!-- Shipments Table -->
        <app-card>
          @if (loading()) {
            <app-loading-spinner />
          } @else if (filteredShipments().length === 0) {
            <div class="text-center py-12">
              <svg
                class="w-16 h-16 mx-auto text-gray-300 mb-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="1.5"
                  d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
                />
              </svg>
              <p class="text-gray-500 text-lg">No shipments found</p>
              <p class="text-gray-400 text-sm mt-1">Try adjusting your search or filters</p>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full">
                <thead>
                  <tr class="text-left text-sm text-gray-500 border-b border-gray-200">
                    <th class="pb-3 font-medium">Tracking Number</th>
                    <th class="pb-3 font-medium">Client Mail </th>
                    <th class="pb-3 font-medium">Receiver Name </th>
                    <th class="pb-3 font-medium">Destination</th>
                    <th class="pb-3 font-medium">Status</th>
                    <th class="pb-3 font-medium">Weight</th>
                  </tr>
                </thead>
                <tbody>
                  @for (shipment of filteredShipments(); track shipment.id) {
                    <tr class="border-b border-gray-100 hover:bg-gray-50 transition-colors">
                      <td class="py-4 font-mono text-sm">
                        <a [routerLink]="['/tracking', shipment.trackingNumber]" class="text-indigo-600 hover:text-indigo-700 font-mono">
                          {{ shipment.trackingNumber }}
                        </a>
                      </td>
                      <td class="py-4 text-sm text-gray-900">{{ shipment.mail }}</td>
                      <td class="py-4 text-sm text-gray-900">{{ shipment.receiverName }}</td>
                      <td class="py-4 text-sm text-gray-500 max-w-[150px] truncate">
                        {{ shipment.destinationAddress }}
                      </td>
                      <td class="py-4">
                        @if (auth.isAdmin() && editingStatusId() === shipment.id) {
                          <select
                            #statusSelect
                            (change)="changeStatus(shipment.trackingNumber, statusSelect.value)"
                            (blur)="editingStatusId.set(null)"
                            class="px-2 py-1 rounded border border-indigo-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                          >
                            @for (status of allStatuses; track status) {
                              <option [value]="status" [selected]="shipment.status === status">{{ formatStatus(status) }}</option>
                            }
                          </select>
                        } @else if (auth.isAdmin()) {
                          <button
                            (click)="editingStatusId.set(shipment.id); $event.stopPropagation()"
                            class="cursor-pointer"
                          >
                            <app-status-badge [status]="shipment.status" />
                          </button>
                        } @else {
                          <app-status-badge [status]="shipment.status" />
                        }
                      </td>
                      <td class="py-4 text-sm text-gray-500">{{ shipment.weight }} kg</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            <div class="mt-4 flex items-center justify-between text-sm text-gray-500">
              <span
                >Showing {{ filteredShipments().length }} of
                {{ shipments().length }} shipments</span
              >
            </div>
          }
        </app-card>
      </div>
    </div>
  `,
  standalone: true,
  imports: [FormsModule, RouterLink, CardComponent, LoadingSpinnerComponent, StatusBadgeComponent],
})
export class ShipmentsListComponent implements OnInit {
  private shipmentService = inject(ShipmentService);
  auth = inject(AuthService);

  loading = signal(true);
  shipments = signal<Shipment[]>([]);
  users = signal<AuthResponse[]>([]);
  selectedUser = signal<AuthResponse | null>(null);
  searchTerm = signal('');
  statusFilter = signal('');
  editingStatusId = signal<number | null>(null);

  showCreateForm = false;
  createLoading = signal(false);
  createError = signal('');
  newShipment = { mail: '', receiverName: '', destinationAddress: '', weight: 0 };

  readonly allStatuses = Object.values(ShipmentStatus);

  get filteredShipments(): () => Shipment[] {
    return () => {
      let result = this.shipments();
      const search = this.searchTerm().toLowerCase();
      const status = this.statusFilter();

      if (search) {
        result = result.filter(
          (s) =>
            s.trackingNumber.toLowerCase().includes(search) ||
            s.mail.toLowerCase().includes(search) ||
            s.receiverName.toLowerCase().includes(search),
        );
      }

      if (status) {
        result = result.filter((s) => s.status === status);
      }

      return result;
    };
  }

  ngOnInit(): void {
    this.loadShipments();
    this.loadUsers();
  }

  formatStatus(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  changeStatus(trackingNumber: string, newStatus: string): void {
    this.editingStatusId.set(null);
    this.shipmentService.updateStatus(trackingNumber, { status: newStatus as ShipmentStatus }).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadShipments();
        }
      },
    });
  }

  loadShipments(): void {
    this.shipmentService.getAll().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.shipments.set(response.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  loadUsers(): void {
    this.auth.getUsers().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.users.set(response.data);
        }
      },
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
    if (this.showCreateForm) {
      this.loadUsers();
    }
  }

  onUserSelect(event: Event): void {
    const mail = (event.target as HTMLSelectElement).value;
    const user = this.users().find((u) => u.mail === mail) || null;
    this.selectedUser.set(user);

    if (user) {
      this.newShipment.mail = user.mail;
      this.newShipment.receiverName = user.fullName;
      this.newShipment.destinationAddress = user.address || '';
    } else {
      this.newShipment.mail = '';
      this.newShipment.receiverName = '';
      this.newShipment.destinationAddress = '';
    }
  }

  createShipment(): void {
    if (
      !this.newShipment.mail ||
      !this.newShipment.destinationAddress ||
      !this.newShipment.receiverName
    ) {
      this.createError.set('Please select a user and ensure they have an address');
      return;
    }

    this.createLoading.set(true);
    this.createError.set('');

    this.shipmentService.create(this.newShipment).subscribe({
      next: (response) => {
        if (response.success) {
          this.showCreateForm = false;
          this.loadShipments();
          this.newShipment = { mail: '', receiverName: '', destinationAddress: '', weight: 0 };
          this.selectedUser.set(null);
        } else {
          this.createError.set(response.message);
        }
        this.createLoading.set(false);
      },
      error: () => {
        this.createError.set('Failed to create shipment');
        this.createLoading.set(false);
      },
    });
  }
}
