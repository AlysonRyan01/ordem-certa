import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaleOutput, SaleStatus } from '../../../core/models/sale.model';
import { ALL_SALE_STATUSES, getSaleStatusMeta } from '../../../core/helpers/sale-status.helper';
import { getPaymentMethodLabel } from '../../../core/helpers/sale-payment-method.helper';
import { SaleService } from '../../../core/services/sale.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageChange, PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { SkeletonTableComponent } from '../../../shared/components/skeleton/skeleton-table.component';

@Component({
  selector: 'app-sale-list',
  standalone: true,
  host: { class: 'flex flex-col flex-1 min-h-0' },
  imports: [
    CurrencyPipe,
    DatePipe,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    PaginationComponent,
    SkeletonTableComponent,
  ],
  templateUrl: './sale-list.component.html',
})
export class SaleListComponent implements OnInit {
  private readonly saleService = inject(SaleService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly sales = signal<SaleOutput[]>([]);
  readonly loading = signal(true);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly statusFilter = new FormControl<SaleStatus | ''>('');
  readonly searchControl = new FormControl('');

  readonly allStatuses = ALL_SALE_STATUSES;
  readonly columns = ['saleNumber', 'customer', 'description', 'status', 'paymentMethod', 'total', 'saleDate', 'actions'];

  readonly filteredSales = computed(() => {
    const term = (this.searchControl.value ?? '').toLowerCase().trim();
    if (!term) return this.sales();
    return this.sales().filter(
      (s) =>
        (s.customerName ?? '').toLowerCase().includes(term) ||
        (s.description ?? '').toLowerCase().includes(term)
    );
  });

  readonly getSaleStatusMeta = getSaleStatusMeta;
  readonly getPaymentMethodLabel = getPaymentMethodLabel;

  ngOnInit(): void {
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

  confirmDelete(sale: SaleOutput): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir venda',
        message: `Deseja excluir a venda #${sale.saleNumber}? Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.saleService.delete(sale.id).subscribe({
        next: () => {
          this.snackBar.open('Venda excluída.', 'Fechar', { duration: 3000 });
          this.load();
        },
      });
    });
  }

  private load(): void {
    this.loading.set(true);
    const status = this.statusFilter.value as SaleStatus | '' | null;
    const obs = status
      ? this.saleService.getByStatus(status as SaleStatus, this.page(), this.pageSize())
      : this.saleService.getAll(this.page(), this.pageSize());

    obs.subscribe((data) => {
      this.sales.set(data);
      this.total.set(
        data.length < this.pageSize()
          ? (this.page() - 1) * this.pageSize() + data.length
          : this.page() * this.pageSize() + 1
      );
      this.loading.set(false);
    });
  }
}
