using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.Events;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders;

public class ServiceOrder : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int OrderNumber { get; private set; }
    public EquipmentInfo Equipment { get; private set; } = null!;
    public ServiceOrderStatus Status { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string? TechnicianName { get; private set; }
    public Budget? Budget { get; private set; }

    protected ServiceOrder() { }

    private ServiceOrder(Guid companyId, Guid customerId, int orderNumber, EquipmentInfo equipment, string? technicianName)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Equipment = equipment;
        Status = ServiceOrderStatus.Received;
        EntryDate = DateTime.UtcNow;
        TechnicianName = technicianName;
    }

    public static Result<ServiceOrder> Create(Guid companyId, Guid customerId, int orderNumber, EquipmentInfo equipment, string? technicianName)
    {
        return new ServiceOrder(companyId, customerId, orderNumber, equipment, technicianName);
    }

    public Result UpdateEquipment(EquipmentInfo equipment)
    {
        Equipment = equipment;
        return Result.Success();
    }

    public Result UpdateTechnician(string? technicianName)
    {
        TechnicianName = technicianName;
        return Result.Success();
    }

    public Result ChangeStatus(ServiceOrderStatus status)
    {
        Status = status;

        if (status == ServiceOrderStatus.ReadyForPickup)
            RaiseDomainEvent(new ServiceReadyEvent(
                Id, OrderNumber, CompanyId, CustomerId,
                Equipment.DeviceType, Equipment.Brand, Equipment.Model));

        return Result.Success();
    }

    public Result CreateBudget(Budget budget)
    {
        Budget = budget;
        Status = ServiceOrderStatus.WaitingApproval;

        RaiseDomainEvent(new BudgetCreatedEvent(
            Id,
            OrderNumber,
            CompanyId,
            CustomerId,
            Equipment.DeviceType,
            Equipment.Brand,
            Equipment.Model,
            budget.Value,
            budget.Description));

        return Result.Success();
    }

    public Result ApproveBudget()
    {
        if (Status != ServiceOrderStatus.WaitingApproval)
            return "A ordem não está aguardando aprovação";

        Status = ServiceOrderStatus.BudgetApproved;
        RaiseDomainEvent(new BudgetRespondedEvent(Id, OrderNumber, CompanyId, Approved: true));
        return Result.Success();
    }

    public Result RefuseBudget()
    {
        if (Status != ServiceOrderStatus.WaitingApproval)
            return "A ordem não está aguardando aprovação";

        Status = ServiceOrderStatus.BudgetRefused;
        RaiseDomainEvent(new BudgetRespondedEvent(Id, OrderNumber, CompanyId, Approved: false));
        return Result.Success();
    }
}
