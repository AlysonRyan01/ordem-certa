import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
import { RepairResult, ServiceOrderOutput, ServiceOrderStatus, WarrantyUnit } from '../../../core/models/service-order.model';
import { ALL_REPAIR_RESULTS, ALL_STATUSES, REPAIR_RESULT_META } from '../../../core/models/service-order-status.helper';
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
  readonly orderService = inject(ServiceOrderService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly order = signal<ServiceOrderOutput | null>(null);
  readonly loading = signal(true);
  readonly changingStatus = signal(false);
  readonly editingBudget = signal(false);

  readonly statusControl = new FormControl<ServiceOrderStatus | null>(null);

  readonly createBudgetForm = new FormGroup({
    value: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    description: new FormControl('', Validators.required),
    repairResult: new FormControl<RepairResult | null>(null),
    warrantyDuration: new FormControl<number | null>(null, Validators.min(1)),
    warrantyUnit: new FormControl<WarrantyUnit | null>(null),
  });

  readonly editBudgetForm = new FormGroup({
    value: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    description: new FormControl('', Validators.required),
    repairResult: new FormControl<RepairResult | null>(null),
    warrantyDuration: new FormControl<number | null>(null, Validators.min(1)),
    warrantyUnit: new FormControl<WarrantyUnit | null>(null),
  });

  readonly allStatuses = ALL_STATUSES;
  readonly allRepairResults = ALL_REPAIR_RESULTS;
  readonly repairResultMeta = REPAIR_RESULT_META;

  readonly warrantyUnits: { value: WarrantyUnit; label: string }[] = [
    { value: 'Days',   label: 'Dias' },
    { value: 'Months', label: 'Meses' },
    { value: 'Years',  label: 'Anos' },
  ];

  readonly canEditBudget = computed(() => {
    const status = this.order()?.status;
    return status !== 'BudgetApproved' && status !== 'Delivered' && status !== 'Cancelled';
  });

  get id(): string { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void { this.load(); }

  applyStatus(): void {
    const status = this.statusControl.value;
    if (!status || this.changingStatus()) return;
    this.changingStatus.set(true);

    this.orderService.changeStatus(this.id, { status }).subscribe({
      next: (updated) => {
        this.setOrder(updated);
        this.snackBar.open('Status atualizado.', 'Fechar', { duration: 3000 });
        this.changingStatus.set(false);

        if (status === 'ReadyForPickup') {
          this.askWhatsApp(
            'Notificar cliente por WhatsApp?',
            'Deseja enviar uma mensagem informando que o equipamento está pronto para retirada?',
            () => this.orderService.notifyReadyForPickup(this.id).subscribe(),
          );
        }

        if (status === 'Delivered') {
          this.orderService.print(this.id);
        }
      },
      error: () => this.changingStatus.set(false),
    });
  }

  createBudget(): void {
    if (this.createBudgetForm.invalid) return;
    const raw = this.createBudgetForm.getRawValue();

    this.orderService.createBudget(this.id, {
      value: raw.value!,
      description: raw.description!,
      repairResult: raw.repairResult ?? undefined,
      warrantyDuration: raw.warrantyDuration ?? undefined,
      warrantyUnit: raw.warrantyUnit ?? undefined,
    }).subscribe({
      next: (updated) => {
        this.setOrder(updated);
        this.createBudgetForm.reset();
        this.snackBar.open('Orçamento criado. Aguardando aprovação do cliente.', 'Fechar', { duration: 4000 });

        this.askWhatsApp(
          'Enviar orçamento por WhatsApp?',
          'Deseja enviar o orçamento para o cliente via WhatsApp?',
          () => this.orderService.notifyBudgetCreated(this.id).subscribe(),
        );
      },
    });
  }

  startEditBudget(): void {
    const o = this.order();
    if (!o) return;
    this.editBudgetForm.setValue({
      value: o.budgetValue ?? null,
      description: o.budgetDescription ?? '',
      repairResult: o.repairResult ?? null,
      warrantyDuration: o.warrantyDuration ?? null,
      warrantyUnit: o.warrantyUnit ?? null,
    });
    this.editingBudget.set(true);
  }

  cancelEditBudget(): void {
    this.editingBudget.set(false);
  }

  saveBudget(): void {
    if (this.editBudgetForm.invalid) return;
    const raw = this.editBudgetForm.getRawValue();

    this.orderService.updateBudget(this.id, {
      value: raw.value!,
      description: raw.description!,
      repairResult: raw.repairResult ?? undefined,
      warrantyDuration: raw.warrantyDuration ?? undefined,
      warrantyUnit: raw.warrantyUnit ?? undefined,
    }).subscribe({
      next: (updated) => {
        this.setOrder(updated);
        this.editingBudget.set(false);
        this.snackBar.open('Orçamento atualizado.', 'Fechar', { duration: 3000 });

        this.askWhatsApp(
          'Reenviar orçamento por WhatsApp?',
          'Deseja enviar o orçamento atualizado para o cliente via WhatsApp?',
          () => this.orderService.notifyBudgetCreated(this.id).subscribe(),
        );
      },
      error: () => {},
    });
  }

  approveBudget(): void {
    this.orderService.approveBudget(this.id).subscribe({
      next: (updated) => {
        this.setOrder(updated);
        this.snackBar.open('Orçamento aprovado.', 'Fechar', { duration: 3000 });

        this.askWhatsApp(
          'Notificar cliente por WhatsApp?',
          'Deseja enviar uma mensagem ao cliente confirmando a aprovação do orçamento?',
          () => this.orderService.notifyBudgetApproved(this.id).subscribe(),
        );
      },
    });
  }

  refuseBudget(): void {
    this.orderService.refuseBudget(this.id).subscribe({
      next: (updated) => {
        this.setOrder(updated);
        this.snackBar.open('Orçamento recusado.', 'Fechar', { duration: 3000 });

        this.askWhatsApp(
          'Notificar cliente por WhatsApp?',
          'Deseja enviar uma mensagem ao cliente informando que o orçamento foi recusado?',
          () => this.orderService.notifyBudgetRefused(this.id).subscribe(),
        );
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

  private askWhatsApp(title: string, message: string, onConfirm: () => void): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title, message, confirmLabel: 'Enviar' },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) onConfirm();
    });
  }

  private setOrder(o: ServiceOrderOutput): void {
    this.order.set(o);
    this.statusControl.setValue(o.status);
  }

  private load(): void {
    this.loading.set(true);
    this.orderService.getById(this.id).subscribe({
      next: (o) => {
        this.setOrder(o);
        this.loading.set(false);
      },
    });
  }
}
