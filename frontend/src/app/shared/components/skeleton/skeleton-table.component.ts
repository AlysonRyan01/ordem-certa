import { Component, computed, input } from '@angular/core';
import { SkeletonComponent } from './skeleton.component';

@Component({
  selector: 'app-skeleton-table',
  standalone: true,
  imports: [SkeletonComponent],
  template: `
    <div class="flex flex-col gap-3">
      @for (row of rowArray(); track $index) {
        <div class="flex gap-4">
          @for (col of colArray(); track $index) {
            <app-skeleton height="2.5rem" [style.flex]="1" />
          }
        </div>
      }
    </div>
  `,
})
export class SkeletonTableComponent {
  readonly rows = input(5);
  readonly cols = input(4);

  readonly rowArray = computed(() => Array.from({ length: this.rows() }));
  readonly colArray = computed(() => Array.from({ length: this.cols() }));
}
