import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { filter, map, startWith } from 'rxjs';

export interface Breadcrumb {
  label: string;
  url: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  template: `
    @if (crumbs().length > 1) {
      <nav class="flex items-center gap-1 text-xs text-slate-400 mb-5">
        @for (crumb of crumbs(); track $index; let last = $last) {
          @if (!last) {
            <a [routerLink]="crumb.url" class="hover:text-slate-600 transition-colors no-underline font-medium">
              {{ crumb.label }}
            </a>
            <mat-icon class="!text-xs !w-3 !h-3 !leading-3 text-slate-300">chevron_right</mat-icon>
          } @else {
            <span class="text-slate-600 font-semibold">{{ crumb.label }}</span>
          }
        }
      </nav>
    }
  `,
})
export class BreadcrumbComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly crumbs = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      startWith(null),
      map(() => this.buildCrumbs(this.route.root))
    ),
    { initialValue: [] as Breadcrumb[] }
  );

  private buildCrumbs(route: ActivatedRoute, url = '', crumbs: Breadcrumb[] = []): Breadcrumb[] {
    const { children } = route;

    for (const child of children) {
      if (!child.snapshot) continue;
      const segments = child.snapshot.url.map((s) => s.path);
      const path = segments.length ? `${url}/${segments.join('/')}` : url;
      const label = child.snapshot.data['breadcrumb'];

      if (label) crumbs.push({ label, url: path });

      this.buildCrumbs(child, path, crumbs);
    }

    return crumbs;
  }
}
