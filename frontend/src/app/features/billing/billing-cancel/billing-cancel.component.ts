import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-billing-cancel',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-50">
      <div class="text-center max-w-md px-6">
        <div class="w-20 h-20 rounded-full bg-slate-100 flex items-center justify-center mx-auto mb-6">
          <mat-icon class="!text-4xl !w-10 !h-10 text-slate-400">cancel</mat-icon>
        </div>
        <h1 class="text-2xl font-bold text-slate-900 mb-2">Checkout cancelado</h1>
        <p class="text-slate-500 mb-8">Nenhuma cobrança foi realizada. Você pode fazer o upgrade quando quiser.</p>
        <a mat-flat-button routerLink="/billing">
          Ver planos
        </a>
      </div>
    </div>
  `,
})
export class BillingCancelComponent {}
