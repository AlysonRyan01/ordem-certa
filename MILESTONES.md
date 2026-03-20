# OrdemCerta — Milestones

---

## Backend

---

### Milestone 1 — Fundação e Infraestrutura ✅

- [x] Configurar solução .NET com projetos por camada (Domain, Application, Infrastructure, Presentation, Shared, Tests)
- [x] Configurar DbContext com PostgreSQL (Npgsql + EF Core)
- [x] Implementar base classes: `Entity`, `AggregateRoot`, `ValueObject`, `Result<T>`, `GetPagedInput`
- [x] Configurar Unit of Work (`IUnitOfWork`, `UnitOfWork`)
- [x] Configurar MediatR
- [x] Configurar FluentValidation
- [x] Configurar Swagger
- [x] Configurar Docker Compose para desenvolvimento (PostgreSQL + API com hot-reload)
- [x] Configurar pipeline de migrations via `AppExtensions`

---

### Milestone 2 — Contexto de Clientes ✅

- [x] Domain: Aggregate `Customer` com Value Objects (`CustomerName`, `CustomerPhone`, `CustomerEmail`, `CustomerDocument`, `CustomerAddress`)
- [x] Domain: Enum `CustomerDocumentType` (CPF / CNPJ)
- [x] Domain: DTOs de saída e `CustomerExtensions` com `ToOutput()`
- [x] Application: `ICustomerService` e `CustomerService`
- [x] Application: Inputs e Validators (Create, Update, AddPhone, RemovePhone)
- [x] Infrastructure: `ICustomerRepository`, `CustomerRepository`, mapeamento EF e migration
- [x] Presentation: `CustomerController` (CRUD + telefones + busca por nome)

---

### Milestone 3 — Empresa ✅

- [x] Domain: Aggregate `Company` com dados da empresa (nome, CNPJ, telefone, endereço)
- [x] Domain: Enum `PlanType` (Demo / Paid)
- [x] Application: `ICompanyService` e `CompanyService` (CRUD)
- [x] Infrastructure: `ICompanyRepository`, `CompanyRepository`, mapeamento EF e migration
- [x] Presentation: `CompanyController`
- [x] `CompanyId` propagado para todas as entidades

---

### Milestone 4 — Usuário e Autenticação ✅

- [x] Domain: Aggregate `User` (nome, e-mail, senha hash, vínculo com empresa)
- [x] Application: `IAuthService` e `AuthService` (login, geração de token JWT com `CompanyId`)
- [x] Application: `IUserService` e `UserService` (perfil)
- [x] Infrastructure: `IUserRepository`, `UserRepository`, mapeamento EF e migration
- [x] Presentation: `AuthController` (login) e `UserController` (perfil)

---

### Milestone 5 — Ordens de Serviço ✅

- [x] Domain: Aggregate `ServiceOrder` com Value Object `EquipmentInfo`
- [x] Domain: Enum `ServiceOrderStatus` (10 status)
- [x] Domain: Enum `RepairResult` (CanBeRepaired, NoFix, NoDefectFound)
- [x] Domain: Value Object `Warranty` (duração + `WarrantyUnit`)
- [x] Domain: Value Object `Budget` (valor + descrição)
- [x] Domain: DTOs de saída e Extensions
- [x] Application: `IServiceOrderService` e `ServiceOrderService` (CRUD + status + orçamento + garantia)
- [x] Application: Inputs e Validators
- [x] Infrastructure: `IServiceOrderRepository`, `ServiceOrderRepository`, mapeamento EF e migration
- [x] Presentation: `ServiceOrderController` com filtros (status, cliente)
- [x] Validação: plano Demo → máximo 10 ordens de serviço

---

### Milestone 6 — Fluxo de Orçamento ✅

- [x] `CreateBudgetAsync` / `UpdateBudgetAsync` — cria/edita orçamento com resultado e garantia
- [x] `ApproveBudgetAsync` / `RefuseBudgetAsync` — aprovação/recusa pelo admin (sem WhatsApp automático)
- [x] `ApproveBudgetFromLinkAsync` / `RefuseBudgetFromLinkAsync` — aprovação/recusa pelo link público (WhatsApp automático)
- [x] Endpoint público `GET /public/orders/{id}/approve` e `/refuse`
- [x] Rate limiting na rota pública (`"public"`: 10 req/min por IP)

---

### Milestone 7 — Integração com WhatsApp ✅

- [x] Provider: Evolution API (`IWhatsAppService` + `WhatsAppService` via `HttpClient`)
- [x] Mensagens enviadas via Hangfire (background jobs com retry automático)
- [x] Templates: orçamento criado, aprovado, recusado, pronto para retirada
- [x] Envio para cliente + cópia para empresa em cada notificação
- [x] Configuração via `EvolutionApi:BaseUrl`, `EvolutionApi:ApiKey`, `EvolutionApi:Instance`
- [x] Dashboard Hangfire em `/hangfire`

---

### Milestone 8 — Qualidade e Produção ✅

- [x] Health check endpoint (`/health`)
- [x] Logging estruturado com Serilog (console + arquivo rotativo)
- [x] Rate limiting por `companyId` nos endpoints autenticados (60 req/min)
- [x] Rate limiting por IP nas rotas públicas (10 req/min)
- [x] Configuração de ambiente de produção via variáveis de ambiente

---

### Milestone 9 — Geração de PDF ✅

- [x] QuestPDF instalado no projeto Application
- [x] `IPdfService` e `PdfService` com três templates:
  - [x] **Comprovante de entrada** — número da ordem, data, cliente, equipamento, defeito, assinatura
  - [x] **Certificado de garantia** — serviço realizado, valor, prazo de garantia, termos, assinatura
  - [x] **Comprovante de devolução** — motivo (NoFix / NoDefectFound), sem cláusula de garantia, assinatura
- [x] `PrintAsync` no `ServiceOrderService` — decide o documento pelo status e `RepairResult`
- [x] `GET /api/service-orders/{id}/print` → retorna `application/pdf`

---

### Milestone 10 — Pagamento com Stripe ✅

- [x] Domain: `Company` com `StripeCustomerId` e `StripeSubscriptionId`
- [x] Domain: métodos `SetStripeCustomerId`, `ActivateSubscription`, `CancelSubscription`
- [x] Infrastructure: `GetByStripeCustomerIdAsync` no `ICompanyRepository`
- [x] Migration: `StripeBilling` (colunas `stripe_customer_id`, `stripe_subscription_id`)
- [x] Application: `IStripeService` e `StripeService` com Stripe.net:
  - [x] `CreateCheckoutSessionAsync` — cria/reutiliza Customer no Stripe, gera sessão de assinatura mensal
  - [x] `CreateCustomerPortalSessionAsync` — portal de gerenciamento (cancelar, atualizar cartão)
  - [x] `HandleWebhookAsync` — processa `checkout.session.completed`, `invoice.payment_succeeded`, `customer.subscription.deleted`
- [x] Presentation: `BillingController` (`POST /api/billing/checkout`, `POST /api/billing/portal`, `POST /api/webhooks/stripe`)
- [x] Webhook sem `[Authorize]` e fora do rate limiter

---

### Milestone 11 — Contexto de Vendas

- [ ] **Domain:** Enum `SaleStatus` (Completed, Cancelled, Returned)
- [ ] **Domain:** Enum `SalePaymentMethod` (Cash, Debit, Credit, Pix, Other)
- [ ] **Domain:** `SaleItem` — owned entity (ProductName, Quantity, UnitPrice, Subtotal calculado)
- [ ] **Domain:** `CompanySaleSequence` — numeração sequencial independente por empresa
- [ ] **Domain:** Aggregate `Sale` com métodos: `Create`, `AddItem`, `RemoveItem`, `UpdateItem`, `UpdateNotes`, `UpdatePaymentMethod`, `SetWarranty`, `ClearWarranty`, `Cancel`, `Return`
- [ ] **Domain:** `SaleOutput` + `SaleItemOutput` (records) e `SaleExtensions.ToOutput()`
- [ ] **Infrastructure:** `ISaleRepository` + `SaleRepository` com `GetByIdTrackedAsync` (necessário para `OwnsMany`)
- [ ] **Infrastructure:** `ICompanySaleSequenceRepository` + `CompanySaleSequenceRepository`
- [ ] **Infrastructure:** `SaleModel` — tabela `sales` + `OwnsOne(Warranty)` + `OwnsMany(Items → sale_items)`
- [ ] **Infrastructure:** `CompanySaleSequenceModel` — tabela `company_sale_sequences`
- [ ] **Application:** Inputs: `CreateSaleInput`, `UpdateSaleInput`, `AddSaleItemInput`, `UpdateSaleItemInput`
- [ ] **Application:** Validators para cada input (cross-field warranty: duração + unidade juntos ou nenhum)
- [ ] **Application:** `ISaleService` + `SaleService` (CRUD + itens + cancel + return)
- [ ] **Presentation:** `SaleController` com 12 endpoints (CRUD, itens, filtros, cancel, return)
- [ ] **Migration:** `SalesMap` gerando tabelas `sales`, `sale_items`, `company_sale_sequences`
- [ ] **PDF:** Templates de venda via QuestPDF:
  - [ ] **Comprovante de venda** — número, data, cliente (se houver), itens, total, forma de pagamento
  - [ ] **Certificado de garantia** — produto(s), valor, prazo de garantia, termos, assinatura
- [ ] **Application:** `PrintSaleAsync` no `SaleService` — decide o documento pelo contexto (com ou sem garantia)
- [ ] **Presentation:** `GET /api/sales/{id}/print` → retorna `application/pdf`

**Endpoints planejados:**

| Verbo | Rota | Ação |
|---|---|---|
| POST | `/api/sales` | Criar venda |
| GET | `/api/sales` | Listagem paginada |
| GET | `/api/sales/{id}` | Detalhe |
| GET | `/api/sales/by-status/{status}` | Filtrar por status |
| GET | `/api/sales/by-customer/{customerId}` | Filtrar por cliente |
| PUT | `/api/sales/{id}` | Atualizar (pagamento, notas, garantia) |
| DELETE | `/api/sales/{id}` | Excluir |
| POST | `/api/sales/{id}/items` | Adicionar item |
| PUT | `/api/sales/{id}/items/{itemId}` | Atualizar item |
| DELETE | `/api/sales/{id}/items/{itemId}` | Remover item |
| POST | `/api/sales/{id}/cancel` | Cancelar |
| POST | `/api/sales/{id}/return` | Registrar devolução |

**Decisões de design:**
- `Warranty` VO reutilizado de `ServiceOrders` (sem cópia)
- `TotalValue` armazenado e recalculado no domínio a cada mutação de item
- `CustomerId` nullable — venda de balcão sem cliente cadastrado é válida
- Mutations de item em endpoints dedicados (evita substituição acidental da lista no `PUT`)

---

## Frontend

---

### Milestone F11 — Vendas (Frontend)

- [ ] **Models:** `sale.model.ts` — interfaces `SaleOutput`, `SaleItemOutput`, `CreateSaleInput`, `UpdateSaleInput`; type aliases `SaleStatus`, `SalePaymentMethod`, `SaleWarrantyUnit`
- [ ] **Helpers:** `sale-status.helper.ts` — `SALE_STATUS_META`, `ALL_SALE_STATUSES`, `saleStatusMeta()`
- [ ] **Helpers:** `sale-payment-method.helper.ts` — `PAYMENT_METHOD_META`, `ALL_PAYMENT_METHODS`, `paymentMethodLabel()`
- [ ] **Shared:** `SaleStatusBadgeComponent` — badge dedicado para `SaleStatus` (separado do `StatusBadgeComponent` existente que é tipado para `ServiceOrderStatus`)
- [ ] **Service:** `sale.service.ts` — `getPaged`, `getByStatus`, `getByCustomer`, `getById`, `create`, `update`, `delete`, `print` (blob → nova aba)
- [ ] **Routes:** `sales.routes.ts` — `''` (lista), `new` (form), `:id` (detalhe), `:id/edit` (form)
- [ ] **app.routes.ts:** adicionar rota `sales` com `loadChildren` dentro do layout autenticado (após `customers`)
- [ ] **main-layout:** adicionar item "Vendas" com ícone `point_of_sale` no `navItems`
- [ ] **SaleListComponent** — tabela paginada, filtro por status, badge, colunas: número, status, pagamento, total, data, ações
- [ ] **SaleFormComponent** — criar/editar; `FormArray` de itens com add/remove; total calculado em tempo real via `computed()`; toggle de garantia; autocomplete de cliente
- [ ] **SaleDetailComponent** — detalhe completo; tabela de itens com rodapé de total; ações Cancelar/Devolver com `ConfirmDialog`; botão Reimprimir; botão Editar (só se `Completed`)

**Decisões de design:**
- `computedTotal` no formulário é apenas visual — o total canônico vem do backend
- Item rows: botão remover desabilitado (não oculto) quando há apenas 1 item
- Warranty toggle: ao desativar, reseta `warrantyDuration` e `warrantyUnit` para null
- Editar venda só disponível quando `status === 'Completed'`
- Cancel e Return sempre via `ConfirmDialogComponent`

**Arquivos novos (13):**
```
core/models/sale.model.ts
core/models/sale-status.helper.ts
core/models/sale-payment-method.helper.ts
core/services/sale.service.ts
shared/components/sale-status-badge/sale-status-badge.component.ts
features/sales/sales.routes.ts
features/sales/sale-list/sale-list.component.ts
features/sales/sale-list/sale-list.component.html
features/sales/sale-form/sale-form.component.ts
features/sales/sale-form/sale-form.component.html
features/sales/sale-detail/sale-detail.component.ts
features/sales/sale-detail/sale-detail.component.html
```

**Arquivos modificados (2):** `app.routes.ts`, `main-layout.component.ts`

---

### Milestone F1 — Fundação e Infraestrutura ✅

- [x] Projeto Angular 19 com standalone components e lazy loading
- [x] Angular Material 20 + TailwindCSS 4
- [x] `HttpClient` com interceptors (auth + error)
- [x] Tipagem completa dos DTOs da API
- [x] Estrutura de pastas: `core/`, `shared/`, `features/`, `layouts/`
- [x] `environment.ts` com `apiUrl`

---

### Milestone F2 — Autenticação ✅

- [x] Página de login (`/login`) com Reactive Form
- [x] Página de cadastro (`/register`)
- [x] `AuthService`: login, registro, logout, decode JWT
- [x] `AuthGuard`: redireciona para `/login` se sem token
- [x] `AuthInterceptor`: injeta `Authorization: Bearer` em toda requisição
- [x] `ErrorInterceptor`: captura erros HTTP e exibe `MatSnackBar`

---

### Milestone F3 — Layout e Navegação ✅

- [x] Layout autenticado com sidenav (Angular Material) e toolbar
- [x] Sidenav com itens: Dashboard, Ordens, Clientes, Perfil, Planos
- [x] Indicador visual do plano ativo (Demo com banner de upgrade / Pago com badge verde)
- [x] Breadcrumb dinâmico por rota
- [x] Layout responsivo (sidenav colapsável em mobile)
- [x] `SkeletonComponent` e `SkeletonTableComponent` reutilizáveis
- [x] Componente de paginação reutilizável
- [x] `StatusBadgeComponent` e `ConfirmDialogComponent`

---

### Milestone F4 — Clientes ✅

- [x] Listagem com paginação e busca por nome (debounce)
- [x] Formulário de criação e edição
- [x] Tela de detalhe com ordens vinculadas
- [x] Adicionar / remover telefones
- [x] Confirmação de exclusão com `MatDialog`
- [x] Máscaras de input (CPF, CNPJ, telefone) com ngx-mask

---

### Milestone F5 — Ordens de Serviço ✅

- [x] Listagem com paginação, badge de status e filtros (status, cliente)
- [x] Formulário de criação e edição
- [x] Tela de detalhe completa (equipamento, status, orçamento com resultado e garantia)
- [x] Seletor de status com confirmação de WhatsApp quando aplicável
- [x] Abertura automática do PDF de comprovante ao criar uma ordem
- [x] Abertura automática do PDF ao marcar como `Delivered`
- [x] Botão "Reimprimir" na tela de detalhe

---

### Milestone F6 — Fluxo de Orçamento ✅

- [x] Formulário unificado de orçamento (valor + descrição + resultado + garantia)
- [x] Edição de orçamento existente
- [x] Botões de aprovar/recusar com modal de confirmação de WhatsApp
- [x] Página pública `/orcamento/order/:id` — visualização e resposta do cliente
- [x] Estados: carregando, visualizando, confirmando, sucesso, já respondido, erro

---

### Milestone F7 — Dashboard ✅

- [x] Cards de resumo: ordens abertas, prontas para retirada, aguardando aprovação
- [x] Lista das últimas ordens com link para detalhe
- [x] Indicador de plano + limite de uso (Demo: X/10 ordens)

---

### Milestone F8 — Perfil e Empresa ✅

- [x] Tela de perfil do usuário (nome, e-mail, alterar senha)
- [x] Tela de dados da empresa (nome, CNPJ, telefone, endereço)

---

### Milestone F9 — Qualidade e Produção ✅

- [x] Tratamento global de erros da API (`ErrorInterceptor` → `MatSnackBar`)
- [x] Página 404 personalizada
- [x] Títulos de página dinâmicos via `title` nas rotas
- [x] Configuração de ambiente de produção (`environment.prod.ts`)
- [x] Docker Compose atualizado para incluir o frontend (Nginx)

---

### Milestone F10 — Pagamento com Stripe ✅

- [x] `BillingService` com `createCheckoutSession()` e `createPortalSession()`
- [x] Página `/billing` com tabela comparativa de planos
- [x] Botão "Fazer upgrade" → redireciona para Stripe Checkout
- [x] Botão "Gerenciar assinatura" (plano Pago) → redireciona para Customer Portal
- [x] Página `/billing/success` — confirmação pós-pagamento
- [x] Página `/billing/cancel` — retorno de checkout abandonado
- [x] Banner de upgrade no sidenav (plano Demo) com link para `/billing`
