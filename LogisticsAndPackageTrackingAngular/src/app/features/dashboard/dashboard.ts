import { Component, signal, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ShipmentService } from '../../core/services/shipment.service';
import { CardComponent } from '../../shared/components/card/card';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge';
import { Shipment, ShipmentStatus } from '../../core/models/api.models';

interface StatCard {
  label: string;
  value: number;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  template: `
    @if (isAuthenticated()) {
      <div class="space-y-6">
        <!-- Stats Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          @for (stat of stats(); track stat.label) {
            <app-card>
              <div class="flex items-center gap-4">
                <div [class]="'w-12 h-12 rounded-xl flex items-center justify-center ' + stat.color">
                  <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" [innerHTML]="stat.icon"></svg>
                </div>
                <div>
                  <p class="text-2xl font-bold text-textMain">{{ stat.value }}</p>
                  <p class="text-sm text-textMuted">{{ stat.label }}</p>
                </div>
              </div>
            </app-card>
          }
        </div>

        <!-- Quick Actions -->
        <app-card>
          <h3 class="text-lg font-semibold text-textMain mb-4">Quick Actions</h3>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <a routerLink="/shipments" class="flex items-center gap-4 p-4 rounded-xl bg-slate-800/30 hover:bg-slate-700/30 border border-slate-700/30 hover:border-brand/30 transition-all duration-200 group">
              <div class="w-10 h-10 rounded-lg bg-blue-500/20 flex items-center justify-center group-hover:bg-blue-500/30 transition-colors">
                <svg class="w-5 h-5 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                </svg>
              </div>
              <div>
                <p class="font-medium text-textMain">View Shipments</p>
                <p class="text-sm text-textMuted">Browse all shipments</p>
              </div>
            </a>

            <a routerLink="/tracking" class="flex items-center gap-4 p-4 rounded-xl bg-slate-800/30 hover:bg-slate-700/30 border border-slate-700/30 hover:border-brand/30 transition-all duration-200 group">
              <div class="w-10 h-10 rounded-lg bg-purple-500/20 flex items-center justify-center group-hover:bg-purple-500/30 transition-colors">
                <svg class="w-5 h-5 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7"/>
                </svg>
              </div>
              <div>
                <p class="font-medium text-textMain">Track Package</p>
                <p class="text-sm text-textMuted">Enter tracking number</p>
              </div>
            </a>

            <button (click)="showCreateForm = !showCreateForm" class="flex items-center gap-4 p-4 rounded-xl bg-slate-800/30 hover:bg-slate-700/30 border border-slate-700/30 hover:border-brand/30 transition-all duration-200 group text-left w-full">
              <div class="w-10 h-10 rounded-lg bg-emerald-500/20 flex items-center justify-center group-hover:bg-emerald-500/30 transition-colors">
                <svg class="w-5 h-5 text-emerald-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
                </svg>
              </div>
              <div>
                <p class="font-medium text-textMain">New Shipment</p>
                <p class="text-sm text-textMuted">Create a new shipment</p>
              </div>
            </button>
          </div>
        </app-card>

        <!-- Create Shipment Form -->
        @if (showCreateForm) {
          <app-card>
            <h3 class="text-lg font-semibold text-textMain mb-4">Create New Shipment</h3>
            <form (ngSubmit)="createShipment()" class="space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-textMuted mb-1.5">Client Email</label>
                  <input [(ngModel)]="newShipment.mail" name="mail" placeholder="client@example.com" class="w-full px-4 py-2.5 rounded-xl bg-slate-800/50 border border-slate-700/50 text-textMain placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-brand/50 transition-all"/>
                </div>
                <div>
                  <label class="block text-sm font-medium text-textMuted mb-1.5">Receiver Name</label>
                  <input [(ngModel)]="newShipment.receiverName" name="receiverName" placeholder="Jane Smith" class="w-full px-4 py-2.5 rounded-xl bg-slate-800/50 border border-slate-700/50 text-textMain placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-brand/50 transition-all"/>
                </div>
              </div>
              <div>
                <label class="block text-sm font-medium text-textMuted mb-1.5">Destination Address</label>
                <input [(ngModel)]="newShipment.destinationAddress" name="destinationAddress" placeholder="456 Oak Ave, City, State" class="w-full px-4 py-2.5 rounded-xl bg-slate-800/50 border border-slate-700/50 text-textMain placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-brand/50 transition-all"/>
              </div>
              <div>
                <label class="block text-sm font-medium text-textMuted mb-1.5">Weight (kg)</label>
                <input type="number" step="0.1" [(ngModel)]="newShipment.weight" name="weight" placeholder="2.5" class="w-full px-4 py-2.5 rounded-xl bg-slate-800/50 border border-slate-700/50 text-textMain placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-brand/50 transition-all"/>
              </div>
              @if (createError()) {
                <div class="p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-300 text-sm">{{ createError() }}</div>
              }
              <div class="flex gap-3">
                <button type="submit" [disabled]="createLoading()" class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-brand to-brand-dark hover:from-brand-light hover:to-brand text-white font-medium transition-all disabled:opacity-50">
                  @if (createLoading()) {
                    Creating...
                  } @else {
                    Create Shipment
                  }
                </button>
                <button type="button" (click)="showCreateForm = false" class="px-5 py-2.5 rounded-xl bg-slate-700/50 hover:bg-slate-600/50 text-textMain font-medium transition-all">Cancel</button>
              </div>
            </form>
          </app-card>
        }

        <!-- Recent Shipments -->
        <app-card>
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-textMain">Recent Shipments</h3>
            <a routerLink="/shipments" class="text-sm text-brand hover:text-brand-light font-medium transition-colors">View All</a>
          </div>

          @if (loading()) {
            <app-loading-spinner />
          } @else if (recentShipments().length === 0) {
            <div class="text-center py-8">
              <svg class="w-16 h-16 mx-auto text-slate-600 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/>
              </svg>
              <p class="text-textMuted">No shipments yet. Create your first one!</p>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full">
                <thead>
                  <tr class="text-left text-sm text-textMuted border-b border-slate-700/50">
                    <th class="pb-3 font-medium">Tracking Number</th>
                    <th class="pb-3 font-medium">Client Mail</th>
                    <th class="pb-3 font-medium">Receiver Name</th>
                    <th class="pb-3 font-medium">Status</th>
                    <th class="pb-3 font-medium">Weight</th>
                    <th class="pb-3 font-medium">Created</th>
                  </tr>
                </thead>
                <tbody>
                  @for (shipment of recentShipments(); track shipment.id) {
                    <tr class="border-b border-slate-800/50 hover:bg-slate-800/30 transition-colors cursor-pointer" [routerLink]="['/tracking', shipment.trackingNumber]">
                      <td class="py-3 font-mono text-sm text-brand">{{ shipment.trackingNumber }}</td>
                      <td class="py-3 text-sm text-textMain">{{ shipment.mail }}</td>
                      <td class="py-3 text-sm text-textMain">{{ shipment.receiverName }}</td>
                      <td class="py-3"><app-status-badge [status]="shipment.status" /></td>
                      <td class="py-3 text-sm text-textMuted">{{ shipment.weight }} kg</td>
                      <td class="py-3 text-sm text-textMuted">{{ shipment.createdAt | date:'short' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </app-card>
      </div>
    } @else {
      <!-- Public Landing Section -->
      <div class="min-h-screen flex items-center justify-center p-4 -m-6">
        <div class="w-full max-w-lg animate-fade-in">
          <div class="text-center mb-8">
            <div class="inline-flex items-center justify-center w-20 h-20 rounded-3xl bg-gradient-to-br from-brand to-purple-600 glow mb-6">
              <svg class="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/>
              </svg>
            </div>
            <h1 class="text-4xl font-bold text-textMain mb-2">ShipTrack</h1>
            <p class="text-lg text-textMuted">Logistics & Package Tracking Platform</p>
          </div>

          <div class="glass rounded-2xl p-8 glow space-y-4">
            <button routerLink="/login" class="w-full px-5 py-3 rounded-xl bg-gradient-to-r from-brand to-brand-dark hover:from-brand-light hover:to-brand text-white font-medium transition-all glow text-lg">
              Sign In
            </button>
            <button routerLink="/register" class="w-full px-5 py-3 rounded-xl bg-slate-700/50 hover:bg-slate-600/50 text-textMain font-medium transition-all border border-slate-600/50 text-lg">
              Create Account
            </button>
          </div>

          <p class="text-center text-textMuted text-sm mt-6">
            Manage shipments, track packages, and more.
          </p>
        </div>
      </div>
    }
  `,
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, CardComponent, LoadingSpinnerComponent, StatusBadgeComponent]
})
export class DashboardComponent implements OnInit {
  private auth = inject(AuthService);
  private shipmentService = inject(ShipmentService);

  isAuthenticated = this.auth.isAuthenticated;

  loading = signal(true);
  shipments = signal<Shipment[]>([]);
  stats = signal<StatCard[]>([]);
  recentShipments = signal<Shipment[]>([]);

  showCreateForm = false;
  createLoading = signal(false);
  createError = signal('');
  newShipment = {
    mail: '',
    receiverName: '',
    destinationAddress: '',
    weight: 0
  };

  ngOnInit(): void {
    if (this.isAuthenticated()) {
      this.loadShipments();
    }
  }

  loadShipments(): void {
    this.shipmentService.getAll().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.shipments.set(response.data);
          this.calculateStats(response.data);
          this.recentShipments.set(response.data.slice(0, 5));
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  calculateStats(shipments: Shipment[]): void {
    this.stats.set([
      { label: 'Total Shipments', value: shipments.length, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/>', color: 'bg-blue-500/20' },
      { label: 'In Transit', value: shipments.filter(s => s.status === ShipmentStatus.InTransit).length, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16V6a1 1 0 00-1-1H4a1 1 0 00-1 1v10a1 1 0 001 1h1m8-1a1 1 0 01-1 1H9m4-1V8a1 1 0 011-1h2.586a1 1 0 01.707.293l3.414 3.414a1 1 0 01.293.707V16a1 1 0 01-1 1h-1m-6-1a1 1 0 001 1h1M5 17a2 2 0 104 0m-4 0a2 2 0 114 0m6 0a2 2 0 104 0m-4 0a2 2 0 114 0"/>', color: 'bg-purple-500/20' },
      { label: 'Delivered', value: shipments.filter(s => s.status === ShipmentStatus.Delivered).length, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>', color: 'bg-emerald-500/20' },
      { label: 'Pending', value: shipments.filter(s => s.status === ShipmentStatus.Pending).length, icon: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>', color: 'bg-yellow-500/20' }
    ]);
  }

  createShipment(): void {
    if (!this.newShipment.mail || !this.newShipment.destinationAddress || !this.newShipment.receiverName) {
      this.createError.set('Please fill in all required fields');
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
        } else {
          this.createError.set(response.message);
        }
        this.createLoading.set(false);
      },
      error: () => {
        this.createError.set('Failed to create shipment');
        this.createLoading.set(false);
      }
    });
  }
}
