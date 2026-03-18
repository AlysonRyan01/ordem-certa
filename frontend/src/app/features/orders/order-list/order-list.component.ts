import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ServiceOrderOutput, ServiceOrderStatus } from '../../../core/models/service-order.model';
import { ALL_STATUSES } from '../../../core/models/service-order-status.helper';
import { ServiceOrderService } from '../../../core/services/service-order.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageChange, PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { SkeletonTableComponent } from '../../../shared/components/skeleton/skeleton-table.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-order-list',
  standalone: true,
  host: { class: 'flex flex-col flex-1 min-h-0' },
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    StatusBadgeComponent,
    PaginationComponent,
    SkeletonTableComponent,
  ],
  templateUrl: './order-list.component.html',
})
export class OrderListComponent implements OnInit {
  private readonly orderService = inject(ServiceOrderService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly orders = signal<ServiceOrderOutput[]>([]);
  readonly loading = signal(true);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly statusFilter = new FormControl<ServiceOrderStatus | ''>('');
  readonly allStatuses = ALL_STATUSES;
  readonly columns = ['orderNumber', 'customer', 'equipment', 'status', 'entryDate', 'technician', 'actions'];

  ngOnInit(): void {
    const statusParam = this.route.snapshot.queryParamMap.get('status') as ServiceOrderStatus | null;
    if (statusParam) {
      this.statusFilter.setValue(statusParam, { emitEvent: false });
    }

    this.statusFilter.valueChanges.subscribe(() => {
      this.page.set(1);
      this.load();
    });
    this.load();
  }

  onPageChange(event: PageChange): void {
    this.page.set(event.page);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  navigateTo(path: string[]): void {
    this.router.navigate(path);
  }

  confirmDelete(order: ServiceOrderOutput): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir ordem',
        message: `Deseja excluir a ordem #${order.orderNumber}? Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.orderService.delete(order.id).subscribe({
        next: () => {
          this.snackBar.open('Ordem excluída.', 'Fechar', { duration: 3000 });
          this.load();
        },
      });
    });
  }

  private load(): void {
    this.loading.set(true);
    const status = this.statusFilter.value;
    const obs = status
      ? this.orderService.getByStatus(status, this.page(), this.pageSize())
      : this.orderService.getPaged(this.page(), this.pageSize());

    obs.subscribe((data) => {
      this.orders.set(data);
      this.total.set(
        data.length < this.pageSize()
          ? (this.page() - 1) * this.pageSize() + data.length
          : this.page() * this.pageSize() + 1
      );
      this.loading.set(false);
    });
  }
}
