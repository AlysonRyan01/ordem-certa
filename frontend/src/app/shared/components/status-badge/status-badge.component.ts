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
      class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold whitespace-nowrap"
      [class]="meta().bg + ' ' + meta().color"
    >
      <mat-icon class="!text-[11px] !w-3 !h-3 !leading-3 flex-shrink-0">{{ meta().icon }}</mat-icon>
      {{ meta().label }}
    </span>
  `,
})
export class StatusBadgeComponent {
  readonly status = input.required<ServiceOrderStatus>();
  readonly meta = computed(() => statusMeta(this.status()));
}
