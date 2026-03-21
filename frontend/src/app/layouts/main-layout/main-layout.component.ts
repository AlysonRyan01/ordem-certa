import { Component, OnInit, OnDestroy, HostListener, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { map } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CompanyService } from '../../core/services/company.service';
import { BreadcrumbComponent } from '../../shared/components/breadcrumb/breadcrumb.component';
import { CommandPaletteComponent } from '../../shared/components/command-palette/command-palette.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatMenuModule,
    MatTooltipModule,
    MatChipsModule,
    MatBadgeModule,
    BreadcrumbComponent,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly companyService = inject(CompanyService);
  private readonly breakpoints = inject(BreakpointObserver);
  private readonly dialog = inject(MatDialog);

  readonly isMobile = toSignal(
    this.breakpoints.observe(Breakpoints.Handset).pipe(map((r) => r.matches)),
    { initialValue: false }
  );

  readonly sidenavOpened = signal(false);
  readonly plan = signal<'Demo' | 'Paid' | null>(null);

  readonly navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Ordens de Serviço', icon: 'build', route: '/orders' },
    { label: 'Vendas', icon: 'shopping_cart', route: '/sales' },
    { label: 'Clientes', icon: 'people', route: '/customers' },
    { label: 'Perfil', icon: 'person', route: '/profile' },
    { label: 'Planos', icon: 'workspace_premium', route: '/billing' },
  ];

  @HostListener('window:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault();
      this.openCommandPalette();
    }
  }

  ngOnInit(): void {
    this.companyService.getMe().subscribe({
      next: (company) => this.plan.set(company.plan),
    });
  }

  openCommandPalette(): void {
    if (this.dialog.openDialogs.length > 0) return;
    this.dialog.open(CommandPaletteComponent, {
      panelClass: 'command-palette-dialog',
      backdropClass: 'command-palette-backdrop',
      hasBackdrop: true,
      autoFocus: false,
      restoreFocus: false,
    });
  }

  toggleSidenav(): void {
    this.sidenavOpened.update((v) => !v);
  }

  closeSidenav(): void {
    this.sidenavOpened.set(false);
  }

  logout(): void {
    this.auth.logout();
  }

  openSupport(): void {
    window.open(`https://wa.me/${environment.supportWhatsApp}`, '_blank');
  }
}
