import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <div class="min-h-screen bg-slate-50 flex flex-col items-center justify-center gap-5 p-6">
      <p class="text-9xl font-bold text-slate-200 font-mono leading-none select-none">404</p>
      <div class="text-center">
        <h1 class="text-2xl font-bold text-slate-700">Página não encontrada</h1>
        <p class="text-slate-400 text-sm mt-1">A página que você procura não existe ou foi movida.</p>
      </div>
      <a mat-flat-button routerLink="/">Voltar ao início</a>
    </div>
  `,
})
export class NotFoundComponent {}
