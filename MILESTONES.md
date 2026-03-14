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

- [ ] Domain: Aggregate `User` (nome, e-mail, senha hash, vínculo com empresa — um usuário por empresa)
- [ ] Application: `IUserService` e `UserService` (registro, atualização de perfil)
- [ ] Application: `IAuthService` e `AuthService` (login, geração de token JWT com `CompanyId`)
- [ ] Infrastructure: `IUserRepository`, `UserRepository`, mapeamento EF e migration
- [ ] Presentation: `AuthController` (login) e `UserController` (perfil)
- [ ] Services usam o `CompanyId` do token diretamente nas queries — sem middleware

---

## Milestone 5 — Ordens de Serviço

- [ ] Domain: Aggregate `ServiceOrder` com informações do equipamento (tipo, marca, modelo, defeito, acessórios, observações)
- [ ] Domain: Enum `ServiceOrderStatus` (Recebido, Em análise, Orçamento pendente, Aguardando aprovação, Orçamento aprovado, Orçamento recusado, Em conserto, Pronto para retirada, Entregue, Cancelado)
- [ ] Domain: Value Objects necessários (ex.: `EquipmentType`, `Budget`)
- [ ] Domain: DTOs de saída e Extensions
- [ ] Application: `IServiceOrderService` e `ServiceOrderService` (CRUD + mudança de status)
- [ ] Application: Inputs e Validators
- [ ] Infrastructure: `IServiceOrderRepository`, `ServiceOrderRepository`, mapeamento EF e migration
- [ ] Presentation: `ServiceOrderController` com filtros (status, cliente, data)
- [ ] Validação: plano Demo → máximo 10 ordens de serviço

---

## Milestone 6 — Fluxo de Orçamento

- [ ] Domain: Value Object `Budget` (valor, descrição)
- [ ] Application: método `CreateBudgetAsync` no `ServiceOrderService` — muda status para "Aguardando aprovação"
- [ ] Application: método `RespondBudgetAsync` — muda status para "Aprovado" ou "Recusado"
- [ ] Geração de token único por ordem para os links de resposta
- [ ] Endpoint público `GET /public/orders/{token}/approve` — aprova direto, sem página
- [ ] Endpoint público `GET /public/orders/{token}/refuse` — recusa direto, sem página

---

## Milestone 7 — Integração com WhatsApp

- [ ] Definir provider de WhatsApp (ex.: Evolution API, Twilio, Z-API)
- [ ] Serviço de envio de mensagem (`IWhatsAppService`)
- [ ] Domain Event `BudgetCreatedEvent` — dispara envio automático ao cliente
- [ ] Domain Event `BudgetRespondedEvent` — dispara notificação para a empresa
- [ ] Template de mensagem: identificação da empresa, resumo da ordem, valor, link público
- [ ] Configuração por empresa (número do WhatsApp, credenciais)

---

## Milestone 8 — Qualidade e Produção

- [ ] Testes unitários para Value Objects e Aggregates
- [ ] Testes de integração para Services e Repositories
- [ ] Configuração de ambiente de produção (variáveis de ambiente, connection string segura)
- [ ] Health check endpoint
- [ ] Logging estruturado (Serilog ou similar)
- [ ] Rate limiting nas rotas públicas
- [ ] Documentação Swagger completa com exemplos
