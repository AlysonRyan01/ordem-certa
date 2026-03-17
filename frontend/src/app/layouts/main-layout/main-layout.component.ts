import { Component, OnInit, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { map } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CompanyService } from '../../core/services/company.service';
import { BreadcrumbComponent } from '../../shared/components/breadcrumb/breadcrumb.component';

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

  readonly isMobile = toSignal(
    this.breakpoints.observe(Breakpoints.Handset).pipe(map((r) => r.matches)),
    { initialValue: false }
  );

  readonly sidenavOpened = signal(false);
  readonly plan = signal<'Demo' | 'Paid' | null>(null);

  readonly navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Ordens de Serviço', icon: 'build', route: '/orders' },
    { label: 'Clientes', icon: 'people', route: '/customers' },
    { label: 'Perfil', icon: 'person', route: '/profile' },
    { label: 'Planos', icon: 'workspace_premium', route: '/billing' },
  ];

  ngOnInit(): void {
    this.companyService.getMe().subscribe({
      next: (company) => this.plan.set(company.plan),
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
}
