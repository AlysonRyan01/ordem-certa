import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <div class="min-h-screen flex flex-col items-center justify-center gap-4">
      <h1 class="text-6xl font-bold text-gray-300">404</h1>
      <p class="text-xl text-gray-600">Página não encontrada</p>
      <a mat-flat-button routerLink="/">Voltar ao início</a>
    </div>
  `,
})
export class NotFoundComponent {}
