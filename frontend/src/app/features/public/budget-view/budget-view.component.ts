import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ServiceOrderOutput, ServiceOrderStatus } from '../../../core/models/service-order.model';
import { ServiceOrderService } from '../../../core/services/service-order.service';

type PageState = 'loading' | 'view' | 'confirming' | 'tracking' | 'error';

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

  readonly trackingSteps = [
    { label: 'Entrada',   icon: 'login' },
    { label: 'Orçamento', icon: 'request_quote' },
    { label: 'Conserto',  icon: 'build' },
    { label: 'Retirada',  icon: 'inventory_2' },
    { label: 'Entregue',  icon: 'done_all' },
  ];

  ngOnInit(): void {
    this.orderService.getPublicById(this.id).subscribe({
      next: (o) => {
        this.order.set(o);
        const awaitingBudget = o.budgetStatus === 'Waiting' || o.budgetStatus === 'Entered';
        this.state.set(awaitingBudget ? 'view' : 'tracking');
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
        this.state.set('tracking');
        this.submitting.set(false);
      },
      error: (err) => {
        if (err.status === 400) {
          this.orderService.getPublicById(this.id).subscribe({
            next: (o) => { this.order.set(o); this.state.set('tracking'); },
            error: () => this.state.set('error'),
          });
        } else {
          this.state.set('error');
        }
        this.submitting.set(false);
      },
    });
  }

  activeStepIndex(status: ServiceOrderStatus): number {
    const map: Partial<Record<ServiceOrderStatus, number>> = {
      UnderAnalysis:    0,
      AwaitingApproval: 1,
      BudgetApproved:   1,
      BudgetRefused:    3,
      UnderRepair:      2,
      ReadyForPickup:   3,
      Delivered:        4,
      Cancelled:        -1,
    };
    return map[status] ?? 0;
  }

  stepState(stepIndex: number, status: ServiceOrderStatus): 'completed' | 'active' | 'pending' {
    if (status === 'Delivered') return 'completed';
    const active = this.activeStepIndex(status);
    if (active === -1) return 'pending';
    if (stepIndex < active) return 'completed';
    if (stepIndex === active) return 'active';
    return 'pending';
  }

  statusIcon(status: ServiceOrderStatus): string {
    const map: Partial<Record<ServiceOrderStatus, string>> = {
      UnderAnalysis:    'pending_actions',
      AwaitingApproval: 'hourglass_top',
      BudgetApproved:   'thumb_up',
      BudgetRefused:    'thumb_down',
      UnderRepair:      'build',
      ReadyForPickup:   'inventory_2',
      Delivered:        'check_circle',
      Cancelled:        'block',
    };
    return map[status] ?? 'info';
  }

  statusHeadline(status: ServiceOrderStatus): string {
    const map: Partial<Record<ServiceOrderStatus, string>> = {
      UnderAnalysis:    'Em análise técnica',
      AwaitingApproval: 'Aguardando decisão',
      BudgetApproved:   'Orçamento aprovado!',
      BudgetRefused:    'Disponível para retirada',
      UnderRepair:      'Em conserto',
      ReadyForPickup:   'Pronto para retirada!',
      Delivered:        'Entregue com sucesso',
      Cancelled:        'Ordem cancelada',
    };
    return map[status] ?? 'Acompanhamento';
  }

  statusDescription(o: ServiceOrderOutput): string {
    const device = `${o.deviceType} ${o.brand} ${o.model}`;
    switch (o.status) {
      case 'UnderAnalysis':
        return `Recebemos seu ${device} e nossa equipe já está realizando a análise técnica. Em breve você receberá uma atualização.`;
      case 'BudgetApproved':
        return `Ótimo! O conserto foi autorizado e nossa equipe iniciará o reparo do seu ${device} em breve.`;
      case 'BudgetRefused':
        return `O orçamento foi recusado. Seu ${device} está disponível para retirada em nossa loja. Entre em contato para combinar.`;
      case 'UnderRepair':
        return `O reparo do seu ${device} está em andamento. Você será notificado assim que estiver pronto.`;
      case 'ReadyForPickup':
        return `Seu ${device} está pronto! Passe em nossa loja para retirar.`;
      case 'Delivered':
        return `Seu equipamento foi entregue. Obrigado pela confiança!`;
      case 'Cancelled':
        return `A ordem #${o.orderNumber} foi cancelada. Entre em contato com a assistência para mais informações.`;
      default:
        return 'Entre em contato com a assistência técnica para mais informações.';
    }
  }

  statusAccentColor(status: ServiceOrderStatus): string {
    const map: Partial<Record<ServiceOrderStatus, string>> = {
      UnderAnalysis:    '#f59e0b',
      AwaitingApproval: '#3b82f6',
      BudgetApproved:   '#10b981',
      BudgetRefused:    '#ef4444',
      UnderRepair:      '#f97316',
      ReadyForPickup:   '#059669',
      Delivered:        '#6366f1',
      Cancelled:        '#94a3b8',
    };
    return map[status] ?? '#64748b';
  }

  statusIconBg(status: ServiceOrderStatus): string {
    const map: Partial<Record<ServiceOrderStatus, string>> = {
      UnderAnalysis:    '#fef3c7',
      AwaitingApproval: '#dbeafe',
      BudgetApproved:   '#d1fae5',
      BudgetRefused:    '#fee2e2',
      UnderRepair:      '#ffedd5',
      ReadyForPickup:   '#d1fae5',
      Delivered:        '#e0e7ff',
      Cancelled:        '#f1f5f9',
    };
    return map[status] ?? '#f1f5f9';
  }

  statusIconColor(status: ServiceOrderStatus): string {
    const map: Partial<Record<ServiceOrderStatus, string>> = {
      UnderAnalysis:    '#d97706',
      AwaitingApproval: '#2563eb',
      BudgetApproved:   '#059669',
      BudgetRefused:    '#dc2626',
      UnderRepair:      '#ea580c',
      ReadyForPickup:   '#059669',
      Delivered:        '#4f46e5',
      Cancelled:        '#94a3b8',
    };
    return map[status] ?? '#64748b';
  }
}
