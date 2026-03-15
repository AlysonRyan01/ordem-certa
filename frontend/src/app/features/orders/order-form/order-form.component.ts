import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CustomerOutput } from '../../../core/models/customer.model';
import { CustomerService } from '../../../core/services/customer.service';
import { ServiceOrderService } from '../../../core/services/service-order.service';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatAutocompleteModule,
  ],
  templateUrl: './order-form.component.html',
})
export class OrderFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly orderService = inject(ServiceOrderService);
  private readonly customerService = inject(CustomerService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editId = signal<string | null>(null);
  readonly customerSuggestions = signal<CustomerOutput[]>([]);
  readonly selectedCustomerId = signal<string | null>(null);

  readonly form = this.fb.group({
    customerSearch: [''],
    deviceType:     ['', Validators.required],
    brand:          ['', Validators.required],
    model:          ['', Validators.required],
    reportedDefect: ['', Validators.required],
    accessories:    [''],
    observations:   [''],
    technicianName: [''],
  });

  get isEdit(): boolean { return !!this.editId(); }

  constructor() {
    this.form.controls.customerSearch.valueChanges.pipe(
      debounceTime(350),
      distinctUntilChanged(),
      switchMap((term) =>
        term && term.length >= 2
          ? this.customerService.search(term, 1, 10)
          : of([])
      ),
      takeUntilDestroyed()
    ).subscribe((customers) => this.customerSuggestions.set(customers));
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(id);
      this.loading.set(true);
      this.orderService.getById(id).subscribe({
        next: (o) => {
          this.selectedCustomerId.set(o.customerId);
          this.form.patchValue({
            deviceType:     o.deviceType,
            brand:          o.brand,
            model:          o.model,
            reportedDefect: o.reportedDefect,
            accessories:    o.accessories ?? '',
            observations:   o.observations ?? '',
            technicianName: o.technicianName ?? '',
          });
          this.loading.set(false);
        },
      });
    }
  }

  selectCustomer(customer: CustomerOutput): void {
    this.selectedCustomerId.set(customer.id);
    this.form.controls.customerSearch.setValue(customer.name, { emitEvent: false });
  }

  displayCustomer = (c: CustomerOutput | null): string => c?.name ?? '';

  onSubmit(): void {
    if (this.form.invalid || this.saving()) return;
    if (!this.isEdit && !this.selectedCustomerId()) {
      this.snackBar.open('Selecione um cliente.', 'Fechar', { duration: 3000 });
      return;
    }

    this.saving.set(true);
    const v = this.form.getRawValue();

    const obs = this.isEdit
      ? this.orderService.update(this.editId()!, {
          deviceType: v.deviceType!, brand: v.brand!, model: v.model!,
          reportedDefect: v.reportedDefect!,
          accessories: v.accessories || undefined,
          observations: v.observations || undefined,
          technicianName: v.technicianName || undefined,
        })
      : this.orderService.create({
          customerId: this.selectedCustomerId()!,
          deviceType: v.deviceType!, brand: v.brand!, model: v.model!,
          reportedDefect: v.reportedDefect!,
          accessories: v.accessories || undefined,
          observations: v.observations || undefined,
          technicianName: v.technicianName || undefined,
        });

    obs.subscribe({
      next: (order) => {
        this.snackBar.open(
          this.isEdit ? 'Ordem atualizada.' : `Ordem #${order.orderNumber} criada.`,
          'Fechar', { duration: 4000 }
        );
        this.router.navigate(['/orders', order.id]);
      },
      error: (err) => {
        if (err.status === 400 && err.error?.errors?.some((e: string) => e.includes('limite'))) {
          this.snackBar.open(
            '⚠️ Limite do plano Demo atingido. Faça upgrade para continuar.',
            'Fechar', { duration: 6000 }
          );
        }
        this.saving.set(false);
      },
    });
  }

  cancel(): void {
    this.router.navigate(this.isEdit ? ['/orders', this.editId()] : ['/orders']);
  }
}
