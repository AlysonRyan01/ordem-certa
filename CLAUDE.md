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
