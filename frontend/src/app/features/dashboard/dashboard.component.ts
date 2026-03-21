import { AfterViewInit, Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Chart, registerables } from 'chart.js';
import { DashboardOutput } from '../../core/models/dashboard.model';
import { DashboardService } from '../../core/services/dashboard.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { STATUS_META, StatusMeta } from '../../core/models/service-order-status.helper';
import { ServiceOrderStatus } from '../../core/models/service-order.model';

const STATUS_ORDER: ServiceOrderStatus[] = [
  'UnderAnalysis', 'AwaitingApproval', 'BudgetApproved', 'BudgetRefused', 'UnderRepair', 'ReadyForPickup', 'Delivered', 'Cancelled',
];

export interface StatusCount {
  status: ServiceOrderStatus;
  meta: StatusMeta;
  count: number;
}

Chart.register(...registerables);

const CHART_COLORS: Record<string, string> = {
  UnderAnalysis:   'rgba(249, 115,  22, 0.7)',
  AwaitingApproval:'rgba(99,  102, 241, 0.7)',
  BudgetApproved:  'rgba(16,  185,  89, 0.7)',
  BudgetRefused:   'rgba(239,  68,  68, 0.7)',
  UnderRepair:     'rgba(168,  85, 247, 0.7)',
  ReadyForPickup:  'rgba(20,  184, 166, 0.7)',
  Delivered:       'rgba(107, 114, 128, 0.7)',
  Cancelled:       'rgba(153,  27,  27, 0.7)',
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    StatusBadgeComponent,
    SkeletonComponent,
  ],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  @ViewChild('chartCanvas') set chartCanvas(el: ElementRef<HTMLCanvasElement> | undefined) {
    if (el) this.renderChart(el.nativeElement);
  }

  private readonly dashboardService = inject(DashboardService);

  readonly data = signal<DashboardOutput | null>(null);
  readonly loading = signal(true);

  allStatusCounts(d: DashboardOutput): StatusCount[] {
    const countMap = new Map(d.ordersByStatus.map((s) => [s.status, s.count]));
    return STATUS_ORDER.map((status) => ({
      status,
      meta: STATUS_META[status],
      count: countMap.get(status) ?? 0,
    }));
  }

  private chart: Chart | null = null;
  private pendingData: DashboardOutput | null = null;

  ngOnInit(): void {
    this.dashboardService.get().subscribe({
      next: (d) => {
        this.pendingData = d;
        this.data.set(d);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private renderChart(canvas: HTMLCanvasElement): void {
    const d = this.pendingData;
    if (!d || !d.ordersByStatus.length) return;

    if (this.chart) {
      this.chart.destroy();
    }

    const labels = d.ordersByStatus.map((s) => {
      const meta = STATUS_META[s.status as ServiceOrderStatus];
      return meta ? meta.label : s.status;
    });

    this.chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Ordens no mês',
          data: d.ordersByStatus.map((s) => s.count),
          backgroundColor: d.ordersByStatus.map((s) => CHART_COLORS[s.status] ?? 'rgba(156,163,175,0.7)'),
          borderRadius: 6,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, ticks: { stepSize: 1 } },
          x: { ticks: { font: { size: 11 } } },
        },
      },
    });
  }
}
