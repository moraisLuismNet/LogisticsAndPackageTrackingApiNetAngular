declare var L: any;

import { Component, signal, inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ShipmentService } from '../../../core/services/shipment.service';
import { CardComponent } from '../../../shared/components/card/card';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge';
import { Shipment, TrackingUpdate, ShipmentStatus } from '../../../core/models/api.models';

@Component({
  selector: 'app-tracking',
  template: `
    <div class="space-y-6">
      @if (loading()) {
        <app-loading-spinner />
      } @else if (!shipment()) {
        <!-- Tracking Number Search -->
        <div class="max-w-2xl mx-auto text-center">
          <h2 class="text-2xl font-bold text-textMain mb-2">Track Your Package</h2>
          <p class="text-textMuted mb-6">Enter a tracking number or shipment ID to see real-time updates</p>

          <div class="glass rounded-2xl p-8 glow">
            <div class="flex gap-4">
              <input
                [(ngModel)]="trackingInput"
                placeholder="Enter tracking number (e.g., abc1234567) or shipment ID"
                class="flex-1 px-4 py-3 rounded-xl bg-slate-800/50 border border-slate-700/50 text-textMain placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-brand/50 transition-all"
                (keyup.enter)="trackPackage()"
              />
              <button
                (click)="trackPackage()"
                [disabled]="!trackingInput() || trackingLoading()"
                class="px-6 py-3 rounded-xl bg-gradient-to-r from-brand to-brand-dark hover:from-brand-light hover:to-brand text-white font-medium transition-all glow disabled:opacity-50"
              >
                @if (trackingLoading()) {
                  <svg class="animate-spin h-5 w-5" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"/>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                  </svg>
                } @else {
                  Track
                }
              </button>
            </div>
            @if (trackingError()) {
              <div class="mt-4 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-300 text-sm">{{ trackingError() }}</div>
            }
          </div>
        </div>
      } @else {
        @let s = shipment()!;

        <!-- Shipment Info Header -->
        <app-card>
          <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
            <div>
              <div class="flex items-center gap-3 mb-2">
                <h2 class="text-2xl font-bold text-textMain">Shipment Details</h2>
                <app-status-badge [status]="s.status" />
              </div>
              <p class="font-mono text-brand text-lg">{{ s.trackingNumber }}</p>
            </div>
            <a routerLink="/tracking" class="px-4 py-2 rounded-xl bg-slate-700/50 hover:bg-slate-600/50 text-textMain font-medium transition-all inline-block text-center cursor-pointer">
              Back
            </a>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mt-6 pt-6 border-t border-slate-700/50">
            <div>
              <p class="text-sm text-textMuted mb-1">Sender</p>
              <p class="font-medium text-textMain">{{ s.mail }}</p>
            </div>
            <div>
              <p class="text-sm text-textMuted mb-1">Receiver</p>
              <p class="font-medium text-textMain">{{ s.receiverName }}</p>
            </div>
            <div>
              <p class="text-sm text-textMuted mb-1">Weight</p>
              <p class="font-medium text-textMain">{{ s.weight }} kg</p>
            </div>

          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
            <div>
              <p class="text-sm text-textMuted mb-1">Origin</p>
              <p class="text-textMain">{{ s.originAddress }}</p>
            </div>
            <div>
              <p class="text-sm text-textMuted mb-1">Destination</p>
              <p class="text-textMain">{{ s.destinationAddress }}</p>
            </div>
          </div>
        </app-card>

        <!-- Map -->
        <app-card>
          <h3 class="text-lg font-semibold text-textMain mb-4">Route Map</h3>
          <div #mapContainer class="w-full h-[400px] rounded-xl overflow-hidden"></div>
        </app-card>

        <!-- Progress Bar -->
        <app-card>
          <h3 class="text-lg font-semibold text-textMain mb-6">Shipment Progress</h3>
          <div class="relative">
            <div class="flex items-center justify-between mb-4">
              @for (step of progressSteps; track step.key) {
                <div class="flex flex-col items-center relative z-10" [class]="step.index === currentStepIndex ? 'scale-110' : ''">
                  <div [class]="'w-10 h-10 rounded-full flex items-center justify-center transition-all duration-300 ' + (step.index === currentStepIndex ? 'bg-red-500 glow animate-pulse-glow' : 'bg-slate-700')">
                    <span [class]="'text-sm font-bold ' + (step.index === currentStepIndex ? 'text-white' : 'text-slate-500')">{{ step.index + 1 }}</span>
                  </div>
                  <span [class]="'text-xs mt-2 font-medium ' + (step.index === currentStepIndex ? 'text-brand' : 'text-textMuted')">{{ step.label }}</span>
                </div>
              }
            </div>
            <div class="absolute top-5 left-0 right-0 h-0.5 bg-slate-700 -z-0">
              <div [class]="'h-full bg-brand transition-all duration-500'" [style.width.%]="progressPercent"></div>
            </div>
          </div>
        </app-card>
      }
    </div>
  `,
  standalone: true,
  imports: [FormsModule, RouterLink, CardComponent, LoadingSpinnerComponent, StatusBadgeComponent]
})
export class TrackingComponent implements OnInit {
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  private shipmentService = inject(ShipmentService);
  private route = inject(ActivatedRoute);
  private auth = inject(AuthService);

  private map: any = null;
  private markers: any[] = [];

  trackingInput = signal('');
  trackingLoading = signal(false);
  trackingError = signal('');

  loading = signal(false);
  shipment = signal<Shipment | null>(null);
  trackingUpdates = signal<TrackingUpdate[]>([]);
  isAdmin = signal(false);

  constructor() {
    this.isAdmin.set(this.auth.isAdmin());
  }

  readonly progressSteps = [
    { key: 'pending', label: 'Pending', index: 0 },
    { key: 'in-transit', label: 'In Transit', index: 1 },
    { key: 'delivered', label: 'Delivered', index: 2 }
  ];

  get currentStepIndex(): number {
    const status = this.shipment()?.status;
    if (!status) return 0;
    const statusOrder: Record<string, number> = {
      'Pending': 0,
      'InTransit': 1,
      'EnRouteToDelivery': 1,
      'Delivered': 2,
      'Cancelled': 0,
      'Incident': 1
    };
    return statusOrder[status] ?? 0;
  }

  get progressPercent(): number {
    return (this.currentStepIndex / (this.progressSteps.length - 1)) * 100;
  }

  ngOnInit(): void {
    const trackingParam = this.route.snapshot.paramMap.get('id');
    if (trackingParam) {
      this.loadByTrackingNumber(trackingParam);
    }
  }

  trackPackage(): void {
    const input = this.trackingInput().trim();
    if (!input) return;
    this.loadByTrackingNumber(input);
  }

  loadByTrackingNumber(trackingNumber: string): void {
    this.trackingLoading.set(true);
    this.loading.set(true);
    this.trackingError.set('');

    this.shipmentService.getByTrackingNumber(trackingNumber).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.shipment.set(response.data);
          this.trackingUpdates.set(response.data.trackingUpdates || []);
          setTimeout(() => this.loadMap(), 200);
        } else {
          this.trackingError.set(response.message || 'Shipment not found');
        }
        this.loading.set(false);
        this.trackingLoading.set(false);
      },
      error: () => {
        this.trackingError.set('Failed to load shipment');
        this.loading.set(false);
        this.trackingLoading.set(false);
      }
    });
  }

  resetTracking(): void {
    this.shipment.set(null);
    this.trackingUpdates.set([]);
    this.trackingInput.set('');
    this.loading.set(false);
  }

  private loadMap(): void {
    const s = this.shipment();
    if (!s || !this.mapContainer) return;

    setTimeout(() => {
      if (this.map) this.map.remove();
      this.markers = [];

      this.map = L.map(this.mapContainer.nativeElement);

      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
      }).addTo(this.map);

      const originMarker = s.status === 'Pending'
        ? L.circleMarker([s.originLatitude, s.originLongitude], { color: 'red', fillColor: '#ff4444', fillOpacity: 0.9, radius: 10 })
        : L.marker([s.originLatitude, s.originLongitude]);
      originMarker.addTo(this.map).bindPopup(`<b>Origin</b><br/>${s.originAddress}`);
      this.markers.push(originMarker);

      const destMarker = s.status === 'Delivered'
        ? L.circleMarker([s.destinationLatitude, s.destinationLongitude], { color: 'red', fillColor: '#ff4444', fillOpacity: 0.9, radius: 10 })
        : L.marker([s.destinationLatitude, s.destinationLongitude]);
      destMarker.addTo(this.map).bindPopup(`<b>Destination</b><br/>${s.destinationAddress}`);
      this.markers.push(destMarker);

      if (s.status === 'InTransit') {
        const midLat = (s.originLatitude + s.destinationLatitude) / 2;
        const midLng = (s.originLongitude + s.destinationLongitude) / 2;
        L.circleMarker([midLat, midLng], {
          color: 'red',
          fillColor: '#ff4444',
          fillOpacity: 0.9,
          radius: 10
        }).addTo(this.map)
          .bindPopup('<b>Current Position</b><br/>Package in transit');
      }

      const updates = this.trackingUpdates();
      if (updates.length > 0) {
        const last = updates[updates.length - 1];
        if (last.latitude && last.longitude) {
          const current = L.marker([last.latitude, last.longitude])
            .addTo(this.map)
            .bindPopup(`<b>Current Location</b><br/>${last.location || ''}`);
          this.markers.push(current);
        }
      }

      const group = L.featureGroup(this.markers);
      this.map.fitBounds(group.getBounds().pad(0.2));

      setTimeout(() => this.map.invalidateSize(), 200);
    }, 100);
  }

}
