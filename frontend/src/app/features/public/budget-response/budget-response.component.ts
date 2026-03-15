import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ServiceOrderOutput } from '../../../core/models/service-order.model';
import { ServiceOrderService } from '../../../core/services/service-order.service';

type PageState = 'loading' | 'success' | 'already-answered' | 'error';

@Component({
  selector: 'app-budget-response',
  standalone: true,
  imports: [
    DecimalPipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './budget-response.component.html',
})
export class BudgetResponseComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(ServiceOrderService);

  readonly state = signal<PageState>('loading');
  readonly order = signal<ServiceOrderOutput | null>(null);
  readonly action = signal<'approve' | 'refuse'>('approve');

  ngOnInit(): void {
    const url = this.route.snapshot.url.map((s) => s.path).join('/');
    const isApprove = url.endsWith('approve');
    this.action.set(isApprove ? 'approve' : 'refuse');

    const id = this.route.snapshot.paramMap.get('id')!;
    const obs = isApprove
      ? this.orderService.approveBudget(id)
      : this.orderService.refuseBudget(id);

    obs.subscribe({
      next: (o) => {
        this.order.set(o);
        this.state.set('success');
      },
      error: (err) => {
        if (err.status === 400) {
          this.state.set('already-answered');
        } else {
          this.state.set('error');
        }
      },
    });
  }
}
