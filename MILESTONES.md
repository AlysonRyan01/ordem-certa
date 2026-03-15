# OrdemCerta — Milestones

---

## Milestone 1 — Fundação e Infraestrutura

- [x] Configurar solução .NET com projetos por camada (Domain, Application, Infrastructure, Presentation, Shared, Tests)
- [x] Configurar DbContext com PostgreSQL (Npgsql + EF Core)
- [x] Implementar base classes: `Entity`, `AggregateRoot`, `ValueObject`, `Result<T>`, `GetPagedInput`
- [x] Configurar Unit of Work (`IUnitOfWork`, `UnitOfWork`)
- [x] Configurar MediatR para publicação de Domain Events
- [x] Configurar FluentValidation
- [x] Configurar Swagger
- [x] Configurar Docker Compose para desenvolvimento (PostgreSQL + API com hot-reload)
- [x] Configurar pipeline de migrations via `AppExtensions`

---

## Milestone 2 — Contexto de Clientes

- [x] Domain: Aggregate `Customer` com Value Objects (`CustomerName`, `CustomerPhone`, `CustomerEmail`, `CustomerDocument`, `CustomerAddress`)
- [x] Domain: Enum `CustomerDocumentType` (CPF / CNPJ)
- [x] Domain: DTOs de saída (`CustomerOutput`, `CustomerPhoneOutput`, `CustomerAddressOutput`, `CustomerDocumentOutput`)
- [x] Domain: `CustomerExtensions` com `ToOutput()`
- [x] Application: `ICustomerService` e `CustomerService`
- [x] Application: Inputs (`CreateCustomerInput`, `UpdateCustomerInput`, `AddPhoneInput`, `RemovePhoneInput`)
- [x] Application: Validators (`CreateCustomerInputValidator`, `UpdateCustomerInputValidator`, `AddPhoneInputValidator`, `RemovePhoneInputValidator`)
- [x] Infrastructure: `ICustomerRepository` e `CustomerRepository`
- [x] Infrastructure: Mapeamento EF (`CustomerModel`) com tabelas `customers` e `customer_phones`
- [x] Infrastructure: Migration `CustomerMap`
- [x] Presentation: `CustomerController` (CRUD + gerenciamento de telefones + busca por nome)

---

## Milestone 3 — Empresa

- [x] Domain: Aggregate `Company` com dados da empresa (nome, CNPJ, telefone de contato, endereço)
- [x] Domain: Enum `PlanType` (Demo / Pago) na `Company`
- [x] Application: `ICompanyService` e `CompanyService` (CRUD)
- [x] Infrastructure: `ICompanyRepository`, `CompanyRepository`, mapeamento EF e migration
- [x] Presentation: `CompanyController`
- [x] Adicionar `CompanyId` em `Customer` e nas futuras entidades

---

## Milestone 4 — Usuário e Autenticação

- [x] Domain: Aggregate `User` (nome, e-mail, senha hash, vínculo com empresa — um usuário por empresa)
- [x] Application: `IUserService` e `UserService` (registro, atualização de perfil)
- [x] Application: `IAuthService` e `AuthService` (login, geração de token JWT com `CompanyId`)
- [x] Infrastructure: `IUserRepository`, `UserRepository`, mapeamento EF e migration
- [x] Presentation: `AuthController` (login) e `UserController` (perfil)
- [x] Services usam o `CompanyId` do token diretamente nas queries — sem middleware

---

## Milestone 5 — Ordens de Serviço

- [x] Domain: Aggregate `ServiceOrder` com informações do equipamento (tipo, marca, modelo, defeito, acessórios, observações)
- [x] Domain: Enum `ServiceOrderStatus` (Recebido, Em análise, Orçamento pendente, Aguardando aprovação, Orçamento aprovado, Orçamento recusado, Em conserto, Pronto para retirada, Entregue, Cancelado)
- [x] Domain: Value Object `EquipmentInfo` (agrupa os dados do equipamento)
- [x] Domain: DTOs de saída e Extensions
- [x] Application: `IServiceOrderService` e `ServiceOrderService` (CRUD + mudança de status)
- [x] Application: Inputs e Validators
- [x] Infrastructure: `IServiceOrderRepository`, `ServiceOrderRepository`, mapeamento EF e migration
- [x] Presentation: `ServiceOrderController` com filtros (status, cliente)
- [x] Validação: plano Demo → máximo 10 ordens de serviço

---

## Milestone 6 — Fluxo de Orçamento

- [x] Domain: Value Object `Budget` (valor, descrição)
- [x] Application: método `CreateBudgetAsync` no `ServiceOrderService` — muda status para "Aguardando aprovação"
- [x] Application: métodos `ApproveBudgetAsync` e `RefuseBudgetAsync` — muda status para "Aprovado" ou "Recusado"
- [x] Geração de token único por ordem para os links de resposta
- [x] Endpoint público `GET /public/orders/{token}/approve` — aprova direto, sem página
- [x] Endpoint público `GET /public/orders/{token}/refuse` — recusa direto, sem página

---

## Milestone 7 — Integração com WhatsApp

- [x] Provider: Evolution API
- [x] Serviço de envio de mensagem (`IWhatsAppService` + `WhatsAppService`)
- [x] Domain Event `BudgetCreatedEvent` — dispara envio automático ao cliente
- [x] Domain Event `BudgetRespondedEvent` — dispara notificação para a empresa
- [x] Template de mensagem: identificação da empresa, resumo da ordem, valor, links de aprovação/recusa
- [x] Configuração global via `EvolutionApi:BaseUrl`, `EvolutionApi:ApiKey`, `EvolutionApi:Instance`

---

## Milestone 8 — Qualidade e Produção

- [x] Testes unitários para Value Objects e Aggregates
- [x] Configuração de ambiente de produção (variáveis de ambiente, connection string segura)
- [x] Health check endpoint
- [x] Logging estruturado (Serilog ou similar)
- [x] Rate limiting nas rotas públicas

---

## Milestone F1 — Frontend: Fundação e Infraestrutura

Stack: **Angular 19**, **TypeScript**, **Angular Material**, **TailwindCSS**, **RxJS**, **Reactive Forms + Zod**

- [x] Criar projeto Angular com standalone components e lazy loading por rota
- [x] Configurar Angular Material + Tailwind CSS
- [x] Configurar `HttpClient` com interceptor para injetar Bearer token e tratar 401 (redirect para login)
- [x] Tipar todos os DTOs de saída da API (`ServiceOrderOutput`, `CustomerOutput`, etc.)
- [x] Estrutura de pastas: `core/`, `shared/`, `features/`, `layouts/`
- [x] Configurar variável de ambiente `apiUrl` via `environment.ts`
- [x] Configurar ESLint + Prettier

---

## Milestone F2 — Frontend: Autenticação

- [x] Página de login (`/login`) com Reactive Form (e-mail + senha) e validação
- [x] `AuthService`: `POST /api/auth/login` → armazenar token JWT em `localStorage`
- [x] `AuthGuard`: redirecionar rotas protegidas para `/login` se sem token
- [x] `AuthInterceptor`: anexar `Authorization: Bearer {token}` em toda requisição autenticada
- [x] Hook de logout (limpa storage + redireciona para `/login`)
- [x] Exibir nome do usuário e empresa no header, decodificados do JWT

---

## Milestone F3 — Frontend: Layout e Navegação

- [x] Layout autenticado com sidenav (Angular Material) e toolbar
- [x] Sidenav com itens: Dashboard, Ordens de Serviço, Clientes, Perfil
- [x] Indicador visual do plano ativo (Demo / Pago) no sidenav
- [x] Breadcrumb dinâmico por rota
- [x] Layout responsivo (sidenav colapsável em mobile)
- [x] Componente de loading skeleton reutilizável
- [x] Componente de paginação reutilizável integrado ao `MatPaginator`

---

## Milestone F4 — Frontend: Clientes

- [x] Listagem de clientes com `MatTable` + paginação (`GET /api/customers`)
- [x] Busca de clientes por nome com debounce (`GET /api/customers/search`)
- [x] Formulário de criação de cliente (nome, CPF/CNPJ, e-mail, endereço, telefone)
- [x] Formulário de edição de cliente
- [x] Tela de detalhe do cliente com lista de ordens de serviço vinculadas
- [x] Adicionar / remover telefones do cliente
- [x] Confirmação de exclusão com `MatDialog`
- [x] Máscara de input para CPF, CNPJ e telefone (ngx-mask)

---

## Milestone F5 — Frontend: Ordens de Serviço

- [x] Listagem de ordens com `MatTable` + paginação e badge de status colorido
- [x] Filtro por status com `MatSelect` (`GET /api/serviceorders/by-status/{status}`)
- [x] Filtro por cliente com autocomplete (`GET /api/serviceorders/by-customer/{customerId}`)
- [x] Formulário de criação de ordem (equipamento: tipo, marca, modelo, defeito, acessórios, observações; cliente; técnico)
- [x] Tela de detalhe da ordem com todas as informações
- [x] Formulário de edição da ordem (equipamento + técnico)
- [x] Seletor de status com `MatSelect` (`PATCH /api/serviceorders/{id}/status`)
- [x] Exibir número da ordem (`#OrderNumber`) em destaque em toda a interface
- [x] Aviso visual (`MatSnackBar`) quando plano Demo atingir o limite de 10 ordens

---

## Milestone F6 — Frontend: Fluxo de Orçamento

- [x] Formulário de criação de orçamento na tela da ordem (valor + descrição)
- [x] Indicador visual de status "Aguardando aprovação" com botões de aprovar/recusar pelo operador
- [x] Exibir valor e descrição do orçamento na tela de detalhe
- [x] Feedback visual após aprovação ou recusa (`MatSnackBar` + atualização de status)
- [x] Páginas públicas de resposta do cliente (rota sem AuthGuard):
  - [x] `/public/orders/:id/approve` → página de confirmação de aprovação
  - [x] `/public/orders/:id/refuse` → página de confirmação de recusa
  - [x] Layout minimalista sem sidenav, exibindo nome da empresa e detalhes do equipamento

---

## Milestone F7 — Frontend: Dashboard

- [x] Cards de resumo: total de ordens abertas, prontas para retirada, aguardando aprovação
- [x] Lista das últimas 5 ordens criadas com link para detalhe
- [x] Gráfico de barras: ordens por status no mês atual (Chart.js)
- [x] Indicador de plano + limite de uso (Demo: X/10 ordens)

---

## Milestone F8 — Frontend: Perfil e Empresa

- [x] Tela de perfil do usuário (nome, e-mail, alterar senha)
- [x] Tela de dados da empresa (nome, CNPJ, telefone, endereço)

---

## Milestone F9 — Frontend: Qualidade e Produção

- [x] Testes unitários de componentes e services críticos com Jasmine/Karma (AuthService, ServiceOrderService, DashboardService)
- [x] Tratamento global de erros da API (`ErrorInterceptor` → `MatSnackBar` com `errors[]` do backend)
- [x] Página 404 personalizada
- [x] Títulos de página dinâmicos via `title` nas rotas (Angular Router nativo)
- [x] Configuração de ambiente de produção (`environment.prod.ts`)
- [x] Build de produção validado (`ng build --configuration production`)
- [x] Docker Compose atualizado para incluir o frontend (imagem Angular com Nginx)