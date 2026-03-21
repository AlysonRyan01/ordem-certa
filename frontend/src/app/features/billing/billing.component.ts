import { Component, OnInit, inject, signal } from '@angular/core';
import { BillingService } from '../../core/services/billing.service';
import { CompanyService } from '../../core/services/company.service';

@Component({
  selector: 'app-billing',
  standalone: true,
  imports: [],
  templateUrl: './billing.component.html',
  styleUrl: './billing.component.scss',
})
export class BillingComponent implements OnInit {
  private readonly billingService = inject(BillingService);
  private readonly companyService = inject(CompanyService);

  readonly plan = signal<string | null>(null);
  readonly loadingCheckout = signal(false);
  readonly loadingPortal = signal(false);

  get isPaid(): boolean {
    return this.plan() === 'Paid';
  }

  ngOnInit(): void {
    this.companyService.getMe().subscribe({
      next: (company) => this.plan.set(company.plan),
    });
  }

  upgrade(): void {
    if (this.loadingCheckout()) return;
    this.loadingCheckout.set(true);
    this.billingService.createCheckoutSession().subscribe({
      next: ({ url }) => window.location.href = url,
      error: () => this.loadingCheckout.set(false),
    });
  }

  manageSubscription(): void {
    if (this.loadingPortal()) return;
    this.loadingPortal.set(true);
    this.billingService.createPortalSession().subscribe({
      next: ({ url }) => window.location.href = url,
      error: () => this.loadingPortal.set(false),
    });
  }
}
