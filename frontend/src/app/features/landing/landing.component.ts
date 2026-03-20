import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { Meta, Title } from '@angular/platform-browser';
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
export class LandingComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly meta = inject(Meta);
  private readonly titleService = inject(Title);
  private readonly document = inject(DOCUMENT);
  private jsonLdScript: HTMLScriptElement | null = null;

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.titleService.setTitle('OrdemCerta — Gestão de Ordens de Serviço para Assistências Técnicas');
    this.meta.updateTag({ name: 'description', content: 'OrdemCerta é o sistema SaaS para assistências técnicas de eletrônicos. Gerencie ordens de serviço, envie orçamentos pelo WhatsApp e tenha aprovação em segundos. Comece grátis.' });
    this.meta.updateTag({ property: 'og:title', content: 'OrdemCerta — Gestão de Ordens de Serviço via WhatsApp' });
    this.meta.updateTag({ property: 'og:description', content: 'Gerencie ordens de serviço, envie orçamentos pelo WhatsApp e tenha aprovação em segundos. O sistema para assistências técnicas que trabalha com você.' });
    this.meta.updateTag({ property: 'og:url', content: 'https://ordemcerta.app/' });

    this.addJsonLd();
  }

  ngOnDestroy(): void {
    this.meta.removeTag('property="og:url"');
    this.jsonLdScript?.remove();
  }

  private addJsonLd(): void {
    const schema = {
      '@context': 'https://schema.org',
      '@type': 'SoftwareApplication',
      name: 'OrdemCerta',
      description: 'Sistema SaaS de gestão de ordens de serviço para assistências técnicas de eletrônicos. Envio de orçamentos via WhatsApp com aprovação em um clique.',
      applicationCategory: 'BusinessApplication',
      operatingSystem: 'Web',
      url: 'https://ordemcerta.app',
      offers: [
        { '@type': 'Offer', name: 'Plano Demo', price: '0', priceCurrency: 'BRL' },
        { '@type': 'Offer', name: 'Plano Pago', price: '49', priceCurrency: 'BRL', billingIncrement: 'P1M' },
      ],
      publisher: {
        '@type': 'Organization',
        name: 'OrdemCerta',
        url: 'https://ordemcerta.app',
        logo: 'https://ordemcerta.app/ordemcerta-favicon-512.png',
      },
    };

    this.jsonLdScript = this.document.createElement('script');
    this.jsonLdScript.type = 'application/ld+json';
    this.jsonLdScript.text = JSON.stringify(schema);
    this.document.head.appendChild(this.jsonLdScript);
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
