import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgxMaskDirective, provideNgxMask } from 'ngx-mask';
import { CustomerOutput } from '../../../core/models/customer.model';
import { CustomerService } from '../../../core/services/customer.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDividerModule,
    MatTooltipModule,
    NgxMaskDirective,
    SkeletonComponent,
  ],
  providers: [provideNgxMask()],
  templateUrl: './customer-detail.component.html',
})
export class CustomerDetailComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly customer = signal<CustomerOutput | null>(null);
  readonly loading = signal(true);
  readonly addingPhone = signal(false);

  readonly phoneControl = new FormControl('', [Validators.required, Validators.minLength(10)]);

  get id(): string {
    return this.route.snapshot.paramMap.get('id')!;
  }

  ngOnInit(): void {
    this.load();
  }

  addPhone(): void {
    if (this.phoneControl.invalid || this.addingPhone()) return;
    this.addingPhone.set(true);
    const phone = this.phoneControl.value!.replace(/\D/g, '');

    this.customerService.addPhone(this.id, { phone }).subscribe({
      next: (updated) => {
        this.customer.set(updated);
        this.phoneControl.reset();
        this.addingPhone.set(false);
        this.snackBar.open('Telefone adicionado.', 'Fechar', { duration: 3000 });
      },
      error: () => this.addingPhone.set(false),
    });
  }

  confirmRemovePhone(phone: string): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Remover telefone',
        message: `Deseja remover o telefone ${phone}?`,
        confirmLabel: 'Remover',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.customerService.removePhone(this.id, { phone }).subscribe({
        next: (updated) => {
          this.customer.set(updated);
          this.snackBar.open('Telefone removido.', 'Fechar', { duration: 3000 });
        },
      });
    });
  }

  confirmDelete(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir cliente',
        message: `Deseja excluir "${this.customer()?.fullName}"? Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.customerService.delete(this.id).subscribe({
        next: () => {
          this.snackBar.open('Cliente excluído.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/customers']);
        },
      });
    });
  }

  private load(): void {
    this.loading.set(true);
    this.customerService.getById(this.id).subscribe({
      next: (c) => {
        this.customer.set(c);
        this.loading.set(false);
      },
    });
  }
}
