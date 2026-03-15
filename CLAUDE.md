# OrdemCerta — Claude Agent Instructions

Sistema SaaS de gerenciamento de ordens de serviço para assistências técnicas de eletrônicos. Multi-tenant, com dois planos: Demo gratuito (limitado) e Pago (completo).

---

## Stack e Dependências

- **Framework:** ASP.NET Core 9.0
- **Banco de dados:** PostgreSQL 16 (Alpine via Docker)
- **ORM:** Entity Framework Core 9.0.4 (Npgsql)
- **Validação:** FluentValidation 12.1.1
- **Mediator:** MediatR 14.0.0
- **Documentação:** Swashbuckle (Swagger) 10.1.2
- **Linguagem de erros/mensagens:** Português pt-BR

---

## Estrutura de Pastas

```
src/
├── OrdemCerta.Domain/            # Entidades, Value Objects, DTOs, Extensions
├── OrdemCerta.Application/       # Services, Inputs (DTOs de entrada), Validações
├── OrdemCerta.Infrastructure/    # Repositórios, DbContext, Mapeamentos EF, UoW
├── OrdemCerta.Presentation/      # Controllers, Extensions (DI), Program.cs
├── OrdemCerta.Shared/            # Entity, AggregateRoot, ValueObject, Result<T>, GetPagedInput
└── OrdemCerta.Tests/             # Testes unitários e de integração
```

**Fluxo de dependências:** Presentation → Application → Infrastructure → Domain → Shared

---

## Arquitetura e Padrões

### DDD (Domain-Driven Design)
- Cada contexto tem seu próprio **Aggregate Root** (ex.: `Customer`)
- **Value Objects** encapsulam lógica de validação e formatação dentro do domínio
- **Domain Events** via MediatR (publicados após `SaveChangesAsync`)

### Result Pattern (Railway-Oriented)
Nunca use exceções para erros de negócio. Use `Result<T>` e `Result`:

```csharp
// Sucesso
return Result<CustomerOutput>.Success(customer.ToOutput());

// Falha
return Result<CustomerOutput>.Failure("Cliente não encontrado.");

// Operadores implícitos — podem ser usados diretamente:
return "Erro de validação.";   // implicitly Result.Failure
return customerOutput;         // implicitly Result<CustomerOutput>.Success
```

### Repository + Unit of Work
- Repositórios só **enfileiram** operações (AddAsync, UpdateAsync, DeleteAsync)
- `UnitOfWork.CommitAsync()` persiste tudo — sempre chame após mutations
- Use `AsNoTracking()` em queries de leitura

### Owned Entities (EF Core)
- Value Objects single-valued: `OwnsOne()`
- Value Objects coleção: `OwnsMany()`
- `ValueGeneratedNever()` para IDs do Aggregate Root (Guid.NewGuid() no domínio)

---

## Como Criar um Novo Contexto (Passo a Passo)

Siga o padrão do contexto `Customers` **exatamente** ao criar um novo aggregate. Substitua `Customer`/`Customers` pelo nome do novo contexto.

### 1. Domain Layer — `OrdemCerta.Domain/{Contexts}/`

**Aggregate Root** (`{Context}.cs`):
```csharp
namespace OrdemCerta.Domain.{Contexts};

public class {Context} : AggregateRoot
{
    // Propriedades como Value Objects
    public {ContextName} Name { get; private set; } = null!;

    // Coleções imutáveis expostas via IReadOnlyCollection
    private readonly List<{ContextItem}> _items = [];
    public IReadOnlyCollection<{ContextItem}> Items => _items.AsReadOnly();

    protected {Context}() { } // EF Core

    // Factory method estático com Result
    public static Result<{Context}> Create(...) { ... }

    // Métodos de mutação com Result
    public Result AddItem(...) { ... }
    public Result UpdateName(...) { ... }
}
```

**Value Objects** em `ValueObjects/`:
- Herdam de `ValueObject`
- Factory `Create(...)` retorna `Result<T>`
- `GetEqualityComponents()` define igualdade por valor
- Mensagens de erro em português

**Enums** em `Enums/`:
- Simples, sem dependências externas

**DTOs de saída** em `DTOs/`:
- Records imutáveis: `{Context}Output`, `{Context}{Item}Output`

**Extensions** em `Extensions/`:
- `{Context}Extensions.cs` com `ToOutput()` (single e IEnumerable)

---

### 2. Application Layer — `OrdemCerta.Application/`

**Interface do Service** em `Services/{Context}Service/I{Context}Service.cs`:
```csharp
public interface I{Context}Service
{
    Task<Result<{Context}Output>> CreateAsync({Create}Input input, CancellationToken cancellationToken);
    Task<Result<{Context}Output>> UpdateAsync(Guid id, {Update}Input input, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<{Context}Output>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<List<{Context}Output>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<List<{Context}Output>>> GetByNameAsync(string searchTerm, GetPagedInput input, CancellationToken cancellationToken);
}
```

**Implementação** em `Services/{Context}Service/{Context}Service.cs`:
- Injeta: `I{Context}Repository`, `IUnitOfWork`, validators (um por Input)
- Valida com FluentValidation antes de qualquer operação
- Cria/atualiza Value Objects verificando falha em cada passo
- Sempre chama `CommitAsync()` após mutations
- Retorna DTOs de saída via `.ToOutput()`

**Inputs** em `Inputs/{Context}Inputs/`:
- `Create{Context}Input` — campos obrigatórios e opcionais para criação
- `Update{Context}Input` — campos atualizáveis
- Use `record` para imutabilidade

**Validações** em `Validations/{Context}Validations/`:
- Um arquivo por Input: `Create{Context}InputValidator`, etc.
- Herdam de `AbstractValidator<T>`
- Mensagens em português

---

### 3. Infrastructure Layer — `OrdemCerta.Infrastructure/`

**Interface do Repositório** em `Repositories/{Context}Repository/I{Context}Repository.cs`:
```csharp
public interface I{Context}Repository
{
    Task<{Context}?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<{Context}>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<{Context}>> GetByNameAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync({Context} entity, CancellationToken cancellationToken);
    Task UpdateAsync({Context} entity, CancellationToken cancellationToken);
    Task DeleteAsync({Context} entity, CancellationToken cancellationToken);
}
```

**Implementação** em `Repositories/{Context}Repository/{Context}Repository.cs`:
- Injeta `ApplicationDataContext`
- Queries: `AsNoTracking()` + `Include()` onde necessário
- `GetPagedAsync`: validar `page > 0` e `1 ≤ pageSize ≤ 100`
- `GetByNameAsync`: usar `EF.Functions.Like()`

**Mapeamento EF** em `DataContext/Models/{Context}Model.cs`:
- Implementa `IEntityTypeConfiguration<{Context}>`
- Nome da tabela: `snake_case` no plural (ex.: `service_orders`)
- Colunas: `snake_case` (ex.: `full_name`, `created_at`)
- `HasKey(e => e.Id)` + `ValueGeneratedNever()`
- Value Objects: `OwnsOne()` ou `OwnsMany()`
- `Ignore(e => e.DomainEvents)` no Aggregate Root

**Registro de DI** em `Presentation/Extensions/BuilderExtensions.cs`:
- Adicione `services.AddScoped<I{Context}Repository, {Context}Repository>()`
- Adicione `services.AddScoped<I{Context}Service, {Context}Service>()`
- O `DbSet<{Context}>` vai em `ApplicationDataContext`

---

### 4. Presentation Layer — `OrdemCerta.Presentation/`

**Controller** em `Controllers/{Context}Controller.cs`:
```csharp
[ApiController]
[Route("api/[controller]")]
public class {Context}Controller : ControllerBase
{
    // POST   /api/{contexts}           → 201 Created + output
    // PUT    /api/{contexts}/{id}      → 200 OK + output
    // DELETE /api/{contexts}/{id}      → 204 No Content
    // GET    /api/{contexts}/{id}      → 200 OK + output
    // GET    /api/{contexts}           → 200 OK + lista paginada
    // GET    /api/{contexts}/search    → 200 OK + lista filtrada
}
```

**Padrão de resposta de erro:**
```json
{ "errors": ["Mensagem de erro em português"] }
```

**Mapeamento de Result para HTTP:**
- `Result.IsSuccess` → status code de sucesso (200/201/204)
- `Result.IsFailure` → `BadRequest(new { errors = result.Errors })`

---

### 5. Migration

Após criar o mapeamento EF, gere a migration:
```bash
dotnet ef migrations add {ContextName}Map --project src/OrdemCerta.Infrastructure --startup-project src/OrdemCerta.Presentation
```

---

## Convenções de Nomenclatura

| Elemento | Convenção | Exemplo |
|---|---|---|
| Classes, Métodos, Propriedades | PascalCase | `CustomerService`, `GetByIdAsync` |
| Métodos assíncronos | sufixo `Async` | `CreateAsync`, `CommitAsync` |
| Tabelas DB | snake_case plural | `customers`, `service_orders` |
| Colunas DB | snake_case | `full_name`, `area_code` |
| Input DTOs | sufixo `Input` | `CreateCustomerInput` |
| Output DTOs | sufixo `Output` | `CustomerOutput` |
| Interfaces | prefixo `I` | `ICustomerService`, `ICustomerRepository` |
| Validators | sufixo `Validator` | `CreateCustomerInputValidator` |
| Namespaces | `OrdemCerta.{Layer}.{Context}` | `OrdemCerta.Domain.Customers.ValueObjects` |

---

## Regras Gerais

1. **Erros de negócio em português** — sempre, sem exceção
2. **Nunca lançar exceções para regras de negócio** — use `Result`
3. **Value Objects são imutáveis** — sempre `private set`
4. **CancellationToken obrigatório** em todos os métodos assíncronos
5. **AsNoTracking** em todas as queries de leitura
6. **Commit explícito** via UnitOfWork após toda mutation
7. **Factory methods estáticos** (`Create(...)`) nos aggregates e Value Objects
8. **Construtores protegidos** (`protected`) para o EF Core
9. **Não adicionar** error handling, validações ou abstrações desnecessárias
10. **Não criar** arquivos de documentação ou README salvo quando solicitado

---

## Contextos Existentes

- **Customers** — Aggregate completo (referência de implementação)
  - Customer, CustomerName, CustomerPhone, CustomerEmail, CustomerDocument, CustomerAddress
  - CRUD completo + gerenciamento de telefones + busca por nome

## Próximos Contextos (conforme SPEC.md)

Ao implementar novos contextos, consulte o `SPEC.md` para requisitos de negócio e siga este guia para estrutura técnica.

---

---

# Frontend — Angular

---

## Stack e Dependências (Frontend)

- **Framework:** Angular 19+ (standalone components, signals API)
- **UI:** Angular Material 20 + TailwindCSS 4
- **HTTP:** `HttpClient` com interceptors funcionais
- **Formulários:** Reactive Forms (`FormControl`, `FormGroup`, `FormBuilder`)
- **Máscaras:** ngx-mask (telefone, CPF/CNPJ)
- **Reatividade:** RxJS 7 + Signals nativos do Angular
- **Idioma:** Português pt-BR em todos os textos, labels, erros e placeholders

---

## Estrutura de Pastas (Frontend)

```
frontend/src/app/
├── core/
│   ├── guards/          # auth.guard.ts — CanActivateFn funcional
│   ├── interceptors/    # auth.interceptor.ts, error.interceptor.ts
│   ├── models/          # Interfaces TypeScript por domínio
│   └── services/        # Serviços HTTP injetáveis (providedIn: 'root')
├── features/
│   ├── auth/login/
│   ├── customers/       # customer-list, customer-detail, customer-form
│   ├── dashboard/
│   ├── orders/          # order-list, order-detail, order-form
│   ├── profile/
│   └── public/          # budget-response (sem AuthGuard)
├── layouts/
│   └── main-layout/     # Sidenav + toolbar + breadcrumb
└── shared/
    └── components/      # Componentes reutilizáveis
        ├── breadcrumb/
        ├── confirm-dialog/
        ├── not-found/
        ├── pagination/
        ├── skeleton/        # SkeletonComponent + SkeletonTableComponent
        └── status-badge/
```

**Fluxo de dependências:** features → core/services → core/models | shared

---

## Padrões de Componente

Todos os componentes são **standalone**. Nunca use NgModules.

```typescript
@Component({
  selector: 'app-{name}',
  standalone: true,
  imports: [/* só o que o template usa */],
  templateUrl: './{name}.component.html',
})
export class {Name}Component {
  // Injeção via inject()
  private readonly service = inject({Name}Service);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  // Estado com signals
  readonly items = signal<{Model}[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);

  // Estado derivado com computed()
  readonly isEmpty = computed(() => this.items().length === 0);
}
```

**Template — fluxo de controle moderno (obrigatório):**
```html
@if (loading()) {
  <app-skeleton />
} @else if (items().length) {
  @for (item of items(); track item.id) { ... }
} @else {
  <p>Nenhum item encontrado.</p>
}
```

Nunca use `*ngIf`, `*ngFor`, `*ngSwitch` — use sempre `@if`, `@for`, `@switch`.

---

## Padrões de Service HTTP

```typescript
@Injectable({ providedIn: 'root' })
export class {Name}Service {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/{endpoint}`;

  getAll(page: number, pageSize: number): Observable<{Model}[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<{Model}[]>(this.base, { params });
  }

  getById(id: string): Observable<{Model}> {
    return this.http.get<{Model}>(`${this.base}/${id}`);
  }

  create(input: Create{Model}Input): Observable<{Model}> {
    return this.http.post<{Model}>(this.base, input);
  }

  update(id: string, input: Update{Model}Input): Observable<{Model}> {
    return this.http.put<{Model}>(`${this.base}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
```

- Retorne sempre `Observable<T>` — nunca use `async/await` nos services
- Subscreva nos componentes com `.subscribe({ next, error })`
- Não trate erros HTTP nos services — o `error.interceptor.ts` exibe o snackBar automaticamente

---

## Padrões de Modelos (Types)

Um arquivo por domínio em `core/models/`:

```typescript
// Output (API → Frontend)
export interface {Model}Output {
  id: string;
  companyId: string;
  // ... demais campos
}

// Inputs (Frontend → API)
export interface Create{Model}Input { ... }
export interface Update{Model}Input { ... }
```

- Todos os campos nullable da API são `?: string | number | etc.`
- Enums como union types: `type {EnumName} = 'Value1' | 'Value2' | ...`
- Status com metadados visuais: adicionar em arquivo `{model}-status.helper.ts`

---

## Padrões de Rota

**`app.routes.ts`** — rotas raiz com lazy loading:
```typescript
{
  path: 'feature',
  data: { breadcrumb: 'Nome no Breadcrumb' },
  loadChildren: () => import('./features/feature/feature.routes').then(m => m.featureRoutes),
}
```

**`feature.routes.ts`** — rotas da feature:
```typescript
export const featureRoutes: Routes = [
  { path: '', title: 'Lista — OrdemCerta', component: FeatureListComponent },
  { path: 'new', title: 'Novo — OrdemCerta', data: { breadcrumb: 'Novo' }, component: FeatureFormComponent },
  { path: ':id', title: 'Detalhe — OrdemCerta', data: { breadcrumb: 'Detalhe' }, component: FeatureDetailComponent },
  { path: ':id/edit', title: 'Editar — OrdemCerta', data: { breadcrumb: 'Editar' }, component: FeatureFormComponent },
];
```

- Rotas protegidas ficam sob o layout autenticado com `canActivate: [authGuard]`
- Rotas públicas (sem login) ficam fora do layout, sem `authGuard`
- Use `loadComponent` para rotas simples, `loadChildren` para features com sub-rotas

---

## Padrões de Formulário

```typescript
// FormControl simples com validação
readonly nameControl = new FormControl('', [Validators.required, Validators.maxLength(100)]);

// FormGroup
readonly form = new FormGroup({
  name: new FormControl('', Validators.required),
  email: new FormControl('', [Validators.required, Validators.email]),
});

// Submissão com guard
save(): void {
  if (this.form.invalid || this.saving()) return;
  this.saving.set(true);
  this.service.create(this.form.getRawValue()).subscribe({
    next: (result) => {
      this.snackBar.open('Salvo com sucesso.', 'Fechar', { duration: 3000 });
      this.router.navigate(['/feature', result.id]);
    },
    error: () => this.saving.set(false),
  });
}
```

**Template de erro de campo:**
```html
<mat-form-field appearance="outline">
  <mat-label>Nome</mat-label>
  <input matInput [formControl]="nameControl" />
  @if (nameControl.hasError('required')) {
    <mat-error>Campo obrigatório.</mat-error>
  }
</mat-form-field>
```

---

## Padrões de UI

### Feedback ao usuário
```typescript
// Sucesso
this.snackBar.open('Operação realizada.', 'Fechar', { duration: 3000 });

// Operações críticas — sempre usar ConfirmDialog antes de delete
const ref = this.dialog.open(ConfirmDialogComponent, {
  data: { title: 'Excluir item', message: 'Esta ação não pode ser desfeita.', confirmLabel: 'Excluir' },
});
ref.afterClosed().subscribe((confirmed) => { if (confirmed) this.delete(); });
```

### Loading skeleton
```html
@if (loading()) {
  <app-skeleton height="2rem" width="40%" />
  <app-skeleton height="10rem" />
} @else { ... }
```

### Status badge
```html
<app-status-badge [status]="order.status" />
```

### Paginação
```html
<app-pagination [total]="total()" [page]="page()" [pageSize]="pageSize()"
  (pageChange)="onPageChange($event)" />
```

### Botão com loading
```html
<button mat-flat-button (click)="save()" [disabled]="form.invalid || saving()">
  @if (saving()) { <mat-spinner diameter="20" /> } @else { Salvar }
</button>
```

---

## Interceptors

**`auth.interceptor.ts`** — injeta automaticamente o Bearer token em toda requisição. Redireciona para `/login` em 401.

**`error.interceptor.ts`** — captura todos os erros HTTP (exceto 401) e exibe snackBar com `error.error?.errors ?? ['Erro inesperado...']`. **Não trate erros HTTP individualmente nos componentes** — o interceptor já faz isso. Use o bloco `error:` no `.subscribe()` apenas para resetar estado de loading/saving.

---

## Convenções de Nomenclatura (Frontend)

| Elemento | Convenção | Exemplo |
|---|---|---|
| Componentes, Services, Guards | PascalCase | `OrderListComponent`, `AuthService` |
| Arquivos | kebab-case | `order-list.component.ts` |
| Signals | camelCase | `loading`, `items`, `saving` |
| Inputs/Outputs de componente | camelCase funcional | `input()`, `output()` |
| Interfaces de modelo | sufixo `Output` / `Input` | `ServiceOrderOutput`, `CreateOrderInput` |
| Status helpers | sufixo `.helper.ts` | `service-order-status.helper.ts` |
| Rotas de feature | sufixo `Routes` | `ordersRoutes`, `customersRoutes` |
| Seletores CSS | `app-` prefix | `app-order-list` |

---

## Regras Gerais (Frontend)

1. **Sempre standalone** — sem NgModules
2. **Signals para estado local** — `signal()`, `computed()`, `toSignal()`
3. **`@if/@for/@switch`** — nunca `*ngIf/*ngFor/*ngSwitch`
4. **inject()** — nunca injeção por construtor
5. **Reactive Forms** — nunca Template-driven Forms
6. **Português** — todos os textos visíveis ao usuário em pt-BR
7. **Sem erros HTTP nos components** — o interceptor trata; use `error:` só para resetar `saving.set(false)`
8. **ConfirmDialog obrigatório** em toda ação destrutiva (delete)
9. **SkeletonComponent** em todo carregamento de dados
10. **MatSnackBar** para feedback de sucesso/aviso (duration 3000–5000ms)
11. **`track item.id`** obrigatório em todo `@for`
12. **Lazy loading** obrigatório para todas as features
13. **Não adicionar** comentários, docstrings ou abstrações desnecessárias
