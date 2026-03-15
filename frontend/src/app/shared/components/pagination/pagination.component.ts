import { Component, input, output } from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

export interface PageChange {
  page: number;
  pageSize: number;
}

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [MatPaginatorModule],
  template: `
    <mat-paginator
      [length]="total()"
      [pageSize]="pageSize()"
      [pageIndex]="page() - 1"
      [pageSizeOptions]="[10, 25, 50]"
      (page)="onPage($event)"
      showFirstLastButtons
    />
  `,
})
export class PaginationComponent {
  readonly total = input.required<number>();
  readonly page = input(1);
  readonly pageSize = input(10);

  readonly pageChange = output<PageChange>();

  onPage(event: PageEvent): void {
    this.pageChange.emit({
      page: event.pageIndex + 1,
      pageSize: event.pageSize,
    });
  }
}
