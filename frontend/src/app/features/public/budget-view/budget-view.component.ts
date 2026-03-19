import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ServiceOrderOutput } from '../../../core/models/service-order.model';
import { ServiceOrderService } from '../../../core/services/service-order.service';

type PageState = 'loading' | 'view' | 'confirming' | 'success' | 'already-answered' | 'error';

@Component({
  selector: 'app-budget-view',
  standalone: true,
  imports: [DecimalPipe, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './budget-view.component.html',
})
export class BudgetViewComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(ServiceOrderService);

  readonly state = signal<PageState>('loading');
  readonly order = signal<ServiceOrderOutput | null>(null);
  readonly action = signal<'approve' | 'refuse' | null>(null);
  readonly submitting = signal(false);

  get id(): string { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void {
    this.orderService.getPublicById(this.id).subscribe({
      next: (o) => {
        this.order.set(o);
        if (o.budgetStatus !== 'Waiting' && o.budgetStatus !== 'Entered') {
          this.state.set('already-answered');
        } else {
          this.state.set('view');
        }
      },
      error: () => this.state.set('error'),
    });
  }

  confirm(action: 'approve' | 'refuse'): void {
    this.action.set(action);
    this.state.set('confirming');
  }

  cancel(): void {
    this.action.set(null);
    this.state.set('view');
  }

  submit(): void {
    if (this.submitting()) return;
    this.submitting.set(true);
    const obs = this.action() === 'approve'
      ? this.orderService.approveBudgetFromLink(this.id)
      : this.orderService.refuseBudgetFromLink(this.id);

    obs.subscribe({
      next: (o) => {
        this.order.set(o);
        this.state.set('success');
        this.submitting.set(false);
      },
      error: (err) => {
        if (err.status === 400) {
          this.state.set('already-answered');
        } else {
          this.state.set('error');
        }
        this.submitting.set(false);
      },
    });
  }
}
