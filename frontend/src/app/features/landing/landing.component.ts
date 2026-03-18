import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }
  readonly steps = [
    { n: 1, title: 'Crie a ordem', desc: 'Registre o equipamento, o defeito relatado e as peças necessárias.' },
    { n: 2, title: 'Envie o orçamento', desc: 'Com um clique, o cliente recebe o valor e o link de aprovação via WhatsApp.' },
    { n: 3, title: 'Cliente aprova', desc: 'Ele acessa a página, vê os detalhes e aprova (ou recusa) com um toque.' },
    { n: 4, title: 'Você é notificado', desc: 'O sistema atualiza o status e te avisa instantaneamente no WhatsApp.' },
  ];

  readonly features = [
    { icon: 'bar_chart',      title: 'Dashboard em tempo real',  desc: 'Visão geral de todas as ordens por status. Saiba exatamente o que precisa de atenção.', green: false },
    { icon: 'picture_as_pdf', title: 'PDF profissional',          desc: 'Gere laudos e ordens de serviço em PDF com a identidade da sua empresa.', green: false },
    { icon: 'chat',           title: 'Notificações via WhatsApp', desc: 'Avisos automáticos para o cliente em cada etapa: orçamento, aprovação, pronto para retirada.', green: true },
    { icon: 'people',         title: 'Gestão de clientes',        desc: 'Histórico completo de cada cliente: equipamentos, ordens abertas e finalizadas.', green: false },
    { icon: 'manage_search',  title: 'Rastreamento completo',     desc: 'Cada OS tem número único, histórico de status e todos os dados do equipamento.', green: false },
    { icon: 'business',       title: 'Multi-empresa',             desc: 'Cada empresa tem seus dados completamente isolados. Seguro e privado.', green: false },
  ];
}
