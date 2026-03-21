import { Component, OnInit, OnDestroy, inject, signal, computed, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, takeUntil, catchError } from 'rxjs';
import { CustomerOutput } from '../../../core/models/customer.model';
import { CustomerService } from '../../../core/services/customer.service';

export interface CommandResult {
  id: string;
  label: string;
  sublabel?: string;
  icon: string;
  route: string[];
  category: string;
}

@Component({
  selector: 'app-command-palette',
  standalone: true,
  imports: [ReactiveFormsModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './command-palette.component.html',
  styleUrl: './command-palette.component.scss',
})
export class CommandPaletteComponent implements OnInit, OnDestroy {
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

  private readonly customerService = inject(CustomerService);
  private readonly router = inject(Router);
  private readonly dialogRef = inject(MatDialogRef<CommandPaletteComponent>);
  private readonly destroy$ = new Subject<void>();

  readonly searchControl = new FormControl('');
  readonly results = signal<CommandResult[]>([]);
  readonly loading = signal(false);
  readonly activeIndex = signal(0);

  readonly hasQuery = computed(() => (this.searchControl.value ?? '').trim().length > 0);

  readonly quickLinks: CommandResult[] = [
    { id: 'q1', label: 'Dashboard', sublabel: 'Visão geral', icon: 'dashboard', route: ['/dashboard'], category: 'Navegar' },
    { id: 'q2', label: 'Nova ordem de serviço', sublabel: 'Criar ordem', icon: 'add_circle', route: ['/orders', 'new'], category: 'Navegar' },
    { id: 'q3', label: 'Novo cliente', sublabel: 'Cadastrar cliente', icon: 'person_add', route: ['/customers', 'new'], category: 'Navegar' },
    { id: 'q4', label: 'Nova venda', sublabel: 'Registrar venda', icon: 'shopping_cart', route: ['/sales', 'new'], category: 'Navegar' },
  ];

  ngOnInit(): void {
    setTimeout(() => this.searchInput?.nativeElement.focus(), 50);

    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((term) => {
        const q = (term ?? '').trim();
        if (q.length < 2) {
          this.results.set([]);
          this.loading.set(false);
          return of([]);
        }
        this.loading.set(true);
        return this.customerService.search(q, 1, 8).pipe(catchError(() => of([])));
      }),
      takeUntil(this.destroy$),
    ).subscribe((customers: CustomerOutput[]) => {
      this.loading.set(false);
      this.activeIndex.set(0);
      this.results.set(customers.map((c) => ({
        id: c.id,
        label: c.fullName,
        sublabel: c.phoneFormatted ?? c.email ?? '',
        icon: 'person',
        route: ['/customers', c.id],
        category: 'Clientes',
      })));
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  displayedItems(): CommandResult[] {
    return this.hasQuery() ? this.results() : this.quickLinks;
  }

  onKeydown(event: KeyboardEvent): void {
    const items = this.displayedItems();
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.activeIndex.update((i) => Math.min(i + 1, items.length - 1));
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.activeIndex.update((i) => Math.max(i - 1, 0));
    } else if (event.key === 'Enter') {
      const item = items[this.activeIndex()];
      if (item) this.select(item);
    } else if (event.key === 'Escape') {
      this.dialogRef.close();
    }
  }

  select(item: CommandResult): void {
    this.router.navigate(item.route);
    this.dialogRef.close();
  }
}
