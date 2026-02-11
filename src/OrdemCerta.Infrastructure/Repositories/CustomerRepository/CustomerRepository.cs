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

    public async Task<Result<Customer>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email != null && c.Email.Value == email.ToLowerInvariant(), cancellationToken);

        if (customer == null)
            return "Cliente não encontrado";

        return customer;
    }

    public async Task<Result<Customer>> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var cleanDocument = new string(document.Where(char.IsDigit).ToArray());

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document != null && c.Document.Value == cleanDocument, cancellationToken);

        if (customer == null)
            return "Cliente não encontrado";

        return customer;
    }

    public async Task<Result<IEnumerable<Customer>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<Customer>>.Success(customers);
    }

    public async Task<Result<IEnumerable<Customer>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return "Página deve ser maior que zero";

        if (pageSize < 1 || pageSize > 100)
            return "Tamanho da página deve estar entre 1 e 100";

        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name.FullName)
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
        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Remove(customer);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Email != null && c.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var cleanDocument = new string(document.Where(char.IsDigit).ToArray());

        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Document != null && c.Document.Value == cleanDocument, cancellationToken);
    }
}