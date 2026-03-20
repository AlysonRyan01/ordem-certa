import { Component, OnInit, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { startWith } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ALL_PAYMENT_METHODS } from '../../../core/helpers/sale-payment-method.helper';
import { SaleService } from '../../../core/services/sale.service';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-sale-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    SkeletonComponent,
  ],
  templateUrl: './sale-form.component.html',
})
export class SaleFormComponent implements OnInit {
  private readonly saleService = inject(SaleService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  readonly saving = signal(false);
  readonly loading = signal(false);
  readonly isEdit = signal(false);

  readonly allPaymentMethods = ALL_PAYMENT_METHODS;

  readonly form = new FormGroup({
    customerName:  new FormControl(''),
    description:   new FormControl(''),
    paymentMethod: new FormControl('', Validators.required),
    notes:         new FormControl(''),
    items: new FormArray<FormGroup>([]),
  });

  get itemsArray(): FormArray {
    return this.form.get('items') as FormArray;
  }

  get id(): string | null {
    return this.route.snapshot.paramMap.get('id');
  }

  // Reacts to FormArray value changes via RxJS → signal
  private readonly itemsValue = toSignal(
    this.itemsArray.valueChanges.pipe(startWith([])),
    { initialValue: [] as any[] }
  );

  grandTotal(): number {
    return this.itemsValue().reduce(
      (sum: number, i: any) =>
        sum + (parseFloat(i?.quantity) || 0) * (parseFloat(i?.unitPrice) || 0),
      0
    );
  }

  itemTotal(ctrl: AbstractControl): number {
    return (parseFloat(ctrl.get('quantity')?.value) || 0) *
           (parseFloat(ctrl.get('unitPrice')?.value) || 0);
  }

  asGroup(ctrl: AbstractControl): FormGroup {
    return ctrl as FormGroup;
  }

  ngOnInit(): void {
    if (this.id) {
      this.isEdit.set(true);
      this.loading.set(true);
      this.saleService.getById(this.id).subscribe({
        next: (sale) => {
          this.form.patchValue({
            customerName:  sale.customerName ?? '',
            description:   sale.description ?? '',
            paymentMethod: sale.paymentMethod,
            notes:         sale.notes ?? '',
          });
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    } else {
      this.addItem();
    }
  }

  addItem(): void {
    this.itemsArray.push(new FormGroup({
      description: new FormControl('', Validators.required),
      quantity:    new FormControl('1'),
      unitPrice:   new FormControl('0'),
    }));
  }

  removeItem(i: number): void {
    this.itemsArray.removeAt(i);
  }

  save(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);

    const v = this.form.getRawValue();

    if (this.isEdit() && this.id) {
      this.saleService.update(this.id, {
        customerName:  v.customerName  || undefined,
        description:   v.description   || undefined,
        paymentMethod: v.paymentMethod as any,
        notes:         v.notes         || undefined,
      }).subscribe({
        next: () => {
          this.snackBar.open('Venda atualizada.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/sales', this.id]);
        },
        error: () => this.saving.set(false),
      });
    } else {
      this.saleService.create({
        customerName:  v.customerName  || undefined,
        description:   v.description   || undefined,
        paymentMethod: v.paymentMethod as any,
        notes:         v.notes         || undefined,
        items: v.items.map((i: any) => ({
          description: i.description,
          quantity:    parseFloat(i.quantity) || 1,
          unitPrice:   parseFloat(i.unitPrice) || 0,
        })),
      }).subscribe({
        next: (sale) => {
          this.snackBar.open('Venda criada com sucesso.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/sales', sale.id]);
        },
        error: () => this.saving.set(false),
      });
    }
  }

  cancel(): void {
    this.router.navigate(this.id ? ['/sales', this.id] : ['/sales']);
  }

  formatCurrency(value: number): string {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }
}
