using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Admin;
using OrdemCerta.Domain.Companies;
using OrdemCerta.Domain.Customers;
using OrdemCerta.Domain.Sales;
using OrdemCerta.Domain.ServiceOrders;
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
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<CompanyOrderSequence> CompanyOrderSequences => Set<CompanyOrderSequence>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<CompanySaleSequence> CompanySaleSequences => Set<CompanySaleSequence>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDataContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }

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