import { Component, input } from '@angular/core';

@Component({
  selector: 'app-card',
  template: `
    <div [class]="containerClass()" class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <ng-content></ng-content>
    </div>
  `,
  standalone: true
})
export class CardComponent {
  variant = input<'glass' | 'solid'>('glass');

  containerClass() {
    return this.variant() === 'glass'
      ? 'shadow-sm border-gray-200'
      : 'shadow-sm border-gray-200';
  }
}
