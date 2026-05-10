import { Component, input } from '@angular/core';
import { ShipmentStatus } from '../../../core/models/api.models';

@Component({
  selector: 'app-status-badge',
  template: `
    <span [class]="badgeClass()" class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold tracking-wide uppercase">
      <span class="w-1.5 h-1.5 rounded-full" [class]="dotClass()"></span>
      {{ statusLabel() }}
    </span>
  `,
  standalone: true
})
export class StatusBadgeComponent {
  status = input.required<ShipmentStatus>();

  badgeClass() {
    switch (this.status()) {
      case ShipmentStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case ShipmentStatus.InTransit:
        return 'bg-blue-100 text-blue-800';
      case ShipmentStatus.Delivered:
        return 'bg-green-100 text-green-800';
      case ShipmentStatus.Cancelled:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  dotClass() {
    switch (this.status()) {
      case ShipmentStatus.Pending:
        return 'bg-yellow-500';
      case ShipmentStatus.InTransit:
        return 'bg-blue-500';
      case ShipmentStatus.Delivered:
        return 'bg-green-500';
      case ShipmentStatus.Cancelled:
        return 'bg-red-500';
      default:
        return 'bg-gray-500';
    }
  }

  statusLabel() {
    return this.status().replace(/([A-Z])/g, ' $1').trim();
  }
}
