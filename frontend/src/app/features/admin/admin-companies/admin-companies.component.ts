import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { DatePipe } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { AdminCompanyOutput } from '../../../core/models/admin.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-admin-companies',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    DatePipe,
    PaginationComponent,
  ],
  templateUrl: './admin-companies.component.html',
})
export class AdminCompaniesComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly companies = signal<AdminCompanyOutput[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);

  readonly searchControl = new FormControl('');

  readonly filtered = computed(() => {
    const term = (this.searchControl.value ?? '').toLowerCase().trim();
    if (!term) return this.companies();
    return this.companies().filter(
      (c) => c.name.toLowerCase().includes(term) || c.email.toLowerCase().includes(term)
    );
  });

  readonly displayedColumns = ['name', 'email', 'plan', 'subscription', 'orders', 'createdAt'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.adminService.getCompanies(this.page(), this.pageSize()).subscribe({
      next: (data) => {
        this.companies.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page.set(event.page);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  goToDashboard(): void {
    this.router.navigate(['/admin']);
  }

  logout(): void {
    this.adminService.logout();
    this.router.navigate(['/admin/login']);
  }
}
