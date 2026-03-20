import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaleOutput } from '../../../core/models/sale.model';
import { getSaleStatusMeta } from '../../../core/helpers/sale-status.helper';
import { getPaymentMethodLabel } from '../../../core/helpers/sale-payment-method.helper';
import { SaleService } from '../../../core/services/sale.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-sale-detail',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
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
    MatTableModule,
    MatTooltipModule,
    SkeletonComponent,
  ],
  templateUrl: './sale-detail.component.html',
})
export class SaleDetailComponent implements OnInit {
  private readonly saleService = inject(SaleService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly sale = signal<SaleOutput | null>(null);
  readonly loading = signal(true);
  readonly acting = signal(false);
  readonly showWarrantyForm = signal(false);

  readonly warrantyForm = new FormGroup({
    duration: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
    unit: new FormControl<'Days' | 'Months' | 'Years' | null>(null, Validators.required),
  });

  readonly warrantyUnits = [
    { value: 'Days' as const,   label: 'Dias' },
    { value: 'Months' as const, label: 'Meses' },
    { value: 'Years' as const,  label: 'Anos' },
  ];

  readonly itemColumns = ['description', 'quantity', 'unitPrice', 'totalPrice'];

  readonly getSaleStatusMeta = getSaleStatusMeta;
  readonly getPaymentMethodLabel = getPaymentMethodLabel;

  get id(): string { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void { this.load(); }

  complete(): void {
    if (this.acting()) return;
    this.acting.set(true);
    this.saleService.complete(this.id).subscribe({
      next: (updated) => {
        this.sale.set(updated);
        this.snackBar.open('Venda concluída.', 'Fechar', { duration: 3000 });
        this.acting.set(false);
      },
      error: () => this.acting.set(false),
    });
  }

  confirmCancel(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancelar venda',
        message: 'Deseja cancelar esta venda? Esta ação não pode ser desfeita.',
        confirmLabel: 'Cancelar venda',
      },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.acting.set(true);
      this.saleService.cancel(this.id).subscribe({
        next: (updated) => {
          this.sale.set(updated);
          this.snackBar.open('Venda cancelada.', 'Fechar', { duration: 3000 });
          this.acting.set(false);
        },
        error: () => this.acting.set(false),
      });
    });
  }

  saveWarranty(): void {
    if (this.warrantyForm.invalid) return;
    const raw = this.warrantyForm.getRawValue();
    this.saleService.setWarranty(this.id, {
      duration: raw.duration!,
      unit: raw.unit!,
    }).subscribe({
      next: (updated) => {
        this.sale.set(updated);
        this.showWarrantyForm.set(false);
        this.warrantyForm.reset();
        this.snackBar.open('Garantia registrada.', 'Fechar', { duration: 3000 });
      },
    });
  }

  printReceipt(): void {
    this.saleService.printReceipt(this.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
    });
  }

  printWarranty(): void {
    this.saleService.printWarranty(this.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
    });
  }

  private load(): void {
    this.loading.set(true);
    this.saleService.getById(this.id).subscribe({
      next: (s) => {
        this.sale.set(s);
        this.loading.set(false);
      },
    });
  }
}
