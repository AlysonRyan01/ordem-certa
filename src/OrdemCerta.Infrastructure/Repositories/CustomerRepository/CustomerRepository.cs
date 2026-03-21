using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Customers;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.CustomerRepository;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDataContext _context;

    public CustomerRepository(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<Result<Customer>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
            return "Cliente não encontrado";

        return customer;
    }

    public async Task<Result<Customer>> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
            return "Cliente não encontrado";

        return customer;
    }

    public async Task<Result<IEnumerable<Customer>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return "Página deve ser maior que zero";

        if (pageSize < 1 || pageSize > 100)
            return "Tamanho da página deve estar entre 1 e 100";

        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<Customer>>.Success(customers);
    }

    public async Task<Result<IEnumerable<Customer>>> GetByNameAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return "Termo de busca é obrigatório";

        if (page < 1)
            return "Página deve ser maior que zero";

        if (pageSize < 1 || pageSize > 100)
            return "Tamanho da página deve estar entre 1 e 100";

        var normalizedSearchTerm = searchTerm.Trim().ToLower();

        var customers = await _context.Customers
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.FullName.ToLower(), $"%{normalizedSearchTerm}%"))
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<Customer>>.Success(customers);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(customer).State == EntityState.Detached)
            _context.Customers.Update(customer);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Remove(customer);
        return Task.CompletedTask;
    }
}