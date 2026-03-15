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
      <nav class="flex items-center gap-1 text-sm text-gray-500 mb-4">
        @for (crumb of crumbs(); track crumb.url; let last = $last) {
          @if (!last) {
            <a [routerLink]="crumb.url" class="hover:text-gray-800 transition-colors">
              {{ crumb.label }}
            </a>
            <mat-icon class="text-base leading-none">chevron_right</mat-icon>
          } @else {
            <span class="text-gray-800 font-medium">{{ crumb.label }}</span>
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
      const segments = child.snapshot.url.map((s) => s.path);
      const path = segments.length ? `${url}/${segments.join('/')}` : url;
      const label = child.snapshot.data['breadcrumb'];

      if (label) crumbs.push({ label, url: path });

      this.buildCrumbs(child, path, crumbs);
    }

    return crumbs;
  }
}
