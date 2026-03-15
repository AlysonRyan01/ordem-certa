import { Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { ServiceOrderStatus } from '../../../core/models/service-order.model';
import { statusMeta } from '../../../core/models/service-order-status.helper';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <span
      class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
      [class]="meta().bg + ' ' + meta().color"
    >
      <mat-icon class="!text-sm !w-4 !h-4 !leading-4">{{ meta().icon }}</mat-icon>
      {{ meta().label }}
    </span>
  `,
})
export class StatusBadgeComponent {
  readonly status = input.required<ServiceOrderStatus>();
  readonly meta = computed(() => statusMeta(this.status()));
}
