import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NgxMaskDirective, provideNgxMask } from 'ngx-mask';
import { CustomerService } from '../../../core/services/customer.service';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    NgxMaskDirective,
  ],
  providers: [provideNgxMask()],
  templateUrl: './customer-form.component.html',
})
export class CustomerFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editId = signal<string | null>(null);

  readonly form = this.fb.group({
    fullName: ['', Validators.required],
    phone: ['', Validators.required],
    email: ['', Validators.email],
    document: [''],
    street: [''],
    number: [''],
    city: [''],
    state: ['', Validators.maxLength(2)],
  });

  get isEdit(): boolean {
    return !!this.editId();
  }

  get documentMask(): string {
    const digits = (this.form.controls.document.value ?? '').replace(/\D/g, '');
    return digits.length <= 11 ? '000.000.000-00' : '00.000.000/0000-00';
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(id);
      this.loading.set(true);
      this.customerService.getById(id).subscribe({
        next: (c) => {
          this.form.patchValue({
            fullName: c.name,
            email: c.email ?? '',
            document: c.document?.value ?? '',
            street: c.address?.street ?? '',
            number: c.address?.number ?? '',
            city: c.address?.city ?? '',
            state: c.address?.state ?? '',
          });
          this.loading.set(false);
        },
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);

    const value = this.form.getRawValue();
    const obs = this.isEdit
      ? this.customerService.update(this.editId()!, {
          fullName: value.fullName!,
          email: value.email || undefined,
          street: value.street || undefined,
          number: value.number || undefined,
          city: value.city || undefined,
          state: value.state || undefined,
        })
      : this.customerService.create({
          fullName: value.fullName!,
          phone: value.phone!,
          email: value.email || undefined,
          document: value.document || undefined,
          street: value.street || undefined,
          number: value.number || undefined,
          city: value.city || undefined,
          state: value.state || undefined,
        });

    obs.subscribe({
      next: (customer) => {
        this.snackBar.open(
          this.isEdit ? 'Cliente atualizado.' : 'Cliente criado.',
          'Fechar',
          { duration: 3000 }
        );
        this.router.navigate(['/customers', customer.id]);
      },
      error: () => this.saving.set(false),
    });
  }

  cancel(): void {
    this.router.navigate(this.isEdit ? ['/customers', this.editId()] : ['/customers']);
  }
}
