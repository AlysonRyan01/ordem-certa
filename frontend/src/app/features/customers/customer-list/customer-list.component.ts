import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CustomerOutput } from '../../../core/models/customer.model';
import { CustomerService } from '../../../core/services/customer.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PaginationComponent, PageChange } from '../../../shared/components/pagination/pagination.component';
import { SkeletonTableComponent } from '../../../shared/components/skeleton/skeleton-table.component';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  host: { class: 'flex flex-col flex-1 min-h-0' },
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    PaginationComponent,
    SkeletonTableComponent,
  ],
  templateUrl: './customer-list.component.html',
})
export class CustomerListComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly customers = signal<CustomerOutput[]>([]);
  readonly loading = signal(true);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly searchControl = new FormControl('');
  readonly columns = ['name', 'document', 'email', 'phones', 'actions'];

  constructor() {
    this.searchControl.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(() => {
        this.page.set(1);
        this.load();
      });
  }

  ngOnInit(): void {
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

  confirmDelete(customer: CustomerOutput): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir cliente',
        message: `Deseja excluir o cliente "${customer.fullName}"? Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.customerService.delete(customer.id).subscribe({
        next: () => {
          this.snackBar.open('Cliente excluído com sucesso.', 'Fechar', { duration: 3000 });
          this.load();
        },
      });
    });
  }

  private load(): void {
    this.loading.set(true);
    const term = this.searchControl.value?.trim();
    const obs = term
      ? this.customerService.search(term, this.page(), this.pageSize())
      : this.customerService.getPaged(this.page(), this.pageSize());

    obs.subscribe((data) => {
      this.customers.set(data);
      this.total.set(data.length < this.pageSize() ? (this.page() - 1) * this.pageSize() + data.length : this.page() * this.pageSize() + 1);
      this.loading.set(false);
    });
  }
}
