import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-button',
  template: `
    <button
      [type]="type()"
      [disabled]="disabled()"
      (click)="btnClick.emit($event)"
      [class]="buttonClass()"
      class="inline-flex items-center justify-center gap-2 px-5 py-2.5 rounded-lg font-medium transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
    >
      @if (loading()) {
        <svg class="animate-spin h-4 w-4" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"/>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
        </svg>
      }
      <span>{{ label() }}</span>
    </button>
  `,
  standalone: true
})
export class ButtonComponent {
  label = input.required<string>();
  type = input<'button' | 'submit'>('button');
  variant = input<'primary' | 'secondary' | 'danger'>('primary');
  disabled = input<boolean>(false);
  loading = input<boolean>(false);
  btnClick = output<MouseEvent>();

  buttonClass() {
    switch (this.variant()) {
      case 'primary':
        return 'bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm';
      case 'secondary':
        return 'bg-gray-100 hover:bg-gray-200 text-gray-700 border border-gray-300';
      case 'danger':
        return 'bg-red-600 hover:bg-red-700 text-white shadow-sm';
      default:
        return '';
    }
  }
}
