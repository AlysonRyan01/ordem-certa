using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Companies;
using OrdemCerta.Domain.Customers;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.DataContext.Context;

public class ApplicationDataContext : DbContext
{
    private readonly IMediator? _mediator;

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options, IMediator? mediator = null)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDataContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_mediator != null)
        {
            var domainEvents = ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .SelectMany(e => e.DomainEvents)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .ToList()
                .ForEach(e => e.ClearDomainEvents());

            return result;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}