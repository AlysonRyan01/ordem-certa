import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ServiceOrderOutput, ServiceOrderStatus } from '../../../core/models/service-order.model';
import { ALL_STATUSES } from '../../../core/models/service-order-status.helper';
import { ServiceOrderService } from '../../../core/services/service-order.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    StatusBadgeComponent,
    SkeletonComponent,
  ],
  templateUrl: './order-detail.component.html',
})
export class OrderDetailComponent implements OnInit {
  private readonly orderService = inject(ServiceOrderService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly order = signal<ServiceOrderOutput | null>(null);
  readonly loading = signal(true);
  readonly changingStatus = signal(false);

  readonly statusControl = new FormControl<ServiceOrderStatus | null>(null);
  readonly budgetValueControl = new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]);
  readonly budgetDescControl = new FormControl('', Validators.required);
  readonly allStatuses = ALL_STATUSES;

  get id(): string { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void { this.load(); }

  applyStatus(): void {
    const status = this.statusControl.value;
    if (!status || this.changingStatus()) return;
    this.changingStatus.set(true);

    this.orderService.changeStatus(this.id, { status }).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.snackBar.open('Status atualizado.', 'Fechar', { duration: 3000 });
        this.changingStatus.set(false);
      },
      error: () => this.changingStatus.set(false),
    });
  }

  createBudget(): void {
    if (this.budgetValueControl.invalid || this.budgetDescControl.invalid) return;

    this.orderService.createBudget(this.id, {
      value: this.budgetValueControl.value!,
      description: this.budgetDescControl.value!,
    }).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.budgetValueControl.reset();
        this.budgetDescControl.reset();
        this.snackBar.open('Orçamento criado. Aguardando aprovação do cliente.', 'Fechar', { duration: 4000 });
      },
    });
  }

  approveBudget(): void {
    this.orderService.approveBudget(this.id).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.snackBar.open('Orçamento aprovado.', 'Fechar', { duration: 3000 });
      },
    });
  }

  refuseBudget(): void {
    this.orderService.refuseBudget(this.id).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.snackBar.open('Orçamento recusado.', 'Fechar', { duration: 3000 });
      },
    });
  }

  confirmDelete(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir ordem',
        message: `Deseja excluir a ordem #${this.order()?.orderNumber}? Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
      },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.orderService.delete(this.id).subscribe({
        next: () => {
          this.snackBar.open('Ordem excluída.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/orders']);
        },
      });
    });
  }

  private load(): void {
    this.loading.set(true);
    this.orderService.getById(this.id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.statusControl.setValue(o.status);
        this.loading.set(false);
      },
    });
  }
}
